-- Requires the FF12 lua loader 1.10.2 or newer.
if not (checkMinVersion and checkMinVersion(1, 10, 2)) then
    error("Rando Open World Archipelago Hook: requires the FF12 lua loader 1.10.2 or newer.")
end

-- Checked once here rather than at the call site. A nil dialog table would otherwise throw inside
-- the poll loop, which ends item delivery for the whole session without any further log output.
if dialog == nil then
    error("Rando Open World Archipelago Hook: this lua loader has no dialog support, which is needed to tell when the field is ready.")
end

-- How often to look for the next item, and how quickly to come back while one is still in flight.
local POLL_MS = 200
local RETRY_MS = 150

-- Consecutive render frames the field has to keep looking ready before anything is granted. Counted
-- over frames that pass the field checks, not frames since a transition started: the loading screen
-- renders too, so counting raw frames would let the window elapse during the load it guards against.
local READY_FRAMES = 30

-- The add item call takes its count as a signed byte, so a bigger stack is handed over in chunks.
local MAX_GRANT_PER_CALL = 127

local function incRandoIndex()
    memory.u32[0x02164480+0x696] = memory.u32[0x02164480+0x696] + 1
end

local function getRandoIndex()
    return memory.u32[0x02164480+0x696]
end

local function incGil(count)
    memory.u32[0x02164480-0x1F8] = memory.u32[0x02164480-0x1F8] + count
end

local function readMapID()
    return memory.u32[0x02164480+0x1044]
end

local function readGameState()
    local pointer1 = memory.u32[0x01E5FFE0 + 0x120000]
    if pointer1 == 0 then
        -- The memory accessors read unmapped addresses as 0, so a torn down state object would
        -- otherwise report 0 here, which is the value that means normal field play.
        return -1
    end

    return memory.u8[pointer1 + 0x3A]
end

local function readScenarioFlag()
    return memory.u16[0x02164480]
end

local current_item = nil
local ready_frames = 0

-- The party is on the field with a loaded save behind it, judged on this frame alone.
local function fieldReady()
    local map_id = readMapID()
    if map_id == 0 or map_id > 0xFFFF or map_id <= 12 or map_id == 274 then
        return false
    end

    if readGameState() ~= 0 then
        return false
    end

    if readScenarioFlag() < 45 then
        return false
    end

    -- The engine refuses native windows while loading or mid transition, which catches the frames
    -- where the map and state already read as the field but the save is still being applied.
    return dialog.ready()
end

local function canReceiveItems()
    return ready_frames >= READY_FRAMES
end

-- Runs on the game thread with the game blocked, so nothing can move underneath it between the
-- checks and the writes. Every write to game memory goes through here for that reason, and it
-- deliberately does no string work so it cannot throw on the game thread.
local function onFlipAdd()
    if fieldReady() then
        if ready_frames < READY_FRAMES then
            ready_frames = ready_frames + 1
        end
    else
        ready_frames = 0
    end

    if current_item == nil or not canReceiveItems() then
        return
    end

    -- The queue is filled on the script thread, which can interleave with a save load restoring a
    -- different index. Granting a stale entry would both hand out an item the save already has and
    -- consume an index it never came from, so drop it and let the loop re-read the current one.
    if current_item[3] ~= getRandoIndex() then
        current_item = nil
        return
    end

    local id = current_item[1]
    local count = current_item[2]

    if id == 0xFFFE and count > 0 then
        incGil(count)
        count = 0
    elseif id ~= 0xFFFF and count > 0 then
        local chunk = count
        if chunk > MAX_GRANT_PER_CALL then
            chunk = MAX_GRANT_PER_CALL
        end

        memory.execute(0x003008A0, memory.arg.void, {memory.arg.u16, memory.arg.s8, memory.arg.u8, memory.arg.u8, memory.arg.u8}, {id, chunk, 0, 1, 1})
        count = count - chunk
    else
        -- Either the explicit no-op id or an amount there is nothing to give for. Consume it rather
        -- than retrying forever on a file that will never grant anything.
        count = 0
    end

    if count > 0 then
        -- Part of the stack is in; keep the index until the rest follows on later frames.
        current_item[2] = count
        return
    end

    -- The item is only in the inventory now, so only now has this index been consumed. Advancing
    -- any earlier means a load or a missed flip drops the item while the index moves on regardless.
    -- This has to happen before the queue is cleared: the script thread treats an empty queue as its
    -- cue to read again, and it would otherwise read this same index a second time.
    incRandoIndex()
    current_item = nil
end

local function onMapJump()
    -- A queued item survives a map change: the index is untouched, so it is still the right item.
    ready_frames = 0
end

local function onSaveLoad()
    -- The load overwrites the save block, including the rando index, so anything queued against the
    -- old index no longer corresponds to anything. Drop it and let the loop re-read against the
    -- index the save actually restored. Nothing is lost: the index was never advanced for it.
    current_item = nil
    ready_frames = 0
end

local function read_comm_file()
    -- Read from %LOCALAPPDATA%/FF12OpenWorldAP/items_received_####.txt

    local appdata = os.getenv("LOCALAPPDATA")
    if appdata == nil then
        return nil
    end

    local index = getRandoIndex()

    local filepath = appdata .. "\\FF12OpenWorldAP\\items_received_" .. string.format("%04d", index) .. ".txt"
    local file = io.open(filepath, "r")
    if not file then return nil end  -- Item not received yet

    -- Parse id num from first line and count num from second line
    local id_line = file:read("*l")
    local count_line = file:read("*l")
    file:close()

    -- Validate what the lines parse to, not that they are present. An empty string is truthy in
    -- Lua, so a file caught half written would otherwise queue a pair of nils.
    local id = tonumber(id_line)
    local count = tonumber(count_line)
    if id == nil or count == nil then
        return nil
    end

    -- Stamped with the index it was read against so the grant can tell it is still current.
    return {id, count, index}
end

-- Returns how long to wait before looking again.
local function pollForItems()
    -- Only one item in flight at a time. Reading the next one while a grant is still pending would
    -- overwrite it and drop it silently.
    if current_item ~= nil then
        return RETRY_MS
    end

    if not canReceiveItems() then
        return POLL_MS
    end

    local next_item = read_comm_file()
    if next_item == nil then
        return POLL_MS
    end

    print("Adding item " .. next_item[1] .. " x" .. next_item[2])

    -- Hand it to the flip handler rather than granting here. This runs on the script's own thread,
    -- so a write from here could land in the middle of a map jump the game thread just started.
    current_item = next_item

    return RETRY_MS
end

local function addItems()
    -- This timer chain is the only thing driving delivery and onInitDone that seeds it is one shot,
    -- so an error escaping here would silently stop every future item for the session. Rearm no
    -- matter what happened.
    local ok, delay = pcall(pollForItems)
    if not ok then
        print("Rando Open World Archipelago Hook: item poll failed: " .. tostring(delay))
        delay = POLL_MS
    end

    event.executeAfterMs(delay, addItems)
end


local function onExit()
    collectgarbage()
end

print("Rando Open World Archipelago Hook: Applying patch.")

event.registerEventAsync("onInitDone", addItems)
event.registerEventAsync("exit", onExit)
event.registerEventSync("onFlip", onFlipAdd)
event.registerEventSync("onMapJump", onMapJump)
event.registerEventSync("onSaveLoad", onSaveLoad)

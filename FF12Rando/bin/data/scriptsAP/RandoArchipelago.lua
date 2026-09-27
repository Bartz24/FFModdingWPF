if not (checkMinVersion and checkMinVersion(1, 10, 2)) then
    error("Rando Open World Archipelago Hook: requires the FF12 lua loader 1.10.2 or newer.")
end

if dialog == nil then
    error("Rando Open World Archipelago Hook: this lua loader has no dialog support, which is needed to tell when the field is ready.")
end

local POLL_MS = 200
local RETRY_MS = 150
local READY_FRAMES = 30
local TREASURE_POLL_MS = 500
local MAX_GRANT_PER_CALL = 127

local TREASURE_BANK_OFFSET = 0x14B4
local TREASURE_BANK_BYTES = 2

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
        return -1
    end

    return memory.u8[pointer1 + 0x3A]
end

local function readScenarioFlag()
    return memory.u16[0x02164480]
end

local current_item = nil
local ready_frames = 0

local treasure_last_map = nil

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

    return dialog.ready()
end

local function canReceiveItems()
    return ready_frames >= READY_FRAMES
end

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
        count = 0
    end

    if count > 0 then
        current_item[2] = count
        return
    end

    incRandoIndex()
    current_item = nil
end

local function onMapJump()
    ready_frames = 0
end

local function onSaveLoad()
    current_item = nil
    ready_frames = 0
    treasure_last_map = nil
end

local function commFolder()
    local appdata = os.getenv("LOCALAPPDATA")
    if appdata == nil then
        return nil
    end

    return appdata .. "\\FF12OpenWorldAP\\"
end

local function read_comm_file()
    local folder = commFolder()
    if folder == nil then
        return nil
    end

    local index = getRandoIndex()

    local filepath = folder .. "items_received_" .. string.format("%04d", index) .. ".txt"
    local file = io.open(filepath, "r")
    if not file then return nil end

    local id_line = file:read("*l")
    local count_line = file:read("*l")
    file:close()

    local id = tonumber(id_line)
    local count = tonumber(count_line)
    if id == nil or count == nil then
        return nil
    end

    return {id, count, index}
end

local function pollForItems()
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
    current_item = next_item

    return RETRY_MS
end

local function writeOpenedTreasures()
    local map_id = readMapID()

    if not fieldReady() then
        treasure_last_map = nil
        return
    end

    if treasure_last_map ~= map_id then
        treasure_last_map = map_id
        return
    end

    local appdata = os.getenv("LOCALAPPDATA")
    if appdata == nil then
        return
    end

    local lines = {}
    for byte = 0, TREASURE_BANK_BYTES - 1 do
        local value = memory.u8[0x02164480 + TREASURE_BANK_OFFSET + byte]
        for bit = 0, 7 do
            if ((value >> bit) & 1) == 1 then
                lines[#lines + 1] = tostring(byte * 8 + bit)
            end
        end
    end

    local text = table.concat(lines, "\n")
    if #lines > 0 then
        text = text .. "\n"
    end

    local filepath = appdata .. "\\FF12OpenWorldAP\\treasures_" .. string.format("%04X", map_id) .. ".txt"
    local file = io.open(filepath, "w")
    if not file then
        return
    end

    file:write(text)
    file:close()
end

local function pollTreasures()
    local ok, err = pcall(writeOpenedTreasures)
    if not ok then
        print("Rando Open World Archipelago Hook: treasure write failed: " .. tostring(err))
    end

    event.executeAfterMs(TREASURE_POLL_MS, pollTreasures)
end

local function addItems()
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
event.registerEventAsync("onInitDone", pollTreasures)
event.registerEventAsync("exit", onExit)
event.registerEventSync("onFlip", onFlipAdd)
event.registerEventSync("onMapJump", onMapJump)
event.registerEventSync("onSaveLoad", onSaveLoad)

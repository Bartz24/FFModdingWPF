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
    return memory.u8[pointer1 + 0x3A]
end

local function readScenarioFlag()
    return memory.u16[0x02164480]
end

local current_item = nil

local function onFlipAdd()    
    if current_item == nil then
        return
    end

    memory.execute(0x003008A0, memory.arg.void, {memory.arg.u16, memory.arg.s8, memory.arg.u8, memory.arg.u8, memory.arg.u8}, {current_item[1], current_item[2], 0, 1, 1})
    current_item = nil
end

local function read_comm_file()
    -- Read from %LOCALAPPDATA%/FF12OpenWorldAP/items_received_####.txt

    local index = getRandoIndex()
    
    local filepath = os.getenv("LOCALAPPDATA") .. "\\FF12OpenWorldAP\\items_received_" .. string.format("%04d", index) .. ".txt"
    local file = io.open(filepath, "r")
    if not file then return nil end  -- Item not received yet

    -- Parse id num from first line and count num from second line
    local id = file:read("*l")
    local count = file:read("*l")
    file:close()
    
    if id and count then
        return {tonumber(id), tonumber(count)}
    end

    return nil  -- Return nil if format is invalid
end

local function addItems()
    local map_id = readMapID()
    local game_state = readGameState()
    local scenario_flag = readScenarioFlag()

    if map_id == 0 or map_id > 0xFFFF or map_id <= 12 or map_id == 274 or game_state ~= 0 or scenario_flag < 45 then
        event.executeAfterMs(200, addItems)
        return
    end

    local next_item = read_comm_file()
    if next_item == nil then
        event.executeAfterMs(200, addItems)
        return
    end

    local id = next_item[1]
    local count = next_item[2]
    print("Adding item " .. id .. " x" .. count)

    if id == 0xFFFE and count > 0 then
        incGil(count)
    elseif id ~= 0xFFFF and count > 0 then
        current_item = {id, count}
    end
    incRandoIndex()

    event.executeAfterMs(150, addItems)
end


local function onExit()
    collectgarbage()
end

print("Rando Open World Archipelago Hook: Applying patch.")

-- Delete the items_received.txt file on start up
local filepath = os.getenv("LOCALAPPDATA") .. "\\FF12OpenWorldAP\\items_received.txt"
os.remove(filepath)

event.registerEventAsync("onInitDone", addItems)
event.registerEventAsync("exit", onExit)
event.registerEventSync("onFlip", onFlipAdd)
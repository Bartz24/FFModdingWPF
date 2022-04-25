local treasures = {}

local function readMapID()
    return memory.u32[0x02164480+0x1044]
end

local function treasureOpened(respawn)
    local byteIndex = respawn // 8
    local byteValue = memory.u8[0x02165934 + byteIndex]
    local bitIndex = respawn % 8
    return byteValue & (2 ^ bitIndex) > 0
end

local function isOverlayMapOpen()
    return memory.u8[0x02092750] > 0
end

local function checkTreasures()
    local map = readMapID()
    if isOverlayMapOpen() and treasures[map] ~= nil then
        local count = 0
        local total = #treasures[map]
        for index, value in ipairs(treasures[map]) do
            if treasureOpened(value) then
                count = count + 1
            end
        end

        local text = message.convert((total - count) .. " treasure(s) remain")
        message.print(text)
    end
    event.executeAfterMs(100, checkTreasures)
end

local function onExit()
    collectgarbage()
end

local function splitString (inputstr, sep)
    if sep == nil then
            sep = "%s"
    end
    local t={}
    for str in string.gmatch(inputstr, "([^"..sep.."]+)") do
            table.insert(t, str)
    end
    return t
end

print("Rando Treasure Tracker: Applying patch.")

local file = io.open("rando/outdata/treasureTracker.txt", "r")
if file~=nil then 
    local fileContent = {}
    for line in file:lines() do
        table.insert (fileContent, line)
    end
    io.close(file)
    
    for index, value in ipairs(fileContent) do
        local splitStr = splitString(value, ",")
        local map = tonumber(splitStr[1], 16)
        treasures[map] = { }
        for i, v in ipairs(splitStr) do
            if i > 1 then
                treasures[map][i-1] = tonumber(splitStr[i])
            end
        end
    end
end

event.registerEventAsync("onInitDone", checkTreasures)
event.registerEventAsync("exit", onExit)
-- Requires the FF12 lua loader 1.10.2 or newer.
if not (checkMinVersion and checkMinVersion(1, 10, 2)) then
    error("Rando Treasure Tracker: requires the FF12 lua loader 1.10.2 or newer.")
end

if modmenu == nil or dialog == nil then
    print("Rando Treasure Tracker: this lua loader is missing mod menu or dialog support, skipping.")
    return
end

local MOD_ID = "randoTreasure"

local REFRESH_MS = 100

local REMINDER_TEXT = "{scale:70}Bind a key in Mod Config to track treasures"

local CONFIG_PATH = "scripts/config/RandoTreasure.json"

local treasures = {}

local hotkeyId = nil

local show_key_bound = false

local tracker_visible = false
local message_up = false

local encoded_cache = {}

local function encoded(text)
    local cached = encoded_cache[text]
    if cached == nil then
        cached = message.convert(text)
        encoded_cache[text] = cached
    end

    return cached
end

local function readMapID()
    return memory.u32[0x02164480+0x1044]
end

local function treasureOpened(respawn)
    local byteIndex = respawn // 8
    local byteValue = memory.u8[0x02165934 + byteIndex]
    local bitIndex = respawn % 8
    return byteValue & (2 ^ bitIndex) > 0
end

local function treasureCounts(map)
    local list = treasures[map]
    if list == nil then
        return 0, 0
    end

    local opened = 0
    for index, value in ipairs(list) do
        if treasureOpened(value) then
            opened = opened + 1
        end
    end

    return #list - opened, #list
end

local function readGameState()
    local pointer1 = memory.u32[0x01E5FFE0 + 0x120000]
    if pointer1 == 0 then
        return -1
    end

    return memory.u8[pointer1 + 0x3A]
end

local function fieldReady()
    local map = readMapID()
    if map == 0 or map > 0xFFFF or map <= 12 or map == 274 then
        return false
    end

    if readGameState() ~= 0 then
        return false
    end

    return dialog.ready()
end

local function treasureCountText()
    local remain, total = treasureCounts(readMapID())
    if total == 0 then
        return "{scale:70}No treasures on this map"
    end

    return "{scale:70}" .. remain .. "/" .. total .. " treasures remain"
end

local function refreshDisplay()
    local text = nil
    if fieldReady() then
        if not show_key_bound then
            text = REMINDER_TEXT
        elseif tracker_visible then
            text = treasureCountText()
        end
    end

    if text ~= nil then
        message.print(encoded(text))
        message_up = true
    elseif message_up then
        message.close()
        message_up = false
    end

    event.executeAfterMs(REFRESH_MS, refreshDisplay)
end

local function saveBinding()
    if config == nil then
        return
    end

    config.saveJson(CONFIG_PATH, { showKey = modmenu.getKey(MOD_ID, "showKey", 0) or 0 })
end

local function restoreBinding()
    if config == nil then
        return
    end

    local data = config.loadJson(CONFIG_PATH)
    if data == nil or type(data.showKey) ~= "number" or data.showKey == 0 then
        return
    end

    modmenu.setKey(MOD_ID, "showKey", 0, data.showKey, false)
end

-- The bound key can change at any time, so the hotkey is registered from the row's own callback.
local function rebindHotkey()
    if hotkeyId then
        modmenu.unregisterHotkey(hotkeyId)
        hotkeyId = nil
    end

    local key = modmenu.getKey(MOD_ID, "showKey", 0)
    show_key_bound = key ~= nil and key ~= 0

    if show_key_bound then
        hotkeyId = modmenu.registerHotkey(key, function()
            -- The refresh loop picks this up and shows or closes the window accordingly.
            tracker_visible = not tracker_visible
        end)
    end
end

local function registerModMenu()
    modmenu.addTable({
        id = MOD_ID,
        name = "Rando Treasure Tracker",
        schema = {
            { label = "Treasure Tracker", type = "divider" },
            {
                id = "showKey",
                label = "Toggle treasure tracker",
                type = "keybind",
                entries = 1,
                help = "Key to show or hide the count of treasures left on the current map (recommend {btn:l2}).",
                callback = function()
                    rebindHotkey()
                    saveBinding()
                end
            },
        },
    })

    restoreBinding()
    rebindHotkey()
end

local function onExit()
    if hotkeyId then
        modmenu.unregisterHotkey(hotkeyId)
        hotkeyId = nil
    end

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

local file = io.open("../rando/treasureTracker.txt", "r")
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

registerModMenu()

event.registerEventAsync("onInitDone", refreshDisplay)
event.registerEventAsync("exit", onExit)

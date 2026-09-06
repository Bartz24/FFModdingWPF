-- Requires the FF12 lua loader 1.10.2 or newer.
if not (checkMinVersion and checkMinVersion(1, 10, 2)) then
    error("Rando Treasure Tracker: requires the FF12 lua loader 1.10.2 or newer.")
end

-- The bound key is the only way to ask for the count, so without a mod menu there is nothing this
-- script can do. dialog is what tells it whether the game is on the field rather than a title or
-- loading screen.
if modmenu == nil or dialog == nil then
    print("Rando Treasure Tracker: this lua loader is missing mod menu or dialog support, skipping.")
    return
end

local MOD_ID = "randoTreasure"

-- How often the display is redrawn. An auto mode message only lasts 2000ms, so staying on screen
-- means reprinting it.
local REFRESH_MS = 100

local REMINDER_TEXT = "{scale:70}Bind a key in Mod Config to track treasures"

-- The mod menu keeps bindings in memory only, so the key is kept here to survive a restart.
-- Relative to the loader's working directory, the same place config.path points at.
local CONFIG_PATH = "scripts/config/RandoTreasure.json"

local treasures = {}

local hotkeyId = nil

-- Cached alongside the binding rather than read per frame.
local show_key_bound = false

-- Whether the player has toggled the tracker on, and whether our message is currently up.
local tracker_visible = false
local message_up = false

-- The docs call out reconverting the same text repeatedly as wasteful, and the display reprints
-- ten times a second, so each distinct line is encoded once and reused.
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

-- Remaining and total for a map. A map with no tracked treasures is 0 of 0 rather than nothing, so
-- that every area has a count to report.
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
        -- The memory accessors read unmapped addresses as 0, so a torn down state object would
        -- otherwise report 0 here, which is the value that means normal field play.
        return -1
    end

    return memory.u8[pointer1 + 0x3A]
end

-- Only draw over actual field play. Mirrors the gate the Archipelago hook uses before it touches
-- the game, which keeps the display off the title screen, loading screens and transitions.
local function fieldReady()
    local map = readMapID()
    if map == 0 or map > 0xFFFF or map <= 12 or map == 274 then
        return false
    end

    if readGameState() ~= 0 then
        return false
    end

    -- The engine refuses messages while loading or mid transition anyway; asking first keeps the
    -- window from being pushed at a frame that cannot take it.
    return dialog.ready()
end

local function treasureCountText()
    local remain, total = treasureCounts(readMapID())
    if total == 0 then
        return "{scale:70}No treasures on this map"
    end

    return "{scale:70}" .. remain .. "/" .. total .. " treasures remain"
end

-- Drives everything on screen. The unbound reminder wins, since with no key assigned there is no
-- way to reach the tracker and nothing else to advertise it; otherwise the count shows for as long
-- as the player has it toggled on, and the window is closed outright when it should not.
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
        -- Toggled off, or a key was just bound. Take our message down now instead of leaving the
        -- last one to time out.
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

-- Puts last session's key back into the row. setKey does not fire the row's callback, so the
-- caller still has to arm the hotkey afterwards. markChanged is false because a restored binding is
-- the baseline, not something the player just changed.
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

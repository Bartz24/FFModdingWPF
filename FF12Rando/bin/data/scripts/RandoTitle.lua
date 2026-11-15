-- Requires the FF12 lua loader 1.10.2 or newer with the dialog service.
if not (checkMinVersion and checkMinVersion(1, 10, 2)) then
    error("Rando Title: requires the FF12 lua loader 1.10.2 or newer.")
end

if dialog == nil or dialog.amest == nil then
    print("Rando Title: this lua loader is missing dialog support, skipping.")
    return
end

local INFO_PATH = "../rando/titleInfo.txt"

local POS_X = 979
local POS_Y = 668

local POLL_MS = 250

local body = nil
local handle = nil
local lastFailure = nil

local function reportFailure(reason)
    reason = tostring(reason)
    if reason ~= lastFailure then
        print("Rando Title: " .. reason)
        lastFailure = reason
    end
end

local function readInfo()
    local file = io.open(INFO_PATH, "r")
    if file == nil then
        return nil
    end

    local lines = {}
    for line in file:lines() do
        if line ~= "" then
            table.insert(lines, line)
        end
    end
    io.close(file)

    if #lines == 0 then
        return nil
    end

    -- Show all the text at once centered.
    return "{speed}{aligncenter}" .. table.concat(lines, "\n")
end

-- Required title screen state to not show on other menus
local TITLE_MENU_ADDR_A = 0x01EFAC12
local TITLE_MENU_VALUE_A = 2
local TITLE_MENU_ADDR_B = 0x02EC0B39
local TITLE_MENU_VALUE_B = 10

local function onTitle()
    return memory.u8[TITLE_MENU_ADDR_A] == TITLE_MENU_VALUE_A
        and memory.u8[TITLE_MENU_ADDR_B] == TITLE_MENU_VALUE_B
end

local function isOpen()
    return handle ~= nil and (handle:alive() or handle:pending())
end

local function open()
    local h, reason = dialog.amest.open {
        body = body,
        x = POS_X,
        y = POS_Y,
        align = 4,
        focus = false,
        dismissible = false,
        onClose = function()
            handle = nil
        end,
        onError = function(err)
            reportFailure("window failed: " .. tostring(err))
            handle = nil
        end,
    }

    if h == nil then
        reportFailure("could not open window: " .. tostring(reason))
    else
        lastFailure = nil
    end

    handle = h
end

local function close()
    if handle ~= nil then
        handle:close()
        handle = nil
    end
end

local function refresh()
    if onTitle() then
        if not isOpen() and dialog.ready() then
            open()
        end
    else
        close()
    end

    event.executeAfterMs(POLL_MS, refresh)
end

body = readInfo()
if body == nil then
    print("Rando Title: no seed info found, skipping.")
    return
end

print("Rando Title: Applying patch.")

event.registerEventAsync("onInitDone", refresh)

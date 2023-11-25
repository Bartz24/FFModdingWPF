local currentPage = 1

local function descriptionSwap()
    local current = "scripts/config/TheInsurgentsDescriptiveInventoryConfig/us.lua"
    local page1 = "scripts/config/TheInsurgentsDescriptiveInventoryConfig/us.lua.page1"
    local page2 = "scripts/config/TheInsurgentsDescriptiveInventoryConfig/us.lua.page2"

    -- Swap the current page and copy the new page to the current file path
    local file = nil
    if currentPage == 1 then
        currentPage = 2
        file = io.open(page2, "r")
    else
        currentPage = 1
        file = io.open(page1, "r")
    end

    local currentFile = io.open(current, "w")
    if file ~= nil and currentFile ~= nil then
        currentFile:write(file:read("*all"))
    end
    if file ~= nil then
        file:close()
    end
    if currentFile ~= nil then
        currentFile:close()
    end
    
    event.executeAfterMs(3500, descriptionSwap)
end

local function onExit()
    collectgarbage()
end

print("Rando Description Swap: Applying patch.")

local configFile = "scripts/config/TheInsurgentsDescriptiveInventoryConfig/us.lua"
local f = io.open(configFile, "r")
if f ~= nil then
    io.close(f)
else
    -- Do not run if the config file does not exist
    print("Rando Description Swap: Skipping due to missing file.")
    return
end

event.registerEventAsync("onInitDone", descriptionSwap)
event.registerEventAsync("exit", onExit)

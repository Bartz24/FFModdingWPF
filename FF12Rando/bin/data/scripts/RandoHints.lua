-- Requires the FF12 lua loader 1.10.2 or newer.
if not (checkMinVersion and checkMinVersion(1, 10, 2)) then
    error("Rando Hints: requires the FF12 lua loader 1.10.2 or newer.")
end

local function countBits(n)
    local count = 0;
    while n > 0 do
        if n & 1 == 1 then
            count = count + 1
        end
        n = n >> 1;
    end
    return count;
end

local function readSkyPiratesDen()
    local count = 0
    for i = 0,3 do
        local val = memory.u8[0x0216DCC4+0x2000+i]
        count = count + countBits(val)
    end    
    return count
end

local function writeSkyPiratesDen()
    memory.writeU8(0x02164480+0xdffd, readSkyPiratesDen())
end

local function checkHints()
    writeSkyPiratesDen()
    event.executeAfterMs(100, checkHints)
end

local function onExit()
    collectgarbage()
end

print("Rando Hints: Applying patch.")

event.registerEventAsync("onInitDone", checkHints)
event.registerEventAsync("exit", onExit)
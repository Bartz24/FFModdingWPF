-- Requires the FF12 lua loader 1.10.2 or newer.
if not (checkMinVersion and checkMinVersion(1, 10, 2)) then
    error("Rando Minimap: requires the FF12 lua loader 1.10.2 or newer.")
end

local SAVE_BASE = 0x02164480
local MINIMAP_FLAGS_OFFSET = 0x1062
local INTERFERENCE_BIT = 0x40

-- Runs on the game thread every frame, so the bit is gone before the minimap next draws.
local function onFlip()
    local address = SAVE_BASE + MINIMAP_FLAGS_OFFSET
    local value = memory.u8[address]
    if value & INTERFERENCE_BIT ~= 0 then
        memory.u8[address] = value & ~INTERFERENCE_BIT
    end
end

print("Rando Minimap: Applying patch.")

event.registerEventSync("onFlip", onFlip)

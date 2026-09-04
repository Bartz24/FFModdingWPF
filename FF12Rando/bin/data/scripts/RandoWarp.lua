-- Adds a "Rando Warp" page to the loader's Mod Config menu (the last entry of the
-- game's main menu) with a warp back to the Sandsea.
--
-- This is an escape hatch for softlocks, not a travel option. Yanking the player out
-- of a map the story is in the middle of driving can leave scenario flags half set,
-- which breaks the save in ways this script cannot detect or undo.
--
-- The warp is never fired straight from the menu. Confirming the button only queues
-- it; the jump happens once the player is back in the field and the game is in a
-- state where a map load is safe. That keeps the warp from firing mid-battle, during
-- an event, or while the game is still sitting in a menu.

-- Requires the FF12 lua loader 1.10.2 or newer.
if not (checkMinVersion and checkMinVersion(1, 10, 2)) then
    error("Rando Warp: requires the FF12 lua loader 1.10.2 or newer.")
end

local MOD_ID = "randoWarp"

-- Destination for the warp. call.mapJump takes (map, entry point, animation).
-- 0x130 is rbn_a16 in data\mapAreas.csv, the Sandsea in Rabanastre.
local SANDSEA_MAP = 0x130
local SANDSEA_ENTRY = 0
local WARP_ANIMATION = 0

local POLL_MS = 200

local pendingWarp = false

-- Encoded lazily: message.convert is a game call, so it is not run at script load.
local function show(text)
    message.print(message.convert("{scale:70}" .. text))
end

local function readGameState()
    local pointer1 = memory.u32[0x01E5FFE0 + 0x120000]
    return memory.u8[pointer1 + 0x3A]
end

-- True only in normal field gameplay. Mirrors the map/state gate the Archipelago
-- hook uses before it touches the inventory.
local function canWarp()
    local map = call.getCurrentMap()
    if map == 0 or map > 0xFFFF or map <= 12 or map == 274 then
        return false
    end

    return readGameState() == 0
end

local function requestWarp()
    if call.getCurrentMap() == SANDSEA_MAP then
        show("Already at the Sandsea.")
        return
    end

    pendingWarp = true
    show("Warping back to the Sandsea...")
end

-- Fires a queued warp as soon as the field is safe.
local function tick()
    if pendingWarp and canWarp() then
        pendingWarp = false
        call.mapJump(SANDSEA_MAP, SANDSEA_ENTRY, WARP_ANIMATION)
    end

    event.executeAfterMs(POLL_MS, tick)
end

if modmenu == nil then
    print("Rando Warp: this lua loader has no mod menu support, skipping.")
    return
end

print("Rando Warp: Applying patch.")

modmenu.addTable({
    id = MOD_ID,
    name = "Rando Warp",
    schema = {
        { label = "Warp", type = "divider" },
        {
            id = "warpSandsea",
            label = "Return to the Sandsea",
            type = "button",
            help = "Softlock escape only. Using this during a story sequence may break your save.",
            action = "back",
            callback = function()
                requestWarp()
            end
        },
        { label = "Warning", type = "divider" },
        { label = "Intended for escaping softlocks only." },
        { label = "Using this during a story sequence" },
        { label = "may permanently break your save." },
    },
})

local function onExit()
    collectgarbage()
end

event.registerEventAsync("onInitDone", tick)
event.registerEventAsync("exit", onExit)

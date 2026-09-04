-- Requires the FF12 lua loader 1.10.2 or newer.
if not (checkMinVersion and checkMinVersion(1, 10, 2)) then
    error("Rando Treasure Bank: requires the FF12 lua loader 1.10.2 or newer.")
end

-- The engine records "this chest has been opened" as a bit in a 256 bit field at
-- save + 0x14B4, indexed by the treasure's Respawn byte. One byte of index means at most
-- 255 chests in the game can ever be one-time, but there are 1916 of them.
--
-- The randomizer therefore hands out respawn ids that are local to a single map (a chest's
-- index within its own map, always 0..15). This script makes that safe by banking: only the
-- current map's slice of the bitfield is ever live, and the rest is held here and persisted
-- alongside the save file. Every chest in the game can then be one-time.

local SAVE_BASE = 0x02164480
local BANK_OFFSET = 0x14B4     -- opened-chest bitfield
local BANK_BYTES = 2           -- 16 ids per map, matching TreasureRando.MaxRespawnIdsPerMap
local MAP_ID_OFFSET = 0x1044

local HANDLER_ID = "RandoTreasureBank"
local STATE_VERSION = 1

-- masks[mapIdAsString] = integer holding that map's BANK_BYTES worth of opened bits.
-- Absent means "nothing opened there yet", which is the correct default for a fresh game
-- and for a save that predates this script.
local masks = {}
local currentMap = nil
local reapplyFrames = 0

local function readMapID()
    return memory.u32[SAVE_BASE + MAP_ID_OFFSET]
end

-- Map ids the game uses for menus, loading and other non-play states. Banking against these
-- would attribute the previous map's bits to a map that has no chests, so they are ignored
-- and the current map is left alone until a real one arrives. Mirrors the guard the
-- Archipelago hook uses.
local function isRealMap(mapID)
    return mapID ~= nil and mapID > 12 and mapID < 0xFFFF and mapID ~= 274
end

local function readBank()
    local value = 0
    for i = 0, BANK_BYTES - 1 do
        value = value | (memory.u8[SAVE_BASE + BANK_OFFSET + i] << (i * 8))
    end
    return value
end

local function writeBank(value)
    for i = 0, BANK_BYTES - 1 do
        memory.u8[SAVE_BASE + BANK_OFFSET + i] = (value >> (i * 8)) & 0xFF
    end
end

-- Fold whatever the engine currently has into the map it belongs to.
local function captureCurrent()
    if currentMap ~= nil then
        masks[tostring(currentMap)] = readBank()
    end
end

local function applyMap(mapID)
    currentMap = mapID
    writeBank(masks[tostring(mapID)] or 0)
end

-- onMapJump is where the swap belongs, but the docs do not pin down its ordering against
-- the engine's own read of the bitfield. Re-applying for a few frames afterwards costs
-- nothing and covers the case where the map's actors are placed first. The player cannot
-- open a chest during a load, so this cannot clobber a real bit.
-- Deliberately does not capture the outgoing map here. By the time a transition is visible
-- the engine may already have rewritten the live bytes for the incoming map, and folding
-- that back into the outgoing map would erase its progress. The per-frame capture in onFlip
-- keeps the outgoing map current as of its last frame, which is close enough -- a chest
-- opening is always followed by many frames before a map can change.
local function switchTo(mapID)
    if not isRealMap(mapID) or mapID == currentMap then
        return
    end

    applyMap(mapID)
    reapplyFrames = 6
end

local function onFlip()
    local mapID = readMapID()

    if mapID ~= currentMap then
        switchTo(mapID)
        return
    end

    if reapplyFrames > 0 then
        reapplyFrames = reapplyFrames - 1
        writeBank(masks[tostring(currentMap)] or 0)
        return
    end

    -- Steady state: the engine owns the live bytes, so keep following them. This is what
    -- actually records a chest being opened.
    captureCurrent()
end

local function onMapJump()
    switchTo(readMapID())
end

-- A save that is loaded carries whichever map's bits happened to be live when it was
-- written. Drop that and re-derive from the sidecar so a stale slice cannot leak into a
-- different map.
local function onSaveLoad()
    currentMap = nil
    local mapID = readMapID()
    if isRealMap(mapID) then
        applyMap(mapID)
        reapplyFrames = 6
    else
        writeBank(0)
    end
end

local function onSave(path)
    captureCurrent()

    -- Returning an empty table tells the loader to delete this handler's saved data, which
    -- is exactly wrong for a new game that has not opened a chest yet. The version key keeps
    -- the table non-empty and gives later formats something to branch on.
    local data = { version = STATE_VERSION, masks = {} }
    for mapKey, mask in pairs(masks) do
        if mask ~= 0 then
            data.masks[mapKey] = mask
        end
    end
    return data
end

local function onLoad(path, data)
    masks = {}
    currentMap = nil

    if type(data) == "table" and type(data.masks) == "table" then
        for mapKey, mask in pairs(data.masks) do
            masks[tostring(mapKey)] = math.floor(tonumber(mask) or 0)
        end
    end
end

local function onExit()
    collectgarbage()
end

print("Rando Treasure Bank: Applying patch.")

event.registerSaveHandler(HANDLER_ID, onSave)
event.registerLoadHandler(HANDLER_ID, onLoad)
event.registerEventSync("onMapJump", onMapJump)
event.registerEventSync("onSaveLoad", onSaveLoad)
event.registerEventSync("onFlip", onFlip)
event.registerEventAsync("exit", onExit)

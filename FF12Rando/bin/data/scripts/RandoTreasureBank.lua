-- Requires the FF12 lua loader 2.0.1 or newer.
if not (checkMinVersion and checkMinVersion(2, 0, 1)) then
    error("Rando Treasure Bank: requires the FF12 lua loader 2.0.1 or newer.")
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

-- Chest icons ----------------------------------------------------------------------------
--
-- The texture overrides in ps2data add navi icon 63 (a chest) and 62 (blank) to the minimap
-- and map screen icon sets. Two things draw chests with them:
--
-- Map screen: the randomizer writes treasureNavi.bin, a replacement navi icon set per map
-- that adds an icon per chest with a condition of 0x8000 | chest number, where the chest
-- number is its line in treasureIcons.txt. The map screen is handed those sets instead of
-- its own copies, and its condition checks are hooked so that range reads a bitset this
-- script keeps of which chests are opened, which is what lets maps other than the current
-- one hide their opened chests. The current map is drawn the same way as every other map.
--
-- Minimap: the chest actors themselves. Each frame every chest actor is drawn as a navi icon
-- instead of a dot, matched to its chest by position so it can switch to the blank icon once
-- opened. Those actor markers are kept off the map screen.
--
-- Urns get the same treatment with their own icon, 61. They are script actors rather than
-- treasure, found by their name (setnpcname 0x01D4) and position, and they record being
-- opened with settreasureflag(n), which sets bit 0x100 + n of the same bitfield. Those bits
-- are global, outside the per-map slice banked above.

local ICON_LIST_PATH = "../rando/treasureIcons.txt"
local NAVI_BLOBS_PATH = "../rando/treasureNavi.bin"

local ACTOR_LIST_COUNT = 0x022C8B00 -- u16: number of actors the minimap considers
local ACTOR_LIST = 0x022C8700       -- u64[]: actor pointers
local ACTOR_LIST_MAX = (ACTOR_LIST_COUNT - ACTOR_LIST) // 8 -- 128: the list ends at the count
local ACTOR_FLAGS = 0x0C            -- bit 0x80 is set by setuptreasure
local TREASURE_FLAG = 0x80
local ACTOR_NAVI_MODE = 0x0F        -- 0x40 | n draws the actor as navi icon n
local ACTOR_ICON_OFFSET_X = 0x21
local ACTOR_ICON_OFFSET_Z = 0x22
local ACTOR_POSITION = 0xB8         -- pointer to the actor's world x, y, z floats
local ACTOR_NAME = 0x102            -- u16 set by setnpcname
local URN_NAME = 0x01D4


local NAVI_ICON = 0x40
local CHEST_ICON = 63
local OPENED_ICON = 62
local URN_ICON = 61
local URN_BIT_BASE = 0x100          -- settreasureflag(n) sets bit 0x100 + n at BANK_OFFSET
local MATCH_DISTANCE_SQ = 1.5 * 1.5

local OPENED_BITS_SIZE = 0x800      -- one bit per possible chest number (0x3FFF)

-- The map screen checks each navi icon's condition with 0x32A8A0 twice: once when fetching
-- it (0x3DFB50) and again in its visibility filter (0x3FFF40, a tail jump). Both are
-- redirected to a check that answers conditions in the chest range from the opened bitset
-- and passes everything else through.
local COND_CALL_ADDRESS = 0x003DFB91
local COND_CALL_BYTES = { 0xE8, 0x0A, 0xAD, 0xF4, 0xFF } -- call 0x0032A8A0
local COND_JUMP_ADDRESS = 0x003FFF62
local COND_JUMP_BYTES = { 0xE9, 0x39, 0xA9, 0xF2, 0xFF } -- jmp 0x0032A8A0

local condCaveTemplate =
[[
rtb_navi_cond:
  mov eax,ecx
  and eax,0xC000
  cmp eax,0x8000
  je rtb_chest
  jmp 0x0032A8A0
rtb_chest:
  mov eax,ecx
  and eax,0x3FFF
  mov rdx,0x%X
  bt dword ptr [rdx],eax
  setnc al
  movzx eax,al
  ret
]]

-- The map screen gets each map's navi icons from 0x3BE2D0(map id), which returns that map's
-- copy inside the loaded map screen data (worldmap/menumap_subNN.bin). For maps listed in
-- treasureNavi.bin it returns the randomizer's copy instead, which adds the chests.
local NAVI_BLOB_ADDRESS = 0x003BE2D0
local NAVI_BLOB_RETURN = 0x003BE2D5
local NAVI_BLOB_BYTES = { 0x48, 0x89, 0x5C, 0x24, 0x08 } -- mov qword ptr [rsp+8],rbx
local NAVI_BLOB_MAX_MAP = 0x7FF

local blobCaveTemplate =
[[
rtb_navi_blob:
  mov r11d,ecx
  cmp r11d,0x%X
  ja rtb_navi_blob_vanilla
  mov rax,0x%X
  mov rax,qword ptr [rax+r11*8]
  test rax,rax
  je rtb_navi_blob_vanilla
  ret
rtb_navi_blob_vanilla:
  mov qword ptr [rsp+8],rbx
  jmp 0x%X
]]

local blobDetour =
[[
  jmp %rtb_navi_blob%
]]

-- The map screen only draws an icon over parts of the map the player has explored, except
-- for a few icon types it always shows (0x4049EE checks the sprite). Chests and urns are
-- added to that list so every unopened one shows.
local EXPLORE_ADDRESS = 0x004049EE
local EXPLORE_ALWAYS_SHOW = 0x00404A4F
local EXPLORE_CONTINUE = 0x004049F4
local EXPLORE_BYTES = { 0x0F, 0xBF, 0x4F, 0x08, 0x8B, 0xC1 } -- movsx ecx,word ptr [rdi+8]; mov eax,ecx

local exploreCaveTemplate =
[[
rtb_explore:
  movsx ecx,word ptr [rdi+8]
  cmp ecx,%d
  je 0x%X
  cmp ecx,%d
  je 0x%X
  mov eax,ecx
  jmp 0x%X
]]

local exploreDetour =
[[
  jmp %rtb_explore%
]]

-- The map screen also draws the current map's minimap markers (0x3C3340 marks each one
-- visible before drawing), so the chest and urn actors would show there next to the map
-- screen's own icons. When the marker list is drawn with a texture set (the map screen; the
-- minimap draws it with none), markers using the chest, opened or urn icons (61 to 63) are
-- marked hidden instead, so every map on the map screen, the current one included, shows the
-- same icons.
local MAPSCREEN_ACTOR_ADDRESS = 0x003C33BC
local MAPSCREEN_ACTOR_CONTINUE = 0x003C33C2
local MAPSCREEN_ACTOR_SKIP = 0x003C3449
local MAPSCREEN_ACTOR_BYTES = { 0x0F, 0xBA, 0xE9, 0x10, 0x89, 0x0B } -- bts ecx,0x10; mov [rbx],ecx
local MARKER_LIST_TEXTURE = 0x1418

local mapScreenActorCaveTemplate =
[[
rtb_mapscreen_actor:
  bts ecx,0x10
  mov dword ptr [rbx],ecx
  cmp qword ptr [rsi+0x%X],0
  je rtb_mapscreen_actor_keep
  cmp cl,5
  jne rtb_mapscreen_actor_keep
  movzx eax,byte ptr [rbx+1]
  cmp eax,%d
  jb rtb_mapscreen_actor_keep
  cmp eax,%d
  ja rtb_mapscreen_actor_keep
  and dword ptr [rbx],0xFFFEFFFF
  jmp 0x%X
rtb_mapscreen_actor_keep:
  jmp 0x%X
]]

local mapScreenActorDetour =
[[
  jmp %rtb_mapscreen_actor%
]]

-- The map screen's side list names every icon on the shown maps (0x402000 builds it from the
-- same navi sections). The chest, opened and urn icons (61 to 63) are left out of it.
local SIDE_LIST_ADDRESS = 0x00402109
local SIDE_LIST_CONTINUE = 0x0040210E
local SIDE_LIST_UNEXPLORED = 0x00402163
local SIDE_LIST_SKIP = 0x0040219D
local SIDE_LIST_BYTES = { 0x4D, 0x85, 0xED, 0x74, 0x55 } -- test r13,r13; je 0x402163

local sideListCaveTemplate =
[[
rtb_side_list:
  movzx eax,word ptr [rbx+8]
  cmp eax,%d
  jb rtb_side_list_keep
  cmp eax,%d
  jbe 0x%X
rtb_side_list_keep:
  test r13,r13
  je 0x%X
  jmp 0x%X
]]

local sideListDetour =
[[
  jmp %rtb_side_list%
]]

-- Urn markers appended to the minimap's marker array (see updateUrnMarkers).
local URN_MARKER_ADDRESS = 0x003C5379
local URN_MARKER_RETURN = 0x003C5380
local URN_MARKER_BYTES = { 0x48, 0x8D, 0x15, 0xF0, 0x06, 0x78, 0x02 } -- lea rdx,[0x2B45A70]
local MARKER_LIST = 0x02B45A70
local MARKER_ARRAY = 0x10
local MARKER_SIZE = 0x20
local MARKER_CAPACITY = 0x40
local MARKER_KIND_ICON = 5
local URN_MARKER_FLAGS = 0x50000     -- drawn (0x10000), navi icon with no actor behind it (0x40000)
local URN_MARKER_ID_BASE = 0x7F000000
local URN_MARKER_MAX = 16

local urnMarkerCaveTemplate =
[[
rtb_urn_markers:
  push rsi
  push rdi
  push rcx
  mov rsi,0x%X
  mov ecx,dword ptr [rsi]
  test ecx,ecx
  je rtb_urn_markers_done
  add rsi,0x%X
  mov rdx,0x%X
rtb_urn_markers_loop:
  mov eax,dword ptr [rdx]
  cmp eax,0x%X
  jae rtb_urn_markers_done
  shl eax,5
  lea rdi,[rdx+rax+0x%X]
  movups xmm0,[rsi]
  movups [rdi],xmm0
  movups xmm0,[rsi+0x10]
  movups [rdi+0x10],xmm0
  inc dword ptr [rdx]
  add rsi,0x20
  dec ecx
  jne rtb_urn_markers_loop
rtb_urn_markers_done:
  pop rcx
  pop rdi
  pop rsi
  mov rdx,0x%X
  jmp 0x%X
]]

local urnMarkerDetour =
[[
  jmp %rtb_urn_markers%
]]

local condCallDetour =
[[
  call %rtb_navi_cond%
]]

local condJumpDetour =
[[
  jmp %rtb_navi_cond%
]]

-- The actor marker loop copies each actor's facing into its marker and the renderer rotates
-- the icon by it. Navi icons from the map data are always drawn upright, so actors drawn as
-- navi icons (the chests) get the same treatment.
local UPRIGHT_ADDRESS = 0x003C5241
local UPRIGHT_RETURN = 0x003C5247
local UPRIGHT_BYTES = { 0x81, 0x0F, 0x00, 0x00, 0x04, 0x00 } -- or dword ptr [rdi],0x40000

local uprightCaveTemplate =
[[
rtb_upright:
  or dword ptr [rdi],0x40000
  mov dword ptr [rdi+0x10],0
  jmp 0x%X
]]

local uprightDetour =
[[
  jmp %rtb_upright%
]]

local caveSymbols = {
    "rtb_navi_cond",
    "rtb_navi_blob",
    "rtb_explore",
    "rtb_mapscreen_actor",
    "rtb_side_list",
    "rtb_urn_markers",
    "rtb_upright"
}

-- chests[number] = { map, respawn, x, z, urn } with number counted from 0. For an urn
-- (urn = true) respawn holds its treasure flag.
local chests = {}
local chestsByMap = {}
local chestCount = 0

-- Address of the opened bitset, once the patch is in.
local openedBits = nil

-- Address of the urn markers (u32 count, then markers from MARKER_ARRAY), once the patch is in.
local urnMarkers = nil
local patched = {}

-- actorChest[actor pointer] = chest number, for the current map.
local actorChest = {}
local actorChestMap = nil

-- Actor pointers are reused when a map is entered again or a save is loaded (opened chests
-- are not spawned, so the same pointer can land on a different chest or an urn), so the
-- cache must not outlive the actors it was built from.
local function resetActorChests()
    actorChest = {}
    actorChestMap = nil
end

local function loadChestList()
    local file = io.open(ICON_LIST_PATH, "r")
    if file == nil then
        return
    end

    for line in file:lines() do
        local map, respawn, x, z, kind = line:match("^(%x+),(%d+),(%-?%d+),(%-?%d+),?(%a*)")
        if map ~= nil then
            local entry = {
                map = tonumber(map, 16),
                respawn = tonumber(respawn),
                x = tonumber(x) / 10,
                z = tonumber(z) / 10,
                urn = kind == "u",
            }
            chests[chestCount] = entry
            chestsByMap[entry.map] = chestsByMap[entry.map] or {}
            table.insert(chestsByMap[entry.map], chestCount)
            chestCount = chestCount + 1
        end
    end
    io.close(file)
end

-- naviBlobs[map id] = NAVIICN2 bytes as a string.
local naviBlobs = {}

local function loadNaviBlobs()
    local file = io.open(NAVI_BLOBS_PATH, "rb")
    if file == nil then
        return
    end

    local data = file:read("a")
    io.close(file)

    local pos = 1
    while pos + 8 <= #data + 1 do
        local map, _, length, next = string.unpack("<I2I2I4", data, pos)
        naviBlobs[map] = data:sub(next, next + length - 1)
        pos = next + length + (4 - length % 4) % 4
    end
end

-- Copies the blobs into memory and returns a table, indexed by map id, of their addresses.
local function installNaviBlobs()
    local tableSize = (NAVI_BLOB_MAX_MAP + 1) * 8
    local lookup = memory.allocExe(tableSize)
    for i = 0, tableSize - 1 do
        memory.u8[lookup + i] = 0
    end

    for map, blob in pairs(naviBlobs) do
        if map <= NAVI_BLOB_MAX_MAP then
            local address = memory.allocExe(#blob)
            for i = 1, #blob do
                memory.u8[address + i - 1] = blob:byte(i)
            end
            memory.u32[lookup + map * 8] = address & 0xFFFFFFFF
            memory.u32[lookup + map * 8 + 4] = address >> 32
        end
    end

    return lookup
end

local function chestOpened(chest)
    if chest.urn then
        local bit = URN_BIT_BASE + chest.respawn
        return (memory.u8[SAVE_BASE + BANK_OFFSET + (bit >> 3)] >> (bit & 7)) & 1 ~= 0
    end

    local mask = masks[tostring(chest.map)] or 0
    return (mask >> chest.respawn) & 1 ~= 0
end

-- A set bit hides the chest's map screen icon.
local function publishChest(number)
    local address = openedBits + (number >> 3)
    local bit = 1 << (number & 7)
    local value = memory.u8[address]
    if chestOpened(chests[number]) then
        value = value | bit
    else
        value = value & ~bit
    end
    memory.u8[address] = value
end

local function refreshAllChests()
    if openedBits == nil then
        return
    end

    for number = 0, chestCount - 1 do
        publishChest(number)
    end
end

local function refreshMapChests(mapID)
    if openedBits == nil or chestsByMap[mapID] == nil then
        return
    end

    for _, number in ipairs(chestsByMap[mapID]) do
        publishChest(number)
    end
end

local function bytesMatch(address, expected)
    return table.concat(memory.readArray(address, #expected), ",") == table.concat(expected, ",")
end

local function applyIconPatches()
    if bytesMatch(UPRIGHT_ADDRESS, UPRIGHT_BYTES) then
        memory.assemble(string.format(uprightCaveTemplate, UPRIGHT_RETURN), { "rtb_upright" })
        memory.assemble(uprightDetour, UPRIGHT_ADDRESS)
        patched.rtb_upright = true
    else
        print("Rando Treasure Bank: Couldn't apply the upright icon patch, executable is unexpectedly modified.")
    end

    if chestCount == 0 then
        -- An older seed without treasureIcons.txt has no map screen chest icons to manage.
        return
    end

    if not (bytesMatch(COND_CALL_ADDRESS, COND_CALL_BYTES) and bytesMatch(COND_JUMP_ADDRESS, COND_JUMP_BYTES)
            and bytesMatch(NAVI_BLOB_ADDRESS, NAVI_BLOB_BYTES)) then
        print("Rando Treasure Bank: Couldn't apply the map screen chest patch, executable is unexpectedly modified.")
        return
    end

    local bits = memory.allocExe(OPENED_BITS_SIZE)
    for i = 0, OPENED_BITS_SIZE - 1 do
        memory.u8[bits + i] = 0
    end
    openedBits = bits
    refreshAllChests()

    memory.assemble(string.format(condCaveTemplate, bits), { "rtb_navi_cond" })
    memory.assemble(condCallDetour, COND_CALL_ADDRESS)
    memory.assemble(condJumpDetour, COND_JUMP_ADDRESS)
    patched.rtb_navi_cond = true

    if next(naviBlobs) ~= nil then
        local lookup = installNaviBlobs()
        memory.assemble(string.format(blobCaveTemplate, NAVI_BLOB_MAX_MAP, lookup, NAVI_BLOB_RETURN), { "rtb_navi_blob" })
        memory.assemble(blobDetour, NAVI_BLOB_ADDRESS)
        patched.rtb_navi_blob = true
    end

    if bytesMatch(MAPSCREEN_ACTOR_ADDRESS, MAPSCREEN_ACTOR_BYTES) then
        memory.assemble(string.format(mapScreenActorCaveTemplate, MARKER_LIST_TEXTURE, URN_ICON, CHEST_ICON,
            MAPSCREEN_ACTOR_SKIP, MAPSCREEN_ACTOR_CONTINUE), { "rtb_mapscreen_actor" })
        memory.assemble(mapScreenActorDetour, MAPSCREEN_ACTOR_ADDRESS)
        patched.rtb_mapscreen_actor = true
    else
        print("Rando Treasure Bank: Couldn't apply the map screen chest actor patch, executable is unexpectedly modified.")
    end

    if bytesMatch(URN_MARKER_ADDRESS, URN_MARKER_BYTES) then
        local markers = memory.allocExe(MARKER_ARRAY + MARKER_SIZE * URN_MARKER_MAX)
        memory.u32[markers] = 0
        memory.assemble(string.format(urnMarkerCaveTemplate, markers, MARKER_ARRAY, MARKER_LIST, MARKER_CAPACITY,
            MARKER_ARRAY, MARKER_LIST, URN_MARKER_RETURN), { "rtb_urn_markers" })
        memory.assemble(urnMarkerDetour, URN_MARKER_ADDRESS)
        urnMarkers = markers
        patched.rtb_urn_markers = true
    else
        print("Rando Treasure Bank: Couldn't apply the minimap urn patch, executable is unexpectedly modified.")
    end

    if bytesMatch(SIDE_LIST_ADDRESS, SIDE_LIST_BYTES) then
        memory.assemble(string.format(sideListCaveTemplate, URN_ICON, CHEST_ICON, SIDE_LIST_SKIP,
            SIDE_LIST_UNEXPLORED, SIDE_LIST_CONTINUE), { "rtb_side_list" })
        memory.assemble(sideListDetour, SIDE_LIST_ADDRESS)
        patched.rtb_side_list = true
    else
        print("Rando Treasure Bank: Couldn't apply the map screen side list patch, executable is unexpectedly modified.")
    end

    if bytesMatch(EXPLORE_ADDRESS, EXPLORE_BYTES) then
        memory.assemble(string.format(exploreCaveTemplate, CHEST_ICON, EXPLORE_ALWAYS_SHOW, URN_ICON, EXPLORE_ALWAYS_SHOW, EXPLORE_CONTINUE), { "rtb_explore" })
        memory.assemble(exploreDetour, EXPLORE_ADDRESS)
        patched.rtb_explore = true
    else
        print("Rando Treasure Bank: Couldn't apply the unexplored chest icon patch, executable is unexpectedly modified.")
    end
    print("Rando Treasure Bank: Chest icon patches applied.")
end

local function ptr64(address)
    return memory.u32[address] | (memory.u32[address + 4] << 32)
end

local function findChestForActor(actor, urn)
    local list = chestsByMap[currentMap]
    if list == nil then
        return nil
    end

    local position = ptr64(actor + ACTOR_POSITION)
    if position == 0 then
        return nil
    end

    local x = memory.readFloat(position)
    local z = memory.readFloat(position + 8)
    for _, number in ipairs(list) do
        local chest = chests[number]
        local dx, dz = chest.x - x, chest.z - z
        if chest.urn == urn and dx * dx + dz * dz <= MATCH_DISTANCE_SQ then
            return number
        end
    end

    return nil
end

local function drawAsIcon(actor, icon)
    memory.u8[actor + ACTOR_NAVI_MODE] = NAVI_ICON | icon
    memory.u8[actor + ACTOR_ICON_OFFSET_X] = 0
    memory.u8[actor + ACTOR_ICON_OFFSET_Z] = 0
end

local function updateChestActors()
    if actorChestMap ~= currentMap then
        actorChest = {}
        actorChestMap = currentMap
    end

    local count = memory.u16[ACTOR_LIST_COUNT]
    if count == 0 or count > ACTOR_LIST_MAX then
        return
    end

    for i = 0, count - 1 do
        local actor = ptr64(ACTOR_LIST + 8 * i)
        if actor ~= 0 and (memory.u8[actor + ACTOR_FLAGS] & TREASURE_FLAG) ~= 0 then
            local number = actorChest[actor]
            if number == nil then
                number = findChestForActor(actor, false) or false
                actorChest[actor] = number
            end

            local icon = CHEST_ICON
            if number and chestOpened(chests[number]) then
                icon = OPENED_ICON
            end

            drawAsIcon(actor, icon)
        end
    end
end

-- Urns never reach the minimap's actor list: its filters reject their actors. So instead of
-- drawing the actors, the minimap's marker array gets a ready-made icon marker per unopened urn
-- on the current map. The array (count at 0x2B45A70, 32-byte markers from +0x10, room for 64)
-- is rebuilt every frame from the actors and the map's static icons, then synced for drawing;
-- a hook between the two (0x3C5379) appends the markers this script keeps in urnMarkers.
local function updateUrnMarkers()
    if urnMarkers == nil then
        return
    end

    local count = 0
    for _, number in ipairs(chestsByMap[currentMap] or {}) do
        local chest = chests[number]
        if chest.urn and not chestOpened(chest) and count < URN_MARKER_MAX then
            local marker = urnMarkers + MARKER_ARRAY + MARKER_SIZE * count
            memory.u32[marker] = URN_MARKER_FLAGS | (URN_ICON << 8) | MARKER_KIND_ICON
            memory.u32[marker + 4] = URN_MARKER_ID_BASE + number
            memory.writeFloat(marker + 8, chest.x)
            memory.writeFloat(marker + 12, chest.z)
            for offset = 16, MARKER_SIZE - 4, 4 do
                memory.u32[marker + offset] = 0
            end
            count = count + 1
        end
    end
    memory.u32[urnMarkers] = count
end

local function onFlip()
    local mapID = readMapID()

    if mapID ~= currentMap then
        if urnMarkers ~= nil then
            -- The outgoing map's urns must not be drawn on the incoming one.
            memory.u32[urnMarkers] = 0
        end
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
    refreshMapChests(currentMap)
    updateChestActors()
    updateUrnMarkers()
end

local function onMapJump()
    resetActorChests()
    switchTo(readMapID())
end

-- A save that is loaded carries whichever map's bits happened to be live when it was
-- written. Drop that and re-derive from the sidecar so a stale slice cannot leak into a
-- different map.
local function onSaveLoad()
    resetActorChests()
    currentMap = nil
    local mapID = readMapID()
    if isRealMap(mapID) then
        applyMap(mapID)
        reapplyFrames = 6
    else
        writeBank(0)
    end
    refreshAllChests()
end

-- New Game never calls the load handler, so without this the masks of whatever save was
-- loaded earlier in the session carry into the new game and get written into its saves.
local function onNewGameStart()
    resetActorChests()
    masks = {}
    currentMap = nil
    reapplyFrames = 0
    writeBank(0)
    refreshAllChests()
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
    resetActorChests()
    masks = {}
    currentMap = nil

    if type(data) == "table" and type(data.masks) == "table" then
        for mapKey, mask in pairs(data.masks) do
            masks[tostring(mapKey)] = math.floor(tonumber(mask) or 0)
        end
    end
    refreshAllChests()
end

local function onExit()
    for _, symbol in ipairs(caveSymbols) do
        if patched[symbol] then
            memory.unregisterSymbol(symbol)
        end
    end
    collectgarbage()
end

print("Rando Treasure Bank: Applying patch.")

loadChestList()
loadNaviBlobs()
event.registerEventAsync("onInitDone", applyIconPatches)

event.registerSaveHandler(HANDLER_ID, onSave)
event.registerLoadHandler(HANDLER_ID, onLoad)
event.registerEventSync("onMapJump", onMapJump)
event.registerEventSync("onSaveLoad", onSaveLoad)
event.registerEventSync("onNewGameStart", onNewGameStart)
event.registerEventSync("onFlip", onFlip)
event.registerEventAsync("exit", onExit)

-- Requires the FF12 lua loader 2.0.1 or newer.
if not (checkMinVersion and checkMinVersion(2, 0, 1)) then
    error("Rando Text: requires the FF12 lua loader 2.0.1 or newer.")
end

-- Runtime text overrides. The randomizer writes ../rando/textOverrides.txt (TextRando) and
-- this script swaps the game's text for the listed messages as they are shown. The file is
-- watched, so anything that rewrites it mid-game (another script, a tracker) takes effect on
-- the next message.
--
-- One entry per line, '#' starts a comment, "\n" in the text is a line break, "\\" is a
-- backslash and the text uses the same {tags} as message.convert:
--
--   dialog|<map id hex>|<message index>|<text>
--       A text box opened by a map's script (ames, amese, aask, ...): the message index is
--       the entry in that map's dialog text (the low 24 bits of the message id). Keep any
--       choice markup the original text had, the script still expects it.
--   itemname|<item id hex>|<name>
--       The item's name inside item get messages: the battle log, chest windows, the key
--       item banner ("You obtain <name>!"), steals and loot. Menus and shops keep the real
--       name. The game's own wording and colours stay.
--   itemmsg|<item id hex>|<text>
--       The whole "Obtained ..." line for that item, from scripts, rewards or chests,
--       including key items. The count is not added, so include it in the text if needed.
--   rewardname|<reward id hex>|<item id hex>|<name>
--   chestname|<map id hex>|<respawn id>|<item id hex>|<name>
--       Like itemname, but only when the item comes from that reward (0x9xxx, from a script
--       or a hunt) or that chest.
--   rewardmsg|<reward id hex>|<item id hex or "gil">|<text>
--   chestmsg|<map id hex>|<respawn id>|<item id hex>|<text>
--       Like itemmsg (or the gil line), but only when it comes from that reward or chest.
--       Only the battle log and chest windows: the key item banner keeps its own line.
--   A reward or chest entry wins over the plain item entry of the same kind.
--   debug
--       Logs "dialog|<map>|<index>" to hook.log for each script text box, and where item
--       loot comes from (reward or chest), to find the key for a message.

local OVERRIDES_PATH = "../rando/textOverrides.txt"
local POLL_MS = 1000

local MAP_ID_ADDRESS = 0x02164480 + 0x1044

-- 0x264D60(script, message id) returns a script's dialog text. Every script text box
-- (ames, amese, aask, ...) gets its text from it.
local DIALOG_ADDRESS = 0x00264D60
local DIALOG_RETURN = 0x00264D67
local DIALOG_BYTES = { 0x4C, 0x8B, 0x81, 0xA0, 0x00, 0x00, 0x00 } -- mov r8,qword ptr [rcx+0xA0]

local dialogCaveTemplate =
[[
rtx_dialog:
  push r9
  push r10
  mov r9,0x%X
  mov r9d,dword ptr [r9]
  mov r10d,edx
  and r10d,0xFFFFFF
  mov rax,0x%X
  mov dword ptr [rax],r9d
  mov dword ptr [rax+4],r10d
  mov rax,0x%X
  mov rax,qword ptr [rax]
  mov r8d,dword ptr [rax]
  test r8d,r8d
  je rtx_dialog_vanilla
  add rax,8
rtx_dialog_loop:
  cmp dword ptr [rax],r9d
  jne rtx_dialog_next
  cmp dword ptr [rax+4],r10d
  jne rtx_dialog_next
  mov rax,qword ptr [rax+8]
  pop r10
  pop r9
  ret
rtx_dialog_next:
  add rax,0x10
  dec r8d
  jne rtx_dialog_loop
rtx_dialog_vanilla:
  pop r10
  pop r9
  mov r8,qword ptr [rcx+0xA0]
  jmp 0x%X
]]

local dialogDetour =
[[
  jmp %rtx_dialog%
]]

-- Where the loot being announced came from, so the same item can read differently per reward
-- or chest. Two u32s: the source key (0x80000000 | reward id, or the map id for a chest;
-- 0xFFFFFFFF for none) and a value or'd with the item id to make the second key (the chest's
-- respawn id << 16, or 0 for a reward).
--
-- Rewards (0x9xxx) are expanded into their gil and items by 0x30AEA0(reward id, ...), for
-- scripts (via 0x2A59C0) and for hunts (0x3F4C30) alike. Chests decide their contents in
-- 0x2FAFD0(treasure record), whose respawn id is at +9. Script loot that is not a reward
-- (0x2A59C0 called from 0x2A3060, 6th argument 0) clears the source, in log and window mode
-- alike, so a later plain give can't pick up an earlier reward's or chest's text. Hunts hand
-- their reward's key items to 0x2A59C0 afterwards, one at a time, through 0x2A5B90 (6th
-- argument 1); those keep the hunt's reward source.
--
-- The source is otherwise left set after the loot is shown. That is harmless: the item get
-- messages (0x24-0x26) only come from loot (0x35DF40 and its window task), which always
-- starts with one of the hooks above, and the only name overrides are on the AP placeholder.
local NO_SOURCE = 0xFFFFFFFF
local REWARD_SOURCE = 0x80000000
local GIL_ITEM = 0xFFFF

local REWARD_SRC_ADDRESS = 0x0030AEA0
local REWARD_SRC_RETURN = 0x0030AEA5
local REWARD_SRC_BYTES = { 0x48, 0x89, 0x5C, 0x24, 0x08 } -- mov qword ptr [rsp+8],rbx

local rewardSrcCaveTemplate =
[[
rtx_reward_src:
  mov r10,0x%X
  movzx eax,cx
  or eax,0x80000000
  mov dword ptr [r10],eax
  mov dword ptr [r10+4],0
  mov qword ptr [rsp+8],rbx
  jmp 0x%X
]]

local rewardSrcDetour =
[[
  jmp %rtx_reward_src%
]]

local LOOT_SRC_ADDRESS = 0x002A59C0
local LOOT_SRC_RETURN = 0x002A59C5
local LOOT_SRC_BYTES = { 0x48, 0x89, 0x5C, 0x24, 0x08 } -- mov qword ptr [rsp+8],rbx

local lootSrcCaveTemplate =
[[
rtx_loot_src:
  movzx eax,word ptr [rcx]
  and eax,0xF000
  cmp eax,0x9000
  je rtx_loot_src_done
  cmp dword ptr [rsp+0x30],0
  jne rtx_loot_src_done
  mov r10,0x%X
  mov dword ptr [r10],0xFFFFFFFF
  mov dword ptr [r10+4],0
rtx_loot_src_done:
  mov qword ptr [rsp+8],rbx
  jmp 0x%X
]]

local lootSrcDetour =
[[
  jmp %rtx_loot_src%
]]

local CHEST_SRC_ADDRESS = 0x002FAFD0
local CHEST_SRC_RETURN = 0x002FAFD5
local CHEST_SRC_BYTES = { 0x48, 0x89, 0x5C, 0x24, 0x08 } -- mov qword ptr [rsp+8],rbx

local chestSrcCaveTemplate =
[[
rtx_chest_src:
  mov r10,0x%X
  mov r11,0x%X
  mov r11d,dword ptr [r11]
  mov dword ptr [r10],r11d
  movzx r11d,byte ptr [rcx+9]
  shl r11d,16
  mov dword ptr [r10+4],r11d
  mov qword ptr [rsp+8],rbx
  jmp 0x%X
]]

local chestSrcDetour =
[[
  jmp %rtx_chest_src%
]]

-- Shared name lookup: eax = item id in, rax = replacement name (0 for none) out. Tries the
-- source's entry (rewardname, chestname) first, then the item's (itemname). Changes only rax,
-- r10, r11 and the flags.
local findNameCaveTemplate =
[[
rtx_find_name:
  push rbx
  push rcx
  mov ecx,eax
  mov r11,0x%X
  mov ebx,dword ptr [r11]
  cmp ebx,0xFFFFFFFF
  je rtx_find_name_item
  mov r11d,dword ptr [r11+4]
  or r11d,ecx
  mov r10,0x%X
  mov r10,qword ptr [r10]
  mov eax,dword ptr [r10]
  test eax,eax
  je rtx_find_name_item
  add r10,8
rtx_find_name_source_loop:
  cmp dword ptr [r10],ebx
  jne rtx_find_name_source_next
  cmp dword ptr [r10+4],r11d
  je rtx_find_name_hit
rtx_find_name_source_next:
  add r10,0x10
  dec eax
  jne rtx_find_name_source_loop
rtx_find_name_item:
  mov r10,0x%X
  mov r10,qword ptr [r10]
  mov eax,dword ptr [r10]
  test eax,eax
  je rtx_find_name_miss
  add r10,8
rtx_find_name_item_loop:
  cmp dword ptr [r10],ecx
  je rtx_find_name_hit
  add r10,0x10
  dec eax
  jne rtx_find_name_item_loop
rtx_find_name_miss:
  xor eax,eax
  pop rcx
  pop rbx
  ret
rtx_find_name_hit:
  mov rax,qword ptr [r10+8]
  pop rcx
  pop rbx
  ret
]]

-- The battle log formatter (0x536410) fills in each message parameter by type. Item
-- parameters (case at 0x536742) look the name up from the item id in [r15] and store the
-- string pointer in [rbp+r14*8]; listed items get their override instead.
local ITEM_NAME_ADDRESS = 0x00536742
local ITEM_NAME_CONTINUE = 0x00536747
local ITEM_NAME_STORED = 0x0053685E
local ITEM_NAME_BYTES = { 0x41, 0x8B, 0x0F, 0x8B, 0xC1 } -- mov ecx,dword ptr [r15]; mov eax,ecx

local itemNameCaveTemplate =
[[
rtx_item_name:
  mov ecx,dword ptr [r15]
  movzx eax,cx
  call %%rtx_find_name%%
  test rax,rax
  je rtx_item_name_vanilla
  mov qword ptr [rbp+r14*8],rax
  jmp 0x%X
rtx_item_name_vanilla:
  mov eax,ecx
  jmp 0x%X
]]

local itemNameDetour =
[[
  jmp %rtx_item_name%
]]

-- Key items in the item get window (the "You obtain <name>!" banner, e.g. hunt rewards) skip
-- the log formatter: 0x35E070 prints the name into menu message 0x833 itself, with
-- 0x2B4090 (Japanese and Chinese) or 0x2B40C0 (the rest), the name in r9 and rdi on the
-- loot entry (u16 item id at +4). The name is swapped just before either call. The Japanese
-- and Chinese branch has already taken the plain name out of the name text, so it gets the
-- override's name past its two byte header.
local KEY_NAME_CJK_ADDRESS = 0x0035E31F
local KEY_NAME_CJK_RETURN = 0x0035E324
local KEY_NAME_CJK_BYTES = { 0xE8, 0x6C, 0x5D, 0xF5, 0xFF } -- call 0x002B4090
local KEY_NAME_ADDRESS = 0x0035E342
local KEY_NAME_RETURN = 0x0035E347
local KEY_NAME_BYTES = { 0xE8, 0x79, 0x5D, 0xF5, 0xFF }     -- call 0x002B40C0

local keyNameCaveTemplate =
[[
%s:
  movzx eax,word ptr [rdi+4]
  call %%rtx_find_name%%
  test rax,rax
  je %s_call
%s
  mov r9,rax
%s_call:
  call 0x%X
  jmp 0x%X
]]

local keyNameCjkDetour =
[[
  jmp %rtx_key_name_cjk%
]]

local keyNameDetour =
[[
  jmp %rtx_key_name%
]]

-- Item get messages (log types 0x25 "Obtained X", 0x24 "Obtained X xN" and 0x26 gil, raised
-- by 0x46AA70 with the item id at +0x20 of the parameters in rsi) are finished by 0x5369C0,
-- which hands the entry at rsp+0x28 to 0x35B990 for the log or a chest window. The entry's
-- text pointer (+0x28) is swapped just before that call: first by source (rewardmsg,
-- chestmsg), then by item (itemmsg).
local ITEM_LOG_ADDRESS = 0x00536D74
local ITEM_LOG_CALL = 0x00536D79
local ITEM_LOG_BYTES = { 0x48, 0x8D, 0x4C, 0x24, 0x28 } -- lea rcx,[rsp+0x28]

local itemLogCaveTemplate =
[[
rtx_item_log:
  lea rcx,[rsp+0x28]
  mov eax,dword ptr [rcx+0x3C]
  and eax,0x7FFF
  cmp eax,0x26
  je rtx_item_log_gil
  cmp eax,0x24
  je rtx_item_log_item
  cmp eax,0x25
  jne rtx_item_log_done
rtx_item_log_item:
  movzx eax,word ptr [rsi+0x20]
  jmp rtx_item_log_source
rtx_item_log_gil:
  mov eax,0xFFFF
rtx_item_log_source:
  mov r11,0x%X
  mov r8d,dword ptr [r11]
  cmp r8d,0xFFFFFFFF
  je rtx_item_log_plain
  mov r11d,dword ptr [r11+4]
  or r11d,eax
  mov r9,0x%X
  mov r9,qword ptr [r9]
  mov r10d,dword ptr [r9]
  test r10d,r10d
  je rtx_item_log_plain
  add r9,8
rtx_item_log_source_loop:
  cmp dword ptr [r9],r8d
  jne rtx_item_log_source_next
  cmp dword ptr [r9+4],r11d
  je rtx_item_log_hit
rtx_item_log_source_next:
  add r9,0x10
  dec r10d
  jne rtx_item_log_source_loop
rtx_item_log_plain:
  cmp eax,0xFFFF
  je rtx_item_log_done
  mov r9,0x%X
  mov r9,qword ptr [r9]
  mov r10d,dword ptr [r9]
  test r10d,r10d
  je rtx_item_log_done
  add r9,8
rtx_item_log_loop:
  cmp dword ptr [r9],eax
  je rtx_item_log_hit
  add r9,0x10
  dec r10d
  jne rtx_item_log_loop
  jmp rtx_item_log_done
rtx_item_log_hit:
  mov rax,qword ptr [r9+8]
  mov qword ptr [rcx+0x28],rax
rtx_item_log_done:
  jmp 0x%X
]]

local itemLogDetour =
[[
  jmp %rtx_item_log%
]]

-- The item get window (0x35E070) builds one line per loot entry into the buffer in rsi.
-- Key items take their own branch there instead of the log formatter; both branches meet at
-- 0x35E386 with rdi on the entry (u32 kind, 0 = item; u16 item id at +4), where listed
-- items have the line replaced.
local ITEM_LINE_ADDRESS = 0x0035E386
local ITEM_LINE_RETURN = 0x0035E38B
local ITEM_LINE_BYTES = { 0x48, 0x8B, 0x4C, 0x24, 0x38 } -- mov rcx,qword ptr [rsp+0x38]
local ITEM_LINE_MAX = 0x17F                                -- the line buffer is 0x180 bytes

local itemLineCaveTemplate =
[[
rtx_item_line:
  cmp dword ptr [rdi],0
  jne rtx_item_line_done
  movzx eax,word ptr [rdi+4]
  mov r9,0x%X
  mov r9,qword ptr [r9]
  mov r10d,dword ptr [r9]
  test r10d,r10d
  je rtx_item_line_done
  add r9,8
rtx_item_line_loop:
  cmp dword ptr [r9],eax
  je rtx_item_line_hit
  add r9,0x10
  dec r10d
  jne rtx_item_line_loop
  jmp rtx_item_line_done
rtx_item_line_hit:
  mov r8,qword ptr [r9+8]
  xor ecx,ecx
rtx_item_line_copy:
  mov dl,byte ptr [r8+rcx]
  mov byte ptr [rsi+rcx],dl
  test dl,dl
  je rtx_item_line_done
  inc ecx
  cmp ecx,0x%X
  jb rtx_item_line_copy
  mov byte ptr [rsi+rcx],0
rtx_item_line_done:
  mov rcx,qword ptr [rsp+0x38]
  jmp 0x%X
]]

local itemLineDetour =
[[
  jmp %rtx_item_line%
]]

local caveSymbols = {
    "rtx_find_name",
    "rtx_dialog",
    "rtx_item_name",
    "rtx_key_name_cjk",
    "rtx_key_name",
    "rtx_reward_src",
    "rtx_loot_src",
    "rtx_chest_src",
    "rtx_item_log",
    "rtx_item_line"
}

-- Table pointers the caves read, one u64 each: dialog, item names, item messages, source
-- messages, source names. A table is a u32 count (padded to 8) followed by 16 byte entries
-- { u32 key1, u32 key2, u64 text }. A reload builds new tables and repoints these, so the
-- game never sees a half built table. After them: the last map id and message index the
-- dialog cave was asked for (u32 each, logged by debug mode) and the loot source (see above).
local SLOT_DIALOG = 0
local SLOT_ITEM_NAME = 8
local SLOT_ITEM_MSG = 16
local SLOT_SOURCE_MSG = 24
local SLOT_SOURCE_NAME = 32
local SLOT_LAST_DIALOG = 40
local SLOT_LOOT_SOURCE = 48
local SLOTS_SIZE = 56
local slots = nil

-- A line reading just "debug" in the file logs every script text box shown.
local DEBUG_POLL_MS = 100
local debugEnabled = false
local lastLogged = nil
local lastLoggedSource = nil

local patched = {}
local lastModified = nil

-- Game encoded text by source text and language. Text memory is never freed: a text box can
-- still be showing an old pointer when the file changes, and the set of texts is small.
local encodedText = {}

-- Game text encoding (credit to TheInsurgentsDescriptiveInventory).
local textLangMap = {[0] = 1, 0, 0, 0, 0, 0, 2, 4, 3}

local function readTextLanguage()
    local gameSettingsBase = memory.u64[0x01F82D20]
    return textLangMap[memory.u32[gameSettingsBase]] or 0
end

-- Item names are not plain strings: the game's name texts start with two zero bytes, then the
-- name and its other forms (article, plural, ...) as separate NUL terminated strings, and the
-- text formatter fetches a form with 0x2B58B0(text, n). A plain string is returned whole for
-- every form, so "{article:0}{arg:0}" would print it twice. Names get that layout with every
-- other form empty.
local NAME_EXTRA_FORMS = 8

-- The key item banner sprintf's a name into the 0x180 byte line buffer with its template
-- ("You obtain ...!" and colour codes) around it, unchecked, so names stay well under it.
local NAME_MAX = 0x140

local function encode(text, language, asName)
    local key = (asName and "n:" or "t:") .. language .. ":" .. text
    local address = encodedText[key]
    if address ~= nil then
        return address
    end

    local status, converted = pcall(message.convert, text, language)
    if not status then
        print("Rando Text: Couldn't convert \"" .. text .. "\": " .. tostring(converted))
        return nil
    end

    if asName and #converted > NAME_MAX then
        print("Rando Text: Skipping a name too long for the item window (" .. #converted .. " bytes): " .. text)
        return nil
    end

    local bytes = { string.byte(converted, 1, -1) }
    local prefix = asName and 2 or 0
    local suffix = asName and NAME_EXTRA_FORMS or 0
    address = memory.allocExe(prefix + #bytes + 1 + suffix)
    for i = 0, prefix - 1 do
        memory.u8[address + i] = 0
    end
    if #bytes > 0 then
        memory.writeArray(address + prefix, bytes)
    end
    for i = 0, suffix do
        memory.u8[address + prefix + #bytes + i] = 0
    end

    encodedText[key] = address
    return address
end

local function buildTable(entries)
    local address = memory.allocExe(8 + 16 * #entries)
    memory.u32[address] = #entries
    memory.u32[address + 4] = 0
    for i, entry in ipairs(entries) do
        local base = address + 8 + 16 * (i - 1)
        memory.u32[base] = entry[1]
        memory.u32[base + 4] = entry[2]
        memory.u64[base + 8] = entry[3]
    end
    return address
end

local function unescape(text)
    -- "\n" is a line break and "\\" a backslash; any other escaped character is kept as is.
    return (text:gsub("\\(.)", function(c)
        return c == "n" and "\n" or c
    end))
end

-- Returns the entries listed in the file, by table (empty when the file is missing). A later
-- line for the same key replaces an earlier one.
local function readOverrides()
    local lists = { dialog = {}, itemname = {}, itemmsg = {}, source = {}, sourcename = {}, debug = false }
    local file = io.open(OVERRIDES_PATH, "r")
    if file == nil then
        return lists
    end

    local language = readTextLanguage()
    local seen = { dialog = {}, itemname = {}, itemmsg = {}, source = {}, sourcename = {} }
    local lineNumber = 0

    local function add(kind, key1, key2, text)
        local address = encode(unescape(text), language, kind == "itemname" or kind == "sourcename")
        if address == nil then
            return
        end

        local key = key1 .. ":" .. key2
        local existing = seen[kind][key]
        if existing ~= nil then
            existing[3] = address
        else
            local entry = { key1, key2, address }
            seen[kind][key] = entry
            table.insert(lists[kind], entry)
        end
    end

    for rawLine in file:lines() do
        lineNumber = lineNumber + 1
        local line = rawLine:gsub("\r$", "")
        if line == "debug" then
            lists.debug = true
        elseif line ~= "" and line:sub(1, 1) ~= "#" then
            local kind, rest = line:match("^(%a+)|(.*)$")
            if kind == "dialog" then
                local map, index, text = rest:match("^(%x+)|(%d+)|(.*)$")
                if map ~= nil then
                    add(kind, tonumber(map, 16), tonumber(index), text)
                else
                    kind = nil
                end
            elseif kind == "itemname" or kind == "itemmsg" then
                local id, text = rest:match("^(%x+)|(.*)$")
                if id ~= nil then
                    add(kind, tonumber(id, 16) & 0xFFFF, 0, text)
                else
                    kind = nil
                end
            elseif kind == "rewardmsg" or kind == "rewardname" then
                local reward, item, text = rest:match("^(%x+)|(%w+)|(.*)$")
                local itemID = item and (item == "gil" and kind == "rewardmsg" and GIL_ITEM or tonumber(item, 16))
                if reward ~= nil and itemID ~= nil then
                    add(kind == "rewardmsg" and "source" or "sourcename",
                        REWARD_SOURCE | (tonumber(reward, 16) & 0xFFFF), itemID & 0xFFFF, text)
                else
                    kind = nil
                end
            elseif kind == "chestmsg" or kind == "chestname" then
                local map, respawn, item, text = rest:match("^(%x+)|(%d+)|(%x+)|(.*)$")
                if map ~= nil then
                    add(kind == "chestmsg" and "source" or "sourcename",
                        tonumber(map, 16), ((tonumber(respawn) & 0xFF) << 16) | (tonumber(item, 16) & 0xFFFF), text)
                else
                    kind = nil
                end
            else
                kind = nil
            end

            if kind == nil then
                print("Rando Text: Ignoring line " .. lineNumber .. " of textOverrides.txt: " .. line)
            end
        end
    end
    io.close(file)

    return lists
end

local function publish(lists)
    memory.u64[slots + SLOT_DIALOG] = buildTable(lists.dialog)
    memory.u64[slots + SLOT_ITEM_NAME] = buildTable(lists.itemname)
    memory.u64[slots + SLOT_ITEM_MSG] = buildTable(lists.itemmsg)
    memory.u64[slots + SLOT_SOURCE_MSG] = buildTable(lists.source)
    memory.u64[slots + SLOT_SOURCE_NAME] = buildTable(lists.sourcename)
end

local debugLoopRunning = false

local function logDialog()
    if not debugEnabled then
        debugLoopRunning = false
        return
    end

    local map = memory.u32[slots + SLOT_LAST_DIALOG]
    local index = memory.u32[slots + SLOT_LAST_DIALOG + 4]
    local key = map .. ":" .. index
    if map ~= 0xFFFFFFFF and key ~= lastLogged then
        lastLogged = key
        print(string.format("Rando Text: dialog|%X|%d", map, index))
    end

    local source = memory.u32[slots + SLOT_LOOT_SOURCE]
    local sourceValue = memory.u32[slots + SLOT_LOOT_SOURCE + 4]
    local sourceKey = source .. ":" .. sourceValue
    if source ~= NO_SOURCE and sourceKey ~= lastLoggedSource then
        lastLoggedSource = sourceKey
        if source & REWARD_SOURCE ~= 0 then
            print(string.format("Rando Text: loot from rewardmsg|%04X|<item>", source & 0xFFFF))
        else
            print(string.format("Rando Text: loot from chestmsg|%X|%d|<item>", source, sourceValue >> 16))
        end
    end

    event.executeAfterMs(DEBUG_POLL_MS, logDialog)
end

local function reload()
    local lists = readOverrides()
    publish(lists)
    print(string.format("Rando Text: Loaded %d dialog, %d item name, %d item message, %d reward/chest name and %d reward/chest message overrides.",
        #lists.dialog, #lists.itemname, #lists.itemmsg, #lists.sourcename, #lists.source))

    debugEnabled = lists.debug
    if debugEnabled and not debugLoopRunning then
        debugLoopRunning = true
        print("Rando Text: Debug logging of script text boxes is on.")
        event.executeAfterMs(DEBUG_POLL_MS, logDialog)
    end
end

local function fileModified()
    if lfs == nil then
        return nil
    end
    return lfs.attributes(OVERRIDES_PATH, "modification") or false
end

local function bytesMatch(address, expected)
    return table.concat(memory.readArray(address, #expected), ",") == table.concat(expected, ",")
end

local function applyPatch(symbol, address, expected, cave, detour)
    if not bytesMatch(address, expected) then
        print("Rando Text: Couldn't apply " .. symbol .. ", executable is unexpectedly modified.")
        return
    end

    memory.assemble(cave, { symbol })
    memory.assemble(detour, address)
    patched[symbol] = true
end

local function pollOverrides()
    local modified = fileModified()
    if modified ~= nil and modified ~= lastModified then
        lastModified = modified
        reload()
    end

    event.executeAfterMs(POLL_MS, pollOverrides)
end

local function applyPatches()
    slots = memory.allocExe(SLOTS_SIZE)
    memory.u32[slots + SLOT_LAST_DIALOG] = 0xFFFFFFFF
    memory.u32[slots + SLOT_LAST_DIALOG + 4] = 0
    memory.u32[slots + SLOT_LOOT_SOURCE] = NO_SOURCE
    memory.u32[slots + SLOT_LOOT_SOURCE + 4] = 0
    lastModified = fileModified()
    reload()

    applyPatch("rtx_dialog", DIALOG_ADDRESS, DIALOG_BYTES,
        string.format(dialogCaveTemplate, MAP_ID_ADDRESS, slots + SLOT_LAST_DIALOG, slots + SLOT_DIALOG, DIALOG_RETURN),
        dialogDetour)

    applyPatch("rtx_reward_src", REWARD_SRC_ADDRESS, REWARD_SRC_BYTES,
        string.format(rewardSrcCaveTemplate, slots + SLOT_LOOT_SOURCE, REWARD_SRC_RETURN),
        rewardSrcDetour)
    applyPatch("rtx_loot_src", LOOT_SRC_ADDRESS, LOOT_SRC_BYTES,
        string.format(lootSrcCaveTemplate, slots + SLOT_LOOT_SOURCE, LOOT_SRC_RETURN),
        lootSrcDetour)
    applyPatch("rtx_chest_src", CHEST_SRC_ADDRESS, CHEST_SRC_BYTES,
        string.format(chestSrcCaveTemplate, slots + SLOT_LOOT_SOURCE, MAP_ID_ADDRESS, CHEST_SRC_RETURN),
        chestSrcDetour)

    -- The name caves call the shared lookup, so it goes in first.
    memory.assemble(string.format(findNameCaveTemplate, slots + SLOT_LOOT_SOURCE, slots + SLOT_SOURCE_NAME, slots + SLOT_ITEM_NAME),
        { "rtx_find_name" })
    patched.rtx_find_name = true

    applyPatch("rtx_item_name", ITEM_NAME_ADDRESS, ITEM_NAME_BYTES,
        string.format(itemNameCaveTemplate, ITEM_NAME_STORED, ITEM_NAME_CONTINUE),
        itemNameDetour)
    applyPatch("rtx_key_name_cjk", KEY_NAME_CJK_ADDRESS, KEY_NAME_CJK_BYTES,
        string.format(keyNameCaveTemplate, "rtx_key_name_cjk", "rtx_key_name_cjk", "  add rax,2", "rtx_key_name_cjk", 0x002B4090, KEY_NAME_CJK_RETURN),
        keyNameCjkDetour)
    applyPatch("rtx_key_name", KEY_NAME_ADDRESS, KEY_NAME_BYTES,
        string.format(keyNameCaveTemplate, "rtx_key_name", "rtx_key_name", "", "rtx_key_name", 0x002B40C0, KEY_NAME_RETURN),
        keyNameDetour)
    applyPatch("rtx_item_log", ITEM_LOG_ADDRESS, ITEM_LOG_BYTES,
        string.format(itemLogCaveTemplate, slots + SLOT_LOOT_SOURCE, slots + SLOT_SOURCE_MSG, slots + SLOT_ITEM_MSG, ITEM_LOG_CALL),
        itemLogDetour)
    applyPatch("rtx_item_line", ITEM_LINE_ADDRESS, ITEM_LINE_BYTES,
        string.format(itemLineCaveTemplate, slots + SLOT_ITEM_MSG, ITEM_LINE_MAX, ITEM_LINE_RETURN),
        itemLineDetour)

    print("Rando Text: Patch applied.")
    event.executeAfterMs(POLL_MS, pollOverrides)
end

local function onExit()
    for _, symbol in ipairs(caveSymbols) do
        if patched[symbol] then
            memory.unregisterSymbol(symbol)
        end
    end
    collectgarbage()
end

print("Rando Text: Applying patch.")

event.registerEventAsync("onInitDone", applyPatches)
event.registerEventAsync("exit", onExit)

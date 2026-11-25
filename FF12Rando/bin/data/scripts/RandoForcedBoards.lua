-- Requires the FF12 lua loader 2.0.1 or newer.
if not (checkMinVersion and checkMinVersion(2, 0, 1)) then
    error("Rando Forced Boards: requires the FF12 lua loader 2.0.1 or newer.")
end

local BOARDS_PATH = "../rando/forcedBoards.txt"

-- The Insurgent's Manifesto lets the menu be left during that animation, which
-- would otherwise skip the picker and leave the second board unset.
local LEARN_ADDRESS = 0x0055DA73
local LEARN_BYTES = { 0x33, 0xC9, 0xE8, 0xF6, 0x57, 0xD2, 0xFF } -- xor ecx,ecx; call 0x283270
local LEARN_RESUME = 0x0055DA7A

-- The picker's first call once the animation ends. The "or [rdi+0x570],0x200" before it
-- (0x55F4A2) is replaced by The Insurgent's Manifesto's quick license purchase, which jumps
-- back here, so the flag it set is cleared again when skipping.
local PURCHASE_ADDRESS = 0x0055F4AC
local PURCHASE_BYTES = { 0xE8, 0xEF, 0x67, 0xD3, 0xFF } -- call 0x295CA0
local PURCHASE_SKIP = 0x0055F6FE

local OWNED_ADDRESS = 0x0055D64E
local OWNED_BYTES = { 0x81, 0x8F, 0x70, 0x05, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00 } -- or dword ptr [rdi+0x570],0x200
local OWNED_SKIP = 0x0055D6E0     -- plays the confirm sound and returns

local HEADER_CLOSE = 0x00561800   -- (board menu) closes the header widget at [menu+0x138]
local HEADER_OPEN = 0x00561830    -- (board menu) creates it again

local PARTY_RECORDS = 0x0209AC30  -- party slot -> character record, see 0x282DF0
local PARTY_DATA = 0x02EBF190     -- character data at [ptr] + 8 + 0x1C8 * character
local SECOND_BOARD_TYPE = 0x1E    -- license type at +0x30 of the board menu's license, [menu+0xC8]
local CHARACTER_COUNT = 6
local BOARD_COUNT = 12

local assignTemplate =
[[
SYM:
  push rax
  push rcx
  push rdx
  movzx ecx,word ptr [rdi+0x160]
  mov rax,0x%X
  mov rax,qword ptr [rax]
  test rax,rax
  je SYM_vanilla
  mov rax,qword ptr [rax+rcx*8+0xAC8]
  test rax,rax
  je SYM_vanilla
  movzx ecx,byte ptr [rax+0x60]
  cmp ecx,%d
  jae SYM_vanilla
  mov rdx,0x%X
  movzx edx,byte ptr [rdx+rcx]
  cmp edx,%d
  jae SYM_vanilla
  imul ecx,ecx,0x1C8
  mov rax,0x%X
  mov rax,qword ptr [rax]
  test rax,rax
  je SYM_vanilla
  lea rax,[rax+rcx+8]
  cmp byte ptr [rax+5],0
  jne SYM_vanilla
  cmp byte ptr [rax+4],6
  ja SYM_vanilla
  cmp byte ptr [rax+0x1C4],0xFF
  jne SYM_assigned
  cmp byte ptr [rax+0x1C3],dl
  je SYM_vanilla
  mov byte ptr [rax+0x1C4],dl
SYM_assigned:
]]

local learnTemplate =
[[
  jmp SYM_vanilla
SYM_check:
  mov rax,qword ptr [rdi+0xC8]
  test rax,rax
  je SYM_skip
  cmp dword ptr [rax+0x30],0x%X
  je SYM
SYM_skip:
  xor ecx,ecx
  call 0x283270
  jmp 0x%X
SYM_vanilla:
  pop rdx
  pop rcx
  pop rax
  jmp SYM_skip
]]

-- Skips the picker when the second board is the forced one, whether it was just written or
-- already written when the license was learned.
local skipTemplate =
[[
  cmp byte ptr [rax+0x1C4],dl
  jne SYM_vanilla
  pop rdx
  pop rcx
  pop rax
%s  mov rcx,rdi
  call 0x%X
  mov rcx,rdi
  call 0x%X
  jmp 0x%X
SYM_vanilla:
  pop rdx
  pop rcx
  pop rax
%s]]

local sites = {
    {
        symbol = "rfb_learn",
        entry = "rfb_learn_check",
        address = LEARN_ADDRESS,
        bytes = LEARN_BYTES,
        tail = function() return string.format(learnTemplate, SECOND_BOARD_TYPE, LEARN_RESUME) end,
    },
    {
        symbol = "rfb_purchase",
        address = PURCHASE_ADDRESS,
        bytes = PURCHASE_BYTES,
        tail = function()
            return string.format(skipTemplate, "  and dword ptr [rdi+0x570],-0x201\n",
                HEADER_CLOSE, HEADER_OPEN, PURCHASE_SKIP, "  call 0x295CA0\n  jmp 0x55F4B1\n")
        end,
    },
    {
        symbol = "rfb_owned",
        address = OWNED_ADDRESS,
        bytes = OWNED_BYTES,
        tail = function()
            return string.format(skipTemplate, "", HEADER_CLOSE, HEADER_OPEN, OWNED_SKIP,
                "  or dword ptr [rdi+0x570],0x200\n  jmp 0x55D658\n")
        end,
    },
}

local patched = {}

local function loadBoards()
    local file = io.open(BOARDS_PATH, "r")
    if file == nil then
        return nil
    end

    local line = file:read("l") or ""
    io.close(file)

    local boards = {}
    local anyForced = false
    for value in line:gmatch("%-?%d+") do
        local board = tonumber(value)
        if board < 0 or board >= BOARD_COUNT then
            board = 0xFF
        else
            anyForced = true
        end
        table.insert(boards, board)
    end

    if #boards ~= CHARACTER_COUNT or not anyForced then
        return nil
    end
    return boards
end

local function bytesMatch(address, expected)
    return table.concat(memory.readArray(address, #expected), ",") == table.concat(expected, ",")
end

local function buildCave(site, boardTable)
    local assign = string.format(assignTemplate, PARTY_RECORDS, CHARACTER_COUNT, boardTable, BOARD_COUNT, PARTY_DATA)
    return (assign .. site.tail()):gsub("SYM", site.symbol)
end

local function applyPatch()
    local boards = loadBoards()
    if boards == nil then
        return
    end

    for _, site in ipairs(sites) do
        if not bytesMatch(site.address, site.bytes) then
            print("Rando Forced Boards: Couldn't apply patch, executable is unexpectedly modified.")
            return
        end
    end

    local boardTable = memory.allocExe(CHARACTER_COUNT)
    memory.writeArray(boardTable, boards)

    for _, site in ipairs(sites) do
        local entry = site.entry or site.symbol
        memory.assemble(buildCave(site, boardTable), { entry })
        memory.assemble(string.format("  jmp %%%s%%\n", entry), site.address)
        patched[entry] = true
    end
    print("Rando Forced Boards: Patch applied.")
end

local function onExit()
    for symbol in pairs(patched) do
        memory.unregisterSymbol(symbol)
    end
    collectgarbage()
end

print("Rando Forced Boards: Applying patch.")

event.registerEventAsync("onInitDone", applyPatch)
event.registerEventAsync("exit", onExit)

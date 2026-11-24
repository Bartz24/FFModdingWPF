if not (checkMinVersion and checkMinVersion(2, 0, 1)) then
    error("Rando License Overview: requires the FF12 lua loader 2.0.1 or newer.")
end

local REMEDY_LORE_1_LICENSE = 165
local REMEDY_LORE_3_LICENSE = REMEDY_LORE_1_LICENSE + 2

-- Addresses are module addresses (RVA + 0x120000).
local LICENSE_CHECK_ADDRESS = 0x002C4BAC -- mov esi,0xF1; lea r15d,[rsi-2]; lea r12d,[rsi-1]
local TIER_NAME_ADDRESS = 0x002C516E     -- lea edx,[rax+0xEE] (name of highest tier obtained)
local LAYOUT_HOOK_ADDRESS = 0x002C4E70   -- mov rax,[rsp+0x60], right after the Remedy Lore entries are built and sorted
local LAYOUT_RETURN_ADDRESS = 0x002C4E75
local MEASURE_TEXT = 0x002B3050          -- (ecx=0, rdx=text, r8=&width, r9=0)
local ZODIARK_CHECK_ADDRESS = 0x002C4F80 -- cmp r14d,0xC01E; mov rcx,rdi; cmove rax,rdi; jmp

local CHECKS = {
    {name = "unlocked statuses", address = LICENSE_CHECK_ADDRESS,
     original = {0xBE, 0xF1, 0x00, 0x00, 0x00, 0x44, 0x8D, 0x7E, 0xFE, 0x44, 0x8D, 0x66, 0xFF}},
    {name = "tier name", address = TIER_NAME_ADDRESS,
     original = {0x8D, 0x90, 0xEE, 0x00, 0x00, 0x00}},
    {name = "layout", address = LAYOUT_HOOK_ADDRESS,
     original = {0x48, 0x8B, 0x44, 0x24, 0x60, 0x48, 0x8D, 0x4D, 0x80}},
    {name = "zodiark", address = ZODIARK_CHECK_ADDRESS,
     original = {0x41, 0x81, 0xFE, 0x1E, 0xC0, 0x00, 0x00, 0x48, 0x8B, 0xCF, 0x48, 0x0F, 0x44, 0xC7, 0xEB, 0x0A}}
}

-- Espers section: an esper you don't have shows "?", but Zodiark (0xC01E) is
--    blanked entirely as a secret by `cmove rax,rdi` (text = null). NOP it so Zodiark
--    shows "?" like the other espers until obtained.
local ZODIARK_PATCH_OFFSET = 10
local ZODIARK_PATCH = {0x0F, 0x1F, 0x40, 0x00}

local COLUMNS = 5
local SLOTS = 20
local ENTRY_SIZE = 0x20

-- Tier label text, index 0 = Default, 1-3 = Remedy Lore 1-3.
local LABELS = {"Default", "Remedy Lore 1", "Remedy Lore 2", "Remedy Lore 3"}
local LEARNED_FORMAT = "{italic}{color:gold}%s{color:none}{/italic}"
-- No color tag when locked: an inline color overrides the cell's dimmed locked color.
local LOCKED_FORMAT = "{italic}%s{/italic}"

-- Column width is scaled from the vanilla 4-column width so the label column fits the panel.
local COLUMN_WIDTH_NUM = 3
local COLUMN_WIDTH_DEN = 4

--[[
Overview function (RVA 0x1a4720), at the hook:
  [rsp+0x70] = overview object. Remedy Lore entries are at obj+0x440 (14 x 0x20 bytes),
               the Remedy Lore list widget is at [obj+0xD0].
  [rsp+0x54] = tiers obtained: bit 1 = RL1, bit 2 = RL2, bit 3 = RL3.
Entry: +0x00 text, +0x08 word status index (0-13), +0x10 help text, +0x18 dword width+0x40 | 0x20000 lit.
List widget: +0xD8 entries, +0xE8 word count, +0xEA word column width, +0xEC byte columns.
]]
local caveTemplate =
[[
rrl_code:
  push rbx
  push rsi
  push rdi
  push rbp
  push r12
  push r13
  push r14
  push r15
  sub rsp, 0x30
  mov r12, qword ptr [rsp+0xE0]
  mov r13d, dword ptr [rsp+0xC4]
  mov r14, 0x%X
  mov r15, qword ptr [r12+0xD0]
  test r15, r15
  je rrl_done

  xor ebx, ebx
rrl_blank_loop:
  mov rax, rbx
  shl rax, 5
  lea rdi, [r14+rax]
  mov rcx, 0x%X
  mov qword ptr [rdi], rcx
  mov qword ptr [rdi+0x08], rbx
  mov qword ptr [rdi+0x10], 0
  mov qword ptr [rdi+0x18], 0x40
  inc ebx
  cmp ebx, %d
  jb rrl_blank_loop

  xor ebx, ebx
rrl_copy_loop:
  mov rax, rbx
  shl rax, 5
  lea rsi, [r12+rax+0x440]
  movzx eax, word ptr [rsi+0x08]
  cmp eax, 14
  jae rrl_copy_next
  lea rcx, [rrl_slot_map]
  movzx eax, byte ptr [rcx+rax]
  shl rax, 5
  lea rdi, [r14+rax]
  movups xmm0, xmmword ptr [rsi]
  movups xmmword ptr [rdi], xmm0
  movups xmm0, xmmword ptr [rsi+0x10]
  movups xmmword ptr [rdi+0x10], xmm0
rrl_copy_next:
  inc ebx
  cmp ebx, 14
  jb rrl_copy_loop

  xor ebx, ebx
rrl_label_loop:
  mov esi, 1
  test ebx, ebx
  je rrl_label_text
  mov ecx, ebx
  mov eax, 1
  shl eax, cl
  xor esi, esi
  test r13d, eax
  setne sil
rrl_label_text:
  lea eax, [rbx+rbx]
  mov edx, esi
  xor edx, 1
  add eax, edx
  lea rcx, [rrl_label_texts]
  mov rbp, qword ptr [rcx+rax*8]
  lea eax, [rbx+rbx*4+4]
  shl rax, 5
  lea rdi, [r14+rax]
  mov qword ptr [rdi], rbp
  xor r9d, r9d
  lea r8, [rsp+0x20]
  mov rdx, rbp
  xor ecx, ecx
  call 0x%X
  movzx eax, word ptr [rsp+0x20]
  add eax, 0x40
  and eax, 0xFFFF
  mov edx, esi
  shl edx, 17
  or eax, edx
  mov dword ptr [rdi+0x18], eax
  inc ebx
  cmp ebx, 4
  jb rrl_label_loop

  cmp byte ptr [r15+0xEC], 4
  jne rrl_set_entries
  movzx eax, word ptr [r15+0xEA]
  imul eax, eax, %d
  xor edx, edx
  mov ecx, %d
  div ecx
  mov word ptr [r15+0xEA], ax
  mov byte ptr [r15+0xEC], %d
rrl_set_entries:
  mov qword ptr [r15+0xD8], r14
  mov word ptr [r15+0xE8], %d

rrl_done:
  add rsp, 0x30
  pop r15
  pop r14
  pop r13
  pop r12
  pop rbp
  pop rdi
  pop rsi
  pop rbx
  mov rax, qword ptr [rsp+0x60]
  jmp 0x%X

rrl_slot_map:
  .db 0, 1, 2, 3, 5, 6, 7, 8, 10, 11, 12, 15, 16, 17
rrl_label_texts:
%s
]]

local caveSymbols = {
    "rrl_code"
}

local detour =
[[
  jmp %rrl_code%
]]

-- Game text encoding (credit to TheInsurgentsDescriptiveInventory).
local textLangMap = {[0] = 1, 0, 0, 0, 0, 0, 2, 4, 3}

local function readTextLanguage()
    local gameSettingsBase = memory.u64[0x01F82D20]
    return textLangMap[memory.u32[gameSettingsBase]] or 0
end

local function allocText(text, language)
    local bytes = { string.byte(message.convert(text, language), 1, -1) }
    local mem = memory.allocExe(#bytes + 1)
    if #bytes > 0 then
        memory.writeArray(mem, bytes)
    end
    memory.u8[mem + #bytes] = 0
    return mem
end

local function toDb(bytes)
    local parts = {}
    for i = 1, #bytes do
        parts[i] = string.format("0x%02X", bytes[i])
    end
    return ".db " .. table.concat(parts, ", ")
end

local function applyPatch()
    for _, check in ipairs(CHECKS) do
        local current = memory.readArray(check.address, #check.original)
        if table.concat(current, ",") ~= table.concat(check.original, ",") then
            print("Rando License Overview: Couldn't apply patch, executable is unexpectedly modified (" .. check.name .. ").")
            return
        end
    end

    -- 1) Remedy Lore license IDs
    memory.assemble(toDb({0xBE, REMEDY_LORE_3_LICENSE, 0x00, 0x00, 0x00}), LICENSE_CHECK_ADDRESS)
    memory.assemble(toDb({0x8D, 0x90, REMEDY_LORE_1_LICENSE - 1, 0x00, 0x00, 0x00}), TIER_NAME_ADDRESS)

    -- 3) Always list Zodiark
    memory.assemble(toDb(ZODIARK_PATCH), ZODIARK_CHECK_ADDRESS + ZODIARK_PATCH_OFFSET)

    -- 2) One row per tier with labels
    local language = readTextLanguage()
    local emptyText = allocText("", language)
    local labelTexts = {}
    for _, label in ipairs(LABELS) do
        table.insert(labelTexts, string.format("  .dq 0x%X", allocText(string.format(LEARNED_FORMAT, label), language)))
        table.insert(labelTexts, string.format("  .dq 0x%X", allocText(string.format(LOCKED_FORMAT, label), language)))
    end

    local entries = memory.allocExe(SLOTS * ENTRY_SIZE)
    for i = 0, SLOTS * ENTRY_SIZE - 1 do
        memory.u8[entries + i] = 0
    end

    local cave = string.format(caveTemplate,
        entries,
        emptyText,
        SLOTS,
        MEASURE_TEXT,
        COLUMN_WIDTH_NUM,
        COLUMN_WIDTH_DEN,
        COLUMNS,
        SLOTS,
        LAYOUT_RETURN_ADDRESS,
        table.concat(labelTexts, "\n"))

    memory.assemble(cave, caveSymbols)
    memory.assemble(detour, LAYOUT_HOOK_ADDRESS)
    print("Rando License Overview: Patch applied.")
end

local function onExit()
    memory.unregisterSymbol(caveSymbols[1])
    collectgarbage()
end

print("Rando License Overview: Applying patch.")

event.registerEventAsync("onInitDone", applyPatch)
event.registerEventAsync("exit", onExit)

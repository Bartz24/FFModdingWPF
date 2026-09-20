if not (checkMinVersion and checkMinVersion(1, 10, 2)) then
    error("Rando Hunt Rewards: requires the FF12 lua loader 1.10.2 or newer.")
end

local PATCH_ADDRESS = 0x003F4203

local ORIGINAL_BYTES = {
    0x0F, 0xB7, 0xCB, 0xE8, 0xC5, 0x69, 0xF0, 0xFF, 0x8B, 0xC8, 0x0F, 0xB7, 0xDB, 0xE8,
    0x9B, 0xF1, 0xF9, 0xFF, 0x8B, 0xC8, 0x8B, 0xD3, 0xE8, 0x62, 0x91, 0xF6, 0xFF
}

local FIRST_ID = 0xC000
local LAST_ID = 0xC01F
local SECOND_BOARD_ID = 0xC01F
local ESPER_RECORD_TYPE = 0x11

-- Crystal icon
local ICON_INDEX = 43

-- Backup text
local SECOND_BOARD_NAME = "Second Board"

local detour =
[[
  jmp %rhr_code%
]]

local caveTemplate =
[[
rhr_code:
  movzx ecx,bx
  cmp ecx,0x%X
  jb rhr_vanilla
  cmp ecx,0x%X
  ja rhr_vanilla

  movzx ebx,bx
  mov ecx,0x%X
  mov edx,ebx
  call 0x0035D380
  mov dword ptr [rax+0x2C],0x%X

  cmp ebx,0x%X
  jne rhr_return
  mov rcx,[rax+0x18]
  test rcx,rcx
  je rhr_board_name
  cmp byte ptr [rcx],0
  jne rhr_return

rhr_board_name:
  mov rcx,0x%X
  mov [rax+0x18],rcx
  jmp rhr_return

rhr_vanilla:
  call 0x002FABD0
  mov ecx,eax
  movzx ebx,bx
  call 0x003933B0
  mov ecx,eax
  mov edx,ebx
  call 0x0035D380

rhr_return:
  jmp 0x003F421E
]]

local caveSymbols = {
    "rhr_code"
}

-- Game text encoding (credit to TheInsurgentsDescriptiveInventory).
local textLangMap = {[0] = 1, 0, 0, 0, 0, 0, 2, 4, 3}

local function readTextLanguage()
    local gameSettingsBase = memory.u64[0x01F82D20]
    return textLangMap[memory.u32[gameSettingsBase]] or 0
end

local function applyPatch()
    local current = memory.readArray(PATCH_ADDRESS, #ORIGINAL_BYTES)
    if table.concat(current, ",") ~= table.concat(ORIGINAL_BYTES, ",") then
        print("Rando Hunt Rewards: Couldn't apply patch, executable is unexpectedly modified.")
        return
    end

    local nameBytes = { string.byte(message.convert(SECOND_BOARD_NAME, readTextLanguage()), 1, -1) }
    local nameMem = memory.allocExe(#nameBytes + 1)
    memory.writeArray(nameMem, nameBytes)
    memory.u8[nameMem + #nameBytes] = 0

    local cave = string.format(caveTemplate,
        FIRST_ID, LAST_ID,
        ESPER_RECORD_TYPE, ICON_INDEX,
        SECOND_BOARD_ID,
        nameMem)

    memory.assemble(cave, caveSymbols)
    memory.assemble(detour, PATCH_ADDRESS)
    print("Rando Hunt Rewards: Patch applied.")
end

local function onExit()
    memory.unregisterSymbol(caveSymbols[1])
    collectgarbage()
end

print("Rando Hunt Rewards: Applying patch.")

event.registerEventAsync("onInitDone", applyPatch)
event.registerEventAsync("exit", onExit)

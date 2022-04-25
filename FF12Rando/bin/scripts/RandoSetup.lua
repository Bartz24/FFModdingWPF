local seedInstalled = false
local pathUpdated = false

local function checkRando()
  if not seedInstalled then
    local text = message.convert("A rando seed has not been generated.\nRun the randomizer program in the FF12 steam folder:\n    Find the steam folder 'FINAL FANTASY XII THE ZODIAC AGE'\n    Go to x64/rando and run FF12Rando.exe")
    message.print(text)
  elseif pathUpdated then
    local text = message.convert("File loader path updated.\nRestart required for rando data to be loaded.")
    message.print(text)
  end
  event.executeAfterMs(100, checkRando)
end

local function onExit()
  collectgarbage()
end

print("Rando Setup: Applying patch.")

local seedFile=io.open("rando/outdata/rando.seed","r")
if seedFile~=nil then 
  io.close(seedFile)
  seedInstalled = true
end

local file = io.open("modules/config/ff12-file-loader.ini", "r")
local fileContent = {}
for line in file:lines() do
    table.insert (fileContent, line)
end
io.close(file)

local paths = false
for index, value in ipairs(fileContent) do
  if value == "[Paths]" then
    paths = true
  elseif paths and not string.find(value, "x64/rando/outdata") then
    fileContent[index] = "rando=x64/rando/outdata\n" .. value
    pathUpdated = true
    paths = false
  elseif paths then
    paths = false
  end
end

if pathUpdated then
  file = io.open("modules/config/ff12-file-loader.ini", "w")
  for index, value in ipairs(fileContent) do
      file:write(value .. "\n")
  end
  io.close(file)
end

event.registerEventAsync("onInitDone", checkRando)
event.registerEventAsync("exit", onExit)
local hints = {}

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

local function readHintNumber()
    return memory.u8[0x02164480+0xdffe]
end

local function writeSkyPiratesDen()
    memory.writeU8(0x02164480+0xdffd, readSkyPiratesDen())
end

local function checkHints()
    writeSkyPiratesDen()
    local hint = readHintNumber()
    if hint > 0 then
        local hintText = "Nothing important left."
        if hints[hint] ~= nil then
            hintText = hints[hint]
        end
        local text = message.convert(hintText)
        message.print(text)
    end
    event.executeAfterMs(100, checkHints)
end

local function onExit()
    collectgarbage()
end

print("Rando Hints: Applying patch.")

local file = io.open("rando/outdata/hints.txt", "r")
if file~=nil then 
    local fileContent = {}
    for line in file:lines() do
        table.insert (fileContent, line)
    end
    io.close(file)
    
    local hint = 0
    local currentHint = ""
    for index, value in ipairs(fileContent) do
      if string.find(value, "%[Hint ") then
        if hint > 0 then
            hints[hint] = currentHint        
        end
        hint = tonumber(string.sub(value, 7, -2))
        currentHint = ""
      elseif hint > 0 then
        if currentHint ~= "" then
            currentHint = currentHint .. "\n"
        end
        currentHint = currentHint .. value
      end
    end
    hints[hint] = currentHint
    for index, value in ipairs(hints) do
        print("Hint Number " .. index)
        print(value)
    end
end

event.registerEventAsync("onInitDone", checkHints)
event.registerEventAsync("exit", onExit)
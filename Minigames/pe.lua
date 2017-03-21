function clone(t) -- deep-copy a table
    if type(t) ~= "table" then return t end
    local meta = getmetatable(t)
    local target = {}
    for k, v in pairs(t) do
        if type(v) == "table" then
            target[k] = clone(v)
        else
            target[k] = v
        end
    end
    setmetatable(target, meta)
    return target
end

AddEventHandler("startGame:pe", function()
	gameRunning = true

	tempPlayers = clone(players)

	-- Get a president
	president = math.random(1, #tempPlayers)
	i = 0
	for x, player in pairs(tempPlayers) do
		i = i + 1

		if i == president then
			players[x].type = "president"
			host = player.player
			TriggerClientEvent("pe:start", player.player, "president")
			TriggerClientEvent("pe:host", player.player)
			table.remove(tempPlayers, i)
			break
		end
	end

	-- Get terrorists
	i = math.ceil(#tempPlayers / 2)
	for x, player in pairs(tempPlayers) do
		players[x].type = "terrorist"
		TriggerClientEvent("pe:start", player.player, "terrorist")
		table.remove(tempPlayers, i)

		i = i - 1
		if i < 1 then
			break
		end
	end

	-- Get bodyguards
	if #tempPlayers > 0 then
		for x, player in pairs(tempPlayers) do
			players[x].type = "bodyguard"
			TriggerClientEvent("pe:start", player.player, "bodyguard")
			table.remove(tempPlayers, i)
		end
	end
end)

RegisterServerEvent("pe:stopGame")
AddEventHandler("pe:stopGame", function(reason)
	gameRunning = false

	TriggerClientEvent("pe:stopGame", -1)

	if reason == "pDead" then
		TriggerClientEvent("chatMessage", -1, "", {0, 0, 0}, "^1The president is dead. Terrorists won.")
	elseif reason == "pLeft" then
		TriggerClientEvent("chatMessage", -1, "", {0, 0, 0}, "^1The president is left.")
	elseif reason == "notEnoughPlayers" then
		TriggerClientEvent("chatMessage", -1, "", {0, 0, 0}, "^1Not enough players. Game will be cancelled.")
	end

	TriggerEvent("newRound")
end)

AddEventHandler("playerDropped", function(reason)
	if gameRunning then
		if source == host then
			TriggerEvent("pe:stopGame", "pLeft")
		elseif #players < 3 then
			TriggerEvent("pe:stopGame", "notEnoughPlayers")
		end
	end
end)
players = {}

local games = {
	{game = "pe", name = "President Escort"}
}

RegisterServerEvent("newPlayer")
AddEventHandler("newPlayer", function(id)
	table.insert(players, {player = source, id = id})
	if gameRunning then
		TriggerClientEvent("attachWaitCam", source, "Round in progress")
	else
		if #players < 2 then
			TriggerClientEvent("attachWaitCam", source, "Waiting for more players...")
		else
			game = games[math.random(1, #games)]

			TriggerClientEvent("deattachWaitCam", -1)
			TriggerClientEvent("chatMessage", -1, "", {0, 0, 0}, "Now playing: " .. game.name)
			SetTimeout(1, function()
				TriggerEvent("startGame:" .. game.game)
			end)
		end
	end
end)

RegisterServerEvent("newRound")
AddEventHandler("newRound", function()
	TriggerClientEvent("attachWaitCam", source, "New round starting...")
	SetTimeout(10000, function()
		game = games[math.random(1, #games)]

		TriggerClientEvent("deattachWaitCam", -1)
		TriggerClientEvent("chatMessage", -1, "", {0, 0, 0}, "Now playing: " .. game.name)
		SetTimeout(1, function()
			TriggerEvent("startGame:" .. game.game)
		end)
	end)
end)

AddEventHandler("playerDropped", function(reason)
	for i, player in pairs(players) do
		if player.player == source then
			table.remove(players, i)
			break
		end
	end
end)
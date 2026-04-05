function RT.SendCurrentGameState()
	local mode = Game.GetCurrentGameModeID()
	local scenario

	if mode == "INGAME" then
		scenario = Game.GetScenarioID()
	else
		scenario = "none"
	end

	local json = string.format('{"mode":%q,"scenario":%q}', mode, scenario)

	RL.SendNewGameState(json)
end

function RT.SendCurrentInventory()
	local player = Game.GetPlayerName()
	local amounts = {}

	for i, item in ipairs(RT.InterestedItems) do
		amounts[i] = Game.GetItemAmount(player, item)
	end

	local json = string.format("[%s]", table.concat(amounts, ","))

	Game.LogWarn(0, "Sending inventory: " .. tostring(#RT.InterestedItems) .. " items")
	RL.SendInventory(json)
end

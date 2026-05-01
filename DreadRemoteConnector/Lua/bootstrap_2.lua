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
		-- See if it's a boss first. If it's not a boss, we treat it as an item
		local bossStatus = RT.IsBossDefeated(item)

		if bossStatus == nil then
			amounts[i] = Game.GetItemAmount(player, item)
		else
			amounts[i] = bossStatus and 1 or 0
		end
	end

	local json = string.format("[%s]", table.concat(amounts, ","))

	Game.LogWarn(0, "Sending inventory: " .. tostring(#RT.InterestedItems) .. " items")
	RL.SendInventory(json)
end

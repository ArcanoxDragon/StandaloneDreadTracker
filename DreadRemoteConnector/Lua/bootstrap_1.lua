RT = RT or {}
RT.InterestedItems = RT.InterestedItems or {}

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

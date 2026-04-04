function RT.SendCurrentInventory(interestedItems)
	if type(interestedItems) ~= "table" then
		error("Invalid value for interestedItems")
	end

	local player = Game.GetPlayerName()
	local amounts = {}

	for i, item in ipairs(interestedItems) do
		amounts[i] = Game.GetItemAmount(player, item)
	end

	local json = string.format("[%s]", table.concat(amounts, ","))

	RL.SendInventory(json)
end
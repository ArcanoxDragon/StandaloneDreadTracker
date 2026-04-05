RT = RT or {}
RT.InterestedItems = RT.InterestedItems or {}

if type(RL) ~= "table" then
	return "RemoteLua library not found. Ensure Remote Lua/Remote Tracking are enabled when exporting the game."
end

-- Use same function name as Randovania - RDV powerups call this when picked up.
-- We also need to overwrite RDV's own bootstrap if it was connected before us.
function RL.UpdateRDVClient()
	Game.AddSF(0.0, "RT.SendCurrentGameState", "")
	if Game.GetCurrentGameModeID() == "INGAME" then
		Game.AddSF(0.05, "RT.SendCurrentInventory", "")
	end
end

return "ok"
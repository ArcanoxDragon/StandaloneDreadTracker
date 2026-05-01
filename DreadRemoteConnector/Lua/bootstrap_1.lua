local function TestBossBlackboardProp(scenarioId, bbPropId, valueWhenDefeated)
	local scenarioSectionId = Game.GetScenarioBlackboardSectionID(scenarioId)

	if not scenarioSectionId then
		return false
	end

	local bbPropValue = Blackboard.GetProp(scenarioSectionId, bbPropId)
	local isDefeated = bbPropValue == valueWhenDefeated

	return isDefeated
end

RT.BossProps = {
	-- EMMIs
	WHITE_EMMI = { "s010_cave", "PRP_CV_CentralUnitCaves:CENTRAL_UNIT:State", 7 },
	GREEN_EMMI = { "s020_magma", "centralunitmagmacontroller:CENTRAL_UNIT:State", 7 },
	YELLOW_EMMI = { "s030_baselab", "centralunitmagmacontroller:CENTRAL_UNIT:State", 7 },
	BLUE_EMMI = { "s050_forest", "centralunitmagmacontroller:CENTRAL_UNIT:State", 7 },
	PURPLE_EMMI = { "s070_basesanc", "centralunitmagmacontroller:CENTRAL_UNIT:State", 7 },
	ORANGE_EMMI = { "s080_shipyard", "centralunitmagmacontroller_000:CENTRAL_UNIT:State", 7 },

	-- Other Bosses
	CORPIUS = { "s010_cave", "SG_Scorpius:SPAWNGROUP:Enabled", false },
	KRAID = { "s020_magma", "SG_Kraid:SPAWNGROUP:Enabled", false },
	DROGYGA = { "s040_aqua", "LUA:HYDROGIGA_DEAD", true },
	EXPERIMENT_Z57 = { "s020_magma", "SG_CooldownX:SPAWNGROUP:Enabled", false },
	GOLZUNA = { "s050_forest", "SG_SuperGoliath:SPAWNGROUP:Enabled", false },
	ESCUE = { "s070_basesanc", "SG_SuperQuetzoa:SPAWNGROUP:Enabled", false },
}

local BOSS_RESOURCE_ID_PREFIX = "BOSS_DEAD_"

function RT.IsBossDefeated(bossResourceId)
	if string.sub(bossResourceId, 1, string.len(BOSS_RESOURCE_ID_PREFIX)) ~= BOSS_RESOURCE_ID_PREFIX then
		return nil
	end

	local bossId = string.sub(bossResourceId, string.len(BOSS_RESOURCE_ID_PREFIX) + 1)
	local bossPropInfo = RT.BossProps[bossId]

	if type(bossPropInfo) ~= "table" or #bossPropInfo ~= 3 then
		return nil
	end

	local scenarioId = bossPropInfo[1]
	local bbPropId = bossPropInfo[2]
	local valueWhenDefeated = bossPropInfo[3]

	return TestBossBlackboardProp(scenarioId, bbPropId, valueWhenDefeated)
end

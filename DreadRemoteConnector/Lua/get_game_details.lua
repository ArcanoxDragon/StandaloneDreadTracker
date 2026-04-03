return string.format(
	"%d,%d,%s,%s,%s",
	RL.Version,
	RL.BufferSize,
	tostring(RL.Bootstrap),
	Init.sLayoutUUID,
	GameVersion)
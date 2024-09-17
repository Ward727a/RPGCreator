extends Button


func _pressed():
	
	var EF: EngineFolder = EngineFolder.new()
	
	EF.load_skill_effects()

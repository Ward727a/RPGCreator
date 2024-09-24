extends Control

const button_group_path = "res://Core/Scenes/database/ButtonGroup/LeftMenu.tres"

var _open_debug_input: InputEventKey = preload("res://Core/Inputs/Shortcuts/debug/open_debug.tres")

func _init():

	await _test_load_skill_from_db()

# Called when the node enters the scene tree for the first time.
func _ready():

	# %Button.connect("pressed", _on_create_file)
	# %Button2.connect("pressed", _on_load_file)
	# %Button3.connect("pressed", _on_list_ref)
	# %Button4.connect("pressed", _on_generate_id)
	%ApplyDB.pressed.connect(_on_apply_changes)
	%CancelDB.pressed.connect(_on_load_file)
	

	var button_group: ButtonGroup = %MenuEntities.button_group
	button_group.pressed.connect(_on_menu_button_pressed)

	await _load_base_conditions()
	await _load_base_effects()

func _shortcut_input(event):
	
	if event.is_match(_open_debug_input, true) and event.is_pressed():
		var debug_menu: ConfirmationDialog = $DebugMain
		
		if !debug_menu.get_opened():
			debug_menu.popup_centered()

func _test_load_skill_from_db():
	
	print("Loading skills...")
	var storage = JsonStorage.new()

	var data = storage.get_all_data("Skills", "Skills")

	print(data)

	if data == null:
		return

	SkillRegister.from_dictionary(data)

func _on_load_file():
	var storage = JsonStorage.new()

	var data = storage.get_all_data("Entities", "Characters")

	print(data)
	
	if data == null:
		return

	for key in data:
		var chara: Character = data[key]
		print(chara.history_text)

func _on_create_file():
	var storage = JsonStorage.new()

	var data: Dictionary = {"Test": "Test", "TestArray": [1, 2, 3], "TestDict": {"Test": "Test"}, "TestArrayDict": [{"Test": "Test"}, {"Test": "Test"}]}

	storage.store_all_data(data, "Entities", "Entities")

	%RichTextLabel.text = "File created."

func _on_list_ref():
	var storage = JsonStorage.new()

	var refs = storage.get_possible_ref("Entities")

	%RichTextLabel.text = str(refs)

func _on_generate_id():
	var id_generator = IdGenerator.new()

	%RichTextLabel.text = id_generator.generate_id()


func _on_apply_changes():
	
	print("Apply changes")


	_save_box_entities()
	_save_box_skills()

	pass

func _save_box_entities():
	var characters = CharacterRegister.characters_to_string()

	var storage = JsonStorage.new()

	var return_data = storage.store_all_data(characters, "Entities", "Characters")
	print("char_data saved: ", return_data)

func _save_box_skills():
	var skills = SkillRegister.to_dictionary()

	var storage = JsonStorage.new()

	var return_data = storage.store_all_data(skills, "Skills", "Skills")
	print("skill_data saved: ", return_data)

func _on_menu_button_pressed(button: Button):
	match(button.text):
		"CHARACTERS":
			%BoxEntities.show()
			%BoxSkills.hide()
			%BoxItems.hide()
		"SKILLS":
			%BoxEntities.hide()
			%BoxSkills.show()
			%BoxItems.hide()
		"ITEM":
			%BoxEntities.hide()
			%BoxSkills.hide()
			%BoxItems.show()

#! This function is still WIP, but for now it works.
#? This function should check the folder "skill/conditions" for all the conditions
func _load_base_conditions(): 

	EngineFolder.new().load_skill_conditions()

#! This function is still WIP, but for now it works.
#? This function should check the folder "skill/effects" for all the effects
func _load_base_effects():
	EngineFolder.new().load_skill_effects()
	pass
	#var data = JsonStorage.new().get_all_data("Skills", "EngineEffects")
	#
	#if data!=null:
		#for key in data:
			#var effect: BaseEffect = data[key]
			#EffectRegister.add_effect(effect)
	#else:
		#print("No default effects found.")
		#print("Creating default effects...")
		#
		#EffectRegister.add_effect(PSE)
		#EffectRegister.add_effect(HEE)
		#EffectRegister.add_effect(STE)
		#
		#JsonStorage.new().store_all_data(EffectRegister.effects_to_string(), "Skills", "EngineEffects")
		#print("Default effects created and saved.")

func test_init_conditions():
	
	EngineFolder.new().load_skill_conditions()
	
	# Init the base engine conditions
	#var MSC = ManaSkillCondition.new()
	#var HSC = HealthSkillCondition.new()
#
	## Add the conditions to the register
	#SkillConditionsRegister.add_condition(MSC)
	#SkillConditionsRegister.add_condition(HSC)

extends Control

const button_group_path = "res://Core/Scenes/database/ButtonGroup/LeftMenu.tres"

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

	_load_base_conditions()


func _test_load_skill_from_db():
	var storage = JsonStorage.new()

	var data = storage.get_all_data("Skills", "Skills")

	print(data)

	if data == null:
		return

	for key in data:
		var skill: BaseSkill = data[key]
		SkillRegister.add_skill(skill)

func _on_load_file():
	var storage = JsonStorage.new()

	var data = storage.get_all_data("Entities", "Characters")

	print(data)

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
	var skills = SkillRegister.skills_to_string()

	var storage = JsonStorage.new()

	var return_data = storage.store_all_data(skills, "Skills", "Skills")
	print("skill_data saved: ", return_data)

func _on_menu_button_pressed(button: Button):
	match(button.text):
		"CHARACTERS":
			%BoxEntities.show()
			%BoxSkills.hide()
		"SKILLS":
			%BoxEntities.hide()
			%BoxSkills.show()

func _load_base_conditions(): # ! Need to rework this so it's not hardcoded, the engine need to be able to generate IDs for the conditions, and the conditions need to be stored in the database.

	var data = JsonStorage.new().get_all_data("Skills", "EngineConditions")

	if data!=null:
		for key in data:
			var condition: BaseSkillCondition = data[key]
			SkillConditionsRegister.add_condition(condition)
	else:
		print("No default conditions found.")
		print("Creating default conditions...")
		var MSC = ManaSkillCondition.new("862473214724818651169126206019312014213811999160139211194216352502359214523915714513")
		var HSC = HealthSkillCondition.new("177112252037022597662098486601872396369109207182149210357923314210892603319848166")

		SkillConditionsRegister.add_condition(MSC)
		SkillConditionsRegister.add_condition(HSC)

		JsonStorage.new().store_all_data(SkillConditionsRegister.conditions_to_string(), "Skills", "EngineConditions")
		print("Default conditions created and saved.")

func test_init_conditions():
	
	# Init the base engine conditions
	var MSC = ManaSkillCondition.new()
	var HSC = HealthSkillCondition.new()

	# Add the conditions to the register
	SkillConditionsRegister.add_condition(MSC)
	SkillConditionsRegister.add_condition(HSC)
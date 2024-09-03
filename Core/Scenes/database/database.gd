extends Control


# Called when the node enters the scene tree for the first time.
func _ready():

	# %Button.connect("pressed", _on_create_file)
	# %Button2.connect("pressed", _on_load_file)
	# %Button3.connect("pressed", _on_list_ref)
	# %Button4.connect("pressed", _on_generate_id)
	%ApplyDB.pressed.connect(_on_apply_changes)
	%CancelDB.pressed.connect(_on_load_file)

	pass # Replace with function body.


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

	pass

func _save_box_entities():
	var characters = CharacterRegister.characters_to_string()

	var storage = JsonStorage.new()

	var return_data = storage.store_all_data(characters, "Entities", "Characters")
	print("Data saved: ", return_data)

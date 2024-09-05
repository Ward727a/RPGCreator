extends HBoxContainer

var last_clicked_node: Node = null

# Called when the node enters the scene tree for the first time.
func _ready():
	%AddCharacter.pressed.connect(_on_new_character)
	%CharList.item_clicked.connect(_on_char_list_clicked)
	%CharList.origin = "BoxEntities"


	await %CharList.add_row("CHAR_NAME", List.ItemType.TEXT)
	await %CharList.add_row("CHAR_SURNAME", List.ItemType.TEXT)

	await _load_characters()
	
	print("BoxEntities ready")

func _load_characters():
	var storage = JsonStorage.new()

	var data = storage.get_all_data("Entities", "Characters")

	print(data)

	for key in data:
		var character: Character = data[key]

		CharacterRegister.add_character(character)

		%CharList.add_item([character.name, character.surname], {"char_idx": character.id})

func _on_new_character():
	var character: Character = CharacterRegister.new_character()

	%CharList.add_item([character.name, character.surname], {"char_idx": character.id})

	var history = HistoryCharactersList.new(EnumRegister.HistoryAction.ADD, character)

	HistoryRegister.add_to_history(history)


func _on_char_list_clicked(_node: Node, _name: String, _metadata: Dictionary):

	print("clicked")

	if last_clicked_node != _node:
		
		if last_clicked_node != null:
			# Edit the theme of the last clicked node to set it back to the default
			var old_theme: StyleBoxFlat = last_clicked_node.base_theme
			last_clicked_node.get_node("Panel").add_theme_stylebox_override("panel", old_theme)

		last_clicked_node = _node
		# Edit the theme of the last clicked node to set it back to the default
		var base_theme: StyleBoxFlat = last_clicked_node.base_theme.duplicate()

		base_theme.border_width_bottom = 2
		base_theme.border_width_top = 2
		base_theme.border_width_left = 2
		base_theme.border_width_right = 2
		base_theme.border_color = Color(0.4, 0.4, 0.4, 1.0)

		last_clicked_node.get_node("Panel").add_theme_stylebox_override("panel", base_theme)

	var char_idx: String = _metadata['char_idx']

	if !CharacterRegister.has_character(char_idx):
		push_error("Character not found")
		return

	DataRegister.register_data("box_entities", "selected_character", _metadata['char_idx'])

	%infoEntities.set_entities(CharacterRegister.get_character(char_idx))


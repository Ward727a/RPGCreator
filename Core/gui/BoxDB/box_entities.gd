extends HBoxContainer

signal change_made(change_origin: String, change_type: EnumRegister.BoxEntitiesChange, change_data: Dictionary)

# Called when the node enters the scene tree for the first time.
func _ready():
	%AddCharacter.pressed.connect(_on_new_character)
	%CharList.item_clicked.connect(_on_char_list_clicked)
	%CharList.origin = "BoxEntities"

func _on_new_character():
	var character: Character = CharacterRegister.new_character()

	%CharList.add_item([character.name, character.surname], {"char_idx": character.id})

	change_made.emit("BoxEntities", EnumRegister.BoxEntitiesChange.ADD_CHARACTER, {"character": character})

func _on_char_list_clicked(_name: String, _metadata: Dictionary):

	var char_idx: String = _metadata['char_idx']

	if !CharacterRegister.has_character(char_idx):
		push_error("Character not found")
		return

	DataRegister.register_data("box_entities", "selected_character", _metadata['char_idx'])

	%infoEntities.set_entities(CharacterRegister.get_character(char_idx))


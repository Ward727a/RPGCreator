extends HistoryBase
class_name HistoryCharacter

var character_idx: String = ""
var edited_data: Dictionary = {}

func _init(_action: EnumRegister.HistoryAction, _character_idx: String, _edited_data: Dictionary = {}):
    location = EnumRegister.HistoryLocation.DB_CHARACTER
    action = _action
    character_idx = _character_idx
    edited_data = _edited_data

func undo_change() -> void:
    print("Undo changement to character")
    var character: Character = CharacterRegister.get_character(character_idx)

    for key in edited_data:
        character.set(key, edited_data[key])
    
    CharacterRegister.set_character(character_idx, character)

func redo_change() -> void:
    print("Redo changement to character")
    var character: Character = CharacterRegister.get_character(character_idx)

    for key in edited_data:
        character.set(key, edited_data[key])
    
    CharacterRegister.set_character(character_idx, character)
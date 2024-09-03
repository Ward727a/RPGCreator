extends HistoryBase
class_name HistoryCharactersList

var character: Character = null

func _init(_action: EnumRegister.HistoryAction, _character: Character = null):
    
    location = EnumRegister.HistoryLocation.DB_CHARACTERSLIST
    action = _action
    character = _character

func undo_add() -> void:
    print("Undo add character list")

func redo_add() -> void:
    print("Redo add character list")

func undo_remove() -> void:
    print("Undo remove character list")

func redo_remove() -> void:
    print("Redo remove character list")

func undo_move() -> void:
    print("Undo move character list")

func redo_move() -> void:
    print("Redo move character list")

func undo_change() -> void:
    print("Undo change character list")

func redo_change() -> void:
    print("Redo change character list")

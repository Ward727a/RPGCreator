extends RefCounted
class_name HistoryBase

var action: EnumRegister.HistoryAction = EnumRegister.HistoryAction.ADD
var location: EnumRegister.HistoryLocation = EnumRegister.HistoryLocation.NONE


func _init():
    pass

func undo() -> void:
    match action:
        EnumRegister.HistoryAction.ADD:
            undo_add()
        EnumRegister.HistoryAction.REMOVE:
            undo_remove()
        EnumRegister.HistoryAction.MOVE:
            undo_move()
        EnumRegister.HistoryAction.CHANGE:
            undo_change()

func redo() -> void:
    match action:
        EnumRegister.HistoryAction.ADD:
            redo_add()
        EnumRegister.HistoryAction.REMOVE:
            redo_remove()
        EnumRegister.HistoryAction.MOVE:
            redo_move()
        EnumRegister.HistoryAction.CHANGE:
            redo_change()

func undo_add() -> void:
    pass

func redo_add() -> void:
    pass

func undo_remove() -> void:
    pass

func redo_remove() -> void:
    pass

func undo_move() -> void:
    pass

func redo_move() -> void:
    pass

func undo_change() -> void:
    pass

func redo_change() -> void:
    pass

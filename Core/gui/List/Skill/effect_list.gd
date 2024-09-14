extends CustomListV2Base

var effect_list: Array[BaseEffect] = []

signal edit_condition(condition_idx:String)

func _ready():
	super()
	

func _on_list_template_edit(condition_idx:String):
	edit_condition.emit(condition_idx)

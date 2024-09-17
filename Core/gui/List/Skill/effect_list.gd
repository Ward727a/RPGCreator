extends CustomListV2Base

var effect_list: Array[BaseEffect] = []

signal edit_effect(effect_idx:String)

func _ready():
	super()
	

func _on_list_template_edit(effect_idx:String):
	edit_effect.emit(effect_idx)

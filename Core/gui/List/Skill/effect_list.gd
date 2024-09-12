extends CustomListV2Base

var effect_list: Array[BaseEffect] = []

func _ready():
	super()
	

func _on_list_template_edit(condition_idx:String):
	print("Edit condition: ", condition_idx)

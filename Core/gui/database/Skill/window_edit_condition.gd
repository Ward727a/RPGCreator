extends ConfirmationDialog

@onready var list: VBoxContainer = get_node("list")

var parameters: Dictionary = {} # The parameters of the condition that is being edited. (export_ & hint_)
var condition_idx: String = "" # The index of the condition that is being edited.

func edit(_condition: BaseSkillCondition):
	condition_idx = _condition.idx

	# Get the list of parameters
	for i in _condition.get_property_list():

		var prop_name: String = i.name
		var prop_name_sanitized: String = sanitaze_param_name(prop_name)

		if prop_name.begins_with("export_"):
			if not parameters.has(prop_name_sanitized):
				parameters[prop_name_sanitized] = {}
			
			parameters[prop_name_sanitized]["export"] = _condition.get(prop_name)
		
		if prop_name.begins_with("hint_"):
			if not parameters.has(prop_name_sanitized):
				parameters[prop_name_sanitized] = {}
			
			parameters[prop_name_sanitized]["hint"] = _condition.get(prop_name)
	
	print(parameters)


func sanitaze_param_name(_name: String) -> String:
	return _name.replace("export_", "").replace("hint_", "").capitalize()
extends ConfirmationDialog

@onready var list: VBoxContainer = get_node("ScrollContainer/list")

var edited_effect: BaseEffect = null

var parameters: Dictionary = {} # The parameters of the condition that is being edited. (export_ & hint_)
var effect_idx: String = "" # The index of the condition that is being edited.

func _ready():
	about_to_popup.connect(_on_about_to_popup)

func get_effect() -> BaseEffect:

	# Edit the condition with the new parameters
	for i in parameters.keys():
		var data: Dictionary = parameters[i]
		var param_data: Variant = data["export"]

		edited_effect.exported_value[i]['value'] = param_data

	return edited_effect

func edit(_effect: BaseEffect) -> bool:

	# Clear the parameters
	parameters = {}

	effect_idx = _effect.id
	edited_effect = _effect
	print(var_to_str((_effect)))

	for key in _effect.exported_value.keys():

		var prop_name: String = key
		var i: Dictionary = _effect.exported_value[key]
		
		parameters[prop_name] = {}
		parameters[prop_name]["export"] = i.value if i.has('value') else i.default
		parameters[prop_name]["type"] = string_type_to_int(i.type)
		parameters[prop_name]["name"] = tr(prop_name)
		parameters[prop_name]["hint"] = i.hint
		
	return true

func string_type_to_int(type_name: String) -> int:
	
	match(type_name.to_snake_case().to_upper()):
		"STRING":
			return TYPE_STRING
		"INT":
			return TYPE_INT
		"BOOL":
			return TYPE_BOOL
		"FLOAT":
			return TYPE_FLOAT
		_:
			return TYPE_NIL

func clear_children():
	for i in list.get_children():
		i.queue_free()

func create_string_edit(default_value: String, hint: String, key: String) -> LineEdit:
	var lineedit: LineEdit = LineEdit.new()
	lineedit.text = default_value
	lineedit.set_tooltip_text(hint)
	lineedit.text_changed.connect(on_edit.bind(key))
	return lineedit

func create_int_edit(default_value: int, hint: String, key: String) -> SpinBox:
	var spinbox: SpinBox = SpinBox.new()
	spinbox.value = default_value
	spinbox.set_tooltip_text(hint)
	spinbox.allow_greater = true
	spinbox.allow_lesser = true
	spinbox.value_changed.connect(on_edit.bind(key))
	return spinbox

func create_bool_edit(default_value: bool, hint: String, node_text: Label, key: String) -> HBoxContainer:

	var hbox: HBoxContainer = HBoxContainer.new()
	node_text.reparent(hbox)

	var checkbox: CheckButton = CheckButton.new()
	checkbox.button_pressed = default_value
	checkbox.set_tooltip_text(hint)
	checkbox.toggled.connect(on_edit.bind(key))
	hbox.add_child(checkbox)
	return hbox

func _on_about_to_popup():
	
	clear_children()

	for i in parameters.keys():
		var _param_name: String = i
		var param_data: Dictionary = parameters[i]

		var param_name_label: Label = Label.new()
		param_name_label.text = param_data["name"] if param_data.has("name") else _param_name
		list.add_child(param_name_label)

		match param_data["type"]:
			TYPE_INT, TYPE_FLOAT:
				var value = param_data["export"]
				var hint = (param_data["hint"] if param_data.has("hint") else "No hint provided.")
				list.add_child(create_int_edit(value, hint, i))
			TYPE_STRING:
				var value = param_data["export"]
				var hint = (param_data["hint"] if param_data.has("hint") else "No hint provided.")
				list.add_child(create_string_edit(value, hint, i))
			TYPE_BOOL:
				var value = param_data["export"]
				var hint = (param_data["hint"] if param_data.has("hint") else "No hint provided.")
				list.add_child(create_bool_edit(value, hint, param_name_label, i))
			_:
				print("Unsupported type: ", param_data["type"])
				print("Trying to retreat with a line edit.")
				print("This shouldn't happen!!")
				var param_input: LineEdit = LineEdit.new()
				param_input.text = str(param_data["export"])
				param_input.set_tooltip_text(param_data["hint"] if param_data.has("hint") else "No hint provided.")
				list.add_child(param_input)

func on_edit(value, key: String):
	parameters[key]["export"] = value

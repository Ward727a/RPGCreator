extends Object
class_name LUADynamicParameter

const needed_key = [
	'display_name',
	'default_value',
	'type'
]

var id: String
var name: String
var default: Variant
var value: Variant
var type: String

static var EVENTS_edit_value: Signal = Utils.make_static_signal(LUADynamicParameter, 'edit_value')
static var EVENTS_edit_name: Signal = Utils.make_static_signal(LUADynamicParameter, 'edit_name')
static var EVENTS_edit_default: Signal = Utils.make_static_signal(LUADynamicParameter, 'edit_default')
static var EVENTS_edit_type: Signal = Utils.make_static_signal(LUADynamicParameter, 'edit_type')

func edit_value(new_value: Variant):
	EVENTS_edit_value.emit(self, new_value)
	value = new_value

func edit_name(new_name: String):
	EVENTS_edit_name.emit(self, new_name)
	name = new_name

func edit_default(new_default: String):
	EVENTS_edit_default.emit(self, new_default)
	default = new_default

func edit_type(new_type: String):
	EVENTS_edit_type.emit(self, new_type)
	type = new_type

func _init(parameter: Dictionary):
	
	for key in needed_key:
		if !parameter.has(key):
			push_error("Parameter given doesn't have the key: %s" % key)
			return
	
	id = IdGenerator.new().generate_id()
	
	name = parameter['display_name']
	value = parameter['default_value']
	default = parameter['default_value']
	type = parameter['type']

func get_id() -> String:
	return id

func get_name() -> String:
	return name

func get_value() -> Variant:
	return value

func get_default() -> Variant:
	return default

func get_type() -> String:
	return type

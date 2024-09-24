class_name ItemParametersRegister

static var parameters_data: Dictionary = {}

const PARAMETER_KEY: Array[String] = [
	"display_name",
	"type",
	"default_value",
	"allow_edit",
	"description",
	"ui_key"
]

enum PARAMETER_EVENT {
	ON_LOAD_UI,
	ON_EDIT_FROM_UI,
	ON_EDITED
}

func lua_fields():
	return ['PARAMETER_KEY', 'parameters_data']

static func load_lua(lua_api: LuaAPI):
	
	lua_api.push_variant('add_parameter', ItemParametersRegister.add_parameter)
	lua_api.push_variant('set_parameter', ItemParametersRegister.set_parameter)
	lua_api.push_variant('set_parameters', ItemParametersRegister.set_parameters)
	lua_api.push_variant('remove_parameter', ItemParametersRegister.remove_parameter)
	lua_api.push_variant('get_category_parameters_key', ItemParametersRegister.get_category_parameters_key)
	lua_api.push_variant('get_parameter', ItemParametersRegister.get_parameter)
	lua_api.push_variant('get_copy_parameter', ItemParametersRegister.get_copy_parameter)
	lua_api.push_variant('get_categories', ItemParametersRegister.get_categories)
	lua_api.push_variant('remove_category', ItemParametersRegister.remove_category)
	lua_api.push_variant('has_category', ItemParametersRegister.has_category)
	lua_api.push_variant('has_parameter', ItemParametersRegister.has_parameter)
	lua_api.push_variant('add_category', ItemParametersRegister.add_category)
	lua_api.push_variant('bind_parameter_to_event', ItemParametersRegister.bind_parameter)
	lua_api.push_variant('bind_category_to_event', ItemParametersRegister.bind_category)
	lua_api.push_variant('PARAMETER_EVENT', ItemParametersRegister.PARAMETER_EVENT)

static func add_parameter(parameter_name: String, parameter_category: String, parameter_data: Dictionary = {
	'display_name': "",
	'type': "",
	'description': "",
	'default_value': "",
	'allow_edit': true
}) -> _lua_parameter:
	
	if !parameters_data.has(parameter_category):
		push_error('ERROR: Their is no "', parameter_category, '" in the list of category! Please check for error or add it if you want to use it!')
		return null
	
	var parent_data = parameters_data[parameter_category]
	var parent_parameters = parent_data['parameters']
	
	if parent_parameters.has(parameter_name):
		push_error("You can't add the parameter '%s' in '%s' category because it already exist!" %[parameter_name, parameter_category])
		return null
	
	var missing_key: Array[String] = []
	
	for needed_key in ItemParametersRegister.PARAMETER_KEY:
		if !parameter_data.has(needed_key):
			missing_key.push_back(needed_key)
	
	if missing_key.size() != 0:
		push_error("Given parameter data has one or more missing key: ", missing_key)
		return null
	
	parent_parameters[parameter_name] = parameter_data
	
	return _lua_parameter.new(parameter_category, parameter_name)

static func set_parameter(parameter_name: String, parameter_category: String, parameter_data: Dictionary = {
	'display_name': "",
	'type': "",
	'description': "",
	'default_value': "",
	'allow_edit': true
}) -> _lua_parameter:
	
	if !parameters_data.has(parameter_category):
		push_error('ERROR: Their is no "', parameter_category, '" in the list of category! Please check for error or add it if you want to use it!')
		return null
	
	var parent_data = parameters_data[parameter_category]
	var parent_parameters = parent_data['parameters']
	
	var missing_key: Array[String] = []
	
	for needed_key in ItemParametersRegister.PARAMETER_KEY:
		if !parameter_data.has(needed_key):
			missing_key.push_back(needed_key)
	
	if missing_key.size() != 0:
		push_error("Given parameter data has one or more missing key: ", missing_key)
		return null
	
	parent_parameters[parameter_name] = parameter_data
	
	if parameter_data.has('events'):
		if parameter_data['events'].has('ON_EDITED'):
			for fn: Callable in parameter_data['events']['ON_EDITED']:
				fn.call(parameter_data)
	
	return _lua_parameter.new(parameter_category, parameter_name)

static func set_parameters(parameter_category: String, parameter_category_data: String) -> bool:
	
	if !parameters_data.has(parameter_category):
		push_error('ERROR: Their is no "', parameter_category, '" in the list of category! Please check for error or add it if you want to use it!')
		return false
	
	var parent_data = parameters_data[parameter_category]
	var parent_parameters = parent_data['parameters']
	
	parent_parameters = parameter_category_data
	
	return true

static func remove_parameter(parameter_name: String, parameter_category: String) -> bool:
	
	if !parameters_data.has(parameter_category):
		push_error('ERROR: Their is no "', parameter_category, '" in the list of category! Please check for error or add it if you want to use it!')
		return false
	
	var parent_data = parameters_data[parameter_category]
	var parent_parameters: Dictionary = parent_data['parameters']
	
	if !parent_parameters.has(parameter_name):
		push_error("You can't remove the parameter '%s' in '%s' category because it doesn't exist!" %[parameter_name, parameter_category])
		return false
	
	return parent_parameters.erase(parameter_name)

static func get_category_parameters_key(parameter_category: String) -> Array[String]:
	
	var keys: Array[String] = []
	
	if !parameters_data.has(parameter_category):
		push_error('ERROR: Their is no "', parameter_category, '" in the list of category! Please check for error or add it if you want to use it!')
		return keys
	
	var parent_data = parameters_data[parameter_category]
	var parent_parameters: Dictionary = parent_data['parameters']
	
	for key in parent_parameters.keys():
		keys.push_back(key)
	
	return keys

static func get_parameter(parameter_name: String, parameter_category: String) -> Dictionary:
	
	if !parameters_data.has(parameter_category):
		push_error('ERROR: Their is no "', parameter_category, '" in the list of category! Please check for error or add it if you want to use it!')
		return {}
	
	var parent_data = parameters_data[parameter_category]
	var parent_parameters: Dictionary = parent_data['parameters']
	
	if !parent_parameters.has(parameter_name):
		push_error("You can't get the parameter '%s' in '%s' category because it doesn't exist!" %[parameter_name, parameter_category])
		return {}
	
	return parent_parameters[parameter_name]

static func get_copy_parameter(parameter_name: String, parameter_category: String) -> Dictionary:
	
	if !parameters_data.has(parameter_category):
		push_error('ERROR: Their is no "', parameter_category, '" in the list of category! Please check for error or add it if you want to use it!')
		return {}
	
	var parent_data = parameters_data[parameter_category]
	var parent_parameters: Dictionary = parent_data['parameters']
	
	if !parent_parameters.has(parameter_name):
		push_error("You can't get the parameter '%s' in '%s' category because it doesn't exist!" %[parameter_name, parameter_category])
		return {}
	
	return parent_parameters[parameter_name].duplicate(true)

static func get_parameters(parameter_category: String) -> Dictionary:
	
	if !parameters_data.has(parameter_category):
		push_error('ERROR: Their is no "', parameter_category, '" in the list of category! Please check for error or add it if you want to use it!')
		return {}
	
	var parent_data = parameters_data[parameter_category]
	var parent_parameters: Dictionary = parent_data['parameters']
	
	return parent_parameters

static func get_copy_parameters(parameter_category: String) -> Dictionary:
	
	if !parameters_data.has(parameter_category):
		push_error('ERROR: Their is no "', parameter_category, '" in the list of category! Please check for error or add it if you want to use it!')
		return {}
	
	var parent_data = parameters_data[parameter_category]
	var parent_parameters: Dictionary = parent_data['parameters']
	
	return parent_parameters.duplicate(true)

static func get_categories() -> Array:
	
	return parameters_data.keys()

static func remove_category(parameter_category: String) -> bool:
	
	if !parameters_data.has(parameter_category):
		push_error('ERROR: Their is no "', parameter_category, '" in the list of category! Please check for error!')
		return false
	
	return parameters_data.erase(parameter_category)

static func has_category(parameter_category: String) -> bool:
	
	return parameters_data.has(parameter_category)

static func has_parameter(parameter_name: String, parameter_category: String) -> bool:
	
	if !parameters_data.has(parameter_category):
		push_error('ERROR: Their is no "', parameter_category, '" in the list of category! Please check for error!')
		return false
	
	var parent_data = parameters_data[parameter_category]
	var parent_parameters: Dictionary = parent_data['parameters']
	
	return parent_parameters.has(parameter_name)

static func add_category(parameter_category: String, category_ui_key: String) -> _lua_category:
	
	if has_category(parameter_category):
		push_error("The category '%s' already exist!" % parameter_category)
		return null
	
	parameters_data[parameter_category] = {
		'parameters': {},
		'ui_key': category_ui_key
	}
	
	return _lua_category.new(parameter_category)

static func get_category(parameter_category: String) -> Dictionary:
	
	if !has_category(parameter_category):
		return {}
	
	return parameters_data[parameter_category]

static func bind_parameter(parameter_category: String, parameter_name: String, parameter_event: PARAMETER_EVENT, fn: Callable) -> bool:
	
	if !has_category(parameter_category):
		push_error('ERROR: Their is no "', parameter_category, '" in the list of category! Please check for error!')
		return false
	
	if !has_parameter(parameter_name, parameter_category):
		push_error("You can't bind an event to the parameter '%s' in '%s' category because it doesn't exist!" %[parameter_name, parameter_category])
		return false
	
	var parent_data = parameters_data[parameter_category]
	var parent_parameters = parent_data['parameters']
	
	var item_data = parent_parameters[parameter_name]
	
	if !item_data.has('events'):
		item_data['events'] = {
			"ON_LOAD_UI": [],
			"ON_EDIT_FROM_UI": [],
			"ON_EDITED": []
		}
	
	if PARAMETER_EVENT.keys()[parameter_event] == null:
		push_error("Their is a problem with the problem event, the event '%s' doesn't exist!" % parameter_event)
		return false
	
	var parameter_event_string: String = PARAMETER_EVENT.keys()[parameter_event].to_upper()
	
	if !(fn is Callable):
		push_error('Strange error due to the bind function not being a callable.')
		return false
	
	if !item_data['events'].has(parameter_event_string):
		item_data['events'][parameter_event_string] = []
	
	item_data['events'][parameter_event_string].push_back(fn)
	
	return true

static func bind_category(parameter_category: String, event: PARAMETER_EVENT, fn: Callable) -> bool:
	
	if !has_category(parameter_category):
		push_error('ERROR: Their is no "', parameter_category, '" in the list of category! Please check for error!')
		return false
	
	if !(fn is Callable):
		push_error('Strange error due to the bind function not being a callable.')
		return false
	
	var parent_data = parameters_data[parameter_category]
	
	if !parent_data.has('events'):
		parent_data['events'] = {
			"ON_LOAD_UI": [],
			"ON_EDIT_FROM_UI": []
		}
	
	if PARAMETER_EVENT.keys()[event] == null:
		push_error("Their is a problem with the problem event, the event '%s' doesn't exist!" % event)
		return false
	
	var parameter_event_string: String = PARAMETER_EVENT.keys()[event].to_upper()
	
	
	parent_data['events'][parameter_event_string].push_back(fn)
	
	return true

static func call_event(event: PARAMETER_EVENT, item_id: String, param_name: String, new_value, category: String):
	
	if !has_category(category):
		return false
	
	if !has_parameter(param_name, category):
		return false
	
	var category_data = get_category(category)
	var category_parameters = category_data['parameters']
	var parameter_data = category_parameters[param_name]
	var events = parameter_data['events']
	
	match(event):
		ItemParametersRegister.PARAMETER_EVENT.ON_EDIT_FROM_UI:
			var event_key = 'ON_EDIT_FROM_UI'
			if !events.has(event_key):
				return false
			
			if typeof(events[event_key]) != TYPE_ARRAY:
				return false
			
			var events_fn = events[event_key]
			
			for fn: Callable in events_fn:
				fn.call(item_id, new_value)

class _lua_parameter:
	
	var category: String
	var name: String
	var data: Dictionary
	
	func _init(_category: String, _name: String):
		category = _category
		name = _name
		data = ItemParametersRegister.get_parameter(name, category)
	
	func lua_fields():
		return ['category', 'name']
		
	func set_data(new_data: Dictionary):
		data = new_data
	
	func connect_on_load_ui(fn: Callable):
		ItemParametersRegister.bind_parameter(category, name, ItemParametersRegister.PARAMETER_EVENT.ON_LOAD_UI, fn)
	
	func connect_on_edit_from_ui(fn: Callable):
		ItemParametersRegister.bind_parameter(category, name, ItemParametersRegister.PARAMETER_EVENT.ON_EDIT_FROM_UI, fn)
	
	func connect_on_edited(fn: Callable):
		ItemParametersRegister.bind_parameter(category, name, ItemParametersRegister.PARAMETER_EVENT.ON_EDITED, fn)

class _lua_category:
	
	var category: String
	
	func _init(_category: String):
		category = _category
	
	func lua_fields():
		return ['category']
	
	func connect_on_load_ui(fn: Callable):
		ItemParametersRegister.bind_category(category, ItemParametersRegister.PARAMETER_EVENT.ON_LOAD_UI, fn)

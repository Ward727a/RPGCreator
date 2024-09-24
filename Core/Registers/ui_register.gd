class_name UIRegister

static var data: Dictionary = {}

func lua_fields():
	return ['data']

static func load_lua(lua: LuaAPI):
	
	lua.push_variant("has_ui", UIRegister.has_ui)
	lua.push_variant("add_ui", UIRegister.add_ui)
	lua.push_variant("set_ui", UIRegister.set_ui)
	lua.push_variant("get_ui", UIRegister.get_ui)
	lua.push_variant("get_ui_list", UIRegister.get_ui_list)
	lua.push_variant("remove_ui", UIRegister.remove_ui)
	lua.push_variant("call_bind", UIRegister.call_bind)
	lua.push_variant("bind", UIRegister.bind_fn_to_ui)
	lua.push_variant("unbind", UIRegister.unbind_fn_to_ui)

static func has_ui(ui_name: String) -> bool:
	return data.has(ui_name)

static func add_ui(ui_name: String, ui: Control) -> bool:
	
	if has_ui(ui_name):
		push_error("ERROR: Can't add an UI to the register with the same name as another one! (%s)" % ui_name)
		return false
	
	data[ui_name] = {
		"node": ui
	}
	
	return true

static func set_ui(ui_name: String, ui: Control) -> bool:
	
	data[ui_name] = {
		"node": ui
	}
	
	return true

static func get_ui(ui_name: String) -> Control:
	
	if !has_ui(ui_name):
		push_error("ERROR: Can't get '%s' UI as it doesn't exist!" % ui_name)
		return null
	
	return data[ui_name]['node']

static func get_ui_list() -> Array:
	
	return data.keys()

static func remove_ui(ui_name: String) -> bool:
	
	if !has_ui(ui_name):
		push_error("ERROR: Can't remove '%s' UI as it doesn't exist!" % ui_name)
		return false
	
	return data.erase(ui_name)

static func call_bind(ui_name: String, bind_name: String, args: Array) -> bool:
	
	if !has_ui(ui_name):
		push_error("ERROR: Can't call bind on '%s' UI as it doesn't exist!" % ui_name)
		return false
	
	var ui_data = data[ui_name]
	
	if !ui_data.has(bind_name):
		return false
	
	var bind_data = ui_data[bind_name]
	
	for fn: Callable in bind_data:
		var binded_fn: Callable
		
		args.reverse()
		
		for arg in args:
			binded_fn = fn.bind(arg)
		
		binded_fn.call()
	
	return true

static func bind_fn_to_ui(ui_name: String, bind_name: String, fn: Callable) -> bool:
	
	if !has_ui(ui_name):
		push_error("ERROR: Can't bind to '%s' UI as it doesn't exist!" % ui_name)
		return false
	
	var ui_data = data[ui_name]
	
	if ui_data.has(bind_name):
		
		ui_data[bind_name].push_back(fn)
		return true
	
	ui_data[bind_name] = [fn]
	
	return true

static func unbind_fn_to_ui(ui_name: String, bind_name: String, fn: Callable) -> bool:
	
	if !has_ui(ui_name):
		push_error("ERROR: Can't unbind to '%s' UI as it doesn't exist!" % ui_name)
		return false
	
	var ui_data = data[ui_name]
	
	if !ui_data.has(bind_name):
		push_error("ERROR: Can't remove the bind named '%s' to the '%s' UI as it doesn't exist!" % [bind_name, ui_name])
		return false
	
	ui_data[bind_name].erase(fn)
	
	if ui_data[bind_name].size() == 0:
		ui_data.erase(bind_name)
	
	return true

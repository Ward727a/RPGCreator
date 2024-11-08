extends Resource
class_name ResPlugin

signal permission_given()
signal toggle(state: bool)
signal auto_started()

var logger: Logger = Logger.new("Plugin - ???")

@export var enable: bool = false
@export var auto_start: bool = false

@export var path: String = ""
@export var name: String = "":
	set(new_value):
		logger.obj_name = str("Plugin - %s" % new_value)
		name = new_value
@export var author: String = ""
@export var description: String = ""
@export var version: String = ""
@export var icon: String = ""

@export var need_plugin: PackedStringArray = []

@export var main_script: String = ""
@export var main_config: String = ""
@export var main_folder: String = ""

@export var permissions_required: Array[ResPermissionObject] = []
@export var linked_obj: Dictionary = {}

@export var imported_lua: Dictionary = {}

# Lua script
var lua_main: LuaObject
var lua_scripts: Array[LuaObject]

func _init():
	toggle.connect(_on_toggle)
	auto_started.connect(ask_auto_start)

func ask_auto_start():
	
	var dialog = dialog_auto_start.new(self)
	if Main.is_valid():
		Main.get_singleton().add_sibling.call_deferred(dialog)
		
		dialog.ready.connect(
			func():
				dialog.popup_centered()
		)

func ask_for_perms():
	
	if Settings.is_valid():
		var plugin_folder = main_folder.rsplit("/", true, 1)[1]
		
		if Settings.get_singleton().SettingsData.base_plugin_folder.has(plugin_folder):
			
			for low in get_low_perm():
				low.allowed = true
			
			for mid in get_mid_perm():
				mid.allowed = true
			
			for high in get_high_perm():
				high.allowed = true
			
			_give_permission()
			return
	_check_lows()

func _check_lows():
	var lows = get_low_perm()
	
	if lows.size() != 0:
		_ask_for_lows(lows)
		return
	_check_mids()

func _check_mids():
	var mids = get_mid_perm()
	
	if mids.size() != 0:
		_ask_for_mids(mids)
		return
	_check_high()

func _check_high():
	var highs = get_high_perm()
	
	if highs.size() != 0:
		_ask_for_high(highs)
		return
	
	_give_permission()

func _ask_for_lows(perm_lows: Array):
	
	var low_dialog: dialog_low = dialog_low.new(perm_lows, self)
	
	if Main.is_valid():
		Main.get_singleton().add_sibling.call_deferred(low_dialog)
		
		low_dialog.confirmed.connect(
			func():
				_check_mids()
		)
		
		low_dialog.cancel.connect(
			func():
				_check_mids()
		)
		
		low_dialog.ready.connect(
			func():
				low_dialog.popup_centered()
		)

func _ask_for_mids(perm_mids: Array):
	
	var mid_dialog: dialog_mid = dialog_mid.new(perm_mids, self)
	
	if Main.is_valid():
		Main.get_singleton().add_sibling.call_deferred(mid_dialog)
		
		mid_dialog.confirmed.connect(
			func():
				_check_high()
		)
		
		mid_dialog.cancel.connect(
			func():
				_check_high()
		)
		
		mid_dialog.ready.connect(
			func():
				mid_dialog.popup_centered()
		)

func _ask_for_high(perm_high: Array):
	
	var high_dialog: dialog_high = dialog_high.new(perm_high, self)
	
	if Main.is_valid():
		Main.get_singleton().add_sibling.call_deferred(high_dialog)
		
		high_dialog.confirmed.connect(
			func():
				_give_permission()
		)
		
		high_dialog.cancel.connect(
			func():
				_give_permission()
		)
		
		high_dialog.ready.connect(
			func():
				high_dialog.popup_centered()
		)

func _give_permission():
	
	if has_allowed('GENERATE_ID'):
		set_link("generate_id", IDGenerator.generate_id)
	
	if PluginManager.is_valid():
		PluginManager.get_singleton().link_plugin(self)
	
	if ProjectManager.is_valid():
		ProjectManager.get_singleton().link_plugin(self)
	
	if DataManager.is_valid():
		DataManager.get_singleton().link_plugin(self)
	
	if GlobalEventManager.is_valid():
		GlobalEventManager.get_singleton().link_plugin(self)
	
	if AssetManager.is_valid():
		AssetManager.get_singleton().link_plugin(self)
	
	if GodotManager.is_valid():
		GodotManager.get_singleton().link_plugin_material(self)
		GodotManager.get_singleton().link_plugin_texture(self)
		GodotManager.get_singleton().link_plugin_style(self)
		GodotManager.get_singleton().link_plugin_ui(self)
		GodotManager.get_singleton().link_plugin_global(self)
	
	if ThemeManager.is_valid():
		ThemeManager.get_singleton().link_plugin(self)
	
	if InterfaceManager.is_valid():
		InterfaceManager.get_singleton().link_plugin(self)
	
	Logger.link_plugin(self)
	URLHelper.link_plugin(self)
	
	set_link("has_permission", has_allowed)
	set_link("import_lua", LuaCallableExtra.with_ref(import_lua))
	set_link("software_version", Settings.get_singleton().SoftwareInfo.version)
	
	enable = true
	
	permission_given.emit()

func _print(message):
	print(message)

func _on_toggle(_state: bool):
	
	if !need_plugin.is_empty():
		if PluginManager.is_valid():
			var PM = PluginManager.get_singleton()
			
			for plugin in need_plugin:
				var _plugin: ResPlugin = PM.GET(plugin)
				
				if _plugin == null:
					logger.error("This plugin need \"%s\" to start, but it's not installed!" % plugin)
					return
				
				if !_plugin.enable:
					logger.error("Thie plugin need plugin \"%s\", but \"%s\" is not started!" % [plugin, plugin])
					return
	
	if toggle:
		if lua_main != null:
			lua_main.start()
		else:
			lua_main = LuaObject.new(name, str(path, '/', main_script))
			
			for link_key in linked_obj:
				
				var obj = linked_obj[link_key]
				
				lua_main.push_variant(link_key, obj)
			
			if lua_main.is_valid():
				lua_main.start()
	else:
		if lua_main != null:
			await lua_main.stop()
			
			for link_key in linked_obj:
				lua_main.push_variant(link_key, null)

func has_allowed(permission_path: String):
	
	return (get_allowed(permission_path).size() != 0)

func has_not_allowed(permission_path: String):
	
	return (get_not_allowed(permission_path).size() != 0)

func has_perm(permission_path: String):
	
	return (get_perm(permission_path).size() != 0)

func get_allowed(permission_path: String):
	
	var perm_allowed = permissions_required.filter(
		func(permission_obj: ResPermissionObject):
			if permission_obj.path == (permission_path) and permission_obj.allowed == true:
				return true
	)
	
	return perm_allowed

func get_not_allowed(permission_path: String):
	
	var perm_not_allowed = permissions_required.filter(
		func(permission_obj: ResPermissionObject):
			if permission_obj.path == (permission_path) and permission_obj.allowed == false:
				return true
	)
	
	return perm_not_allowed

func get_perm(permission_path: String):
	
	var perms = permissions_required.filter(
		func(permission_obj: ResPermissionObject):
			if permission_obj.path == (permission_path):
				return true
	)
	
	return perms

func _get_perm_from_level(level: int = 0) -> Array[ResPermissionObject]:
	
	var perm_by_level = permissions_required.filter(
		func(permission_obj: ResPermissionObject):
			if permission_obj.level == level:
				return true
	)
	
	return perm_by_level

func get_low_perm() -> Array[ResPermissionObject]:
	
	return _get_perm_from_level(0)

func get_mid_perm() -> Array[ResPermissionObject]:
	
	return _get_perm_from_level(1)

func get_high_perm() -> Array[ResPermissionObject]:
	
	return _get_perm_from_level(2)

func has_link(_name: String):
	
	return linked_obj.has(_name)

func remove_link(_name: String) -> bool:
	
	if !has_link(_name):
		return true
	
	return linked_obj.erase(_name)

func set_link(_name: String, obj: Variant):
	
	linked_obj[_name] = obj

func add_link(_name: String, obj: Variant) -> bool:
	
	if has_link(_name):
		return false
	
	set_link(_name, obj)
	return true

func get_link(_name: String, default = null) -> Variant:
	
	if !has_link(_name):
		return default
	
	return linked_obj[_name]

func import_lua(from_lua: LuaAPI, lua_script: String, content_to_give: Dictionary = {}) -> bool:
	
	var from_origin: bool = false
	
	if lua_script.begins_with("./"):
		lua_script = lua_script.split("./", true, 1)[1]
		from_origin = true
	
	if lua_script.begins_with("/"):
		lua_script = lua_script.split("/", true, 1)[1]
		from_origin = true
	
	var script_path: String = ""
	
	if from_origin:
		script_path = str(main_folder, "/", lua_script)
	else:
		script_path = str(from_lua.get_registry_value('folder_path'), "/", lua_script)
	
	if script_path.is_empty() or !script_path.ends_with(".lua"):
		logger.error("Script path is empty!")
		return false
	
	if !FileAccess.file_exists(script_path):
		push_error("%s doesn't exist!" % script_path)
		return false
	
	var api: LuaObject = LuaObject.new(name, script_path)
	
	for link_key in linked_obj:
		
		var obj = linked_obj[link_key]
		
		api.push_variant(link_key, obj)
	
	if api.is_valid():
		
		if content_to_give.size() != 0:
			for key in content_to_give:
				var value = content_to_give[key]
				
				if typeof(value) == TYPE_NIL:
					logger.warn("Giving %s object, but it's null!")
				
				api.push_variant(key, value)
		
		var started = await api.start()
		
		if !started:
			return false
	
	if api.has_fn('_on_import'):
		var import_dic = api.CALL('_on_import')
		
		if from_lua == null:
			logger.error("Lua ref not valid!")
			return false
		
		for key in import_dic:
			var value = import_dic[key]
			
			if typeof(value) == TYPE_NIL:
				logger.error("Can't import %s because his value is null!" % key)
				continue
			from_lua.push_variant(key, value)
	
	return true

class dialog_low extends ConfirmationDialog:
	
	signal cancel()
	
	var perms: Array = []
	var allowed: Array = []
	
	var main_container: VBoxContainer
	
	var upper_text: Label
	
	var background_container: PanelContainer
	var margin_container: MarginContainer
	var scroll_container: ScrollContainer
	var vbox_container: VBoxContainer
	
	func _init(_perms: Array, plugin_obj: ResPlugin):
		
		perms = _perms
		allowed = _perms
		
		set_title(str("Low-level permissions confirmation"))
		
		main_container = VBoxContainer.new()
		add_child(main_container)
		
		upper_text = Label.new()
		main_container.add_child(upper_text)
		upper_text.set_text(
			str(
			"\"%s\" ask for the following low-level permission." % plugin_obj.name,
			"\nLow-level permission are normaly not a risk for your computer / project, but please check before accepting!"
			)
		)
		upper_text.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		upper_text.add_theme_font_size_override('font_size', 14)
		
		background_container = PanelContainer.new()
		main_container.add_child(background_container)
		background_container.set_h_size_flags(Control.SIZE_EXPAND_FILL)
		background_container.set_v_size_flags(Control.SIZE_EXPAND_FILL)
		
		margin_container = MarginContainer.new()
		background_container.add_child(margin_container)
		margin_container.add_theme_constant_override("margin_top", 4)
		margin_container.add_theme_constant_override("margin_left", 4)
		margin_container.add_theme_constant_override("margin_bottom", 4)
		margin_container.add_theme_constant_override("margin_right", 4)
		
		scroll_container = ScrollContainer.new()
		margin_container.add_child(scroll_container)
		scroll_container.set_v_size_flags(Control.SIZE_EXPAND_FILL)
		
		vbox_container = VBoxContainer.new()
		scroll_container.add_child(vbox_container)
		vbox_container.set_h_size_flags(Control.SIZE_EXPAND_FILL)
		
		for perm: ResPermissionObject in perms:
			perm.allowed = true
			vbox_container.add_child(_create_option(perm, perm.path, perm.description))
		set_min_size(Vector2i(750, 350))
		
		# remove hide from pressing cancel button
		for connection in get_cancel_button().pressed.get_connections():
			get_cancel_button().pressed.disconnect(connection.callable)
		
		get_cancel_button().pressed.connect(_ask_confirm)
		
		borderless = true
	
	func _process(_delta):
		set_size(Vector2i(750, 350))
		popup_centered()
		set_process(false)
	
	func _ask_confirm():
		
		var confirm: ConfirmationDialog = ConfirmationDialog.new()
		confirm.set_title("Are you sure?")
		confirm.set_text("If you continue, all permissions will be refused.\nDon't worry, you can change that at any moment in the settings.")
		
		confirm.confirmed.connect(_on_cancel)
		
		confirm.confirmed.connect(
			func():
				confirm.queue_free()
		)
		confirm.canceled.connect(
			func():
				confirm.queue_free()
		)
		
		add_child(confirm)
		confirm.popup_centered()
	
	func _on_cancel():
		for perm in perms:
			perm.allowed = false
		
		hide()
		cancel.emit()
	
	func _on_toggle_option(toggle: bool, perm_obj: ResPermissionObject):
		
		if toggle:
			perm_obj.allowed = true
			#allowed.push_back(perm_obj)
		else:
			perm_obj.allowed = false
			#if allowed.has(perm_obj):
				#allowed.erase(perm_obj)
				#perm_obj.allowed = false
	
	func _create_option(perm_obj: ResPermissionObject, perm_name: String, perm_description: String) -> VBoxContainer:
		
		var container: VBoxContainer = VBoxContainer.new()
		
		var hbox_container: HBoxContainer = HBoxContainer.new()
		container.add_child(hbox_container)
		
		var label_name: Label = Label.new()
		hbox_container.add_child(label_name)
		label_name.set_text(perm_name)
		label_name.set_h_size_flags(Control.SIZE_EXPAND_FILL)
		
		var toggle_perm: CheckBox = CheckBox.new()
		hbox_container.add_child(toggle_perm)
		toggle_perm.flat = true
		toggle_perm.set_pressed_no_signal(true)
		toggle_perm.add_theme_stylebox_override('focus', StyleBoxEmpty.new())
		toggle_perm.toggled.connect(_on_toggle_option.bind(perm_obj))
		
		if !perm_description.is_empty():
			var label_description: Label = Label.new()
			container.add_child(label_description)
			label_description.set_text(perm_description)
			label_description.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		
		var h_separator: HSeparator = HSeparator.new()
		container.add_child(h_separator)
		
		return container

class dialog_mid extends ConfirmationDialog:
	
	signal cancel()
	
	var setted_size: bool = false
	
	var perms: Array = []
	var allowed: Array = []
	
	var wait_timer: Timer
	
	var main_container: VBoxContainer
	
	var upper_text: Label
	
	var background_container: PanelContainer
	var margin_container: MarginContainer
	var scroll_container: ScrollContainer
	var vbox_container: VBoxContainer
	
	func _init(_perms: Array, plugin_obj: ResPlugin):
		
		perms = _perms
		allowed = _perms
		
		set_title(str("Medium-level permissions confirmation"))
		
		wait_timer = Timer.new()
		wait_timer.autostart = true
		wait_timer.set_wait_time(5)
		wait_timer.timeout.connect(_on_timer_timeout)
		add_child(wait_timer)
		
		main_container = VBoxContainer.new()
		add_child(main_container)
		
		upper_text = Label.new()
		main_container.add_child(upper_text)
		upper_text.set_text(
			str(
			"\"%s\" ask for the following medium-level permission." % plugin_obj.name,
			"\nMedium-level permission could be a risk for your project but not your computer, so check before accepting!"
			)
		)
		upper_text.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		upper_text.add_theme_font_size_override('font_size', 14)
		
		background_container = PanelContainer.new()
		main_container.add_child(background_container)
		background_container.set_h_size_flags(Control.SIZE_EXPAND_FILL)
		background_container.set_v_size_flags(Control.SIZE_EXPAND_FILL)
		
		margin_container = MarginContainer.new()
		background_container.add_child(margin_container)
		margin_container.add_theme_constant_override("margin_top", 4)
		margin_container.add_theme_constant_override("margin_left", 4)
		margin_container.add_theme_constant_override("margin_bottom", 4)
		margin_container.add_theme_constant_override("margin_right", 4)
		
		scroll_container = ScrollContainer.new()
		margin_container.add_child(scroll_container)
		scroll_container.set_v_size_flags(Control.SIZE_EXPAND_FILL)
		
		vbox_container = VBoxContainer.new()
		scroll_container.add_child(vbox_container)
		vbox_container.set_h_size_flags(Control.SIZE_EXPAND_FILL)
		
		for perm: ResPermissionObject in perms:
			perm.allowed = true
			vbox_container.add_child(_create_option(perm, perm.path, perm.description))
		set_min_size(Vector2i(750, 350))
		
		# remove hide from pressing cancel button
		for connection in get_cancel_button().pressed.get_connections():
			get_cancel_button().pressed.disconnect(connection.callable)
		
		get_cancel_button().pressed.connect(_ask_confirm)
		
		borderless = true
	
	func _on_timer_timeout():
		wait_timer.stop()
	
	func _process(_delta):
		if !setted_size:
			set_size(Vector2i(750, 350))
			popup_centered()
			setted_size = true
		
		if !wait_timer.is_stopped():
			get_ok_button().disabled = true
			set_ok_button_text(str("Please wait %d" % round(wait_timer.time_left)))
		else:
			get_ok_button().disabled = false
			set_ok_button_text("Accept")
	
	func _ask_confirm():
		
		var confirm: ConfirmationDialog = ConfirmationDialog.new()
		confirm.set_title("Are you sure?")
		confirm.set_text("If you continue, all permissions will be refused.\nDon't worry, you can change that at any moment in the settings.")
		
		confirm.confirmed.connect(_on_cancel)
		
		confirm.confirmed.connect(
			func():
				confirm.queue_free()
		)
		confirm.canceled.connect(
			func():
				confirm.queue_free()
		)
		
		add_child(confirm)
		confirm.popup_centered()
	
	func _on_cancel():
		for perm in perms:
			perm.allowed = false
		
		hide()
		cancel.emit()
	
	func _on_toggle_option(toggle: bool, perm_obj: ResPermissionObject):
		
		if toggle:
			perm_obj.allowed = true
			#allowed.push_back(perm_obj)
		else:
			perm_obj.allowed = false
			#if allowed.has(perm_obj):
				#allowed.erase(perm_obj)
				#perm_obj.allowed = false
	
	func _create_option(perm_obj: ResPermissionObject, perm_name: String, perm_description: String) -> VBoxContainer:
		
		var container: VBoxContainer = VBoxContainer.new()
		
		var hbox_container: HBoxContainer = HBoxContainer.new()
		container.add_child(hbox_container)
		
		var label_name: Label = Label.new()
		hbox_container.add_child(label_name)
		label_name.set_text(perm_name)
		label_name.set_h_size_flags(Control.SIZE_EXPAND_FILL)
		
		var toggle_perm: CheckBox = CheckBox.new()
		hbox_container.add_child(toggle_perm)
		toggle_perm.flat = true
		toggle_perm.set_pressed_no_signal(true)
		toggle_perm.add_theme_stylebox_override('focus', StyleBoxEmpty.new())
		toggle_perm.toggled.connect(_on_toggle_option.bind(perm_obj))
		
		if !perm_description.is_empty():
			var label_description: Label = Label.new()
			container.add_child(label_description)
			label_description.set_text(perm_description)
			label_description.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		
		var h_separator: HSeparator = HSeparator.new()
		container.add_child(h_separator)
		
		return container

class dialog_high extends ConfirmationDialog:
	
	signal cancel()
	
	var setted_size: bool = false
	
	var perms: Array = []
	var allowed: Array = []
	
	var wait_timer: Timer
	
	var main_container: VBoxContainer
	
	var upper_text: Label
	
	var background_container: PanelContainer
	var margin_container: MarginContainer
	var scroll_container: ScrollContainer
	var vbox_container: VBoxContainer
	
	var check_text_label: Label
	var check_text_input: LineEdit
	
	func _init(_perms: Array, plugin_obj: ResPlugin):
		
		perms = _perms
		allowed = _perms
		
		set_title(str("High-level permissions confirmation"))
		
		wait_timer = Timer.new()
		wait_timer.autostart = true
		wait_timer.set_wait_time(15)
		wait_timer.timeout.connect(_on_timer_timeout)
		add_child(wait_timer)
		
		main_container = VBoxContainer.new()
		add_child(main_container)
		
		upper_text = Label.new()
		main_container.add_child(upper_text)
		upper_text.set_text(
			str(
			"\"%s\" ask for the following high-level permission." % plugin_obj.name,
			"\nHigh-level permission could be a risk for your project AND your computer, so check before accepting!!!"
			)
		)
		upper_text.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		upper_text.add_theme_font_size_override('font_size', 14)
		
		background_container = PanelContainer.new()
		main_container.add_child(background_container)
		background_container.set_h_size_flags(Control.SIZE_EXPAND_FILL)
		background_container.set_v_size_flags(Control.SIZE_EXPAND_FILL)
		
		margin_container = MarginContainer.new()
		background_container.add_child(margin_container)
		margin_container.add_theme_constant_override("margin_top", 4)
		margin_container.add_theme_constant_override("margin_left", 4)
		margin_container.add_theme_constant_override("margin_bottom", 4)
		margin_container.add_theme_constant_override("margin_right", 4)
		
		scroll_container = ScrollContainer.new()
		margin_container.add_child(scroll_container)
		scroll_container.set_v_size_flags(Control.SIZE_EXPAND_FILL)
		
		vbox_container = VBoxContainer.new()
		scroll_container.add_child(vbox_container)
		vbox_container.set_h_size_flags(Control.SIZE_EXPAND_FILL)
		
		for perm: ResPermissionObject in perms:
			perm.allowed = true
			vbox_container.add_child(_create_option(perm, perm.path, perm.description))
		set_min_size(Vector2i(750, 450))
		
		main_container.add_child(HSeparator.new())
		
		check_text_label = Label.new()
		main_container.add_child(check_text_label)
		check_text_label.set_text("As those permission could put your computer at risk, please write the following text in the input text below to confirm (case-sensitive):\nI Accept The Risk")
		check_text_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		check_text_label.add_theme_font_size_override('font_size', 14)
		
		check_text_input = LineEdit.new()
		main_container.add_child(check_text_input)
		check_text_input.set_placeholder("Write \"I Accept The Risk\" (case-sensitive)")
		
		# remove hide from pressing cancel button
		for connection in get_cancel_button().pressed.get_connections():
			get_cancel_button().pressed.disconnect(connection.callable)
		
		get_cancel_button().pressed.connect(_ask_confirm)
		
		borderless = true
	
	func _on_close_requested():
		pass
	
	func _on_timer_timeout():
		wait_timer.stop()
	
	func _process(_delta):
		if !setted_size:
			set_size(Vector2i(750, 350))
			popup_centered()
			setted_size = true
		
		if !wait_timer.is_stopped():
			get_ok_button().disabled = true
			set_ok_button_text(str("Please wait %d" % round(wait_timer.time_left)))
		else:
			if check_text_input.get_text() == "I Accept The Risk": 
				get_ok_button().disabled = false
				set_ok_button_text("Accept")
			else:
				get_ok_button().disabled = true
				set_ok_button_text("Waiting for user confirmation...")
	
	func _ask_confirm():
		
		var confirm: ConfirmationDialog = ConfirmationDialog.new()
		confirm.set_title("Are you sure?")
		confirm.set_text("If you continue, all permissions will be refused.\nDon't worry, you can change that at any moment in the settings.")
		
		confirm.confirmed.connect(_on_cancel)
		
		confirm.confirmed.connect(
			func():
				confirm.queue_free()
		)
		confirm.canceled.connect(
			func():
				confirm.queue_free()
		)
		
		add_child(confirm)
		confirm.popup_centered()
	
	func _on_cancel():
		for perm in perms:
			perm.allowed = false
		
		hide()
		cancel.emit()
	
	func _on_toggle_option(toggle: bool, perm_obj: ResPermissionObject):
		
		if toggle:
			perm_obj.allowed = true
			#allowed.push_back(perm_obj)
		else:
			perm_obj.allowed = false
			#if allowed.has(perm_obj):
				#allowed.erase(perm_obj)
				#perm_obj.allowed = false
	
	func _create_option(perm_obj: ResPermissionObject, perm_name: String, perm_description: String) -> VBoxContainer:
		
		var container: VBoxContainer = VBoxContainer.new()
		
		var hbox_container: HBoxContainer = HBoxContainer.new()
		container.add_child(hbox_container)
		
		var label_name: Label = Label.new()
		hbox_container.add_child(label_name)
		label_name.set_text(perm_name)
		label_name.set_h_size_flags(Control.SIZE_EXPAND_FILL)
		
		var toggle_perm: CheckBox = CheckBox.new()
		hbox_container.add_child(toggle_perm)
		toggle_perm.flat = true
		toggle_perm.set_pressed_no_signal(true)
		toggle_perm.add_theme_stylebox_override('focus', StyleBoxEmpty.new())
		toggle_perm.toggled.connect(_on_toggle_option.bind(perm_obj))
		
		if !perm_description.is_empty():
			var label_description: Label = Label.new()
			container.add_child(label_description)
			label_description.set_text(perm_description)
			label_description.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		
		var h_separator: HSeparator = HSeparator.new()
		container.add_child(h_separator)
		
		return container

class dialog_auto_start extends ConfirmationDialog:
	
	func _init(plugin_obj: ResPlugin):
		
		borderless = true
		
		set_title("Auto-start request")
		set_text("The plugin \"%s\" ask to be started, do you allow it?\nIf you refuse, the plugin could still be started in the settings of the software.\n\nPlugin description:\n%s" % [plugin_obj.name, plugin_obj.description])
		
		set_min_size(Vector2i(250, 200))
		
		confirmed.connect(_on_accept.bind(plugin_obj))
		canceled.connect(_on_refuse)
	
	func _process(_delta):
		set_size(Vector2i(250, 200))
		set_process(false)
	
	func _on_accept(plugin_obj: ResPlugin):
		
		plugin_obj.toggle.emit(true)
		
		pass
	
	func _on_refuse():
		
		queue_free()
		
		pass

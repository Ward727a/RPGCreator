extends VBoxContainer

var data: Dictionary = {}

signal edit_pressed(id: String)
signal cat_pressed(id: String, shown: bool)

func add_lua_item(name: String, type: String, default_value: String, param_id: String, _parent: Control = null):
	
	var object: LuaItemParameters = LuaItemParameters.new()
	
	object.type = type
	object.default_value = default_value.replace('\r', '\\r').replace('\n', '\\n').replace('\t', '\\t')
	object.text_content = name
	object.param_id = param_id
	
	var node = object.get_node()
	var id = object.get_id()
	
	if _parent == null:
		data[id] = {
			"object": object
		}
		add_child(node)
	else:
		
		if _parent.has_meta('id'):
			var parent_id: String = _parent.get_meta('id')
			
			if !data.has(parent_id):
				return
			
			data[parent_id].elements.push_back(
				{
					"object": object,
					"id": id
				}
			)
		
		_parent.add_child(node)

func add_category(name: String) -> VBoxContainer:
	
	var object: LuaCategory = LuaCategory.new()
	var node = object.get_node()
	
	object.title_text = name
	
	add_child(node)
	
	object.top_title.gui_input.connect(_on_click_top_container.bind(object.content_container))
	
	var cat_id = object.get_id()
	
	data[cat_id] = {
		"object": object,
		"elements": []
	}
	
	return object.get_parent()

func add_soft_item(name: String, type: String, default_value: String, param_id: String, _parent: Control = null):
	
	var object: SoftItemParameters = SoftItemParameters.new()
	
	object.type = type
	object.default_value = default_value.replace('\r', '\\r').replace('\n', '\\n').replace('\t', '\\t')
	object.text_content = name
	object.param_id = param_id
	
	var node = object.get_node()
	var id = object.get_id()
	
	if _parent == null:
		data[id] = {
			"object": object
		}
		object.edit.pressed.connect(_on_edit_pressed.bind(id))
		add_child(node)
	else:
		if _parent.has_meta('id'):
			var parent_id: String = _parent.get_meta('id')
			
			if !data.has(parent_id):
				return
			
			data[parent_id].elements.push_back(
				{
					"object": object,
					"id": id
				}
			)
			
			object.edit.pressed.connect(_on_edit_pressed.bind(str(parent_id, '/', id)))
		
		_parent.add_child(node)

func _on_edit_pressed(id: String):
	edit_pressed.emit(id)

func _on_click_top_container(event, content_container):
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
			if content_container.visible:
				content_container.hide()
				cat_pressed.emit(content_container.get_meta('id'), false)
				return
			content_container.show()
			cat_pressed.emit(content_container.get_meta('id'), true)

class LuaCategory:
	var body: VBoxContainer
	var top_container: HBoxContainer
	var top_title: Label
	var content_container: VBoxContainer
	var scroller: ScrollContainer
	var bar: VSeparator
	var style_container: HFlowContainer
	var id: String
	
	var title_text: String:
		set(new_value):
			top_title.set_text(new_value)
			title_text = new_value
	
	func _init():
		
		# Create the unique id of the object
		id = IdGenerator.new().generate_small_id()
		
		# Create each node
		body = VBoxContainer.new()
		top_container = HBoxContainer.new()
		top_title = Label.new()
		content_container = VBoxContainer.new()
		scroller = ScrollContainer.new()
		style_container = HFlowContainer.new()
		bar = VSeparator.new()
		
		# Edit the node with the specific parameters
		scroller.size_flags_vertical = Control.SIZE_EXPAND_FILL
		scroller.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		
		style_container.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		
		bar.size_flags_vertical = Control.SIZE_EXPAND_FILL
		
		body.add_child(top_container)
		body.add_child(style_container)
		style_container.add_child(bar)
		style_container.add_child(content_container)
		
		top_container.add_child(top_title)
		
		top_title.mouse_filter = Control.MOUSE_FILTER_STOP
		top_title.mouse_default_cursor_shape = Control.CURSOR_POINTING_HAND
		
		content_container.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		content_container.set_meta('id', id) # Set the id to the metadata of the container
		content_container.hide() # Hide the container so when the title is pressed, the container is shown and vice-versa
	
	func get_id() -> String:
		return id
	
	func get_node():
		return body
	
	func get_parent():
		return content_container

class LuaItemParameters:
	
	const LUA_icon_path: String = "res://Core/Assets/icons/Lua.svg"
	
	var id: String
	var param_id: String
	
	var container: HBoxContainer
	var text: RichTextLabel
	var type: String = ""
	var edit: LineEdit
	
	var default_value: String = "":
		set(new_value):
			if new_value is String:
				edit.set_text(new_value)
				default_value = new_value
	
	var text_content: String = "":
		set(new_value):
			if new_value is String:
				text.set_text("[img]%s[/img] [font_size=12]%s - %s" %[LUA_icon_path, new_value, type])
				text_content = new_value
	
	func _init():
		
		id = IdGenerator.new().generate_small_id()
		container = HBoxContainer.new()
		text = RichTextLabel.new()
		text.bbcode_enabled = true
		text.fit_content = true
		text.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		
		edit = LineEdit.new()
		edit.editable = false
		edit.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		edit.set_meta('id', id)
		
		container.set_meta('id', id)
		
		container.add_child(text)
		container.add_child(edit)
	
	func get_id() -> String:
		return id
	
	func get_node():
		return container

class SoftItemParameters:
	
	var id: String = ""
	var param_id: String
	
	var container: HBoxContainer
	var text: RichTextLabel
	var type: String = ""
	var edit: Button
	var default_value: String = ""
	
	var text_content: String = "":
		set(new_value):
			if new_value is String:
				text.set_text("[font_size=12]%s - %s" %[new_value, type])
				edit.set_meta('key', text_content)
				text_content = new_value
	
	func _init():
		
		id = IdGenerator.new().generate_small_id()
		container = HBoxContainer.new()
		text = RichTextLabel.new()
		text.bbcode_enabled = true
		text.fit_content = true
		text.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		
		edit = Button.new()
		edit.set_text('EDIT_PARAM')
		edit.set_meta('id', id)
		
		container.set_meta('id', id)
		
		container.add_child(text)
		container.add_child(edit)
	
	func get_id() -> String:
		return id
	
	func get_node():
		return container

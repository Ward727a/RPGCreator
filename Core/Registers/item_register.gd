class_name ItemRegister

static var data: Dictionary = {}

static func add_item(item: Item) -> bool:
	
	var id = item.id
	
	if has_item(item):
		return false
	
	data[id] = item
	
	return true

static func delete_item(item_id: String) -> bool:
	
	if !has_item(item_id):
		return false
	
	EventsBus.EVENTS_Item.delete_item.emit(item_id)
	
	return data.erase(item_id)

static func set_item(item_id: String, item: Item) -> bool:
	
	if !has_item(item):
		return false
	
	data[item_id] = item
	
	return true

static func get_item(item_id: String) -> Item:
	
	if !has_item(item_id):
		return null
	
	return data[item_id]

static func edit_item(item_id: String, edited_data: Dictionary) -> bool:
	
	if !has_item(item_id):
		return false
	
	var item: Item = get_item(item_id)
	
	for edit_key in edited_data.keys():
		var edit_data = edited_data[edit_key]
		
		if item.get(edit_key) != null:
			
			if typeof(item.get(edit_key)) != typeof(edit_data):
				continue
			
			item.set(edit_key, edit_data)
	
	return true

static func edit_item_parameters(item_id: String, edited_parameters: Dictionary, category: String) -> bool:
	
	if !has_item(item_id):
		push_error("Item %s doesn't exist!" % item_id)
		return false
	
	var item: Item = get_item(item_id)
	var param_data: Dictionary = item.parameters
	
	if !param_data.has(category):
		push_error('Item parameters don\'t have %s category' % category)
		return false
	
	for edit_key in edited_parameters.keys():
		var edit_data = edited_parameters[edit_key]
		
		if !param_data[category]['parameters'].has(edit_key):
			push_error('Category dont have parameter %s' % edit_key)
			continue
		
		if typeof(param_data[category]['parameters'][edit_key]['default']) != typeof(edit_data):
			push_error('Not same value type between default and edit_value')
			continue
		
		param_data[category]['parameters'][edit_key]['value'] = edit_data
		
		# Signal the edited parameters of the new value
		ItemParametersRegister.call_event(ItemParametersRegister.PARAMETER_EVENT.ON_EDIT_FROM_UI, item_id, edit_key, edit_data, category)
		
	return true

static func has_item(item) -> bool:

	var id: String = ""

	if item is Item:
		id = item.id
	elif typeof(item) == TYPE_STRING:
		id = item
	else:
		push_error("id given not valid")
		return false
	
	if id == "":
		push_error("id given is empty.")
		return false
	
	return data.has(id)

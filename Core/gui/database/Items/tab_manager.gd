extends TabContainer

var param_ui_list: Dictionary = {}

# Called when the node enters the scene tree for the first time.
func _ready():
	pass
	#var EF = EngineFolder.new()
	#
	#await EF.load_items_parameters()
	#await EF.load_ui()
	#
	#var categories = ItemParametersRegister.get_categories()
	#
	#for category in categories:
		#var category_data = ItemParametersRegister.get_category(category)
		#var category_ui_key = category_data['ui_key']
		#
		#if !UIRegister.has_ui(category_ui_key):
			#push_error("Can't load category '%s' because the ui_key (%s) does not exist in the UI Register, did you create it?" % [category, category_ui_key])
			#continue
		#
		#var ui = UIRegister.get_ui(category_ui_key)
		#
		#ui.set_name(category)
		#
		#ui.ready.connect(ui_ready_category.bind(category))
		#
		#for param_key: String in category_data['parameters']:
			#
			#var param_data = ItemParametersRegister.get_parameter(param_key, category)
			#var param_generate_ui: bool = param_data['generate_ui'] if param_data.has('generate_ui') else true
			#var param_ui_key: String = param_data['ui_key']
			#
			#if !param_generate_ui:
				#
				#if !param_ui_list.has(param_ui_key):
					#continue
				#
				#continue
			#
			#if !UIRegister.has_ui(param_ui_key):
				#push_warning("The parameter '%s' of category '%s' UI key \"%s\" does not exist in the UI Register." % [param_key, category, param_ui_key])
				#continue
			#
			#var param_ui = UIRegister.get_ui(param_ui_key)
			#
			#param_ui.ready.connect(ui_ready_parameter.bind(category, param_key, param_ui))
			#
			#ui.add_child(param_ui)
			#
			#param_ui_list[param_ui_key] = {
				#"node": param_ui
			#}
		#
		#add_child(ui)
	#
	#pass # Replace with function body.
#
### Call the lua function linked to the ready event
#func ui_ready_category(cat_name: String):
	#
	#var cat_data = ItemParametersRegister.get_category(cat_name)
	#
	#if !cat_data.has('events'):
		#return
	#
	#if cat_data['events'].has('ON_LOAD_UI'):
		#for fn: Callable in cat_data['events']['ON_LOAD_UI']:
			#fn.call(cat_data['parameters'])
#
#func ui_ready_parameter(cat_name: String, param_name: String, ui: Control):
	#
	#var param_data = ItemParametersRegister.get_parameter(param_name, cat_name)
	#
	#if !param_data.has('events'):
		#return
	#
	#if param_data['events'].has('ON_LOAD_UI'):
		#for fn: Callable in param_data['events']['ON_LOAD_UI']:
			#fn.call(ui)
#
	#print(ItemParametersRegister.get_parameter('durability', 'General')['type'])

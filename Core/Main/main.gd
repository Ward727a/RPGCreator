extends Node
class_name Main

static var singleton: Main

var _in_crash_process: bool = false
var _close_requested: bool = false

var plugin_ready_count: int = 0

static func get_singleton() -> Main:
	return singleton

static func is_valid() -> bool:
	return (Main.get_singleton() != null)

func _init():
	singleton = self

	add_child(GlobalEventManager.new())
	add_child(Debugger.new())
	add_child(Settings.new())
	
	add_child(GodotManager.new())
	add_child(InterfaceManager.new())
	
	add_child(DataManager.new())
	DataManager.get_singleton().load_data()
	
	add_child(AssetManager.new())
	AssetManager.get_singleton().load_data()
	
	add_child(ProjectManager.new())
	add_child(PermissionManager.new())
	add_child(PluginManager.new())
	
	GlobalEventManager.get_singleton().SET("main/quit", _on_quit)
	tree_exiting.connect(_on_tree_exiting)
	child_exiting_tree.connect(_on_child_exiting_tree)

func _on_quit():
	notification(NOTIFICATION_WM_CLOSE_REQUEST)
	_close_requested = true
	get_tree().quit(0)

func _ready():
	
	# Create a simple control that will contains all the futur UI
	# We create a new one because if the user replace the parent of this node (Main)
	# The software will stop itself because the Main node will be removed.
	#
	# With this new node, the user can replace as he want, it will not give any
	# error.
	var main_control = Control.new()
	add_sibling.call_deferred(main_control)
	main_control.set_name("main control")
	main_control.set_anchors_preset(Control.PRESET_FULL_RECT)
	main_control.offset_bottom = 0
	main_control.offset_top = 0
	main_control.offset_left = 0
	main_control.offset_right = 0
	
	if main_control != null:
		InterfaceManager.get_singleton().create_ui("main", main_control)
	
	PluginManager.get_singleton().plugin_ready.connect(_on_plugin_ready)
	PluginManager.get_singleton().VERIFY_PLUGINS()

func _add_child(child: Node, force_readable_name: bool = false, internal: InternalMode = 0):
	get_parent().add_child.call_deferred(child, force_readable_name, internal)

func _on_plugin_ready(plugin_name):
	
	plugin_ready_count += 1
	
	var plugin: ResPlugin = PluginManager.get_singleton().GET(plugin_name)
	
	if plugin != null:
		if Settings.get_singleton().SettingsData.base_plugin_folder.has(plugin.main_folder.rsplit("/", true, 1)[1]):
			
			plugin.toggle.emit(true)
			return
		
		if plugin.auto_start:
			plugin.auto_started.emit()
	

func _notification(what):
	if what == NOTIFICATION_WM_CLOSE_REQUEST:
		# We set this so the node doesn't show an error when closing software
		_close_requested = true

# Make so the program save data and stop itself before something bad happen to the user project
func _on_tree_exiting():
	
	if _close_requested:
		return
	
	if _in_crash_process:
		return
	_in_crash_process = true
	OS.alert("RPG Creator has detected that one of the main node is being deleted.For security reason, the software will stop itself and save your data!", "Error!")
	
	push_error("!!!   THE MAIN NODE IS BEING REMOVED      !!!")
	push_error("!!!   SAVING DATA AND STOPPING SOFTWARE   !!!")
	DataManager.get_singleton().save_data()
	GlobalEventManager.get_singleton().CALL("main/crash")
	
	get_tree().root.propagate_notification(NOTIFICATION_CRASH)
	get_tree().root.propagate_notification(NOTIFICATION_WM_CLOSE_REQUEST)
	
	get_tree().quit(1)


# Make so the program recreate the node that has been removed
func _on_child_exiting_tree(_child: Node):
	
	if _close_requested:
		return
	if _in_crash_process:
		return
	
	_in_crash_process = true
	OS.alert("RPG Creator has detected that one of the main node is being deleted.\n\nFor security reason, the software will stop itself and save your data!", "Error!")
	
	
	push_error("!!! ONE OF THE MAIN NODE IS BEING REMOVED !!!")
	push_error("!!! SAVING DATA AND STOPPING SOFTWARE     !!!")
	
	DataManager.get_singleton().save_data()
	if GlobalEventManager.get_singleton() != null:
		GlobalEventManager.get_singleton().CALL("main/crash")
	
	get_tree().root.propagate_notification(NOTIFICATION_CRASH)
	get_tree().root.propagate_notification(NOTIFICATION_WM_CLOSE_REQUEST)
	
	get_tree().quit(1)

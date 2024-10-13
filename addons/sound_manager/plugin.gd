@tool
extends EditorPlugin

# Variables

var dock : Node
var sound_type_change_requested := ""

################

# Signals

signal check_file_names_requested()
signal file_names_updated(file_names)
signal signals_connected()

############

# Set the Sound Manager Module scene as autoload, instance a new dock scene and configure the EditorFileDialog instance 
func _enter_tree() -> void:
	add_autoload_singleton("SoundManager", "res://addons/sound_manager/module/SoundManager.tscn")
	dock = preload("res://addons/sound_manager/dock/SoundManagerDock.tscn").instantiate()
	dock.name = dock.TITLE
	add_control_to_dock(DOCK_SLOT_LEFT_UL, dock)
	_connect_signals()
	
	# Check for fylesystem changes
	get_editor_interface().get_resource_filesystem().filesystem_changed.connect(_on_filesystem_changed)


# Quit the Sound Manager Module scene as autoload, remove the dock scene and free the 'file_dialog' and 'dock' variables from memory
func _exit_tree() -> void:
	remove_autoload_singleton("SoundManager")
	remove_control_from_docks(dock)
	dock.free()


func _connect_signals() -> void:
	dock.check_file_names_requested.connect(_on_check_file_names_requested)
	file_names_updated.connect(dock._on_file_names_updated)
	signals_connected.connect(dock._on_plugin_signals_connected)
	emit_signal("signals_connected")


#############################
#	FILE SYSTEM HANDLERS	#
#############################

func _get_sound_file_names_from_path_r(path : String) -> PackedStringArray:
	var directory : EditorFileSystemDirectory = get_editor_interface().get_resource_filesystem().get_filesystem_path(path)
	var file_name := _get_sound_file_names_from_dir_r(directory)
#	directory.free()
	return file_name


func _get_sound_file_names_from_dir_r(directory : EditorFileSystemDirectory) -> PackedStringArray:
	if directory == null:
		return PackedStringArray([])
	var file_name = _get_sound_file_names_from_dir(directory)
	for i in range(0, directory.get_subdir_count()):
		var subdir = directory.get_subdir(i)
		if subdir.get_name() != "addons":
			file_name += _get_sound_file_names_from_dir_r(directory.get_subdir(i))
	return file_name


func _get_sound_file_names_from_dir(directory : EditorFileSystemDirectory) -> PackedStringArray:
	var file_names : PackedStringArray = []
	if directory:
		for i in range(0, directory.get_file_count()):
			var file_name = directory.get_file(i)
			if (file_name.get_extension() == "ogg" or
					file_name.get_extension() == "mp3" or
					file_name.get_extension() == "wav" or
					file_name.get_extension() == "opus"):
				file_name = directory.get_path() + file_name
				file_names.append(file_name)
	return file_names


func _check_file_names_from_paths() -> void:
	var file_names = _get_sound_file_names_from_path_r("res://")
	emit_signal("file_names_updated", file_names)


func _on_filesystem_changed() -> void:
	_check_file_names_from_paths()


func _on_check_file_names_requested() -> void:
	_check_file_names_from_paths()

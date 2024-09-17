extends Button

var user_data_folder: String = ProjectSettings.globalize_path("user://")

func _pressed():
	
	OS.shell_open(user_data_folder)

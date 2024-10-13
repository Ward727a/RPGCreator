extends Node
class_name Settings

static var singleton: Settings

static func get_singleton() -> Settings:
	return singleton

static func is_valid() -> bool:
	return (Settings.get_singleton() != null)

var logger: Logger = Logger.new("Settings")

const software_path: String = "res://Assets/Resources/software_info.tres"
const update_path: String = "res://Assets/Resources/update_log.tres"
const setting_path: String = "res://Assets/Resources/settings.tres"

var SoftwareInfo: ResSoftwareInfo
var UpdateLog: ResUpdateLog
var SettingsData: ResSettingsData

func _init():
	
	if !_load_project_info():
		return
	
	if !_load_project_update_log():
		return
	
	if !_load_setting():
		return
	
	singleton = self

func _load_project_info():
	
	if !FileAccess.file_exists(software_path):
		logger.error("Software info path is not valid! %s not exists!" % software_path)
		return false
	
	SoftwareInfo = load(software_path)
	return true

func _load_project_update_log():
	
	if !FileAccess.file_exists(update_path):
		logger.error("Update logs path is not valid! %s not exists!" % update_path)
		return false
	
	UpdateLog = load(update_path)
	return true

func _load_setting():
	
	if !FileAccess.file_exists(setting_path):
		logger.error("Settings path is not valid! %s not exists!" % setting_path)
		return false
	
	SettingsData = load(setting_path)
	return true
	

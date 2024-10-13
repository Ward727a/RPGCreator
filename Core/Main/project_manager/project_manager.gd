extends Node
class_name ProjectManager

static var singleton: ProjectManager

static func get_singleton() -> ProjectManager:
	return singleton

static func is_valid() -> bool:
	return (ProjectManager.get_singleton() != null)

var logger: Logger = Logger.new("ProjectManager")

var actual_project: ResProject
var DM: DataManager

func _init():
	singleton = self
	
	if !DataManager.is_valid():
		logger.error("Can't access DataManager, the software can't work!")
		queue_free()
	
	if !Settings.is_valid():
		logger.error("Can't access Settings, the software can't work!")
		queue_free()
	
	DM = DataManager.get_singleton()

func _os_get_username() -> String:
	
	match OS.get_name():
		"Windows":
			return OS.get_environment("USERNAME")
		"Linux", "macOS", "FreeBSD", "NetBSD", "OpenBSD", "BSD":
			return OS.get_environment("USER")
		_:
			if OS.get_environment("USER").is_empty():
				logger.warn("OS \"%s\" not supported for automatic username retrieval." % OS.get_name())
				return "username"
			logger.warn("OS \"%s\" is not supported, but the automatic username retrieval will still work." % OS.get_name())
			return OS.get_environment("USER")

func CREATE(project_name: String):
	
	var project_data = ResProject.new()
	
	project_data.project_id = IDGenerator.generate_id(12)
	
	project_data.project_name = project_name
	project_data.project_author = _os_get_username()
	project_data.project_last_edit = "Never opened"
	project_data.project_version_created = Settings.get_singleton().SoftwareInfo.version
	
	DM.SET('editor/projects/%s' % project_data.project_id, project_data)

func LOAD(project_id: String):
	
	if !DM.HAS('editor/projects/%s' % project_id):
		logger.error("Project with id %s can't be found in the DataManager." % project_id)
		OS.alert("Project id is invalid, please contact us.", "Project ID not found!")
		return null
	
	var project = DM.GET('editor/projects/%s' % project_id)
	
	if typeof(project) == TYPE_NIL:
		logger.error("Project with id %s is null in the DataManager!" % project_id)
		OS.alert("Project is empty in the DataManager, please contact us.", "Project found but NULL!")
		return null
	
	if !(project is ResProject):
		logger.error("Project %s is not a valid ResProject" % project_id)
		logger.error("Found data: %s" % project)
		OS.alert("Project found but data is invalid, please contact us.", "Project not ResProject!")
		return null
	
	if actual_project != null:
		SAVE(actual_project)
	
	project.last_edited = Time.get_datetime_string_from_system(false, true)
	actual_project = project

func SAVE(project_data: ResProject):
	DM.SET('editor/projects/%s' % project_data.project_id, project_data)

func link_plugin(_plugin_obj: ResPlugin):
	
	var fields = []
	
	var possible_perm = [
		"CREATE",
		"LOAD",
		"SAVE"
	]
	
	for perm in possible_perm:
		if _plugin_obj.has_allowed("PROJECT_MANAGER"):
			continue
		
		if !_plugin_obj.has_allowed(str("PROJECT_MANAGER:%s" % perm)):
			fields.push_back(perm)
	
	if fields.size() < possible_perm.size():
		var lua_obj = _lua_class.new(fields)
		
		_plugin_obj.set_link("ProjectManager", lua_obj)

class _lua_class:
	
	var fields = []
	var singleton: ProjectManager
	
	func _init(_fields: Array):
		fields = _fields
		fields.push_back('singleton')
		singleton = ProjectManager.get_singleton()
	
	func lua_fields():
		return fields
	
	func is_valid() -> bool:
		return singleton != null
	
	func reload_singleton() -> bool:
		singleton = ProjectManager.get_singleton()
		return is_valid()
	
	func CREATE(project_name: String):
		singleton.CREATE(project_name)
	
	func LOAD(project_id: String):
		singleton.LOAD(project_id)
	
	func SAVE(project_data: ResProject):
		singleton.SAVE(project_data)
	

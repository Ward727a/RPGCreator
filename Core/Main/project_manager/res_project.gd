extends Resource
class_name ResProject

@export var project_id: String

# info
@export var project_name: String
@export var project_author: String
@export var project_last_edit: String
@export var project_picture: ResAssets
@export var project_version_created: String

@export var project_enabled_plugins: PackedStringArray = []
@export_placeholder("This type should be a custom resource type")
var project_settings: String = "This type should be a custom resource"

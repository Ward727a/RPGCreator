extends Resource
class_name ResUpdateLog

@export var logs: Array[ResLog] = []

class ResLog extends Resource:
	@export var order: int
	@export var title: String = ""
	@export var version: String = ""
	@export var sub_title: String = ""
	@export_multiline var content: String = ""

extends Node
class_name Debugger

signal debug_loop()

static var singleton: Debugger

static func get_singleton() -> Debugger:
	return singleton

func _init():
	singleton = self

func _process(_delta):
	
	debug_loop.emit()

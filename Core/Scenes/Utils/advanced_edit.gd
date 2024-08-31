extends MarginContainer
class_name AdvancedEdit

const path_to_scene: String = "res://Core/Scenes/Utils/advanced_edit.tscn"

signal text_changed(content: String)

@onready var text_edit: TextEdit = $VBoxContainer/TextEdit

func _init():

	var content = preload(path_to_scene).instantiate().get_child(0).get_child(0).duplicate()

	add_theme_constant_override("margin_left", 4)
	add_theme_constant_override("margin_top", 4)
	add_theme_constant_override("margin_right", 4)
	add_theme_constant_override("margin_bottom", 4)

	add_child(content)

func _ready():
	text_edit.connect("text_changed", _on_text_changed)

func get_content() -> String:
	return text_edit.get_text()

func set_content(content: String) -> void:
	text_edit.set_text(content)

func _on_text_changed(content: String):
	text_changed.emit(content)
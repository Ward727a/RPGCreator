extends RefCounted
class_name CustomListTemplateRow

@export var row_name: String = "Row":
    set(new):
        row_name = tr(new)
    
@export var row_type: List.ItemType = List.ItemType.TEXT

@export var row_metadata: Dictionary = {}
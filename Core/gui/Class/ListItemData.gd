class_name ListItem

enum ItemType {
    TEXT,
    IMAGE,
    BUTTON,
    CHECKBOX
}

var name: String = ""
var item_metadata: Dictionary = {}

const scene_path: String = "res://Core/Scenes/database/CustomListItem.tscn"

var item_type: Array = []
var content: Array = []


func _init(_name: String, structure: Array):

    name = _name

    for i in structure:

        print(i)

        if i >= 0 and i < 5:
            item_type.append(i)
        else:
            push_error("Invalid item type")
            return

func set_content(_content: Array):

    content.clear()

    if _content.size() != item_type.size():
        push_error("Invalid content size")
        return

    for i in _content.size():

        var object = _content[i]
        var type = item_type[i]

        match type:
            ItemType.TEXT:
                if typeof(object) == TYPE_STRING:
                    content.append(object)
                else:
                    push_error("Invalid content type")
                    return
            ItemType.IMAGE:
                if typeof(object) == TYPE_STRING:
                    if FileAccess.file_exists(object):
                        content.append(object)
                else:
                    push_error("Invalid content type")
                    return
            ItemType.BUTTON:
                if typeof(object) == TYPE_STRING:
                    content.append(object)
                else:
                    push_error("Invalid content type")
                    return
            ItemType.CHECKBOX:
                if typeof(object) == TYPE_BOOL:
                    content.append(object)
                elif typeof(object) == TYPE_STRING:
                    content.append((object == "true"))
                elif typeof(object) == TYPE_INT:
                    content.append((object == 1))
                else:
                    push_error("Invalid content type")
                    return

func get_content() -> Array:
    return content

func remove_content_at(index: int):
    content.remove_at(index)
    item_type.remove_at(index)

func set_metadata(_metadata: Dictionary):
    item_metadata = _metadata

func add_metadata(key: String, value: Variant):
    item_metadata[key] = value

func add_metadatas(_metadata: Dictionary):
    for key in _metadata.keys():
        item_metadata[key] = _metadata[key]

func get_metadata() -> Dictionary:
    return item_metadata
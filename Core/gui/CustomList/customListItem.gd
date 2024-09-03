extends VBoxContainer

var metadata: Dictionary = {}
var content = []
var types = []
var item_name: String = ""
var base_theme: StyleBoxFlat
var node_item: HBoxContainer

signal clicked_on_item(node: Node, name: String, metadata: Dictionary)

func _ready():
    print("Hi")
    node_item = %itemContent

    get_node("Panel").clicked_on_item.connect(_click_on_panel)

func set_item_name(_name: String):
    item_name = _name

func set_item_content(_content: Array, _type: Array):
    content = _content
    types = _type

    for i in content.size():
        match types[i]:
            List.ItemType.TEXT:
                var label = Label.new()
                label.text = content[i]
                label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
                label.size_flags_vertical = Control.SIZE_SHRINK_CENTER
                node_item.add_child(label)
            List.ItemType.IMAGE:
                var texture = TextureRect.new()

                texture.texture = load(content[i])
                texture.size_flags_horizontal = Control.SIZE_EXPAND_FILL
                texture.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
                texture.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
                texture.custom_minimum_size = Vector2(128, 128)
                node_item.add_child(texture)
            List.ItemType.BUTTON:
                var button = Button.new()
                button.text = content[i]
                button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
                button.size_flags_vertical = Control.SIZE_SHRINK_CENTER
                node_item.add_child(button)
            List.ItemType.CHECKBOX:
                var checkbox:CheckBox = CheckBox.new()
                checkbox.button_pressed = content[i]
                checkbox.size_flags_horizontal = Control.SIZE_EXPAND_FILL
                checkbox.size_flags_vertical = Control.SIZE_SHRINK_CENTER
                node_item.add_child(checkbox)


func set_item_metadata(_metadata: Dictionary):
    metadata = _metadata

func _click_on_panel():
    clicked_on_item.emit(self, item_name, metadata)
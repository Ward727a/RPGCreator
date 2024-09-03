extends Panel
class_name CustomList

var list: VBoxContainer
var rows: HBoxContainer
var list_object: List

var list_node: Dictionary = {}

const itemList_NodePath: String = "res://Core/Scenes/database/custom_list_item.tscn"

signal item_clicked(node: Node, name: String, metadata: Dictionary)

var db_index_name: String = "char_idx"
var db_context: String = "box_entities"
var db_char_list_key: String = "characters_list"

var origin: String = "CustomList"

func connect_signal():

    DataRegister.data_registered.connect(_on_data_registered)

func set_db_data(_db_index_name: String, _db_context: String, _db_char_list_key: String):
    db_index_name = _db_index_name
    db_context = _db_context
    db_char_list_key = _db_char_list_key

func _init():
    list_object = List.new()
    connect_signal()

func _ready():

    rows = %row_cat
    list = %list_content

    var data: Dictionary = list_object.get_list_property()

    if data.size() == 0:
        return

    add_row("CHAR_NAME", List.ItemType.TEXT)
    add_row("CHAR_SURNAME", List.ItemType.TEXT)

# Add a row to the list
func add_row(row_text: String, row_type: List.ItemType, min_size: Vector2 = Vector2(0,0)):
    list_object.add_row(row_text, row_type)

    var row: Label = Label.new()
    row.text = row_text

    if min_size != Vector2(0,0):
        min_size = Vector2(min_size.x, 0)
        row.custom_minimum_size = min_size
    else:
        row.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    
    row.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER

    rows.add_child(row)

# Add an item to the list
# It returns the index of the item in the list and the engine index
# The index of the item SHOULD NOT be trusted, as it can change if an item is removed / added
# If you need to do something with the item, use the "engine index" instead, this index can't change and is unique to the item
func add_item(Content: Array, _metadata: Dictionary = {}):
    var idx = list_object.add_item(Content, "", _metadata)

    var item_object = list_object.get_list_property()['items'][idx]

    var item_name = item_object['name']
    var item_content = item_object['content']
    var item_metadata = item_object['metadata']

    item_metadata['engine_index'] = IdGenerator.new().generate_id() # This the main index that the engine will use to identify the item

    var node = preload(itemList_NodePath).instantiate()

    # Set the background color of the item
    var background: Panel = node.get_node("Panel")
    var bg_theme: StyleBoxFlat = background.get_theme_stylebox("panel").duplicate()

    # If the list has an even number of items, set the background color to a darker color
    if list_node.size() % 2 == 0:
        bg_theme.bg_color = Color(0.1, 0.1, 0.1, 1.0)
    else:
        bg_theme.bg_color = Color(0.135, 0.135, 0.135, 1.0)
    
    # Set the theme to the background
    background.add_theme_stylebox_override("panel",  bg_theme)

    list.add_child(node)

    node.set_item_name(item_name)
    node.set_item_content(item_content, list_object.get_list_property()['rows_type'])
    node.set_item_metadata(item_metadata)

    if DataRegister.has_key(db_context, db_char_list_key):
        var datas_list = DataRegister.get_data(db_context, db_char_list_key)

        var data_content = item_content
        data_content.append(item_metadata['engine_index'])

        datas_list[item_metadata[db_index_name]] = data_content

        DataRegister.register_data(db_context, db_char_list_key, datas_list, origin)
    else:
        var datas_list = {}

        var data_content = item_content
        data_content.append(item_metadata['engine_index'])

        datas_list[item_metadata[db_index_name]] = data_content

        DataRegister.register_data(db_context, db_char_list_key, datas_list, origin)
    
    print("DataRegisted: ", DataRegister.get_data(db_context, db_char_list_key))

    node.base_theme = bg_theme
    node.clicked_on_item.connect(_click_on_item)

    list_node[item_metadata['engine_index']] = node

    return {"index": idx, "engine_index": item_metadata['engine_index']}


# Remove an item from the list
func remove_item(engine_index: String):

    var item_index = -1

    for i in list_object.get_items():
        item_index += 1
        if i.get_metadata()['engine_index'] == engine_index:
            list_object.remove_item(item_index)
            list_node[engine_index].queue_free()
            list_node.erase(engine_index)
            return


####################
# SIGNALS
####################

func _on_data_registered(_context: String, _key: String, _value: Variant, _origin: String):

    if _origin == origin:
        return

    print("Data registered: ", _context, _key)


    if _context == db_context and _key == db_char_list_key:
        var datas_list = DataRegister.get_data(db_context, db_char_list_key)

        print(_key)

        # Erase all items
        for key in datas_list:

            # get last item in the content
            var last_item = datas_list[key].size() - 1
            var last_content = datas_list[key][last_item]

            # remove last item
            datas_list[key].erase(last_content)

            remove_item(last_content)
        
        # Add all items
        for key in datas_list:
            add_item(datas_list[key], {db_index_name: key})

func _click_on_item(node: Node, _name: String, _metadata: Dictionary):
    item_clicked.emit(node, _name, _metadata)
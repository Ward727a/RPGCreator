class_name List
# This is the base class for all List - This class will be used to edit / create custom List elements (mainly for the plugin system)

enum ItemType {
    TEXT,
    IMAGE,
    BUTTON,
    CHECKBOX
}

var rows: Array = []
var rows_type: Array = []
var list_metadata: Dictionary = {}
var items: Array = []


func add_row(row_text: String, row_type: ItemType):
    rows.append(row_text)
    rows_type.append(row_type)

func remove_row(row_text: String):
    var id = rows.find(row_text)


    if id != -1:
        rows.remove_at(id)
        rows_type.remove_at(id)
        if items.size() > 0:
            for i in items.size():
                items[i].remove_content_at(id)

func clear_rows():
    rows.clear()

func get_rows() -> Array:
    return rows

func add_item(item_content: Array, item_name: String = "", _item_metadata: Dictionary = {}) -> int:

    var listItem = ListItem.new(item_name, rows_type)

    listItem.set_content(item_content)
    listItem.add_metadatas(_item_metadata)

    items.append(listItem)

    return items.size() - 1

func remove_item(item_index: int):
    items.erase(item_index)

func clear_items():
    items.clear()

func get_items() -> Array:
    return items

## This function should return a dictionnary like this:
## [codeblock]
## {
##     "rows": ["Row 1", "Row 2", "Row 3", "Row 4"],
##     "rows_type": [ItemType.TEXT, ItemType.IMAGE, ItemType.BUTTON, ItemType.CHECKBOX],
##     "items": [
##         {
##             "name": "Item 1",
##             "content": ["Content 1", "Content 2", "Content 3", "Content 4"],
##             "metadata": { # Each metadata are optional, and unique to each item
##                 "key1": "value1",
##                 "key2": "value2",
##                 "key3": "value3"
##             }
##         },
##         {
##             "name": "Item 2",
##             "content": ["Content 1", "Content 2", "Content 3", "Content 4"]
##             "metadata": { # Each metadata are optional, and unique to each item
##                 "key1": "value1",
##                 "key2": "value2",
##                 "key3": "value3"
##             }
##         },
##         {
##             "name": "Item 3",
##             "content": ["Content 1", "Content 2", "Content 3", "Content 4"]
##             "metadata": { # Each metadata are optional, and unique to each item
##                 "key1": "value1",
##                 "key2": "value2",
##                 "key3": "value3"
##             }
##         }
##     ],
##     "metadata": { # This metadata is optional, and unique to the list (it will not be shared with the items)
##         "key1": "value1",
##         "key2": "value2",
##         "key3": "value3"
##     }
## }
## [/codeblock]
func get_list_property() -> Dictionary:
    var list_property = {
        "rows": rows,
        "rows_type": rows_type,
        "items": [],
        "metadata": list_metadata
    }

    for itemList in items:
        var item_property = {
            "name": itemList.name,
            "content": itemList.get_content(),
            "metadata": itemList.item_metadata
        }

        list_property["items"].append(item_property)

    return list_property


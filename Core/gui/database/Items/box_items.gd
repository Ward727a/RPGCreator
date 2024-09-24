extends HBoxContainer

var items_list = []
var edited_item: Item = null

# Called when the node enters the scene tree for the first time.
func _ready():
	EventsBus.EVENTS_Item.new_item.connect(_on_new_item)
	
	pass # Replace with function body.

func _on_item_list_create_pressed():
	
	var new_item: Item = Item.new()
	
	pass # Replace with function body.

func _on_new_item(item: Item):
	
	items_list.push_back(item.id)
	
	ItemRegister.add_item(item)
	
	var test_idx = %itemList.add_item({
		"item_name": item.name,
		"item_id": item.id
	})
	
	var item_node = %itemList.get_item(test_idx).template_node
	
	# Link event when item is clicked on in the list
	item_node.item_clicked.connect(_on_item_clicked)
	
	# Link event when item is deleted in the list
	item_node.delete_item.connect(_on_item_delete.bind(item_node))

func _on_item_clicked(item_id: String):
	
	print("clicked on item %s" % item_id)
	
	if ItemRegister.has_item(item_id):
		edited_item = ItemRegister.get_item(item_id)
		return
	
	push_error('Item ID: %s doesn\'t exist' % item_id)

func _on_item_delete(item_id: String, item_node):
	
	print("deleting item %s" % item_id)
	
	item_node.queue_free()
	
	ItemRegister.delete_item(item_id)

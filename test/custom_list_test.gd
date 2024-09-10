extends Control


# Called when the node enters the scene tree for the first time.
func _ready():

	%CustomList.add_row("Test", List.ItemType.TEXT)

	# Test option_button
	var options = ["Option 1", "Option 2", "Option 3"]
	%CustomList.add_row("Test", List.ItemType.OPTION_BUTTON)

	%CustomList.add_item(["Test", 0], { "options": options, "char_idx": 0 })

	%CustomList.option_chosen.connect(_on_option_chosen)

	pass # Replace with function body.


func _on_option_chosen(option: String, option_button: OptionButton):
	print("Option chosen: ", option)
	print("Option button: ", option_button)
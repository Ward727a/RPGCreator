extends TextEdit

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


var selection_text = ""

func get_caret_pos() -> Dictionary:

	print(get_caret_line())
	return {
		'y': get_caret_line(),
		'x': get_caret_column()
	}

var color = ""

func _notification(what):
	# If the textedit will lose focus
	if what == NOTIFICATION_FOCUS_EXIT:
		# Save the selection text
		selection_text = get_selected_text()

func get_text_as_array() -> Array:
	return get_text().split("\n")

func add_tag_to_selected(tag: String, caret_pos: Dictionary, end_tag: String ="") -> void:
	var _text = get_text_as_array()
	var selected_text = selection_text
	var array_key = caret_pos['y']
	var text_at = caret_pos['x']

	if end_tag == "":
		end_tag = tag
	

	_text[array_key] = _text[array_key].replace(selected_text, str("[", tag, "]",selected_text,"[/", end_tag, "]"))

	var new_text_string = '\n'.join(_text)

	set_text(new_text_string)
	set_caret_line(array_key)
	set_caret_column(text_at)

func remove_tag_to_selected(tag: String, caret_pos: Dictionary, end_tag: String = "") -> void:
	var _text = get_text_as_array()
	var selected_text = selection_text
	var array_key = caret_pos['y']
	var text_at = caret_pos['x']

	if end_tag == "":
		end_tag = tag

	# Replace the tag with an empty string
	var new_selected_text = selected_text.replace(str("[", tag, "]"), "")
	new_selected_text = new_selected_text.replace(str("[/", end_tag, "]"), "")

	# Replace the tag with an empty string
	_text[array_key] = _text[array_key].replace(selected_text, new_selected_text)

	var new_text_string = '\n'.join(_text)

	set_text(new_text_string)
	set_caret_line(array_key)
	set_caret_column(text_at)

func add_tag_to_pos(tag: String, caret_pos: Dictionary, end_tag: String ="", base_content: String = "") -> void:
	var _text = get_text_as_array()
	var array_key = caret_pos['y']
	var text_at = caret_pos['x']

	if end_tag == "":
		end_tag = tag
	
	_text[array_key] = _text[array_key].insert(text_at, str("[",tag,"]"))
	_text[array_key] = _text[array_key].insert(text_at + (str("[",tag,"]").length()), str(base_content, "[/",end_tag,"]"))

	var new_text_string = '\n'.join(_text)

	set_text(new_text_string)
	set_caret_line(array_key)
	set_caret_column(text_at + 3)

func _on_set_italic_pressed():

	# Get the caret position
	var caret_pos = get_caret_pos()

	if has_focus() == true:
		selection_text = get_selected_text()

	# check if some text is selected
	if selection_text != "":
		add_tag_to_selected("i", caret_pos)
	else:
		add_tag_to_pos("i", caret_pos)

func _on_set_bold_pressed(): # Tag => [b][/b]

	# Get the caret position
	var caret_pos = get_caret_pos()

	if has_focus() == true:
		selection_text = get_selected_text()

	# check if some text is selected
	if selection_text != "":
		add_tag_to_selected("b", caret_pos)
	else:
		add_tag_to_pos("b", caret_pos)

func _on_set_strikethrough_pressed(): # Tag => [s][/s]

	# Get the caret position
	var caret_pos = get_caret_pos()

	if has_focus() == true:
		selection_text = get_selected_text()

	# check if some text is selected
	if selection_text != "":
		add_tag_to_selected("s", caret_pos)
	else:
		add_tag_to_pos("s", caret_pos)

func _on_set_under_pressed(): # Tag => [u][/u]

	# Get the caret position
	var caret_pos = get_caret_pos()

	if has_focus() == true:
		selection_text = get_selected_text()

	# check if some text is selected
	if selection_text != "":
		add_tag_to_selected("u", caret_pos)
	else:
		add_tag_to_pos("u", caret_pos)

func _on_set_font_color_color_changed(_color:Color): # Tag => [color=#ffffff][/color]

	self.color = _color


func _on_set_size_value_changed(value:float): # Tag => [font_size=12][/font_size]

	# Get the caret position
	var caret_pos = get_caret_pos()

	if has_focus() == true:
		selection_text = get_selected_text()

	# check if some text is selected
	if selection_text != "":
		add_tag_to_selected(str("font_size=",value), caret_pos, "font_size")
	else:
		add_tag_to_pos(str("font_size=",value), caret_pos, "font_size")

func _on_set_fill_pressed(): # Tag => [fill][/fill]

	# Get the caret position
	var caret_pos = get_caret_pos()

	if has_focus() == true:
		selection_text = get_selected_text()

	# check if some text is selected
	if selection_text != "":
		add_tag_to_selected("fill", caret_pos)
	else:
		add_tag_to_pos("fill", caret_pos)

func _on_set_right_pressed(): # Tag => [right][/right]

	# Get the caret position
	var caret_pos = get_caret_pos()

	if has_focus() == true:
		selection_text = get_selected_text()

	# check if some text is selected
	if selection_text != "":
		add_tag_to_selected("right", caret_pos)
	else:
		add_tag_to_pos("right", caret_pos)

func _on_set_middle_pressed(): # Tag => [center][/center]

	# Get the caret position
	var caret_pos = get_caret_pos()

	if has_focus() == true:
		selection_text = get_selected_text()

	# check if some text is selected
	if selection_text != "":
		add_tag_to_selected("center", caret_pos)
	else:
		add_tag_to_pos("center", caret_pos)

func _on_set_left_pressed(): # Tag => [left][/left]

	# Get the caret position
	var caret_pos = get_caret_pos()

	if has_focus() == true:
		selection_text = get_selected_text()

	# check if some text is selected
	if selection_text != "":
		add_tag_to_selected("left", caret_pos)
	else:
		add_tag_to_pos("left", caret_pos)

func _on_set_hint_pressed(): # Tag => [hint={tooltip}][/hint]

	# Get the caret position
	var caret_pos = get_caret_pos()

	if has_focus() == true:
		selection_text = get_selected_text()
	
	# check if some text is selected
	if selection_text != "":
		add_tag_to_selected(str("hint=",selection_text), caret_pos, "hint")
	else:
		add_tag_to_pos(str("hint=replace this"), caret_pos, "hint")

	pass # Replace with function body.

func _on_set_img_pressed(): # Tag => [img={width}x{height}]{url}[/img]
	# Get the caret position
	var caret_pos = get_caret_pos()

	if has_focus() == true:
		selection_text = get_selected_text()
	
	# check if some text is selected
	if selection_text != "":
		add_tag_to_selected(str("img={width}x{height}"), caret_pos, "img")
	else:
		add_tag_to_pos(str("img={width}x{height}"), caret_pos, "img", "img_url")

func _on_set_link_pressed(): # Tag => [url={url}]{text}[/url] or [url]{url}[/url]
	# Get the caret position
	var caret_pos = get_caret_pos()

	if has_focus() == true:
		selection_text = get_selected_text()
	
	# check if some text is selected
	if selection_text != "":
		# Check if the selected text is a url
		if selection_text.find("http") != -1 or selection_text.find("www.") != -1:
			add_tag_to_selected(str("url=",selection_text), caret_pos, "url")
		else:
			add_tag_to_selected(str("url=replace this"), caret_pos, "url")
	else:
		add_tag_to_pos(str("url=replace_this"), caret_pos, "url")

func _on_set_font_pressed(): # Tag => [font={font}]{text to apply font}[/font]
	# Get the caret position
	var caret_pos = get_caret_pos()

	if has_focus() == true:
		selection_text = get_selected_text()
	
	# check if some text is selected
	if selection_text != "":
		add_tag_to_selected(str("font=replace this"), caret_pos, "font")
	else:
		add_tag_to_pos(str("font=replace_this"), caret_pos, "font")

func _on_set_back_color_color_changed(_color:Color): # Tag => [bgcolor=#ffffff][/back_color]

	self.color = _color

func _on_set_font_color_popup_closed():
	# Get the caret position
	var caret_pos = get_caret_pos()

	if has_focus() == true:
		selection_text = get_selected_text()

	# check if some text is selected
	if selection_text != "":
		add_tag_to_selected(str("color=",Utils.to_hex_color(color)), caret_pos, "color")
	else:
		add_tag_to_pos(str("color=",Utils.to_hex_color(color)), caret_pos, "color")


func _on_set_back_color_popup_closed():

	# Get the caret position
	var caret_pos = get_caret_pos()

	if has_focus() == true:
		selection_text = get_selected_text()

	# check if some text is selected
	if selection_text != "":
		add_tag_to_selected(str("bgcolor=",Utils.to_hex_color(color)), caret_pos, "bgcolor")
	else:
		add_tag_to_pos(str("bgcolor=",Utils.to_hex_color(color)), caret_pos, "bgcolor")

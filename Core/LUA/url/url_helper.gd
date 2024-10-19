class_name URLHelper

static var blocked_link: PackedStringArray = []

static func open_link(link: String, parent: Node, additional_info: String = ""):
	
	var logger: Logger = Logger.new("URLHelper")
	
	if link.begins_with("user://") or link.begins_with("res://"):
		link = ProjectSettings.globalize_path(link)
	
	if blocked_link.has(link):
		return
	
	var main_ask: ConfirmationDialog = ConfirmationDialog.new()
	main_ask.set_title("Open an external link")
	main_ask.set_text("Do you want to open the link: \"%s\"?\n%s" % [link,additional_info])
	main_ask.add_button("Block link", true, "block_link")
	main_ask.confirmed.connect(
		func():
			OS.shell_open(link)
	)
	main_ask.canceled.connect(
		func():
			main_ask.hide()
			main_ask.queue_free()
	)
	main_ask.custom_action.connect(
		func(button_action: String):
			if button_action == "block_link":
				blocked_link.push_back(link)
				main_ask.hide()
				main_ask.queue_free()
	)
	
	if parent != null and parent.is_inside_tree():
		parent.add_child(main_ask)
		main_ask.popup_centered()
	else:
		logger.error("Parent node not inside tree or not valid")

static func link_plugin(plugin: ResPlugin):
	
	if plugin.has_allowed("HELPER:OPEN_LINK"):
		plugin.add_link("open_link", URLHelper.open_link)


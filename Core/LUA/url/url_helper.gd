class_name URLHelper

static var blocked_link: PackedStringArray = []

static var popup: Window
static var _interact_blocker: Panel
static var popup_theme: Theme
static var _info: Label
static var _more_info: Label
static var url: String

static func open_link(link: String, parent: Node, additional_info: String = ""):
	
	var logger: Logger = Logger.new("URLHelper")
	
	if link.begins_with("user://") or link.begins_with("res://"):
		link = ProjectSettings.globalize_path(link)
	
	if blocked_link.has(link):
		return
	
	var interact_blocker: Panel = Panel.new()
	_interact_blocker = interact_blocker
	
	var blocker_stylebox: StyleBoxFlat = StyleBoxFlat.new()
	blocker_stylebox.bg_color = Color(0,0,0,.3)
	
	interact_blocker.add_theme_stylebox_override("Panel", blocker_stylebox)
	parent.add_child(interact_blocker)
	interact_blocker.set_anchors_preset(Control.PRESET_FULL_RECT)
	interact_blocker.set_offsets_preset(Control.PRESET_FULL_RECT)
	
	url = link
	
	if popup != null:
		if parent == null:
			return false
		
		_info.set_text("Do you want to open the following link?\n\n%s\n\nBe aware that it can be unsafe to open an external link!" % link)
		
		if !additional_info.is_empty():
			_more_info.show()
			_more_info.set_text("\nInfo provided by the plugin:\n%s" % additional_info)
		popup.hide()
		popup.reparent(parent)
		popup.popup_centered()
		popup.ready.connect(func():
			popup.show()
			)
		return true
	
	var ask_theme: Theme = Theme.new()
	
	var button_inner_margin = 24
	
	var button_box_normal: StyleBoxFlat = StyleBoxFlat.new()
	button_box_normal.bg_color = Color(0, 0, 0, 0.2)
	button_box_normal.set_content_margin(SIDE_LEFT, 0 + button_inner_margin)
	button_box_normal.set_content_margin(SIDE_RIGHT, 0 + button_inner_margin)
	button_box_normal.set_content_margin(SIDE_TOP, -16 + button_inner_margin)
	button_box_normal.set_content_margin(SIDE_BOTTOM, -16 + button_inner_margin)
	var button_box_focus: StyleBoxEmpty = StyleBoxEmpty.new()
	var button_box_pressed: StyleBoxFlat = StyleBoxFlat.new()
	button_box_pressed.bg_color = Color(0, 0, 0, 0.6)
	var button_box_disabled: StyleBoxFlat = StyleBoxFlat.new()
	button_box_disabled.bg_color = Color(0, 0, 0, 0.1)
	var button_box_hover: StyleBoxFlat = StyleBoxFlat.new()
	button_box_hover.bg_color = Color(0, 0, 0, 0.3)
	
	ask_theme.set_stylebox('normal', 'Button', button_box_normal)
	ask_theme.set_stylebox('pressed', 'Button', button_box_pressed)
	ask_theme.set_stylebox('disabled', 'Button', button_box_disabled)
	ask_theme.set_stylebox('focus', 'Button', button_box_focus)
	ask_theme.set_stylebox('hover', 'Button', button_box_hover)
	
	var main_container_box: StyleBoxFlat = StyleBoxFlat.new()
	main_container_box.bg_color = Color(0.059, 0.059, 0.075, 1)
	
	var sub_container_box: StyleBoxFlat = StyleBoxFlat.new()
	sub_container_box.bg_color = Color(0.102, 0.102, 0.125)
	
	var main_ask: Window = Window.new()
	main_ask.set_theme(ask_theme)
	main_ask.set_title("Open an external link")
	main_ask.borderless = true
	main_ask.set_min_size(Vector2(550, 350))
	popup = main_ask
	
	var ask_container: PanelContainer = PanelContainer.new()
	main_ask.add_child(ask_container)
	ask_container.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	
	var ask_box: VBoxContainer = VBoxContainer.new()
	ask_container.add_child(ask_box)
	ask_box.set_h_size_flags(Control.SIZE_EXPAND_FILL)
	ask_box.set_v_size_flags(Control.SIZE_EXPAND_FILL)
	
	var top_container: PanelContainer = PanelContainer.new()
	ask_box.add_child(top_container)
	top_container.set_h_size_flags(Control.SIZE_EXPAND_FILL)
	top_container.set_custom_minimum_size(Vector2(0, 32))
	
	var top_margin: MarginContainer = MarginContainer.new()
	top_container.add_child(top_margin)
	top_margin.set_h_size_flags(Control.SIZE_EXPAND_FILL)
	
	var top_box: HBoxContainer = HBoxContainer.new()
	top_margin.add_child(top_box)
	
	var title: Label = Label.new()
	top_box.add_child(title)
	title.set_h_size_flags(Control.SIZE_EXPAND_FILL)
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.set_text("Opening external link")
	
	var main_margin: MarginContainer = MarginContainer.new()
	ask_box.add_child(main_margin)
	main_margin.set_h_size_flags(Control.SIZE_EXPAND_FILL)
	main_margin.set_v_size_flags(Control.SIZE_EXPAND_FILL)
	
	var center_container: PanelContainer = PanelContainer.new()
	main_margin.add_child(center_container)
	center_container.set_h_size_flags(Control.SIZE_EXPAND_FILL)
	
	var center_scroll: ScrollContainer = ScrollContainer.new()
	center_container.add_child(center_scroll)
	center_scroll.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	center_scroll.set_h_size_flags(Control.SIZE_EXPAND_FILL)
	
	var center_box: VBoxContainer = VBoxContainer.new()
	center_scroll.add_child(center_box)
	center_box.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	center_box.set_h_size_flags(Control.SIZE_EXPAND_FILL)
	center_box.set_v_size_flags(Control.SIZE_EXPAND_FILL)
	center_box.alignment = BoxContainer.ALIGNMENT_CENTER
	
	var info: Label = Label.new()
	center_box.add_child(info)
	info.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	info.set_text("Do you want to open the following link?\n\n%s\n\nBe aware that it can be unsafe to open an external link!" % link)
	
	_info = info
	
	var more_info: Label = Label.new()
	center_box.add_child(more_info)
	more_info.set_text("\nInfo provided by the plugin:\n%s" % additional_info)
	more_info.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	more_info.hide()
	if !additional_info.is_empty():
		more_info.show()
	_more_info = more_info

	var bottom_container: PanelContainer = PanelContainer.new()
	ask_box.add_child(bottom_container)
	bottom_container.set_h_size_flags(Control.SIZE_EXPAND_FILL)
	bottom_container.set_custom_minimum_size(Vector2(0, 32))
	
	var bottom_margin: MarginContainer = MarginContainer.new()
	bottom_container.add_child(bottom_margin)
	bottom_margin.set_h_size_flags(Control.SIZE_EXPAND_FILL)
	
	var bottom_box: HBoxContainer = HBoxContainer.new()
	bottom_margin.add_child(bottom_box)
	bottom_box.alignment = BoxContainer.ALIGNMENT_CENTER
	
	var block_button: Button = Button.new()
	bottom_box.add_child(block_button)
	block_button.set_text('Block this link')
	
	var refuse_button: Button = Button.new()
	bottom_box.add_child(refuse_button)
	refuse_button.set_text("Refuse")
	
	var open_button: Button = Button.new()
	bottom_box.add_child(open_button)
	open_button.set_text("Accept")
	
	ask_box.add_theme_constant_override('separation', 0)
	ask_container.add_theme_stylebox_override("panel", main_container_box)
	top_container.add_theme_stylebox_override("panel", sub_container_box)
	center_container.add_theme_stylebox_override("panel", sub_container_box)
	top_margin.add_theme_constant_override("margin_top", 4)
	top_margin.add_theme_constant_override("margin_left", 4)
	top_margin.add_theme_constant_override("margin_right", 4)
	top_margin.add_theme_constant_override("margin_bottom", 4)
	main_margin.add_theme_constant_override("margin_top", 4)
	main_margin.add_theme_constant_override("margin_left", 4)
	main_margin.add_theme_constant_override("margin_right", 4)
	main_margin.add_theme_constant_override("margin_bottom", 4)
	bottom_margin.add_theme_constant_override("margin_top", 4)
	bottom_margin.add_theme_constant_override("margin_left", 4)
	bottom_margin.add_theme_constant_override("margin_right", 4)
	bottom_margin.add_theme_constant_override("margin_bottom", 4)
	bottom_container.add_theme_stylebox_override("panel", sub_container_box)
	
	open_button.pressed.connect(
		func():
			OS.shell_open(url)
			popup.hide()
			_interact_blocker.hide()
	)
	refuse_button.pressed.connect(
		func():
			popup.hide()
			_interact_blocker.hide()
	)
	block_button.pressed.connect(
		func():
			blocked_link.push_back(url)
			popup.hide()
			_interact_blocker.hide()
	)
	
	if parent != null and parent.is_inside_tree():
		main_ask.hide()
		parent.add_child(main_ask)
		main_ask.popup_centered()
		main_ask.ready.connect(func():
			main_ask.show()
			)
	else:
		logger.error("Parent node not inside tree or not valid")

static func link_plugin(plugin: ResPlugin):
	
	if plugin.has_allowed("HELPER:OPEN_LINK"):
		plugin.add_link("open_link", URLHelper.open_link)


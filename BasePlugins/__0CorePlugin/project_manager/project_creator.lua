
local theme
local creator_window

local vbox

local hbox_name
local label_name
local input_name

local hbox_path
local label_path
local input_path

function start()

    create_theme()
    create_ui()

    return true
end

function stop()

    return true
end

function create_theme()

    theme = Theme.new()

    theme.set_constant("separation", "VBoxContainer", 10)

end

function create_ui()

    creator_window = ConfirmationDialog.new()
    creator_window.set_title("Create a new project")
    creator_window.set_size(Vector2(400, 200))
    creator_window.set_theme(theme)

    vbox = VBoxContainer.new()
    creator_window.add_child(vbox)

    hbox_name = HBoxContainer.new()
    vbox.add_child(hbox_name)
    hbox_name.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    label_name = Label.new()
    hbox_name.add_child(label_name)
    label_name.set_text("Name:")
    label_name.set_custom_minimum_size(Vector2(60, 0))

    input_name = LineEdit.new()
    hbox_name.add_child(input_name)
    input_name.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    hbox_path = HBoxContainer.new()
    vbox.add_child(hbox_path)
    hbox_path.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    label_path = Label.new()
    hbox_path.add_child(label_path)
    label_path.set_text("Path:")
    label_path.set_custom_minimum_size(Vector2(60, 0))

    input_path = LineEdit.new()
    hbox_path.add_child(input_path)
    input_path.set_text("This feature is not implemented yet")
    input_path.set_editable(false)
    input_path.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    InterfaceManager.ADD("project_creator", creator_window)

end
local background_top

function start()

    background_top = PanelContainer.new()
    background_top.set_custom_minimum_size(Vector2(0, 48))
    background_top.set_theme_type_variation("PanelTopBottom")

    margin_top = MarginContainer.new()
    background_top.add_child(margin_top)
    margin_top.add_theme_constant_override("margin_top", 4)
    margin_top.add_theme_constant_override("margin_left", 4)
    margin_top.add_theme_constant_override("margin_right", 4)
    margin_top.add_theme_constant_override("margin_bottom", 4)

    hbox_top_menu = HBoxContainer.new()
    margin_top.add_child(hbox_top_menu)
    hbox_top_menu.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    create_project_button = Button.new()
    hbox_top_menu.add_child(create_project_button)
    create_project_button.set_text("Create new project...")
    create_project_button.set_default_cursor_shape(Control.CURSOR_POINTING_HAND)

    open_plugins_folder_button = Button.new()
    hbox_top_menu.add_child(open_plugins_folder_button)
    open_plugins_folder_button.set_text("Open plugins folder...")
    open_plugins_folder_button.set_default_cursor_shape(Control.CURSOR_POINTING_HAND)
    open_plugins_folder_button.pressed.connect(
        function()
            open_plugins()
        end
    )

    separator_buttons = PanelContainer.new()
    hbox_top_menu.add_child(separator_buttons)
    separator_buttons.set_custom_minimum_size(Vector2(1, 0))
    separator_buttons.set_h_size_flags(Control.SIZE_EXPAND_FILL)
    separator_buttons.set_theme_type_variation("PanelCenter")

    open_github_button = Button.new()
    hbox_top_menu.add_child(open_github_button)
    open_github_button.set_text("Open GitHub repo...")
    open_github_button.set_default_cursor_shape(Control.CURSOR_POINTING_HAND)
    open_github_button.pressed.connect(
        function()
            open_github()
        end
    )

    return true
end

function stop()

    return true
end

function _on_import()

    return {background_top = background_top}
end

function create_project()
    Logger.log("CorePlugin", "Creating new project...")
    local project_creator = InterfaceManager.GET("project_creator")

    if project_creator == nil then
        Logger.error("CorePlugin", "Project creator not found.")
        return
    end

    if not project_creator.is_inside_tree() then
        print("parent == nil")
        InterfaceManager.GET("project_manager").add_child(project_creator)
    end

    project_creator.popup_centered()
end

function open_plugins()
    -- The first two arguments are NEEDED, the third one is optional. The third one is the message that will be displayed in the notification.
    open_link("user://plugins", project_main, "This will open the plugins folder in your file manager.")
end

function open_github()
    -- The first two arguments are NEEDED, the third one is optional. The third one is the message that will be displayed in the notification.
    open_link("https://github.com/Ward727a/RPGCreator", project_main, "This will open the GitHub repository of RPG Creator in your browser.")
end

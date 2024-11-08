
local project_main

local margin_top
local hbox_top_menu
local create_project_button
local open_github_button

local background_panel_content
local background_panel_content_top
local separator_content_stylebox

local background_title_projects

local background_projects
local vbox_projects

local plugin_list

local margin_bottom
local hbox_bottom_menu

local bottom_copyright_text
local bottom_divider


function start()

    import_lua("./project_manager/project_creator.lua")

    import_lua("theme.lua")
    
    create_base()

    Logger.log("CorePlugin", "Importing project manager components...")
    import_lua("top_bar.lua", {project_theme = project_theme, project_main = project_main})
    import_lua("bottom_bar.lua", {project_theme = project_theme, project_main = project_main})
    import_lua("content.lua", {project_theme = project_theme})
    Logger.log("CorePlugin", "Project manager components imported.")

    create_ui()

    return true
end

function create_base()
    Logger.log("CorePlugin", "Creating project manager base interface...")

    project_main = Window.new()
    project_main.set_title("Project Manager")
    project_main.set_exclusive(true)
    project_main.set_min_size(Vector2(1152, 648))
    project_main.set_theme(project_theme)
    project_main.ready.connect(
        function()
            _on_ready()
        end
    )
    project_main.close_requested.connect(
        function()
            EventsManager.CALL("main/quit")
        end
    )

    main_background = PanelContainer.new()
    project_main.add_child(main_background)
    main_background.set_theme_type_variation("PanelBackground")
    main_background.set_anchors_preset(Control.PRESET_FULL_RECT)

    main_container = VBoxContainer.new()
    main_background.add_child(main_container)
    main_container.add_theme_constant_override("separation", 0)

    Logger.log("CorePlugin", "Project manager base interface created.")
end

function create_ui()
    if has_permission("LOGGER") then
        Logger.log("CorePlugin", "Loading project manager...")

        Logger.log("CorePlugin", "Linking project manager components...")
        main_container.add_child(background_top)
        main_container.add_child(background_center)
        main_container.add_child(background_bottom)
        Logger.log("CorePlugin", "Project manager components linked.")

        InterfaceManager.ADD("project_manager", project_main)
        
        InterfaceManager.GET("main").add_child(project_main)

        Logger.log("CorePlugin", "Project manager loaded.")
    end
end

function _on_ready()
    project_main.popup_centered()
    -- add_new_plugin("CorePlugin")
    EventsManager.CALL("editor/add_plugin", {"CorePlugin"})
end

function add_new_plugin(plugin_name)

    Logger.log("CorePlugin", "Adding new plugin: " .. plugin_name)

    local has_plugin = PluginManager.HAS(plugin_name)

    if has_plugin ~= true then
        Logger.error("CorePlugin", "Plugin not found.")
        return
    end

    local plugin = PluginManager.GET(plugin_name)

    local plugin_container = PanelContainer.new()
    plugin_list.add_child(plugin_container)
    plugin_container.set_custom_minimum_size(Vector2(0, 48))
    plugin_container.set_h_size_flags(Control.SIZE_EXPAND_FILL)
    local plugin_stylebox = StyleBoxFlat.new()
    plugin_stylebox.set_bg_color(Color(0, 0, 0, 0.2))
    plugin_container.add_theme_stylebox_override("panel", plugin_stylebox)

    local margin_plugin = MarginContainer.new()
    plugin_container.add_child(margin_plugin)
    margin_plugin.add_theme_constant_override("margin_top", 4)
    margin_plugin.add_theme_constant_override("margin_left", 4)
    margin_plugin.add_theme_constant_override("margin_right", 4)
    margin_plugin.add_theme_constant_override("margin_bottom", 4)

    local vbox_plugin = VBoxContainer.new()
    margin_plugin.add_child(vbox_plugin)
    vbox_plugin.set_h_size_flags(Control.SIZE_EXPAND_FILL)
    vbox_plugin.set_v_size_flags(Control.SIZE_EXPAND_FILL)
    vbox_plugin.add_theme_constant_override("separation", 4)

    local hbox_plugin_info = HBoxContainer.new()
    vbox_plugin.add_child(hbox_plugin_info)
    hbox_plugin_info.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    local plugin_name_label = Label.new()
    hbox_plugin_info.add_child(plugin_name_label)
    plugin_name_label.set_text(plugin_name)
    plugin_name_label.add_theme_font_size_override("font_size", 16)
    plugin_name_label.set_horizontal_alignment(HORIZONTAL_ALIGNMENT_CENTER)
    plugin_name_label.set_h_size_flags(Control.SIZE_EXPAND_FILL)
    plugin_name_label.set_stretch_ratio(1.5)

    local plugin_version_label = Label.new()
    hbox_plugin_info.add_child(plugin_version_label)
    plugin_version_label.set_text(plugin.version)
    plugin_version_label.add_theme_color_override("font_color", Color(0.592, 0.592, 0.592))

    local hbox_plugin_buttons = HBoxContainer.new()
    vbox_plugin.add_child(hbox_plugin_buttons)
    hbox_plugin_buttons.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    local plugin_toggle_button = Button.new()
    plugin_toggle_button.set_custom_minimum_size(Vector2(0, 32))
    plugin_toggle_button.set_h_size_flags(Control.SIZE_EXPAND_FILL)
    plugin_toggle_button.set_default_cursor_shape(Control.CURSOR_POINTING_HAND)

    if plugin.enable then
        plugin_toggle_button.set_text("Enabled")
        plugin_toggle_button.set_theme_type_variation("ButtonConfirm")
    else
        plugin_toggle_button.set_text("Disabled")
        plugin_toggle_button.set_theme_type_variation("ButtonCancel")
    end

    hbox_plugin_buttons.add_child(plugin_toggle_button)

    local plugin_settings_button = Button.new()
    hbox_plugin_buttons.add_child(plugin_settings_button)
    plugin_settings_button.set_text("Details")
    plugin_settings_button.set_h_size_flags(Control.SIZE_EXPAND_FILL)
    plugin_settings_button.set_default_cursor_shape(Control.CURSOR_POINTING_HAND)

end


function stop()

    return true
end
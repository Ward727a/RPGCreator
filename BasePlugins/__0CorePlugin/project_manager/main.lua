local project_theme

local project_main
local vbox

local background_top
local margin_top
local hbox_top_menu
local create_project_button
local open_plugins_button
local open_github_button

local background_content
local margin_content
local hbox_content

local hsplit_main

local background_projects
local margin_projects
local vbox_projects

local background_menu
local margin_menu
local vbox_menu

local background_bottom
local margin_bottom
local hbox_bottom_menu

local bottom_copyright_text
local bottom_divider
local bottom_contact_text

function start()

    import_lua("./project_manager/project_creator.lua")

    create_theme()
    create_ui()

    return true
end

function create_theme()
    Logger.log("CorePlugin", "Creating project manager theme...")
    project_theme = Theme.new()

    local test_color = Color(1, 1, .1, .1)
    local margin_v_unit = 4
    local margin_h_unit = 4
    local theme_box_separation = 0

    project_theme.set_constant("separation", "BoxContainer", theme_box_separation)

    project_theme.set_constant("margin_top", "margin_top_menu", margin_v_unit)
    project_theme.set_constant("margin_left", "margin_top_menu", margin_h_unit)
    project_theme.set_constant("margin_right", "margin_top_menu", margin_h_unit)
    project_theme.set_constant("margin_bottom", "margin_top_menu", margin_v_unit)
    project_theme.set_type_variation("margin_top_menu", "MarginContainer")

    project_theme.set_constant("separation", "hbox_top", 4)
    project_theme.set_type_variation("hbox_top", "HBoxContainer")

    local background_top_panel = StyleBoxFlat.new()
    background_top_panel.set_corner_radius_all(0)
    background_top_panel.set_bg_color(Color(1, .1, 1, .1))
    project_theme.set_stylebox("panel", "background_top_panel", background_top_panel)
    project_theme.set_type_variation("background_top_panel", "PanelContainer")
    
    project_theme.set_constant("margin_top", "margin_content", margin_v_unit)
    project_theme.set_constant("margin_left", "margin_content", margin_h_unit)
    project_theme.set_constant("margin_right", "margin_content", margin_h_unit)
    project_theme.set_constant("margin_bottom", "margin_content", margin_v_unit)
    project_theme.set_type_variation("margin_content", "MarginContainer")

    local background_content_panel = StyleBoxFlat.new()
    background_content_panel.set_corner_radius_all(0)
    background_content_panel.set_bg_color(test_color)
    project_theme.set_stylebox("panel", "background_content_panel", background_content_panel)
    project_theme.set_type_variation("background_content_panel", "PanelContainer")
    
    project_theme.set_constant("margin_top", "margin_projects", margin_v_unit)
    project_theme.set_constant("margin_left", "margin_projects", margin_h_unit)
    project_theme.set_constant("margin_right", "margin_projects", margin_h_unit)
    project_theme.set_constant("margin_bottom", "margin_projects", margin_v_unit)
    project_theme.set_type_variation("margin_projects", "MarginContainer")

    project_theme.set_constant("separation", "hsplit_menu", 4)
    project_theme.set_type_variation("hsplit_menu", "HSplitContainer")

    local background_projects_panel = StyleBoxFlat.new()
    background_projects_panel.set_corner_radius_all(0)
    background_projects_panel.set_bg_color(test_color)
    project_theme.set_stylebox("panel", "background_projects_panel", background_projects_panel)
    project_theme.set_type_variation("background_projects_panel", "PanelContainer")

    project_theme.set_constant("margin_top", "margin_menu", margin_v_unit)
    project_theme.set_constant("margin_left", "margin_menu", margin_h_unit)
    project_theme.set_constant("margin_right", "margin_menu", margin_h_unit)
    project_theme.set_constant("margin_bottom", "margin_menu", margin_v_unit)
    project_theme.set_type_variation("margin_menu", "MarginContainer")

    local background_menu_panel = StyleBoxFlat.new()
    background_menu_panel.set_corner_radius_all(0)
    background_menu_panel.set_bg_color(test_color)
    project_theme.set_stylebox("panel", "background_menu_panel", background_menu_panel)
    project_theme.set_type_variation("background_menu_panel", "PanelContainer")

    local background_bottom_panel = StyleBoxFlat.new()
    background_bottom_panel.set_corner_radius_all(0)
    background_bottom_panel.set_bg_color(Color(.1, 1, 1, .1))
    project_theme.set_stylebox("panel", "background_bottom_panel", background_bottom_panel)
    project_theme.set_type_variation("background_bottom_panel", "PanelContainer")

    project_theme.set_constant("margin_top", "margin_bottom", margin_v_unit)
    project_theme.set_constant("margin_left", "margin_bottom", margin_h_unit)
    project_theme.set_constant("margin_right", "margin_bottom", margin_h_unit)
    project_theme.set_constant("margin_bottom", "margin_bottom", margin_v_unit)
    project_theme.set_type_variation("margin_bottom", "MarginContainer")

    project_theme.set_font_size("font_size", "label_bottom", 12)
    project_theme.set_type_variation("label_bottom", "Label")

    Logger.log("CorePlugin", "Project manager theme created.")
end

function create_ui()
    if has_permission("LOGGER") then
        Logger.log("CorePlugin", "Loading project manager...")

        project_main = Window.new()
        project_main.set_title("Project Manager")
        project_main.set_exclusive(true)
        project_main.set_min_size(Vector2(840, 480))
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

        vbox = VBoxContainer.new()
        project_main.add_child(vbox)
        vbox.set_h_size_flags(Control.SIZE_EXPAND_FILL)
        vbox.set_v_size_flags(Control.SIZE_EXPAND_FILL)
        vbox.set_anchors_preset(Control.PRESET_FULL_RECT)
        vbox.set_offset(0)

        -- Top parts

        background_top = PanelContainer.new()
        vbox.add_child(background_top)
        background_top.set_h_size_flags(Control.SIZE_EXPAND_FILL)
        background_top.set_theme_type_variation("background_top_panel")

        margin_top = MarginContainer.new()
        background_top.add_child(margin_top)
        margin_top.set_theme_type_variation("margin_top_menu")

        hbox_top_menu = HBoxContainer.new()
        margin_top.add_child(hbox_top_menu)
        hbox_top_menu.set_theme_type_variation("hbox_top")

        create_project_button = Button.new()
        hbox_top_menu.add_child(create_project_button)
        create_project_button.set_text("Create New Project")
        create_project_button.pressed.connect(
            function()
                create_project()
            end
        )

        open_plugins_button = Button.new()
        hbox_top_menu.add_child(open_plugins_button)
        open_plugins_button.set_text("Open Plugins")
        open_plugins_button.pressed.connect(
            function()
                open_plugins()
            end
        )

        local divider = HBoxContainer.new()
        hbox_top_menu.add_child(divider)
        divider.set_h_size_flags(Control.SIZE_EXPAND_FILL)

        open_github_button = Button.new()
        hbox_top_menu.add_child(open_github_button)
        open_github_button.set_text("Open GitHub")
        open_github_button.pressed.connect(
            function()
                open_github()
            end
        )

        -- Content parts

        background_content = PanelContainer.new()
        vbox.add_child(background_content)
        background_content.set_h_size_flags(Control.SIZE_EXPAND_FILL)
        background_content.set_v_size_flags(Control.SIZE_EXPAND_FILL)
        background_content.set_theme_type_variation("background_content_panel")

        margin_content = MarginContainer.new()
        background_content.add_child(margin_content)
        margin_content.set_theme_type_variation("margin_content")

        hbox_content = HBoxContainer.new()
        margin_content.add_child(hbox_content)
        hbox_content.set_h_size_flags(Control.SIZE_EXPAND_FILL)
        hbox_content.set_v_size_flags(Control.SIZE_EXPAND_FILL)

        hsplit_main = HSplitContainer.new()
        hbox_content.add_child(hsplit_main)
        hsplit_main.set_h_size_flags(Control.SIZE_EXPAND_FILL)
        hsplit_main.set_theme_type_variation("hsplit_menu")

        -- Content > Projects parts

        background_projects = PanelContainer.new()
        hsplit_main.add_child(background_projects)
        background_projects.set_h_size_flags(Control.SIZE_EXPAND_FILL)
        background_projects.set_theme_type_variation("background_projects_panel")

        margin_projects = MarginContainer.new()
        background_projects.add_child(margin_projects)
        margin_projects.set_theme_type_variation("margin_projects")

        vbox_projects = VBoxContainer.new()
        margin_projects.add_child(vbox_projects)

        -- Content > Menu parts

        background_menu = PanelContainer.new()
        hsplit_main.add_child(background_menu)
        background_menu.set_custom_minimum_size(Vector2(200, 0))
        background_menu.set_theme_type_variation("background_menu_panel")

        margin_menu = MarginContainer.new()
        background_menu.add_child(margin_menu)
        margin_menu.set_theme_type_variation("margin_menu")

        vbox_menu = VBoxContainer.new()
        margin_menu.add_child(vbox_menu)

        local test_text = Label.new()
        test_text.set_text("Test")
        vbox_menu.add_child(test_text)

        -- Bottom parts

        background_bottom = PanelContainer.new()
        vbox.add_child(background_bottom)
        background_bottom.set_theme_type_variation("background_bottom_panel")

        margin_bottom = MarginContainer.new()
        background_bottom.add_child(margin_bottom)
        margin_bottom.set_theme_type_variation("margin_bottom")

        hbox_bottom_menu = HBoxContainer.new()
        margin_bottom.add_child(hbox_bottom_menu)

        bottom_copyright_text = Label.new()
        hbox_bottom_menu.add_child(bottom_copyright_text)
        -- Copyright text
        bottom_copyright_text.set_text("© 2024-present Ward727 under MIT License")
        bottom_copyright_text.set_theme_type_variation("label_bottom")

        bottom_divider = HBoxContainer.new()
        hbox_bottom_menu.add_child(bottom_divider)
        bottom_divider.set_h_size_flags(Control.SIZE_EXPAND_FILL)

        bottom_contact_text = Label.new()
        hbox_bottom_menu.add_child(bottom_contact_text)
        -- Contact text
        bottom_contact_text.set_text("Contact at: Ward727a@gmail.com - Discord: ward727")
        bottom_contact_text.set_theme_type_variation("label_bottom")

        InterfaceManager.ADD("project_manager", project_main)
        
        InterfaceManager.GET("main").add_child(project_main)

        -- create_project_button.pressed.connect(
        --     function()
        --         local test_array = {"project_manager", "test", 0, 5}
        --         EventsManager.CALL("coreplugin/notification_system/add_notification", test_array)
        --     end
        -- )

        Logger.log("CorePlugin", "Project manager loaded.")
    end
end

function _on_ready()
    project_main.popup_centered()
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
    Logger.log("CorePlugin", "Opening plugins folder...")
    -- The first two arguments are NEEDED, the third one is optional. The third one is the message that will be displayed in the notification.
    open_link("user://plugins", project_main, "This will open the plugins folder in your file manager.")
end

function open_github()
    Logger.log("CorePlugin", "Opening GitHub repository...")
    -- The first two arguments are NEEDED, the third one is optional. The third one is the message that will be displayed in the notification.
    open_link("https://github.com/Ward727a/RPGCreator", project_main, "This will open the GitHub repository of RPG Creator in your browser.")
end

function stop()

    return true
end
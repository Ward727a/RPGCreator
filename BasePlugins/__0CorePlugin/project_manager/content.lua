local background_center

function start()

    background_center = PanelContainer.new()
    background_center.set_v_size_flags(Control.SIZE_EXPAND_FILL)
    background_center.set_h_size_flags(Control.SIZE_EXPAND_FILL)
    background_center.set_theme_type_variation("PanelCenter")

    margin_center = MarginContainer.new()
    background_center.add_child(margin_center)
    margin_center.set_v_size_flags(Control.SIZE_EXPAND_FILL)
    margin_center.set_h_size_flags(Control.SIZE_EXPAND_FILL)
    margin_center.add_theme_constant_override("margin_top", 4)
    margin_center.add_theme_constant_override("margin_left", 4)
    margin_center.add_theme_constant_override("margin_right", 4)
    margin_center.add_theme_constant_override("margin_bottom", 4)

    hsplit_container = HBoxContainer.new()
    margin_center.add_child(hsplit_container)
    hsplit_container.set_v_size_flags(Control.SIZE_EXPAND_FILL)
    hsplit_container.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    background_projects = PanelContainer.new()
    hsplit_container.add_child(background_projects)
    background_projects.set_theme_type_variation("PanelContent")
    background_projects.set_stretch_ratio(2)
    background_projects.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    vbox_projects = VBoxContainer.new()
    background_projects.add_child(vbox_projects)
    vbox_projects.set_h_size_flags(Control.SIZE_EXPAND_FILL)
    vbox_projects.set_v_size_flags(Control.SIZE_EXPAND_FILL)
    vbox_projects.add_theme_constant_override("separation", 0)

    background_title_projects = PanelContainer.new()
    vbox_projects.add_child(background_title_projects)
    background_title_projects.set_custom_minimum_size(Vector2(0, 32))
    background_title_projects.set_theme_type_variation("PanelContentTop")

    title_projects = Label.new()
    background_title_projects.add_child(title_projects)
    title_projects.set_text("Project(s) list")
    title_projects.set_horizontal_alignment(HORIZONTAL_ALIGNMENT_CENTER)

    separator_title_projects = PanelContainer.new()
    vbox_projects.add_child(separator_title_projects)
    separator_title_projects.set_custom_minimum_size(Vector2(0, 1))
    separator_title_projects.set_theme_type_variation("SeparatorContent")

    background_plugins = PanelContainer.new()
    hsplit_container.add_child(background_plugins)
    background_plugins.set_theme_type_variation("PanelContent")
    background_plugins.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    vbox_plugins = VBoxContainer.new()
    background_plugins.add_child(vbox_plugins)
    vbox_plugins.set_h_size_flags(Control.SIZE_EXPAND_FILL)
    vbox_plugins.set_v_size_flags(Control.SIZE_EXPAND_FILL)
    vbox_plugins.add_theme_constant_override("separation", 0)

    background_title_plugins = PanelContainer.new()
    vbox_plugins.add_child(background_title_plugins)
    background_title_plugins.set_custom_minimum_size(Vector2(0, 32))
    background_title_plugins.set_theme_type_variation("PanelContentTop")

    title_plugins = Label.new()
    background_title_plugins.add_child(title_plugins)
    title_plugins.set_text("Plugins")
    title_plugins.set_horizontal_alignment(HORIZONTAL_ALIGNMENT_CENTER)

    separator_title_plugins = PanelContainer.new()
    vbox_plugins.add_child(separator_title_plugins)
    separator_title_plugins.set_custom_minimum_size(Vector2(0, 1))
    separator_title_plugins.set_theme_type_variation("SeparatorContent")

    plugin_scroll = ScrollContainer.new()
    vbox_plugins.add_child(plugin_scroll)
    plugin_scroll.set_v_size_flags(Control.SIZE_EXPAND_FILL)
    plugin_scroll.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    plugin_margin = MarginContainer.new()
    plugin_scroll.add_child(plugin_margin)
    plugin_margin.set_v_size_flags(Control.SIZE_EXPAND_FILL)
    plugin_margin.set_h_size_flags(Control.SIZE_EXPAND_FILL)
    plugin_margin.add_theme_constant_override("margin_top", 4)
    plugin_margin.add_theme_constant_override("margin_left", 4)
    plugin_margin.add_theme_constant_override("margin_right", 4)
    plugin_margin.add_theme_constant_override("margin_bottom", 4)

    plugin_list = VBoxContainer.new()
    plugin_margin.add_child(plugin_list)
    plugin_list.set_h_size_flags(Control.SIZE_EXPAND_FILL)
    plugin_list.set_v_size_flags(Control.SIZE_EXPAND_FILL)
    plugin_list.add_theme_constant_override("separation", 4)

    EventsManager.SET("editor/add_plugin", add_plugin)

    return true
end

function stop()

    return true
end

function _on_import()

    return {background_center = background_center}
end

function add_plugin(plugin_name)

    import_lua("plugin_components.lua", {plugin_name = plugin_name})

    plugin_list.add_child(plugin_component)

end
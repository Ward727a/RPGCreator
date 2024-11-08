local plugin_component

function start()

    local plugin = PluginManager.GET(plugin_name)

    plugin_component = PanelContainer.new()
    plugin_component.set_custom_minimum_size(Vector2(0, 48))
    plugin_component.set_theme_type_variation("PanelCenter")
    plugin_component.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    margin_plugin = MarginContainer.new()
    plugin_component.add_child(margin_plugin)
    margin_plugin.add_theme_constant_override("margin_top", 4)
    margin_plugin.add_theme_constant_override("margin_left", 4)
    margin_plugin.add_theme_constant_override("margin_right", 4)
    margin_plugin.add_theme_constant_override("margin_bottom", 4)
    margin_plugin.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

    vbox_plugin = VBoxContainer.new()
    margin_plugin.add_child(vbox_plugin)
    vbox_plugin.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    header_plugin_box = HBoxContainer.new()
    vbox_plugin.add_child(header_plugin_box)
    header_plugin_box.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    plugin_title = Label.new()
    header_plugin_box.add_child(plugin_title)
    plugin_title.set_text(plugin.name)

    plugin_version = Label.new()
    header_plugin_box.add_child(plugin_version)
    plugin_version.set_text(plugin.version)

    menu_plugin_box = HBoxContainer.new()
    vbox_plugin.add_child(menu_plugin_box)

    plugin_toggle_button = Button.new()
    menu_plugin_box.add_child(plugin_toggle_button)
    plugin_toggle_button.set_text("Unknown status")
    plugin_toggle_button.set_custom_minimum_size(Vector2(0, 24))

    if plugin.enable then
        plugin_toggle_button.set_text("Enabled")
        plugin_toggle_button.set_theme_type_variation("ButtonConfirm")
    else
        plugin_toggle_button.set_text("Disabled")
        plugin_toggle_button.set_theme_type_variation("ButtonCancel")
    end

    plugin_details_button = Button.new()
    menu_plugin_box.add_child(plugin_details_button)
    plugin_details_button.set_text("Details")
    plugin_details_button.set_custom_minimum_size(Vector2(0, 24))

    return true
end

function stop()

    return true
end

function _on_import()

    return {plugin_component = plugin_component}
end
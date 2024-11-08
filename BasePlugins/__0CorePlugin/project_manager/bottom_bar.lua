local background_bottom

function start()

    import_lua("about/about.lua", {parent = project_main})

    background_bottom = PanelContainer.new()
    background_bottom.set_custom_minimum_size(Vector2(0, 48))
    background_bottom.set_theme_type_variation("PanelTopBottom")

    margin_bottom = MarginContainer.new()
    background_bottom.add_child(margin_bottom)
    margin_bottom.add_theme_constant_override("margin_top", 4)
    margin_bottom.add_theme_constant_override("margin_left", 4)
    margin_bottom.add_theme_constant_override("margin_right", 4)
    margin_bottom.add_theme_constant_override("margin_bottom", 4)

    hbox_bottom_menu = HBoxContainer.new()
    margin_bottom.add_child(hbox_bottom_menu)
    hbox_bottom_menu.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    bottom_copyright_text = Label.new()
    hbox_bottom_menu.add_child(bottom_copyright_text)
    bottom_copyright_text.set_text("© 2024-Present Ward727 under Apache License 2.0.")
    bottom_copyright_text.add_theme_color_override("font_color", Color(0.592, 0.592, 0.592))

    bottom_divider = PanelContainer.new()
    hbox_bottom_menu.add_child(bottom_divider)
    bottom_divider.set_custom_minimum_size(Vector2(1, 0))
    bottom_divider.set_h_size_flags(Control.SIZE_EXPAND_FILL)
    bottom_divider.set_theme_type_variation("PanelCenter")

    bottom_about_button = Button.new()
    hbox_bottom_menu.add_child(bottom_about_button)
    bottom_about_button.set_text("About...")
    bottom_about_button.set_default_cursor_shape(Control.CURSOR_POINTING_HAND)
    bottom_about_button.pressed.connect(
        function()
            show_about()
        end
    )

    return true
end

function stop()

    return true
end

function _on_import()

    return {background_bottom = background_bottom}
end

function show_about()
    Logger.log("CorePlugin", "Showing about dialog...")
    about_main.hide()

    if not about_main.is_inside_tree() then
        project_main.add_child(about_main)
    end
    about_main.popup_centered()
    about_main.ready.connect(
        function()
            about_main.show()
        end
    )
end
local about_top

function start()

    about_top = PanelContainer.new()
    about_top.set_h_size_flags(Control.SIZE_EXPAND_FILL)
    about_top.set_theme_type_variation("PanelCenter")
    about_top.set_custom_minimum_size(Vector2(0, 32))
    about_top.set_theme_type_variation("PanelTopBottom")

    margin_top = MarginContainer.new()
    about_top.add_child(margin_top)
    margin_top.set_h_size_flags(Control.SIZE_EXPAND_FILL)
    margin_top.add_theme_constant_override("margin_top", 4)
    margin_top.add_theme_constant_override("margin_left", 4)
    margin_top.add_theme_constant_override("margin_right", 4)
    margin_top.add_theme_constant_override("margin_bottom", 4)

    hbox_top_menu = HBoxContainer.new()
    margin_top.add_child(hbox_top_menu)
    hbox_top_menu.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    about_title = Label.new()
    hbox_top_menu.add_child(about_title)
    about_title.set_text("About RPG Creator version ".. software_version)
    about_title.set_horizontal_alignment(HORIZONTAL_ALIGNMENT_CENTER)
    about_title.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    return true
end

function stop()

    return true
end

function _on_import()

    return {about_top = about_top}
end
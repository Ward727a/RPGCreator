local about_content

function start()

    about_content = MarginContainer.new()
    about_content.set_v_size_flags(Control.SIZE_EXPAND_FILL)
    about_content.set_h_size_flags(Control.SIZE_EXPAND_FILL)
    about_content.add_theme_constant_override("margin_top", 4)
    about_content.add_theme_constant_override("margin_left", 4)
    about_content.add_theme_constant_override("margin_right", 4)
    about_content.add_theme_constant_override("margin_bottom", 4)

    about_background = PanelContainer.new()
    about_content.add_child(about_background)
    about_background.set_v_size_flags(Control.SIZE_EXPAND_FILL)
    about_background.set_h_size_flags(Control.SIZE_EXPAND_FILL)
    about_background.set_theme_type_variation("PanelTopBottom")

    about_scroll = ScrollContainer.new()
    about_background.add_child(about_scroll)
    about_scroll.set_v_size_flags(Control.SIZE_EXPAND_FILL)
    about_scroll.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    about_vbox = VBoxContainer.new()
    about_scroll.add_child(about_vbox)
    about_vbox.set_v_size_flags(Control.SIZE_EXPAND_FILL)
    about_vbox.set_h_size_flags(Control.SIZE_EXPAND_FILL)
    about_vbox.add_theme_constant_override("separation", 0)
    about_vbox.alignment = BoxContainer.ALIGNMENT_CENTER

    about_content_text = Label.new()
    about_vbox.add_child(about_content_text)
    about_content_text.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    about_content_text.set_h_size_flags(Control.SIZE_EXPAND_FILL)
    about_content_text.set_text(
    "\n"..
    "RPG Creator is a free, open-source software"..
    "\n"..
    "created by Ward727 under Apache License 2.0\n\n"..
    "The software is still in development and"..
    "\n"..
    "is not yet ready for production use."..
    "\n\n"..
    "If you want to contribute to the project,"..
    "\n"..
    "please visit the GitHub repository."..
    "\n\n"..
    "This software would not be possible without the amazing work of:"..
    "\n"..
    "- Mike Shulze for GDUnit4 (4.4.1) [MIT License]"..
    "\n"..
    "- Patrick Dawson for imgui-godot (6.2.1) [MIT License]"..
    "\n"..
    "- Xecestel & Sarturo for Sound Manager (4.0) [MIT License]"..
    "\n"..
    "- Trey2k for LuaAPI (2.0.2) [MIT License]"..
    "\n"..
    "- Godot Engine contributors for Godot Engine (Custom built 4.2.3) [MIT License]"..
    "\n"..
    "- Kenney for the permission to use a lots of free assets in the software [CC0 License]"..
    "\n\n"
    )


    return true
end

function stop()


    return true
end

function _on_import()

    return {about_content = about_content}
end
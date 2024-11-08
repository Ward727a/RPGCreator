local about_main

function start()

    create_base()

    Logger.log("CorePlugin", "Importing about components...")
    import_lua("about_top.lua")
    import_lua("about_content.lua")
    import_lua("about_bottom.lua", {about_main = about_main})
    Logger.log("CorePlugin", "About components imported.")

    create_ui()

    return true
end

function create_base()

    Logger.log("CorePlugin", "Creating about base interface...")

    about_main = Window.new()
    about_main.set_title("About RPG Creator Version ".. software_version)
    about_main.set_exclusive(true)
    about_main.set_min_size(Vector2(530, 300))
    about_main.ready.connect(
        function()
            _on_ready()
        end
    )
    about_main.close_requested.connect(
        function()

            stop()
        end
    )

    about_background = PanelContainer.new()
    about_main.add_child(about_background)
    about_background.set_theme_type_variation("PanelBackground")
    about_background.set_anchors_preset(Control.PRESET_FULL_RECT)

    about_box = VBoxContainer.new()
    about_background.add_child(about_box)
    about_box.set_h_size_flags(Control.SIZE_EXPAND_FILL)
    about_box.set_v_size_flags(Control.SIZE_EXPAND_FILL)
    about_box.add_theme_constant_override("separation", 0)

    Logger.log("CorePlugin", "About base interface created.")

    return true
end

function create_ui()

    about_box.add_child(about_top)
    about_box.add_child(about_content)
    about_box.add_child(about_bottom)

end

function _on_ready()
end

function stop()

    about_main.hide()

    return true
end

function _on_import()

    return {about_main = about_main}
end
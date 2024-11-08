local about_bottom

function start()

    about_bottom = Button.new()
    about_bottom.set_text("Join our community!")
    about_bottom.set_default_cursor_shape(Control.CURSOR_POINTING_HAND)
    about_bottom.pressed.connect(
        function()
            open_link("https://linktr.ee/rpgcreator", about_main, "This will show you all the links available to join our community.")
        end
    )
    about_bottom.set_custom_minimum_size(Vector2(0, 32))

    return true
end

function stop()


    return true
end

function _on_import()

    return {about_bottom = about_bottom}
end
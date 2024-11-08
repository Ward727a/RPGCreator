local project_theme


function start()
    Logger.log("CorePlugin", "Creating project manager theme...")
    project_theme = Theme.new()

    local button_inner_margin = 24
    local background_panel_stylebox = StyleBoxFlat.new()

    project_theme.set_font_size("font_size", "Label", 12)
    project_theme.set_font_size("font_size", "Button", 12)

    stylebox_button_normal = StyleBoxFlat.new()
    stylebox_button_normal.set_bg_color(Color(0, 0, 0, 0.2))
    stylebox_button_normal.set_content_margin(SIDE_LEFT, 0 + button_inner_margin)
    stylebox_button_normal.set_content_margin(SIDE_RIGHT, 0 + button_inner_margin)

    stylebox_button_focus = StyleBoxEmpty.new()

    stylebox_button_pressed = StyleBoxFlat.new()
    stylebox_button_pressed.set_bg_color(Color(0, 0, 0, 0.6))
    stylebox_button_pressed.set_content_margin(SIDE_LEFT, 0 + button_inner_margin)
    stylebox_button_pressed.set_content_margin(SIDE_RIGHT, 0 + button_inner_margin)

    stylebox_button_disabled = StyleBoxFlat.new()
    stylebox_button_disabled.set_bg_color(Color(0, 0, 0, 0.1))
    stylebox_button_disabled.set_content_margin(SIDE_LEFT, 0 + button_inner_margin)
    stylebox_button_disabled.set_content_margin(SIDE_RIGHT, 0 + button_inner_margin)

    stylebox_button_hover = StyleBoxFlat.new()
    stylebox_button_hover.set_bg_color(Color(0, 0, 0, 0.3))
    stylebox_button_hover.set_content_margin(SIDE_LEFT, 0 + button_inner_margin)
    stylebox_button_hover.set_content_margin(SIDE_RIGHT, 0 + button_inner_margin)

    project_theme.set_stylebox("normal", "Button", stylebox_button_normal)
    project_theme.set_stylebox("focus", "Button", stylebox_button_focus)
    project_theme.set_stylebox("pressed", "Button", stylebox_button_pressed)
    project_theme.set_stylebox("disabled", "Button", stylebox_button_disabled)
    project_theme.set_stylebox("hover", "Button", stylebox_button_hover)

    stylebox_button_normal_confirm = StyleBoxFlat.new()
    stylebox_button_normal_confirm.set_bg_color(Color(0.4, 0.886, 0.373, 0.2))
    stylebox_button_normal_confirm.set_content_margin(SIDE_LEFT, 0 + button_inner_margin)
    stylebox_button_normal_confirm.set_content_margin(SIDE_RIGHT, 0 + button_inner_margin)

    stylebox_button_focus_confirm = StyleBoxEmpty.new()

    stylebox_button_pressed_confirm = StyleBoxFlat.new()
    stylebox_button_pressed_confirm.set_bg_color(Color(0.4, 0.886, 0.373, 0.6))
    stylebox_button_pressed_confirm.set_content_margin(SIDE_LEFT, 0 + button_inner_margin)
    stylebox_button_pressed_confirm.set_content_margin(SIDE_RIGHT, 0 + button_inner_margin)

    stylebox_button_disabled_confirm = StyleBoxFlat.new()
    stylebox_button_disabled_confirm.set_bg_color(Color(0.4, 0.886, 0.373, 0.1))
    stylebox_button_disabled_confirm.set_content_margin(SIDE_LEFT, 0 + button_inner_margin)
    stylebox_button_disabled_confirm.set_content_margin(SIDE_RIGHT, 0 + button_inner_margin)

    stylebox_button_hover_confirm = StyleBoxFlat.new()
    stylebox_button_hover_confirm.set_bg_color(Color(0.4, 0.886, 0.373, 0.3))
    stylebox_button_hover_confirm.set_content_margin(SIDE_LEFT, 0 + button_inner_margin)
    stylebox_button_hover_confirm.set_content_margin(SIDE_RIGHT, 0 + button_inner_margin)

    project_theme.set_stylebox("normal", "ButtonConfirm", stylebox_button_normal_confirm)
    project_theme.set_stylebox("focus", "ButtonConfirm", stylebox_button_focus_confirm)
    project_theme.set_stylebox("pressed", "ButtonConfirm", stylebox_button_pressed_confirm)
    project_theme.set_stylebox("disabled", "ButtonConfirm", stylebox_button_disabled_confirm)
    project_theme.set_stylebox("hover", "ButtonConfirm", stylebox_button_hover_confirm)
    project_theme.set_type_variation("ButtonConfirm", "Button")

    stylebox_button_normal_cancel = StyleBoxFlat.new()
    stylebox_button_normal_cancel.set_bg_color(Color(0.886, 0.373, 0.373, 0.2))
    stylebox_button_normal_cancel.set_content_margin(SIDE_LEFT, 0 + button_inner_margin)
    stylebox_button_normal_cancel.set_content_margin(SIDE_RIGHT, 0 + button_inner_margin)

    stylebox_button_focus_confirm = StyleBoxEmpty.new()

    stylebox_button_pressed_cancel = StyleBoxFlat.new()
    stylebox_button_pressed_cancel.set_bg_color(Color(0.886, 0.373, 0.373, 0.6))
    stylebox_button_pressed_cancel.set_content_margin(SIDE_LEFT, 0 + button_inner_margin)
    stylebox_button_pressed_cancel.set_content_margin(SIDE_RIGHT, 0 + button_inner_margin)

    stylebox_button_disabled_cancel = StyleBoxFlat.new()
    stylebox_button_disabled_cancel.set_bg_color(Color(0.886, 0.373, 0.373, 0.1))
    stylebox_button_disabled_cancel.set_content_margin(SIDE_LEFT, 0 + button_inner_margin)
    stylebox_button_disabled_cancel.set_content_margin(SIDE_RIGHT, 0 + button_inner_margin)

    stylebox_button_hover_cancel = StyleBoxFlat.new()
    stylebox_button_hover_cancel.set_bg_color(Color(0.886, 0.373, 0.373, 0.3))
    stylebox_button_hover_cancel.set_content_margin(SIDE_LEFT, 0 + button_inner_margin)
    stylebox_button_hover_cancel.set_content_margin(SIDE_RIGHT, 0 + button_inner_margin)

    project_theme.set_stylebox("normal", "ButtonCancel", stylebox_button_normal_cancel)
    project_theme.set_stylebox("focus", "ButtonCancel", stylebox_button_focus_cancel)
    project_theme.set_stylebox("pressed", "ButtonCancel", stylebox_button_pressed_cancel)
    project_theme.set_stylebox("disabled", "ButtonCancel", stylebox_button_disabled_cancel)
    project_theme.set_stylebox("hover", "ButtonCancel", stylebox_button_hover_cancel)
    project_theme.set_type_variation("ButtonCancel", "Button")

    background_panel_stylebox.set_bg_color(Color(0.059, 0.059, 0.075, 1))
    background_panel_top_bottom = StyleBoxFlat.new()
    background_panel_top_bottom.set_bg_color(Color(0.102, 0.102, 0.125))
    background_panel_center = StyleBoxEmpty.new()

    background_panel_content = StyleBoxFlat.new()
    background_panel_content_top = StyleBoxFlat.new()
    separator_content_stylebox = StyleBoxFlat.new()

    background_panel_content.set_bg_color(Color(0.102, 0.102, 0.125))
    background_panel_content_top.set_bg_color(Color(0, 0, 0, .2))
    separator_content_stylebox.set_bg_color(Color(0, 0, 0, .3))

    scroll_background_stylebox = StyleBoxFlat.new()
    scroll_background_stylebox.set_bg_color(Color(0.031, 0.031, 0.043, .75))
    scroll_background_stylebox.set_content_margin(SIDE_LEFT,3)
    scroll_background_stylebox.set_content_margin(SIDE_RIGHT,3)

    scroll_focus_stylebox = StyleBoxEmpty.new()

    scroll_grabber_box = StyleBoxFlat.new()
    scroll_grabber_box.set_bg_color(Color(0.102, 0.102, 0.137, .6))
    scroll_grabber_box_hover = StyleBoxFlat.new()
    scroll_grabber_box_hover.set_bg_color(Color(0.102, 0.102, 0.137, .8))

    scroll_grabber_box_pressed = StyleBoxFlat.new()
    scroll_grabber_box_pressed.set_bg_color(Color(0.102, 0.102, 0.137, .5))

    project_theme.set_stylebox("panel", "PanelBackground", background_panel_stylebox)
    project_theme.set_stylebox("panel", "PanelContent", background_panel_content)
    project_theme.set_stylebox("panel", "PanelTopBottom", background_panel_top_bottom)
    project_theme.set_stylebox("panel", "PanelCenter", background_panel_center)
    project_theme.set_stylebox("panel", "PanelContentTop", background_panel_content_top)
    project_theme.set_stylebox("panel", "SeparatorContent", separator_content_stylebox)

    project_theme.set_stylebox("scroll", "ScrollBar", scroll_background_stylebox)
    project_theme.set_stylebox("scroll_focus", "ScrollBar", scroll_focus_stylebox)
    project_theme.set_stylebox("grabber", "ScrollBar", scroll_grabber_box)
    project_theme.set_stylebox("grabber_highlight", "ScrollBar", scroll_grabber_box_hover)
    project_theme.set_stylebox("grabber_pressed", "ScrollBar", scroll_grabber_box_pressed)

    project_theme.set_type_variation("PanelBackground", "Panel")
    project_theme.set_type_variation("PanelEmpty", "Panel")
    project_theme.set_type_variation("PanelContent", "Panel")
    project_theme.set_type_variation("PanelTopBottom", "Panel")
    project_theme.set_type_variation("PanelCenter", "Panel")
    project_theme.set_type_variation("PanelContentTop", "Panel")
    project_theme.set_type_variation("SeparatorContent", "Panel")

    Logger.log("CorePlugin", "Project manager theme created.")
    return true
end

function stop()

    return true
end

function _on_import()

    return {project_theme = project_theme}
end
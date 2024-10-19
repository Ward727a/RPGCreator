
local theme_info

local parent_list = {}

function start()

    Logger.log("CorePlugin", "Creating notification events...")
    create_events()
    Logger.log("CorePlugin", "Notification events created successfully")

    Logger.log("CorePlugin", "Creating notification theme...")
    create_theme()
    Logger.log("CorePlugin", "Notification theme created successfully")

    Logger.log("CorePlugin", "Creating notification UI...")
    create_ui()
    Logger.log("CorePlugin", "Notification UI created successfully")

    return true
end

function stop()

    return true
end


function create_events()

    EventsManager.SET("coreplugin/notification_system/add_notification", function (parent_node, message, level, duration)
        add_notification(parent_node, message, level, duration)
    end)

end

function add_notification(parent_node, message, level, duration)

    local notification

    if level == 0 then
        notification = InterfaceManager.GET("notification_system/notification_info").duplicate()
    elseif level == 1 then
        notification = InterfaceManager.GET("notification_system/notification_warning").duplicate()
    elseif level == 2 then
        notification = InterfaceManager.GET("notification_system/notification_error").duplicate()
    end

    -- Generate a test random number

    local notif_count = math.random(1, 1000)
    local title_node = notification.get_node("margin/vbox/title")
    title_node.set_text(message..notif_count)
    local timer_node = notification.get_node("margin/vbox/timer")
    timer_node.set_wait_time(duration)

    local obj_parent_node = InterfaceManager.GET(parent_node)

    if parent_list[parent_node] == nil then
        local notif_list = VBoxContainer.new()
        parent_list[parent_node] = {}
        parent_list[parent_node].notif_list = notif_list
        obj_parent_node.add_child(parent_list[parent_node].notif_list)
        notif_list.set_mouse_filter(Control.MOUSE_FILTER_IGNORE)
        notif_list.add_theme_constant_override("separation", 10)
        notif_list.offset_left = 0
        notif_list.offset_right = 0
        notif_list.set_anchor(SIDE_BOTTOM, 1, false, true)
        notif_list.offset_top = 0
        notif_list.offset_bottom = 0
        notif_list.set_alignment(BoxContainer.ALIGNMENT_END)
    end

    local notif_list = parent_list[parent_node].notif_list

    notif_list.add_child(notification)


    notification.mouse_entered.connect(
        function()
            timer_node.paused = true
        end
    )

    notification.mouse_exited.connect(
        function()
            timer_node.paused = false
        end
    )
    timer_node.timeout.connect(
        function()
            notification.call_deferred("hide")

            local hide_timer = Timer.new()
            hide_timer.set_wait_time(math.random(5, 20))
            hide_timer.autostart = true
            hide_timer.set_one_shot(true)
            notification.add_child(hide_timer)
            hide_timer.timeout.connect( -- We do this otherwise the software will crash (Godot bug)
                function()
                    notif_list.remove_child(notification)
                end
            )
        end
    )

end

function create_theme()

    theme_notification = Theme.new()

    local test_panel = StyleBoxFlat.new()
    test_panel.set_bg_color(Color(0,0,0,.5))
    test_panel.set_corner_radius_all(0)
    test_panel.corner_radius_top_left = 2
    test_panel.corner_radius_bottom_left = 2
    test_panel.border_width_left = 2
    test_panel.expand_margin_left = 4

    local info_panel = test_panel.duplicate()
    info_panel.border_color = Color(.5, .5, .9, 1)

    local warning_panel = test_panel.duplicate()
    warning_panel.border_color = Color(.9, .9, .5, 1)

    local error_panel = test_panel.duplicate()
    error_panel.border_color = Color(.9, .5, .5, 1)

    theme_notification.set_stylebox("panel", "PanelContainer", StyleBoxEmpty.new())
    theme_notification.set_stylebox("panel", "info_panel", info_panel)
    theme_notification.set_stylebox("panel", "warning_panel", warning_panel)
    theme_notification.set_stylebox("panel", "error_panel", error_panel)

    theme_notification.set_constant("margin_top", "MarginContainer", 0)
    theme_notification.set_constant("margin_left", "MarginContainer", 10)

    theme_notification.set_type_variation("info_panel", "Panel")
    theme_notification.set_type_variation("warning_panel", "Panel")
    theme_notification.set_type_variation("error_panel", "Panel")
end

function create_ui()

    create_info()
    create_warning()
    create_error()

end

-- UI functions parts

function test_process(delta)
    print("Hi!")
end

function create_info()

    local popup_panel = PanelContainer.new()
    popup_panel.set_theme(theme_notification)
    popup_panel.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    local margin_panel = MarginContainer.new()
    popup_panel.add_child(margin_panel)
    margin_panel.set_name("margin")
    margin_panel.set_custom_minimum_size(Vector2(300, 50))

    local background_notif = Panel.new()
    margin_panel.add_child(background_notif)
    background_notif.set_name("background")
    background_notif.set_theme_type_variation("info_panel")

    local vbox_panel = VBoxContainer.new()
    margin_panel.add_child(vbox_panel)
    vbox_panel.set_name("vbox")

    local label_text = Label.new()
    vbox_panel.add_child(label_text)
    label_text.set_text("Test")
    label_text.set_name("title")

    local timer = Timer.new()
    vbox_panel.add_child(timer)
    timer.set_wait_time(5)
    timer.autostart = true
    timer.set_one_shot(true)
    timer.set_name("timer")

    InterfaceManager.ADD("notification_system/notification_info", popup_panel)

end

function create_warning()

    local popup_panel = PanelContainer.new()
    popup_panel.set_theme(theme_notification)
    popup_panel.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    local margin_panel = MarginContainer.new()
    popup_panel.add_child(margin_panel)
    margin_panel.set_name("margin")
    margin_panel.set_custom_minimum_size(Vector2(300, 50))

    local background_notif = Panel.new()
    margin_panel.add_child(background_notif)
    background_notif.set_name("background")
    background_notif.set_theme_type_variation("warning_panel")

    local vbox_panel = VBoxContainer.new()
    margin_panel.add_child(vbox_panel)
    vbox_panel.set_name("vbox")

    local label_text = Label.new()
    vbox_panel.add_child(label_text)
    label_text.set_text("Test")
    label_text.set_name("title")

    local timer = Timer.new()
    vbox_panel.add_child(timer)
    timer.set_wait_time(5)
    timer.autostart = true
    timer.set_one_shot(true)
    timer.set_name("timer")

    InterfaceManager.ADD("notification_system/notification_warning", popup_panel)
end

function create_error()

    local popup_panel = PanelContainer.new()
    popup_panel.set_theme(theme_notification)
    popup_panel.set_h_size_flags(Control.SIZE_EXPAND_FILL)

    local margin_panel = MarginContainer.new()
    popup_panel.add_child(margin_panel)
    margin_panel.set_name("margin")
    margin_panel.set_custom_minimum_size(Vector2(300, 50))

    local background_notif = Panel.new()
    margin_panel.add_child(background_notif)
    background_notif.set_name("background")
    background_notif.set_theme_type_variation("error_panel")

    local vbox_panel = VBoxContainer.new()
    margin_panel.add_child(vbox_panel)
    vbox_panel.set_name("vbox")

    local label_text = Label.new()
    vbox_panel.add_child(label_text)
    label_text.set_text("Test")
    label_text.set_name("title")

    local timer = Timer.new()
    vbox_panel.add_child(timer)
    timer.set_wait_time(5)
    timer.autostart = true
    timer.set_one_shot(true)
    timer.set_name("timer")

    InterfaceManager.ADD("notification_system/notification_error", popup_panel)
end

local project_main

function start()

    create_ui()

    return true
end

function create_ui()
    if has_permission("LOGGER") then
        Logger.log("CorePlugin", "Loading project manager...")

        project_main = Window.new()
        project_main.set_title("Project Manager")
        project_main.set_exclusive(true)
        project_main.set_min_size(Vector2(840, 480))

        InterfaceManager.ADD("project_manager", project_main)
        
        InterfaceManager.GET("main").add_child(project_main)

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
    end
end

function _on_ready()
    project_main.popup_centered()
end

function stop()

    return true
end
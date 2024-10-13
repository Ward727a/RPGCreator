
local test_window

function start()

    if has_permission("LOGGER") then
        Logger.log("CorePlugin", "Importing project manager...")
        if import_lua("./project_manager/main.lua") then
            Logger.log("CorePlugin", "Project manager imported successfully")
        else
            Logger.error("CorePlugin", "Failed to import project manager")
        end
    end

    return true
end

function stop()

    Logger.error("CorePlugin", "!!! CorePlugin stopped !!!")

    return true
end
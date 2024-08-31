extends Base
class_name settings
## This is the base class for ALL settings in the engine
##
## Settings are used to store and manage the engine settings (not the game settings! For the game settings, go to the "game_settings" class [gameSettings.gd])

# Engine settings

const base_settings: Dictionary = {
    "version": "0.0.1",
    "name": "RPG Creator",
    "size": Vector2(1280, 720),
    "maximized": false,
    "debug": false,
    "last_project": ""
}

var engine_settings: Dictionary = {}

var engine_settings_path: String = "user://engine_settings.json"

func _init():
    set_custom_name("settings")

    allowed_events = [
        "pre_setsetting", "post_setsetting",
        "getsetting",
        "pre_savesettings", "post_savesettings",
        "pre_loadsettings", "post_loadsettings",
        "getsettings"
    ]

    load_settings()

    super()

# Load the engine settings
func load_settings():

    # Emit the pre_loadsettings event
    call_event("pre_loadsettings", [])

    var file = FileAccess.open(engine_settings_path, FileAccess.READ)

    if FileAccess.get_open_error() != OK:
        
        error(str("Error loading engine settings: ", FileAccess.get_open_error()))

        engine_settings = base_settings

        call_event("post_loadsettings", [engine_settings, false])
        return
    
    var json = JSON.new()

    var data = json.parse(file.get_as_text())

    if data.error != OK:
        error(str("Error parsing engine settings: ", data.error))

        engine_settings = base_settings

        call_event("post_loadsettings", [engine_settings, false])
        return
    
    engine_settings = data.result

    file.close()

    call_event("post_loadsettings", [engine_settings, true])

# Save the engine settings
func save_settings():

    # Emit the pre_savesettings event
    call_event("pre_savesettings", [engine_settings])

    var file = FileAccess.open(engine_settings_path, FileAccess.WRITE)

    if FileAccess.get_open_error() != OK:
        
        error(str("Error saving engine settings: ", FileAccess.get_open_error()))
        call_event("post_savesettings", [engine_settings, false])
        return
    
    var json = JSON.new()

    var data = json.print(engine_settings)

    if data.error != OK:
        error(str("Error saving engine settings: ", data.error))
        call_event("post_savesettings", [engine_settings, false]) # We return false because the settings were not saved due to an error
        return
    
    file.store_string(data.result)

    file.close()

    call_event("post_savesettings", [engine_settings, true])

# Set a setting
func set_setting(_setting: String, _value: Variant):

    # Emit the pre_setsetting event
    call_event("pre_setsetting", [_setting, _value])

    engine_settings[_setting] = _value

    # Emit the post_setsetting event
    call_event("post_setsetting", [_setting, _value])

# Get a setting
func get_setting(_setting: String):

    # Emit the pre_getsetting event
    call_event("getsetting", [_setting])

    return engine_settings[_setting]

# Get all settings
func get_settings():

    # Emit the getsettings event
    call_event("getsettings", [engine_settings])

    return engine_settings
class_name DataRegister
# This class is used to register all the temporary data of the project.
# This can be used to store data that needs to be accessed from different parts of the project.

## Signal to notify that the data has been cleared[br]
## Parameters: [br]
## - context: String[br]
static var data_cleared: Signal = Utils.make_static_signal(DataRegister, "data_cleared")
## Signal to notify that the data has been removed [br]
## [br]
## Parameters: [br]
## - context: String[br]
## - key: String[br]
static var data_removed: Signal = Utils.make_static_signal(DataRegister, "data_cleared")
## Signal to notify that the data has been registered[br]
## [br]
## Parameters: [br]
## - context: String[br]
## - key: String[br]
## - value: Variant[br]
## - origin: String[br]
static var data_registered: Signal = Utils.make_static_signal(DataRegister, "data_cleared")

static var data: Dictionary = {}

## Register data in the data register[br]
## [br]
## Parameters:[br]
## - _context: The context in which the data is stored[br]
## - _key: The key of the data[br]
## - _value: The value of the data[br]
## - _origin(optional, default: ""): The origin of the data, this can be used for example to avoid infinite loops with signals
static func register_data(_context: String, _key: String, _value: Variant, _origin: String = ""):
    if data.has(_context):
        data[_context][_key] = _value
    else:
        data[_context] = {_key: _value}
    
    data_registered.emit(_context, _key, _value, _origin)

## Get data from the data register[br]
##[br]
## Parameters:[br]
## - _context: The context in which the data is stored[br]
## - _key: The key of the data
static func get_data(_context: String, _key: String) -> Variant:
    if data.has(_context):
        if data[_context].has(_key):
            return data[_context][_key]
        else:
            return null
    else:
        return null

## Get all data from the data register[br]
##[br]
## Parameters:[br]
## - _context: The context in which the data is stored[br]
static func get_all_data(_context: String) -> Dictionary:
    if data.has(_context):
        return data[_context]
    else:
        return {}

## Remove data from the data register[br]
##[br]
## Parameters:[br]
## - _context: The context in which the data is stored[br]
## - _key: The key of the data[br]
static func remove_data(_context: String, _key: String):
    if data.has(_context):
        if data[_context].has(_key):
            data[_context].erase(_key)
            data_removed.emit(_context, _key)
        else:
            push_error("Key not found")
    else:
        push_error("Context not found")

## Clear all data from the data register[br]
##[br]
## Parameters:[br]
## - _context: The context in which the data is stored[br]
static func clear_data(_context: String):
    if data.has(_context):
        data.erase(_context)
        data_cleared.emit(_context)
    else:
        push_error("Context not found")

## Clear all data from the data register
static func clear_all_data():
    data.clear()
    data_cleared.emit("all")

## Get all contexts from the data register
static func get_all_contexts() -> Array:
    return data.keys()

## Get all keys from the data register[br]
##[br]
## Parameters:[br]
## - _context: The context in which the data is stored[br]
static func get_all_keys(_context: String) -> Array:
    if data.has(_context):
        return data[_context].keys()
    else:
        return []

## Get all values from the data register[br]
##[br]
## Parameters:[br]
## - _context: The context in which the data is stored[br]
static func get_all_values(_context: String) -> Array:
    if data.has(_context):
        return data[_context].values()
    else:
        return []

## Get all data as a string[br]
##[br]
## Parameters:[br]
## - _context: The context in which the data is stored[br]
static func get_all_data_as_string(_context: String) -> String:
    var data_string = ""
    if data.has(_context):
        for key in data[_context].keys():
            data_string += key + ": " + str(data[_context][key]) + "\n"
    return data_string

## Check if the data register has a context[br]
##[br]
## Parameters:[br]
## - _context: The context in which the data is stored[br]
static func has_context(_context: String) -> bool:
    return data.has(_context)

## Check if the data register has a key[br]
##[br]
## Parameters:[br]
## - _context: The context in which the data is stored[br]
## - _key: The key of the data[br]
static func has_key(_context: String, _key: String) -> bool:
    if data.has(_context):
        return data[_context].has(_key)
    else:
        return false
extends Object
class_name Base
# This class NEED to be inherited by all classes in the engine

var custom_name = "" # Custom name for the object

var tags: Array = [] # Tags for the object
var metadata: Dictionary = {} # Metadata for the object

var allowed_events: Array = [] # Allowed events for the object (e.g. on_hit, on_death, on_spawn, etc.)
var events: Dictionary = {} # Events assigned to the object

var register: Register

var need_register: bool = true # If the object need to be registered

## Initialize the object to register the class to the register (Use "super" to call it) - You need to call this to be able to use the register
func _init(_custom_name: String = ""):

    if need_register:
        if custom_name == "":
            if _custom_name != "":
                custom_name = _custom_name
            else:
                return
        
        register = Register.get_instance()

        register.register_class(self)

        allowed_events.append_array(["pre_unregister", "post_unregister", "pre_register", "post_register"])

#####################################
# Custom logging functions          #
# For now it just print the message #
#####################################
# Later on they will be used to log the messages to a file, or to a custom log system
# Maybe to the debug screen? TBD

## Push a warning message
func warn(message: String) -> void:
    push_warning(message)

## Push an error message
func error(message: String) -> void:
    push_error(message)

## Push a log message
func write(message: String) -> void:
    print(message)

#################
# Tag functions #
#################

## Check if the tags array has a specific tag
func has_tag(tag: String) -> bool:
    return tags.has(tag)

## Set the tags array
func set_tags(_tags: Array) -> void:
    tags = _tags

## Add a tag to the tags array
func add_tag(tag: String) -> void:
    tags.append(tag)

## Add multiple tags to the tags array
func add_all_tags(_tags: Array) -> void:
    tags.append_array(_tags)

## Remove a tag from the tags array
func remove_tag(tag: String) -> void:
    tags.erase(tag)

## Get all tags from the tags array
func get_tags() -> Array:
    return tags

######################
# Metadata functions #
######################

## Check if the metadata dictionary has a specific key
func has_metadata(key: String) -> bool:
    return metadata.has(key)

## Set the metadata dictionary
func set_metadata(data: Dictionary) -> void:
    metadata = data

## Add a metadata to the metadata dictionary
func add_metadata(key: String, value: Variant) -> void:
    metadata[key] = value

## Get a metadata from the metadata dictionary
func get_metadata(key: String) -> Variant:
    return metadata[key]

## Remove a metadata from the metadata dictionary
func remove_metadata(key: String) -> void:
    metadata.erase(key)

## Get the metadata dictionary
func get_all_metadata() -> Dictionary:
    return metadata

###################
# Event functions #
###################

## Add an event to the events dictionary
func add_event(key: String, value: Callable) -> void:

    if !allowed_events.has(key):
        error(str("Event ", key, " is not allowed, allowed events are: ", allowed_events))
        return

    if !events.has(key):
        events[key] = []
    
    events[key].append(value)

## Get all events from the events dictionary
func get_event(key: String) -> Callable:
    return events[key]

## Remove all events from the events dictionary
func remove_event(key: String) -> void:
    events.erase(key)

## Remove a specific event from the events dictionary
func remove_specific_event(key: String, value: Callable) -> void:
    if events.has(key):
        for i in range(events[key].size()):
            if events[key][i] == value:
                events[key].erase(i)
                break

## Call an event from the events dictionary
func call_event(key: String, args: Array) -> void:

    if !allowed_events.has(key):
        error(str("Event ", key, " is not allowed, allowed events are: ", allowed_events))
        return

    if events.has(key):
        for i in range(events[key].size()):
            events[key][i].call(args)

#####################
# Utility functions #
#####################

## Get the class custom name (it will be automatically converted to lower case)
func get_custom_name() -> String:
    return custom_name.to_lower()

## Set the class custom name (it will be automatically converted to lower case)
func set_custom_name(name: String) -> void:
    custom_name = name.to_lower()

## Check if the class has a custom name
func has_custom_name() -> bool:
    return custom_name != ""

## Register class notify the class that it will be unregistered[br]
## [br]
## For the [Register] class to work this function NEED to emit the post_unregister
func notify_unregister() -> void:
    call_event("post_unregister", [self])

## Register class notify the class that it is registered
func notifiy_registered() -> void:
    pass
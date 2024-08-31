extends RefCounted
class_name BaseStorage

func get_name() -> String:
    return "NaN"

func get_description() -> String:
    return "This is the base class for all storage classes in the game."

func store_all_data(_data: Variant, _identifier: String, _ref: String) -> Variant:
    return {}

func get_all_data(_identifier: String, _ref: String) -> Variant:
    return ""

func load_from_file(_file_path: String) -> Variant:
    return ""
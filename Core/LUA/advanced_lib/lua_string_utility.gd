class_name LUA_StringUtility

static func begins_with(text: String, start: String) -> bool:
	
	return text.begins_with(start)

static func replace(text: String, search_for: String, replacement: String) -> String:
	
	return text.replace(search_for, replacement)

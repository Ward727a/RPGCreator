extends Resource
class_name ResPermission

var permissions: Dictionary

func HAS(permission_path: String, strict_check: bool = true) -> bool:

	var path_part: PackedStringArray = permission_path.split(':')
	var last_part: String = path_part[path_part.size() - 1]

	var data = permissions

	for part in path_part:

		if data.has(part):
			if part == last_part:
				return true
			data = data[part]
			continue
		
		break
	
	if !strict_check and data != permissions:
		return true
	
	return false

func SET(permission_path: String):

	permission_path = permission_path.to_upper()

	var path_part: PackedStringArray = permission_path.split(':')

	var data = permissions

	for part in path_part:

		if data.has(part):
			data = data[part]
			continue

		data[part] = {}
		data = data[part]

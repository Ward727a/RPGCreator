extends BaseStorage
class_name JsonStorage

var directory = "user://data"

func get_name() -> String:
	return "JsonStorage"

func get_description() -> String:
	return "This is the class for storing data in JSON format."

# Store all data to a file (data is of Variant type due to the fact that some data might need to be given in a string and not as a dictionary)
func store_all_data(data: Variant, identifier: String, ref: String) -> Variant:
	var file_path = get_file_path(identifier, ref)
	
	var file = FileAccess.open(file_path, FileAccess.WRITE)
	
	if file != null:

		if data is String:
			file.store_string(data.replace("\n", "\\n").replace("\t", "\\t").replace("\r", "\\r").replace("\f", "\\f").replace("\b", "\\b"))
		else:
			file.store_string(var_to_str(data).replace("\n", "\\n").replace("\t", "\\t").replace("\r", "\\r").replace("\f", "\\f").replace("\b", "\\b"))
		
		print("saving: ", data)
		
		file.close()
	
	return data

func get_all_data(identifier: String, ref: String) -> Variant:
	var file_path = get_file_path(identifier, ref)

	print("File path: ", file_path)
	
	return load_from_file(file_path)

func load_from_file(file_path: String) -> Variant:    
	var file = FileAccess.open(file_path, FileAccess.READ)

	if FileAccess.file_exists(file_path) and file != null:
		var content = file.get_as_text().replace("\\n", "\n").replace("\\t", "\t").replace("\\r", "\r").replace("\\f", "\f").replace("\\b", "\b")
		
		print("Content: ", file.get_as_text())
		file.close()

		return str_to_var(content)
	else:
		return null

func get_possible_ref(identifier: String) -> Array:
	var dir_path = get_directory(identifier)
	
	var dir = DirAccess.open(dir_path)
	
	var refs = []
	
	dir.list_dir_begin()

	while true:
		var file = dir.get_next()
		
		if file == "":
			break
		
		if file.ends_with(".rpgcdat"):
			refs.append(file)
	
	dir.list_dir_end()
	
	return refs


func get_directory(identifier: String) -> String:
	
	var dir_path = ""

	if directory.ends_with("//"):
		dir_path = directory + identifier + "/"
	else:
		dir_path = directory + "/" + identifier + "/"
	
	if not DirAccess.dir_exists_absolute(dir_path):
		DirAccess.make_dir_recursive_absolute(dir_path)
	
	return dir_path

# Get file path to store data (identifier = directory name, ref = file name (Objects, Entities, etc.))
func get_file_path(identifier: String, ref: String) -> String:
	return get_directory(identifier) + ref + ".rpgcdat"

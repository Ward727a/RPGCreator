class_name LUA_FileUtility

var file_permission: FileAccess.ModeFlags
var file_compress_permission: FileAccess.ModeFlags
var file_encrypt_permission: FileAccess.ModeFlags

var plugin: ResPlugin
var limited: bool = true
var logger: Logger

func _init(_plugin: ResPlugin):
	
	plugin = _plugin
	
	logger = Logger.new(str("%s - FileUtility" % _plugin.name))
	
	# Normal files
	if (plugin.has_allowed("FILE:READ") and plugin.has_allowed("FILE:WRITE")) or plugin.has_allowed('FILE'):
		file_permission = FileAccess.READ_WRITE
	elif plugin.has_allowed("FILE:READ"):
		file_permission = FileAccess.READ
	elif plugin.has_allowed("FILE:WRITE"):
		file_permission = FileAccess.WRITE
	
	# Encrypted files
	if (plugin.has_allowed('FILE:READ_ENCRYPT') and plugin.has_allowed('FILE:WRITE_ENCRYPT')) or plugin.has_allowed('FILE'):
		file_encrypt_permission = FileAccess.READ_WRITE
	elif plugin.has_allowed('FILE:READ_ENCRYPT'):
		file_encrypt_permission = FileAccess.READ
	elif plugin.has_allowed('FILE:WRITE_ENCRYPT'):
		file_encrypt_permission = FileAccess.WRITE
	
	# Compressed files
	if (plugin.has_allowed('FILE:READ_COMPRESS') and plugin.has_allowed('FILE:WRITE_COMPRESS')) or plugin.has_allowed('FILE'):
		file_compress_permission = FileAccess.READ_WRITE
	elif plugin.has_allowed('FILE:READ_COMPRESS'):
		file_compress_permission = FileAccess.READ
	elif plugin.has_allowed('FILE:WRITE_COMPRESS'):
		file_compress_permission = FileAccess.WRITE

func lua_fields():
	return ['_all_can_read', '_all_can_write', 'file_compress_permission', 'file_encrypt_permission', 'file_permission', 'plugin', 'limited', 'logger', '_init', 'new', '_is_allowed_path', 'link_plugin']

# all only allowed to use for software

func _all_can_read():
	
	return (can_read() or can_read_compress() or can_read_encrypt())

func _all_can_write():
	
	return (can_write() or can_write_compress() or can_write_encrypt())

# Normal files

func can_read() -> bool:
	if file_permission == FileAccess.READ_WRITE or file_permission == FileAccess.READ:
		return true
	logger.error('Plugin doesn\'t have the permission to read!')
	return false

func can_write() -> bool:
	if file_permission == FileAccess.READ_WRITE or file_permission == FileAccess.WRITE:
		return true
	logger.error('Plugin doesn\'t have the permission to write!')
	return false

# Encrypted files

func can_read_encrypt() -> bool:
	if file_encrypt_permission == FileAccess.READ_WRITE or file_encrypt_permission == FileAccess.READ:
		return true
	logger.error("Plugin doesn't have the permission to read encrypted file!")
	return false

func can_write_encrypt() -> bool:
	if file_encrypt_permission == FileAccess.READ_WRITE or file_encrypt_permission == FileAccess.WRITE:
		return true
	logger.error("Plugin doesn't have the permission to write encrypted file!")
	return false

# Compressed files

func can_read_compress() -> bool:
	if file_compress_permission == FileAccess.READ_WRITE or file_compress_permission == FileAccess.READ:
		return true
	logger.error("Plugin doesn't have the permission to read compressed file!")
	return false

func can_write_compress() -> bool:
	if file_compress_permission == FileAccess.READ_WRITE or file_compress_permission == FileAccess.WRITE:
		return true
	logger.error("Plugin doesn't have the permission to write compressed file!")
	return false

func _convert_path(path: String) -> String:
	if path.begins_with("//"):
		return path.replace("//", str(plugin.path, "/"))
	return path

func _is_allowed_path(path: String, can_write: bool = false) -> bool:
	print(plugin.path)
	
	if limited:
		
		if path.begins_with(plugin.path):
			
			# We do the check here so the plugin can't know if the file exist 
			# if it doesn't have the permission to access all files.
			if FileAccess.file_exists(path) or can_write:
				return true
			
			logger.error('Path "%s" doesn\'t exist!' % path)
			return false
			
		logger.error('Path "%s" not allowed!' % path)
		return false
	
	if FileAccess.file_exists(path) or can_write:
		return true
	
	logger.error('Path "%s" doesn\'t exist!' % path)
	return false

func open_file(path: String) -> FileAccess:

	if !can_read() and !can_write():
		return null
	
	path = _convert_path(path)
	
	if !_is_allowed_path(path, can_write()):
		return null
	
	return FileAccess.open(path, file_permission)

func get_file_as_text(path: String) -> String:
	
	if !_all_can_read():
		return ''
	
	path = _convert_path(path)
	
	if !_is_allowed_path(path):
		return ''
	
	return FileAccess.get_file_as_string(path)

func get_file_as_bytes(path: String) -> Array:
	
	if !_all_can_read():
		return []
	
	path = _convert_path(path)
	
	if !_is_allowed_path(path):
		return []
	
	return FileAccess.get_file_as_bytes(path)

func is_file_hidden(path: String) -> bool:
	
	if !_all_can_read():
		return false
	
	path = _convert_path(path)
	
	if !_is_allowed_path(path):
		return false
	
	return FileAccess.get_hidden_attribute(path)

func set_file_hidden(path: String, hide: bool):
	
	if !_all_can_write():
		return
	
	path = _convert_path(path)
	
	if !_is_allowed_path(path):
		return
	
	FileAccess.set_hidden_attribute(path, hide)

func is_file_read_only(path: String) -> bool:
	
	if !_all_can_read():
		return false
	
	path = _convert_path(path)
	
	if !_is_allowed_path(path):
		return false
	
	return FileAccess.get_read_only_attribute(path)

func set_file_read_only(path: String, read_only: bool):
	
	if !_all_can_write():
		return
	
	path = _convert_path(path)
	
	if !_is_allowed_path(path):
		return
	
	FileAccess.set_read_only_attribute(path, read_only)

func is_file_exist(path: String) -> bool:
	
	if !_all_can_read() and !_all_can_write():
		return false
	
	path = _convert_path(path)
	
	if !_is_allowed_path(path):
		return false
	
	return FileAccess.file_exists(path)

func open_encrypt(path: String, key: Array) -> FileAccess:
	
	if !can_read_encrypt() and !can_write_encrypt():
		return null
	
	path = _convert_path(path)
	
	if !_is_allowed_path(path, can_write_encrypt()):
		return null
	
	return FileAccess.open_encrypted(path, file_encrypt_permission, key)

func open_encrypt_with_pass(path: String, password: String) -> FileAccess:
	
	if !can_read_encrypt() and !can_write_encrypt():
		return null
	
	path = _convert_path(path)
	
	if !_is_allowed_path(path, can_write_encrypt()):
		return null
	
	return FileAccess.open_encrypted_with_pass(path, file_encrypt_permission, password)

func open_compressed(path: String, compression_mode: FileAccess.CompressionMode):
	
	if !can_read_compress() and !can_write_compress():
		return null
	
	path = _convert_path(path)
	
	if !_is_allowed_path(path, can_write_compress()):
		return null
	
	return FileAccess.open_compressed(path, file_compress_permission, compression_mode)

func get_md5(path: String) -> String:
	
	if !_all_can_read():
		return ""
	
	path = _convert_path(path)
	
	if !_is_allowed_path(path):
		return ""
	
	return FileAccess.get_md5(path)

func get_modified_time(path: String) -> int:
	
	if !_all_can_read():
		return -1
	
	path = _convert_path(path)
	
	if !_is_allowed_path(path):
		return -1
	
	return FileAccess.get_modified_time(path)

static func link_plugin(plugin: ResPlugin):
	
	if plugin.has_allowed("FILE"):
		var file_utility = LUA_FileUtility.new(plugin)
		file_utility.limited = true
		plugin.add_link("file_utility", file_utility)
		return
	
	if (
		plugin.has_allowed("FILE:READ") or 
		plugin.has_allowed("FILE:WRITE") or 
		plugin.has_allowed('FILE:READ_COMPRESS') or 
		plugin.has_allowed('FILE:WRITE_COMPRESS') or 
		plugin.has_allowed('FILE:READ_ENCRYPT') or 
		plugin.has_allowed('FILE:WRITE_ENCRYPT')):
			
			var file_utility = LUA_FileUtility.new(plugin)
			
			if plugin.has_allowed('FILE:ALL_FILES'):
				file_utility.limited = false
			
			plugin.add_link("file_utility", file_utility)
			return

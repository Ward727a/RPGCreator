extends SoundManagerModule

####################################################################
#	SOUND MANAGER MODULE FOR GODOT 4
#			Version 5.0
#			© Xecestel
####################################################################
#
# This Source Code Form is subject to the terms of the MIT License.
# If a copy of the license was not distributed with this
# file, You can obtain one at https://mit-license.org/.
#
#####################################################################

# Variables

@export var Default_Sounds_Properties : Dictionary = {
	"BGM" : {
		"Volume" : 0,
		"Pitch" : 1,
	},
	"BGS" : {
		"Volume" : 0,
		"Pitch" : 1,
	},
	"SFX" : {
		"Volume" : 0,
		"Pitch" : 1,
	},
	"MFX" : {
		"Volume" : 0,
		"Pitch" : 1,
	},
}

var _preload_resources : bool = false
var _enable_nodes_preinstantiation : bool = false
var _debug : bool = false

@onready var Audiostreams : Array = get_children()
@onready var soundmgr_dir_rel_path : String = get_script().get_path().get_base_dir()

var _playing_sounds : Array = []
var _bgm_playing : String
var _bgs_playing : Array = [ "BGS0" ]
var _se_playing : Array = [ "SFX0" ]
var _me_playing : Array = [ "MFX0" ]

var _Audio_Busses : Dictionary = {
	"BGM" : "Master",
	"BGS" : "Master",
	"SFX" : "Master",
	"MFX" : "Master",
}

var Preloaded_Resources : Dictionary = {}
var Instantiated_Nodes : Array = []

##################

# Methods


##################################################
#				SOUNDS HANDLING					 #
# Use this methods to handle sounds in your game #
##################################################

# Plays a given BGM
func play_bgm(bgm : String, from_position : float = 0.0, volume_db : float = -81, pitch_scale : float = -1, sound_to_override : String = "") -> void:
	if bgm != "" and bgm != null:
		_play_deferred("BGM", bgm, from_position, volume_db, pitch_scale, sound_to_override)
	elif _debug:
		print_debug("No sound selected.")


# Plays a given BGS
func play_bgs(bgs : String, from_position : float = 0.0, volume_db : float = -81, pitch_scale : float = -1, sound_to_override : String = "") -> void:
	if bgs != "" and bgs != null:
		_play_deferred("BGS", bgs, from_position, volume_db, pitch_scale, sound_to_override)
	elif _debug:
		print_debug("No BGS selected.")


# Plays selected Sound Effect
func play_sfx(sound_effect : String, from_position : float = 0.0, volume_db : float = -81, pitch_scale : float = -1, sound_to_override : String = "") -> void:
	if sound_effect != "" and sound_effect != null:
		_play_deferred("SFX", sound_effect, from_position, volume_db, pitch_scale, sound_to_override)
	elif _debug:
		print_debug("No sound effect selected.")


# Play a given Music Effect
func play_mfx(music_effect : String, from_position : float = 0.0, volume_db : float = -81, pitch_scale : float = -1, sound_to_override : String = "") -> void:
	if music_effect != "" and music_effect != null:
		_play_deferred("MFX", music_effect, from_position, volume_db, pitch_scale, sound_to_override)
	elif _debug:
		print_debug("No sound selected.")


# Stops selected Sound
func stop(sound : String) -> void:
	var sound_index = 0
	if sound != "" and sound != null:
		if is_playing(sound):
			sound_index = find_sound(sound)
			if sound_index >= 0:
				Audiostreams[sound_index].stop()
				if not _enable_nodes_preinstantiation:
					_erase_sound(sound)
					_playing_sounds.erase(sound)
			elif _debug:
				print_debug("No sound found: " + sound)
	elif _debug:
		print_debug("No sound selected")


# Stops all playing sounds
func stop_all() -> void:
	var playing_sounds = _playing_sounds.duplicate()
	for sound in playing_sounds:
		stop(sound)


func pause_all() -> void:
	for sound in _playing_sounds:
		pause(sound)


func unpause_all() -> void:
	for sound in _playing_sounds:
		unpause(sound)


# Fades in a given BGM
func fade_in_bgm(sound : String, duration : float, from_position : float = 0.0, volume_db : float = -81, pitch_scale : float = -1, sound_to_override : String = "") -> void:
	if sound == "" or sound == null:
		if _debug:
			print_debug("No sound selected")
		return
	
	if is_playing(sound):
		stop(sound)
	
	play_bgm(sound, from_position, -60, pitch_scale, sound_to_override)
	_fade_in_deferred("BGM", sound, duration, from_position, volume_db, pitch_scale, sound_to_override)


# Fades in a given BGS
func fade_in_bgs(sound : String, duration : float, from_position : float = 0.0, volume_db : float = -81, pitch_scale : float = -1, sound_to_override : String = "") -> void:
	if sound == "" or sound == null:
		if _debug:
			print_debug("No sound selected")
		return
	
	if is_playing(sound):
		stop(sound)
	
	play_bgs(sound, from_position, -60, pitch_scale, sound_to_override)
	_fade_in_deferred("BGS", sound, duration, from_position, volume_db, pitch_scale, sound_to_override)


# Fades in a given SFX
func fade_in_sfx(sound : String, duration : float, from_position : float = 0.0, volume_db : float = -81, pitch_scale : float = -1, sound_to_override : String = "") -> void:
	if sound == "" or sound == null:
		if _debug:
			print_debug("No sound selected")
		return
	
	if is_playing(sound):
		stop(sound)
	
	play_sfx(sound, from_position, -60, pitch_scale, sound_to_override)
	_fade_in_deferred("SFX", sound, duration, from_position, volume_db, pitch_scale, sound_to_override)


# Fades in a given MFX
func fade_in_mfx(sound : String, duration : float, from_position : float = 0.0, volume_db : float = -81, pitch_scale : float = -1, sound_to_override : String = "") -> void:
	if sound == "" or sound == null:
		if _debug:
			print_debug("No sound selected")
		return
	
	if is_playing(sound):
		stop(sound)
	
	play_mfx(sound, from_position, -60, pitch_scale, sound_to_override)
	_fade_in_deferred("MFX", sound, duration, from_position, volume_db, pitch_scale, sound_to_override)


func fade_into_bgm(sound : String, sound_to_overwrite : String, duration : float, from_position : float = 0.0, volume_db : float = -81, pitch_scale : float = -1) -> void:
	if sound == "" or sound == null:
		if _debug:
			print_debug("No new sound selected")
		return
	
	if sound_to_overwrite == "" or sound_to_overwrite == null:
		if _debug:
			print_debug("No old sound selected")
		return
	
	if not is_playing(sound_to_overwrite):
		if _debug:
			print_debug("Sound to overwrite not found")
		return
	
	fade_out(sound_to_overwrite, duration/2)
	await get_tree().create_timer(duration/2).timeout
	fade_in_bgm(sound, duration/2, from_position, volume_db, pitch_scale)


func fade_into_bgs(sound : String, sound_to_overwrite : String, duration : float, from_position : float = 0.0, volume_db : float = -81, pitch_scale : float = -1) -> void:
	if sound == "" or sound == null:
		if _debug:
			print_debug("No new sound selected")
		return
	
	if sound_to_overwrite == "" or sound_to_overwrite == null:
		if _debug:
			print_debug("No old sound selected")
		return
	
	if not is_playing(sound_to_overwrite):
		if _debug:
			print_debug("Sound to overwrite not found")
		return
	
	fade_out(sound_to_overwrite, duration/2)
	await get_tree().create_timer(duration/2).timeout
	fade_in_bgs(sound, duration/2, from_position, volume_db, pitch_scale)


func fade_into_sfx(sound : String, sound_to_overwrite : String, duration : float, from_position : float = 0.0, volume_db : float = -81, pitch_scale : float = -1) -> void:
	if sound == "" or sound == null:
		if _debug:
			print_debug("No new sound selected")
		return
	
	if sound_to_overwrite == "" or sound_to_overwrite == null:
		if _debug:
			print_debug("No old sound selected")
		return
	
	if not is_playing(sound_to_overwrite):
		if _debug:
			print_debug("Sound to overwrite not found")
		return
	
	fade_out(sound_to_overwrite, duration/2)
	await get_tree().create_timer(duration/2).timeout
	fade_in_sfx(sound, duration/2, from_position, volume_db, pitch_scale)


func fade_into_mfx(sound : String, sound_to_overwrite : String, duration : float, from_position : float = 0.0, volume_db : float = -81, pitch_scale : float = -1) -> void:
	if sound == "" or sound == null:
		if _debug:
			print_debug("No new sound selected")
		return
	
	if sound_to_overwrite == "" or sound_to_overwrite == null:
		if _debug:
			print_debug("No old sound selected")
		return
	
	if not is_playing(sound_to_overwrite):
		if _debug:
			print_debug("Sound to overwrite not found")
		return
	
	fade_out(sound_to_overwrite, duration/2)
	await get_tree().create_timer(duration/2).timeout
	fade_in_mfx(sound, duration/2, from_position, volume_db, pitch_scale)


# Fades out a given sound
func fade_out(sound : String, duration : float) -> void:
	if sound == "" or sound == null:
		if _debug:
			print_debug("No sound selected")
		return
		
	var tween = get_tree().create_tween()
	var sound_index = find_sound(sound)
	
	if sound_index < 0:
		if _debug:
			print_debug("No sound found: " + sound)
		return
	
	var sound_node = Audiostreams[sound_index]
	tween.tween_property(sound_node, "volume_db", -60.0, duration)
	tween.play()
	await tween.finished
	if sound_node != null:
		if sound_node.is_queued_for_deletion():
			sound_node.stop()
			_erase_sound(sound)
			_playing_sounds.erase(sound)


# Returns the index of the given sound if it's playing
# Returns -1 if it doesn't exist
func find_sound(sound : String) -> int:
	var sound_index = -1
	if not is_audio_file(sound):
		sound = Audio_Files_Dictionary.get(sound, null)
	if sound != null and sound != "":
		sound_index = _playing_sounds.find(sound)
	return sound_index


# Returns true if the selected sound is playing
func is_playing(sound : String) -> bool:
	var playing : bool = false
	if sound != "" and sound != null:
		var sound_index = find_sound(sound)
		playing = sound_index >= 0
		playing = playing && Audiostreams[sound_index].is_playing()
	elif _debug:
		print_debug("Sound not found: " + sound)
	return playing


func pause(sound : String) -> void:
	set_paused(sound, true)


func unpause(sound : String) -> void:
	set_paused(sound, false)


func set_paused(sound : String, paused : bool = true) -> void:
	var sound_index = find_sound(sound)
	if sound_index >= 0:
		Audiostreams[sound_index].set_stream_paused(paused)
	elif _debug:
		print_debug("Sound not found: " + sound)


# Returns true if the given sound is paused
func is_paused(sound : String) -> bool:
	var sound_index = find_sound(sound)
	var paused : bool = false
	if sound_index >= 0:
		paused = Audiostreams[sound_index].get_stream_paused()
	elif _debug:
		print_debug("Sound not found: " + sound)
	return paused



#################################
#		GETTERS AND SETTERS		#
#################################

# Returns the name of the currently playing sounds
func get_playing_sounds() -> Array:
	return _playing_sounds


# Sound Properties

func set_bgm_volume_db(volume_db : float) -> void:
	set_sound_property("BGM", "Volume", volume_db)


func get_bgm_volume_db() -> float:
	return Default_Sounds_Properties["BGM"]["Volume"]


func set_bgm_pitch_scale(pitch_scale : float) -> void:
	set_sound_property("BGM", "Pitch", pitch_scale)


func get_bgm_pitch_scale() -> float:
	return Default_Sounds_Properties["BGM"]["Pitch"]


func set_bgs_volume_db(volume_db : float) -> void:
	set_sound_property("BGS", "Volume", volume_db)


func get_bgs_volume_db() -> float:
	return Default_Sounds_Properties["BGS"]["Volume"]


func set_bgs_pitch_scale(pitch_scale : float) -> void:
	set_sound_property("BGS", "Pitch", pitch_scale)


func get_bgs_pitch_scale() -> float:
	return Default_Sounds_Properties["BGS"]["Pitch"]


func set_sfx_volume_db(volume_db : float) -> void:
	set_sound_property("SFX", "Volume", volume_db)


func get_sfx_volume_db() -> float:
	return Default_Sounds_Properties["SFX"]["Volume"]


func set_sfx_pitch_scale(pitch_scale : float) -> void:
	set_sound_property("SFX", "Pitch", pitch_scale)


func get_sfx_pitch_scale() -> float:
	return Default_Sounds_Properties["SFX"]["Pitch"]


func set_mfx_volume_db(volume_db : float) -> void:
	set_sound_property("MFX", "Volume", volume_db)


func get_mfx_volume_db() -> float:
	return Default_Sounds_Properties["MFX"]["Volume"]


func set_mfx_pitch_scale(pitch_scale : float) -> void:
	set_sound_property("MFX", "Pitch", pitch_scale)


func get_mxf_pitch_scale() -> float:
	return Default_Sounds_Properties["MFX"]["Pitch"]


func set_volume_db(volume_db : float, sound : String) -> void:
	var sound_index = find_sound(sound)
	if sound_index >= 0:
		Audiostreams[sound_index].set_volume_db(volume_db)
	elif _debug:
		print_debug("Sound not found: " + sound)


func get_volume_db(sound : String) -> float:
	var sound_index = find_sound(sound)
	var volume_db : float = -81.0
	if sound_index >= 0:
		volume_db = Audiostreams[sound_index].get_volume_db()
	elif _debug:
		print_debug("Sound not found: " + sound)
	return volume_db


func set_pitch_scale(pitch_scale : float, sound : String = "") -> void:
	var sound_index = find_sound(sound)
	if sound_index >= 0:
		Audiostreams[sound_index].set_pitch_scale(sound)
	elif _debug:
		print_debug("Soud not found: " + sound)


func get_pitch_scale(sound : String = "") -> float:
	var sound_index = find_sound(sound)
	var pitch_scale : float = -1.0
	if sound_index >= 0:
		pitch_scale = Audiostreams[sound_index].get_pitch_scale()
	elif _debug:
		print_debug("Sound not found: " + sound)
	return pitch_scale


func get_default_sound_properties(sound_type : String) -> Dictionary:
	return Default_Sounds_Properties[sound_type]


func set_sound_property(sound_type : String, property : String, value : float) -> void:
	match property:
		"Volume":
			value = clamp(value, -80, 24)
		"Pitch":
			value = clamp(value, 0.01, 4)
	Default_Sounds_Properties[sound_type][property] = value


# Audio Files Dictionary

# Returns the Audio Files Dictionary
func get_audio_files_dictionary() -> Dictionary:
	return Audio_Files_Dictionary


# Returns the file name of the given stream name
# Returns null if an error occures.
func get_config_value(stream_name : String) -> String:
	return Audio_Files_Dictionary.get(stream_name)


# Allows you to change or add a stream file and name to the dictionary in runtime
func set_config_key(new_stream_name : String, new_stream_file : String) -> void:
	if (new_stream_file == "" or new_stream_name == ""):
		if _debug:
			print_debug("Invalid arguments")
		return
	
	add_to_dictionary(new_stream_name, new_stream_file)
	
	if _preload_resources:
		preload_resource_from_string(new_stream_file)


# Adds a new voice to the Audio Files Dictionary
func add_to_dictionary(audio_name : String, audio_file : String) -> void:
	Audio_Files_Dictionary[audio_name] = audio_file


# Resources preloading #

func enable_preload_resources(enabled : bool = true) -> void:
	_preload_resources = enabled


func is_preload_resources_enabled() -> bool:
	return _preload_resources


#############################
#	RESOURCE PRELOADING		#
#############################

func preload_audio_files_from_path(path : String):
	var file_name : String
	var dir = DirAccess.open(path)
	dir.list_dir_begin()
	if dir:
		file_name = dir.get_next()
		while file_name != "":
			if is_audio_file(file_name):
				var file_path = dir.get_current_dir()
				if file_path != "res://":
					file_path += "/"
				file_path += file_name
				preload_resource_from_string(file_path)
			file_name = dir.get_next()
	elif _debug:
		print_debug("An error occurred when trying to access the path: " + path)
		


func preload_resources_from_list(files_list : Array) -> void:
	if _preload_resources:
		if _debug:
			print_debug("Resources already preloaded.")
		return
			
	for file in files_list:
		if (file is String):
			preload_resource_from_string(file)
		elif (file is Resource):
			preload_resource(file)


func preload_resource(file : Resource) -> void:		
	if (file == null):
		if _debug:
			print_debug("Invalid resource passed")
		return
	
	var file_path = file.get_path()
	
	Preloaded_Resources[file_path] = file


func preload_resource_from_string(file : String) -> void:	
	var res = null
	var file_name = file
	
	if is_import_file(file):
		file_name = file_name.get_basename()
	elif not is_audio_file(file):
		file_name = Audio_Files_Dictionary.get(file)
		if file_name == null:
			if _debug:
				print_debug("Audio File not found in Dictionary")
			return
	
	res = load(file_name)
	
	if res:
		Preloaded_Resources[file_name] = res
	elif _debug:
		print_debug("An error occured while preloading resource: " + file)


func unload_all_resources(force_unload : bool = false) -> void:
	if _preload_resources:
		if force_unload == false:
			if _debug:
				print_debug("To unload resources with Preload Resources variable on, pass force_unload argument on true")
			return
		_preload_resources = false
		
	Preloaded_Resources.clear()


func unload_resources_from_list(files_list : Array) -> void:
	for file in files_list:
		if (file is String):
			unload_resource_from_string(file)


func unload_resource_from_string(file : String) -> void:	
	var file_name = file
	
	if is_import_file(file):
		file_name = file_name.get_basename()
	elif not is_audio_file(file):
		file_name = Audio_Files_Dictionary.get(file)
		if file_name == null:
			if _debug:
				print_debug("Audio File not found in Dictionary")
			return
	
	if Preloaded_Resources.has(file_name):
		Preloaded_Resources.erase(file_name)
	elif _debug:
		print_debug("An error occured while unloading resource: " + file)


func unload_resources_from_dir(path : String) -> void:
	var dir : DirAccess
	if dir.open(path + "/") != null:
		dir.list_dir_begin()
		var file_name = dir.get_next()
		while (file_name != ""):
			if (is_audio_file(file_name)):
				unload_resource_from_string(dir.get_current_dir() + file_name)
			file_name = dir.get_next()
	elif _debug:
		print_debug("An error occurred when trying to access the path: " + path)


#############################
#	NODES PREINSTANTIATION	#
#############################

func preinstantiate_nodes_from_path(path : String, sound_type : String = "") -> void:
	var file_name : String
	var dir := DirAccess.open(path)
	dir.open(path)
	if dir:
		dir.list_dir_begin()
		file_name = dir.get_next()
		while file_name != "":
			if is_audio_file(file_name) or is_import_file(file_name):
				var file_path = dir.get_current_dir()
				if file_path != "res://":
					file_path += "/"
				file_path += file_name
				preinstantiate_node_from_string(file_path, sound_type)
			file_name = dir.get_next()
	elif _debug:
		print_debug("An error occurred when trying to access the path: " + path)


func preinstantiate_nodes_from_list(files_list : Array, type_list : Array, all_same_type : bool = false) -> void:	
	var index = 0
	for file in files_list:
		if file is String:
			if (all_same_type == false):
				index = files_list.find(file)
			preinstantiate_node_from_string(file, type_list[index])


func preinstantiate_node_from_string(file : String, sound_type : String = "") -> void:
	var Stream = null
	var file_name = file
	var sound_index = 0

	if is_import_file(file):
		file_name = file_name.get_basename()
	elif not is_audio_file(file):
		file_name = Audio_Files_Dictionary.get(file)
		if file_name == null:
			if _debug:
				print_debug("Audio File not found in Dictionary")
			return
	
	if Preloaded_Resources.has(file_name):
		Stream = Preloaded_Resources.get(file_name)
	else:
		Stream = load(file_name)
	
	if not preinstantiate_node(Stream, sound_type) and _debug:
		print_debug("An error occured while creating a node from resource: " + file)


func preinstantiate_node(stream : Resource, sound_type : String = "") -> bool:
	if stream != null:
		var file_name = stream.get_path()
		if not Instantiated_Nodes.has(file_name):
			var sound_index = 0
			sound_index = _add_sound(file_name, sound_type, true)
			Audiostreams[sound_index].set_stream(stream)
			_playing_sounds.append(file_name)
		elif _debug:
			print_debug("Node already instantiated")
		return true
	
	return false


func uninstantiate_all_nodes(force_uninstantiation : bool = false) -> void:
	if _enable_nodes_preinstantiation:
		if not force_uninstantiation:
			if _debug:
				print_debug("To uninstantiate resources with Preinstantiate Nodes on, pass force_uninstantiation argument on true")
			return
		_enable_nodes_preinstantiation = false

	uninstantiate_nodes_from_list(Instantiated_Nodes)


func uninstantiate_nodes_from_list(files_list : Array) -> void:
	var index = 0
	
	var list = files_list.duplicate()
	
	for file in list:
		if (file is String):
			uninstantiate_node_from_string(file)
	list.clear()


func uninstantiate_node_from_string(file : String) -> void:
	var file_name = file

	if is_import_file(file):
		file_name = file_name.get_basename()
	elif not is_audio_file(file):
		file_name = Audio_Files_Dictionary.get(file)
		if file_name == null:
			if _debug:
				print_debug("Audio File not found in Dictionary")
			return
	
	_erase_sound(file_name)


func uninstantiate_nodes_from_dir(path : String) -> void:
	var dir : DirAccess
	if dir.open(path + "/") != null:
		dir.list_dir_begin()
		var file_name = dir.get_next()
		var sound_index = 0
		while (file_name != ""):
			if (is_audio_file(file_name)):
				uninstantiate_node_from_string(file_name)
			file_name = dir.get_next()
	elif _debug:
		print_debug("An error occurred when trying to access the path: " + path)


func _enable_node_preinstantiation(enabled : bool = true) -> void:
	_enable_nodes_preinstantiation = enabled


func is_preinstantiate_nodes_enabled() -> bool:
	return _enable_nodes_preinstantiation


#############################
#	INTERNAL METHODS		#
#############################

# Called when the node enters the scene for the first time
func _ready() -> void:
	if(ProjectSettings.get_setting("editor_plugins/enabled") and
	Array(ProjectSettings.get_setting("editor_plugins/enabled")).has("res://addons/sound_manager/plugin.cfg")):
			_get_sound_manager_settings()
	if _debug:
		print_debug("Error: sound manager is not enabled")
			
	if _preload_resources and Preloaded_Resources.is_empty():
		if _debug:
			print_debug("Preloading...")
		_preload_audio_files()
	if _enable_nodes_preinstantiation:
		if _debug:
			print_debug("Instantiating nodes...")
		_preinstantiate_nodes()


# Load the Sound Manager settings from the JSON file:  sound_manager.json
func _get_sound_manager_settings()-> void:
	var data_settings : Dictionary
	var file : FileAccess
	file = FileAccess.open("res://addons/sound_manager/sound_manager.json", FileAccess.READ)
	var json = JSON.new()
	json.parse(file.get_as_text())
	file.close()
	if typeof(json.data) == TYPE_DICTIONARY:
		data_settings = json.data
		
		Default_Sounds_Properties = data_settings["DEFAULT_SOUNDS_PROPERTIES"]
		_Audio_Busses = data_settings["Audio_Busses"]
		Audio_Files_Dictionary = data_settings["Audio_Files_Dictionary"]
		_preload_resources = data_settings["PRELOAD_RES"]
		_enable_nodes_preinstantiation = data_settings["PREINSTANTIATE_NODES"]
		_debug = data_settings["DEBUG"]
		
	elif _debug:
		print_debug("Failed to load the sound manager's settings file: " + 'res://addons/sound_manager/sound_manager.json')


# Calls the play method as deferred
func _play_deferred(sound_type : String, sound : String, from_position : float = 1.0, volume_db : float = -81, pitch_scale : float = -1, sound_to_override : String = "") -> void:
	call_deferred("_play", sound_type, sound, from_position, volume_db, pitch_scale, sound_to_override)


func _fade_in_deferred(sound_type : String, sound : String, duration : float, from_position : float = 1.0, volume_db : float = -81, pitch_scale : float = -1, sound_to_override : String = "") -> void:
	call_deferred("_fade_in", sound_type, sound, duration, from_position, volume_db, pitch_scale, sound_to_override)


# Plays the selected sound
func _play(sound_type : String, sound : String, from_position : float = 1.0, volume_db : float = -81, pitch_scale : float = -1, sound_to_override : String = "") -> void:
	var sound_path : String
	var volume = Default_Sounds_Properties[sound_type]["Volume"] if volume_db < -80 else volume_db
	var pitch = Default_Sounds_Properties[sound_type]["Pitch"] if pitch_scale < 0 else pitch_scale
	var audiostream : AudioStreamPlayer
	var sound_index = 0

	if Audio_Files_Dictionary.has(sound):
		if _debug:
			print_debug("Sound found on dictionary: " + sound)
		sound_path = Audio_Files_Dictionary.get(sound)
	elif sound.is_absolute_path() and is_audio_file(sound.get_file()):
		sound_path = sound
	else:
		if _debug:
			print_debug("Error: file not found " + sound)
		return
	if Instantiated_Nodes.has(sound_path):
		sound_index = Instantiated_Nodes.find(sound_path)
		audiostream = Audiostreams[sound_index]
		if audiostream.get_bus() != _Audio_Busses[sound_type]:
			audiostream.set_bus(_Audio_Busses[sound_type])
		if _debug:
			print_debug("Node preinstantiated " + sound_path)
	else:
		if sound_to_override != "":
			if is_playing(sound):
				return
		
		var Stream
		if Preloaded_Resources.has(sound_path):
			if _debug:
				print_debug("Resource preloaded " + sound_path)
			Stream = Preloaded_Resources.get(sound_path)
		else:
			Stream = load(sound_path)
			
			if Stream == null:
				if _debug:
					print_debug("Failed to load file from path: " + sound_path)
				return
		
		if sound_to_override != "":
			sound_index = find_sound(sound_to_override)
				
			if sound_index < 0:
				if _debug:
					print_debug("Sound not found: " + sound_to_override)
				return
		else:
			sound_index = _add_sound(sound_path, sound_type)
		audiostream = Audiostreams[sound_index]
		audiostream.set_stream(Stream)
	
	audiostream.set_volume_db(volume)
	audiostream.set_pitch_scale(pitch)
	
	
	audiostream.play(from_position)
	if audiostream.get_script() != null:
		audiostream.set_sound_name(sound)
	if sound_index < _playing_sounds.size():
		_playing_sounds[sound_index] = sound_path
	else:
		_playing_sounds.append(sound_path)


func _fade_in(sound_type : String, sound : String, duration : float, from_position : float = 1.0, volume_db : float = -81, pitch_scale : float = -1, sound_to_override : String = "") -> void:
	volume_db = Default_Sounds_Properties[sound_type]["Volume"] if volume_db < -80 else volume_db
	var tween = get_tree().create_tween()
	var sound_index = find_sound(sound)
	
	if sound_index < 0:
		if _debug:
			print_debug("No sound found: " + sound)
		return
	
	var sound_node = Audiostreams[sound_index]
	tween.tween_property(sound_node, "volume_db", volume_db, duration)
	tween.play()


# Adds a new AudioStreamPlayer
func _add_sound(sound : String, sound_type : String, preinstance : bool = false) -> int:
	var sound_index
	var new_audiostream = AudioStreamPlayer.new()
	var sound_script = load(soundmgr_dir_rel_path + "/Sounds.gd")
	var bus : String
	
	if sound_type == "":
		bus = "Master"
	else:
		bus = _Audio_Busses[sound_type]
	
	add_child(new_audiostream)
	if not preinstance:
		new_audiostream.set_script(sound_script)
		new_audiostream.set_sound_name(sound)
		new_audiostream.set_sound_type(sound_type)
		new_audiostream.connect_signals(self)
	new_audiostream.set_bus(bus)
	sound_index = new_audiostream.get_index()
	if not Instantiated_Nodes.has(sound):
		Instantiated_Nodes.append(sound)
	Audiostreams.append(new_audiostream)
	sound_index = Instantiated_Nodes.find(sound)
	
	if _debug:
		print_debug(sound_type)
	return sound_index


func _erase_sound(sound : String) -> void:
	var sound_index : int = find_sound(sound)
	
	if sound_index >= 0:
		Instantiated_Nodes.remove_at(sound_index)
		Audiostreams[sound_index].queue_free()
		Audiostreams.remove_at(sound_index)
		_playing_sounds.remove_at(sound_index)
	elif _debug:
		print_debug("Sound not found: " + sound)


func _on_sound_finished(sound_name : String) -> void:
	if not _enable_nodes_preinstantiation:
		_erase_sound(sound_name)


func _preload_audio_files() -> void:
	var directory = DirAccess.open("res://")
	preload_audio_files_from_path("res://")
	_preload_audio_files_r(directory)


func _preload_audio_files_r(directory : DirAccess) -> void:
	if directory == null:
		return
	directory.list_dir_begin()
	var dir_name = directory.get_next()
	while dir_name != "":
		if directory.current_is_dir():
			if dir_name != "addons":
				var dir_path = directory.get_current_dir() + dir_name
				preload_audio_files_from_path(dir_path)
		dir_name = directory.get_next()


func _preinstantiate_nodes() -> void:
	var directory = DirAccess.open("res://")
	_enable_node_preinstantiation(true)
	preinstantiate_nodes_from_path("res://")
	_preinstatiate_nodes_r(directory)


func _preinstatiate_nodes_r(directory : DirAccess) -> void:
	if directory == null:
		return
	directory.list_dir_begin()
	var dir_name = directory.get_next()
	while dir_name != "":
		if directory.current_is_dir():
			if dir_name != "addons":
				var dir_path = directory.get_current_dir() + dir_name
				preinstantiate_nodes_from_path(dir_path)
		dir_name = directory.get_next()


func is_audio_file(file_name : String) -> bool:
	return	(file_name.get_extension() == "wav" or
			file_name.get_extension() == "ogg" or
			file_name.get_extension() == "opus" or
			file_name.get_extension() == "mp3")


func is_import_file(file_name : String) -> bool:
	return (file_name.get_extension() == "import" and
			is_audio_file(file_name.get_basename()))

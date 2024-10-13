extends AudioStreamPlayer

####################################################################
#SOUNDS SCRIPT FOR THE SOUND MANAGER MODULE FOR GODOT 4
#			© Xecestel
####################################################################
#
# This Source Code Form is subject to the terms of the MIT License.
# If a copy of the license was not distributed with this
# file, You can obtain one at https://mit-license.org/.
#
#####################################

# Variables

var sound_type : String
var sound_name : String;

###########

# Signals

signal finished_playing(sound_name : String)

##########


func _ready():
	self.set_properties();


func connect_signals(connect_to : Node) -> void:
	finished.connect(_on_self_finished)
	finished_playing.connect(connect_to._on_sound_finished);


func set_properties(volume_db : float = 0.0, pitch_scale : float = 1.0) -> void:
	self.set_volume_db(volume_db);
	self.set_pitch_scale(pitch_scale);


func set_sound_name(sound_name : String) -> void:
	self.sound_name = sound_name;


func set_sound_type(type : String) -> void:
	self.sound_type = type


func get_sound_type() -> String:
	return self.sound_type


func get_sound_name() -> String:
	return sound_name


func _on_self_finished() -> void:
	emit_signal("finished_playing", self.get_sound_name());


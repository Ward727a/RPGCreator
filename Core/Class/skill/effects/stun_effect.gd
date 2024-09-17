extends BaseEffect
class_name StunEffect

var export_stun_duration: int = 0
var hint_stun_duration: String = "The duration of the stun"
var name_stun_duration: String = "Stun Duration"

func _init():

	id = "stun_effect"
	name = "EFFECT_STUN_NAME"
	description = "EFFECT_STUN_DESCRIPTION"

func apply_effect(_target: Character) -> void:
	pass

func duplicate() -> StunEffect:
	
	var new_effect = StunEffect.new()
	
	new_effect.export_stun_duration = export_stun_duration
	new_effect.id = id
	new_effect.name = name
	new_effect.description = description
	
	return new_effect

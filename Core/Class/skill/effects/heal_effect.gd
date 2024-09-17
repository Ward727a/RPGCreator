extends BaseEffect
class_name HealEffect

var export_heal_amount: int = 0
var hint_heal_amount: String = "The amount of health to heal"
var name_heal_amount: String = "Heal Amount"

func _init():

	id = "heal_effect"
	name = "EFFECT_HEAL_NAME"
	description = "EFFECT_HEAL_DESCRIPTION"

func apply_effect(_target: Character) -> void:
	pass

func duplicate() -> HealEffect:

	var new_effect = HealEffect.new()
	
	new_effect.export_heal_amount = export_heal_amount
	new_effect.id = id
	new_effect.name = name
	new_effect.description = description
	
	return new_effect

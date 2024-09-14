extends BaseEffect
class_name PoisonEffect

var export_poison_damage: int = 0 # The amount of damage the poison will do
var hint_poison_damage: String = "The amount of damage the poison will do"
var name_poison_damage: String = "Poison Damage"

var export_poison_duration: int = 0 # The duration of the poison
var hint_poison_duration: String = "The duration of the poison"
var name_poison_duration: String = "Poison Duration"

var export_can_kill: bool = false # If the poison can kill the target
var hint_can_kill: String = "If the poison can kill the target"
var name_can_kill: String = "Can Kill Target"

func _init():

    id = "poison_effect" # Define the effect ID here, IT NEED to be unique than the other effects!!
    name = "EFFECT_POISON_NAME" # "Poison" - Those lines will be automatically translated if you have a translation file
    description = "EFFECT_POISON_DESCRIPTION" # "The target will be poisoned for X turns and will take Y damage each turn" - Those lines will be automatically translated if you have a translation file

func apply_effect(_target: Character) -> void:
    pass

func duplicate() -> PoisonEffect:
    
    var new_effect = PoisonEffect.new()
    
    new_effect.export_poison_damage = export_poison_damage
    new_effect.export_poison_duration = export_poison_duration
    new_effect.id = id
    new_effect.name = name
    new_effect.description = description
    
    return new_effect
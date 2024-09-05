extends Base
class_name BaseEffect

const EffectTypes = EnumRegister.EffectTypes
const EffectOutcome = EnumRegister.EffectOutcome
const EffectTime = EnumRegister.EffectTime
const EffectTarget = EnumRegister.EffectTarget

var name: String = "" # Effect's name
var description: String = "" # Effect's description

var duration: int = 0 # Effect's duration if applicable

var effect_type: EffectTypes = EffectTypes.CORE_STATS # Effect's type
var effect_outcome: EffectOutcome = EffectOutcome.NEUTRAL # Effect's outcome
var effect_time: EffectTime = EffectTime.INSTANT # Effect's time (instant, over time, or permanent)
var effect_target: EffectTarget = EffectTarget.SELF # Effect's target (self, ally, enemy, all)

# Function to apply the effect
# This function should be overridden in the child classes
# It should apply the effect to the target
func apply_effect(_target: Character) -> void:
    pass
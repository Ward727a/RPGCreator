extends Base
class_name BaseEffect

const EffectTypes = preload("res://Core/Enums/effects/effect_type.gd").EffectTypes
const EffectOutcome = preload("res://Core/Enums/effects/effect_outcome.gd").EffectOutcome
const EffectTime = preload("res://Core/Enums/effects/effect_time.gd").EffectTime
const EffectTarget = preload("res://Core/Enums/effects/effect_target.gd").EffectTarget

var name: String = "" # Effect's name
var description: String = "" # Effect's description

var duration: int = 0 # Effect's duration if applicable

var effect_type: EffectTypes = EffectTypes.CORE_STATS # Effect's type
var effect_outcome: EffectOutcome = EffectOutcome.NEUTRAL # Effect's outcome
var effect_time: EffectTime = EffectTime.INSTANT # Effect's time (instant, over time, or permanent)
var effect_target: EffectTarget = EffectTarget.SELF # Effect's target (self, ally, enemy, all)
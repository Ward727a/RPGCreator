extends BaseSkillCondition
class_name HealthSkillCondition

var export_health_required: int = 0 # The amount of health required to use the skill - We add "export_" at the start of the variable name to make it editable in the editor
var hint_health_required: String = "The amount of health required to use the skill" # We add "hint_" at the start of the variable name to make it a hint

## We will initialize the condition here[br]
## Be sure to set a default idx for the condition[br]
func _init(_default_idx: String = ""):

    id = _default_idx
    name = "SKILL_CONDITION_HEALTH_NAME"
    description = "SKILL_CONDITION_HEALTH_DESCRIPTION" # "The character needs to have at least X amount of HP to use this skill"

# Function to check if the condition is met
# This function should be overridden in the child classes
# It should return true if the condition is met, and false if it's not
func check_condition(_character: Character) -> bool:
    return _character.health >= export_health_required

func duplicate() -> HealthSkillCondition:
    
    var new_condition = HealthSkillCondition.new()
    
    new_condition.export_health_required = export_health_required
    new_condition.id = id
    new_condition.name = name
    new_condition.description = description
    
    return new_condition
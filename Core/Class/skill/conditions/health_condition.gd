extends BaseSkillCondition
class_name HealthSkillCondition

var export_health_required: int = 0 ## The amount of health required to use the skill - We add "export_" at the start of the variable name to make it editable in the editor
var hint_health_required: String = "The amount of health required to use the skill" ## We add "hint_" at the start of the variable name to make it a hint
var name_health_required: String = "Health Required" ## We add "name_" at the start of the variable name to make it a name

var export_health_max_required: int = 0
var hint_health_max_required: String = "At what health percentage the skill can't be used"
var name_health_max_required: String = "Max Health Until Skill Can't Be Used"

var export_can_be_used_when_dead: bool = false
var hint_can_be_used_when_dead: String = "If the skill can be used when the character is dead"
var name_can_be_used_when_dead: String = "Can Be Used When Dead"

## We will initialize the condition here[br]
## Be sure to set a default idx for the condition[br]
func _init():

	id = "health_skill_condition" # Define the condition ID here, IT NEED to be unique than the other conditions!!
	name = "SKILL_CONDITION_HEALTH_NAME" # "Has enough HP" - Those lines will be automatically translated if you have a translation file
	description = "SKILL_CONDITION_HEALTH_DESCRIPTION" # "The character needs to have at least X amount of HP to use this skill"

# Function to check if the condition is met
# This function should be overridden in the child classes
# It should return true if the condition is met, and false if it's not
func check_condition(_character: Character) -> bool:
	return _character.health >= export_health_required

# If you don't know what this function does, just copy it as it is
# Don't edit it, unless you know what you are doing because it's a VERY important function!
#
# If you copy it, don't forget to change the class name to the name of the class you are using
func duplicate() -> HealthSkillCondition:
	
	var new_condition = HealthSkillCondition.new()
	
	new_condition.export_health_required = export_health_required
	new_condition.export_health_max_required = export_health_max_required
	new_condition.export_can_be_used_when_dead = export_can_be_used_when_dead
	new_condition.id = id
	new_condition.name = name
	new_condition.description = description
	
	return new_condition

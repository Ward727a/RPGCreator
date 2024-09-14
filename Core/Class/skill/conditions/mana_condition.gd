extends BaseSkillCondition
class_name ManaSkillCondition

var export_mana_required: int = 0 # The amount of mana required to use the skill

## We will initialize the condition here[br]
func _init():

    id = "mana_skill_condition" # Define the condition ID here, IT NEED to be unique!!
    name = "SKILL_CONDITION_MANA_NAME" # "Has enough MP"
    description = "SKILL_CONDITION_MANA_DESCRIPTION" # "The character needs to have at least X amount of MP to use this skill"


# Function to check if the condition is met
# This function should be overridden in the child classes
# It should return true if the condition is met, and false if it's not
func check_condition(_character: Character) -> bool:
    return _character.stats.mana >= export_mana_required


func duplicate() -> ManaSkillCondition:
    
    var new_condition = ManaSkillCondition.new()
    
    new_condition.export_mana_required = export_mana_required
    new_condition.id = id
    new_condition.name = name
    new_condition.description = description
    
    return new_condition
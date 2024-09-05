extends Base
class_name BaseSkillCondition
# This is the base class for the skill conditions
# Skill conditions are conditions that a character needs to meet to use a skill
# It's not recommended to use this class directly, but to inherit from it
# [br]
# This class contains the basic properties that a skill condition should have
# All the functions in this class should be overridden in the child classes to make them work properly
# [br]	
# If you want to create a property that can be edited in the editor, 
# you should create a new property in the child class and prefix the name with "export_" (e.g. var export_my_property: int = 0)

var name: String = "" # Condition's name (e.g. "Has enough MP")
var description: String = "" # Condition's description (e.g. "The character needs to have at least 10 MP to use this skill")

# Function to check if the condition is met
# This function should be overridden in the child classes
# It should return true if the condition is met, and false if it's not
func check_condition(_character: Character) -> bool:
    return false
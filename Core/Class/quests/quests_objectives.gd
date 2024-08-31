extends Base
class_name quests_objectives
# This is the base class for all quest objectives in the game

const ObjectiveType = EnumRegister.ObjectiveType

var quest_id: int = 0
var objective_id: int = 0

var is_completed: bool = false
var objective_type: ObjectiveType = ObjectiveType.KILL

func _init(type: ObjectiveType):
    objective_type = type

    allowed_events = ["pre_complete", "post_complete"]

# Function that need to be implemented in the child classes
func complete():
    # Emit the pre_complete event
    call_event("pre_complete", [quest_id, objective_id])

    is_completed = true

    # Emit the post_complete event
    call_event("post_complete", [quest_id, objective_id])
    pass


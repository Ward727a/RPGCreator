extends Base
class_name quest
# This is the base class for all quests in the game

# Import the Enums
const QuestStatus = preload("res://Core/Enums/quests/quest_status.gd").QuestStatus
const QuestType = preload("res://Core/Enums/quests/quest_type.gd").QuestType

var quest_id: int = 0 # Quest ID
var quest_name: String = "Quest" # Quest name
var quest_description: String = "Quest description" # Quest description (This can be used for story, or simply to describe the quest)
var quest_aditional_info: String = "Aditional info" # Quest aditional info (can be used for hints, etc.)
var quest_area: String = "Area" # Quest area (where the quest is located)
var quest_level: int = 1 # Quest level (level RECOMMENDED to start the quest)
var quest_reward: Loottables = Loottables.new(0, "default", "default", []) # Quest reward
var quest_status: QuestStatus = QuestStatus.HIDDEN # Quest status
var quest_type: QuestType = QuestType.MAIN # Quest type
var quest_objectives: Array = [] # Quest objectives

func _init(_quest_id: int, _name: String, _description: String, _aditional_info: String, _area: String, _level: int, _reward: Loottables, _status: QuestStatus, _type: QuestType, _objectives: Array):
    quest_id = _quest_id
    quest_name = _name
    quest_description = _description
    quest_aditional_info = _aditional_info
    quest_area = _area
    quest_level = _level
    quest_reward = _reward
    quest_status = _status
    quest_type = _type
    quest_objectives = _objectives

    set_custom_name("quest")

    allowed_events = [
        "pre_start", "post_start", 
        "pre_complete", "post_complete", 
        "pre_fail", "post_fail",
        "pre_addobjective", "post_addobjective",
        "pre_removeobjective", "post_removeobjective",
        "pre_setobjectives", "post_setobjectives",
        "getobjectives",
        "getobjective",
        "getreward",
        "is_completed",
        "getname", "setname",
        "getdescription", "setdescription",
        "getaditionalinfo", "setaditionalinfo",
        "getarea", "setarea",
        "getlevel", "setlevel",
        "getstatus", "setstatus",
        "gettype", "settype",
        "getid", "setid"
        ]

    super()

# Start the quest
func start():
    # Emit the pre_start event
    call_event("pre_start", [quest_name])
    
    quest_status = QuestStatus.IN_PROGRESS
    
    # Emit the post_start event
    call_event("post_start", [quest_name])


func complete():
    # Emit the pre_complete event
    call_event("pre_complete", [quest_name])
    
    quest_status = QuestStatus.COMPLETED
    
    # Emit the post_complete event
    call_event("post_complete", [quest_name])

func fail():
    # Emit the pre_fail event
    call_event("pre_fail", [quest_name])
    
    quest_status = QuestStatus.FAILED
    
    # Emit the post_fail event
    call_event("post_fail", [quest_name])

# Set the quest objectives
func set_objectives(_objectives: Array):
    # Emit the pre_setobjectives event
    call_event("pre_setobjectives", [_objectives])

    quest_objectives = _objectives

    # Emit the post_setobjectives event
    call_event("post_setobjectives", [_objectives])

# Add an objective to the quest
func add_objective(_objective: quests_objectives):
    # Emit the pre_addobjective event
    call_event("pre_addobjective", [_objective])
    
    # Assign the quest_id and objective_id
    _objective.quest_id = quest_id
    _objective.objective_id = quest_objectives.size()

    quest_objectives.append(_objective)
    
    # Emit the post_addobjective event
    call_event("post_addobjective", [_objective])

# Remove an objective from the quest
func remove_objective(_objective: quests_objectives):
    # Emit the pre_removeobjective event
    call_event("pre_removeobjective", [_objective])
    
    var success: bool = false
    
    for i in range(quest_objectives.size()):
        if quest_objectives[i] == _objective:
            quest_objectives.erase(i)
            success = true
            break
    
    # Emit the post_removeobjective event
    call_event("post_removeobjective", [_objective, success])

# Get the quest objectives
func get_objectives():
    # Emit the pre_getobjectives event
    call_event("getobjectives", [])
    
    return quest_objectives

# Get a specific objective
func get_objective(_objective_id: int):
    # Emit the pre_getobjective event
    call_event("getobjective", [_objective_id])
    
    return quest_objectives[_objective_id]

# Check if the quest is completed
func is_completed():
    for objective in quest_objectives:
        if objective.is_completed == false:

            # Emit the is_completed event
            call_event("is_completed", [false])

            return false
    
    # Emit the is_completed event
    call_event("is_completed", [true])

    return true

# Get the quest reward
func get_reward():
    # Emit the getreward event
    call_event("getreward", [quest_reward])
    
    return quest_reward

# Set the reward for the quest
func set_reward(_reward: Loottables):
    # Emit the setreward event
    call_event("setreward", [quest_reward, _reward])

    quest_reward = _reward

# Get the quest status
func get_status():
    # Emit the getstatus event
    call_event("getstatus", [quest_status])

    return quest_status

# Set the quest status
func set_status(_status: QuestStatus):
    # Emit the setstatus event
    call_event("setstatus", [quest_status, _status])

    quest_status = _status

# Get the quest type
func get_type():
    # Emit the gettype event
    call_event("gettype", [quest_type])

    return quest_type

# Set the quest type
func set_type(_type: QuestType):
    # Emit the settype event
    call_event("settype", [quest_type, _type])

    quest_type = _type

# Get the quest ID
func get_id():
    # Emit the getid event
    call_event("getid", [quest_id])

    return quest_id

# Set the quest ID
func set_id(_id: int):
    # Emit the setid event
    call_event("setid", [quest_id, _id])

    quest_id = _id

# Get the quest name
func get_name():
    # Emit the getname event
    call_event("getname", [quest_name])

    return quest_name

# Set the quest name
func set_name(_name: String):
    # Emit the setname event
    call_event("setname", [quest_name, _name])

    quest_name = _name

# Get the quest description
func get_description():
    # Emit the getdescription event
    call_event("getdescription", [quest_description])

    return quest_description

# Set the quest description
func set_description(_description: String):
    # Emit the setdescription event
    call_event("setdescription", [quest_description, _description])

    quest_description = _description

# Get the quest aditional info
func get_aditional_info():
    # Emit the getaditionalinfo event
    call_event("getaditionalinfo", [quest_aditional_info])

    return quest_aditional_info

# Set the quest aditional info
func set_aditional_info(_aditional_info: String):
    # Emit the setaditionalinfo event
    call_event("setaditionalinfo", [quest_aditional_info, _aditional_info])

    quest_aditional_info = _aditional_info

# Get the quest area
func get_area():
    # Emit the getarea event
    call_event("getarea", [quest_area])

    return quest_area

# Set the quest area
func set_area(_area: String):
    # Emit the setarea event
    call_event("setarea", [quest_area, _area])

    quest_area = _area

# Get the quest level
func get_level():
    # Emit the getlevel event
    call_event("getlevel", [quest_level])

    return quest_level

# Set the quest level
func set_level(_level: int):
    # Emit the setlevel event
    call_event("setlevel", [quest_level, _level])

    quest_level = _level

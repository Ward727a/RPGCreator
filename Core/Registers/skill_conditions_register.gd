extends Base
class_name SkillConditionsRegister
# This class is used to register all the skill conditions in the game

func _init():
	need_register = false # Set it to false to avoid registering this class in the class register

	set_custom_name("SkillConditionsRegister")


static var data: Dictionary = {} # Contains all the skill conditions

static func set_conditions(conditions: Dictionary) -> void:
	# Set the skill conditions data
	data = conditions

static func set_condition(condition_id: String, condition: BaseSkillCondition) -> void:
	# Set a skill condition data
	data[condition_id] = condition

static func get_condition(condition_id: String) -> BaseSkillCondition:
	# Get a skill condition data
	return data[condition_id].duplicate()

static func get_pure_condition(condition_id: String) -> BaseSkillCondition:
	# Get a skill condition data without duplicating it - WARNING: Use this function only if you know what you are doing
	return data[condition_id]

static func get_all_conditions() -> Dictionary:
	# Get all skill conditions data
	return data

static func get_all_conditions_names() -> Array:
	# Get all skill conditions names
	var names = []
	for condition in data.values():
		names.append(condition.name)
	return names

static func get_all_conditions_ids() -> Array:
	# Get all skill conditions ids
	return data.keys()

static func get_all_conditions_name_by_ids() -> Array:
	# Get all skill conditions names by ids
	var names = []
	for condition_id in data.keys():
		names.append({"name": data[condition_id].name, "value": condition_id})
	return names

static func remove_condition(condition_id: String) -> bool:
	# Remove a skill condition data
	return data.erase(condition_id)

static func add_condition(condition: BaseSkillCondition) -> String:
	# Add a skill condition data

	# Check if the skill condition has an id, if not, generate one
	if condition.id == "":
		condition.id = IdGenerator.new().generate_id()

	var condition_id = condition.id
	data[condition_id] = condition
	return condition_id

static func new_condition(lua_script: String) -> BaseSkillCondition:
	# Create a new skill condition

	var condition = BaseSkillCondition.new(lua_script)
	
	add_condition(condition)

	return condition

static func get_condition_by_name(name: String) -> BaseSkillCondition:
	# Get a skill condition data by name
	for condition in data.values():
		if condition.name == name:
			return condition.duplicate()
	return null

static func conditions_to_string() -> String:
	# Convert all skill conditions data to string
	return var_to_str(data)

static func has_condition(condition_id: String) -> bool:
	# Check if the skill condition exists
	return data.has(condition_id)

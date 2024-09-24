extends Base
class_name SkillRegister
# This class is used to register all the skills in the game

func _init():
	need_register = false # Set it to false to avoid registering this class in the class register

	set_custom_name("SkillRegister")

static var data: Dictionary = {}

static func set_skills(skills: Dictionary) -> void:
	# Set the skills data
	data = skills

static func set_skill(skill_id: String, skill: BaseSkill) -> void:
	# Set a skill data
	data[skill_id] = skill

static func get_skill(skill_id: String) -> BaseSkill:
	# Get a skill data
	return data[skill_id]

static func get_all_skills() -> Dictionary:
	# Get all skills data
	return data

static func get_all_skills_names() -> Array:
	# Get all skills names
	var names = []
	for skill in data.values():
		names.append(skill.name)
	return names

static func get_all_skills_ids() -> Array:
	# Get all skills ids
	return data.keys()

static func get_all_skills_name_by_ids() -> Array:
	# Get all skills names by ids
	var names = []
	for skill_id in data.keys():
		names.append({"name": data[skill_id].name, "value": skill_id})
	return names

static func remove_skill(skill_id: String) -> bool:
	# Remove a skill data
	return data.erase(skill_id)

static func add_skill(skill: BaseSkill) -> String:
	# Add a skill data

	# Check if the skill has an id, if not, generate one
	if skill.id == "":
		skill.id = IdGenerator.new().generate_id()

	var skill_id = skill.id
	data[skill_id] = skill
	return skill_id

static func new_skill() -> BaseSkill:
	# Create a new skill

	var id: String = IdGenerator.new().generate_id()

	var skill = BaseSkill.new()

	skill.id = id
	skill.name = "Skill"
	skill.set_custom_name(id)

	add_skill(skill)

	return skill

static func skills_to_string() -> String:
	# Convert the skills data to a string
	return var_to_str(data)

static func to_dictionary() -> Dictionary:
	
	var return_data: Dictionary = {}
	
	for key in data.keys():
		print("Converting %s skill..." % key)
		
		return_data[key] = data[key].to_dictionary()
		
		print("Converted %s skill!" % key)
	
	return return_data

static func from_dictionary(new_data: Dictionary):
	
	for key in new_data.keys():
		print("Loading %s skill..." % key)
		
		data[key] = BaseSkill.from_dictionary(new_data[key])
		
		print("Loaded %s skill!" % key)

static func has_skill(skill_id: String) -> bool:
	# Check if a skill exists
	return data.has(skill_id)

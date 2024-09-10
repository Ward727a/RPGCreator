extends VBoxContainer

const SkillTypes = EnumRegister.SkillTypes
const SkillTargets = EnumRegister.SkillTarget

var infoID: InfoItemText
var infoName: InfoItemLineEdit
var infoDesc: InfoItemTextedit
var infoType: InfoItemOptions
var infoTarget: InfoItemOptions
var infoCooldown: InfoItemSpinBox
var infoCooldownType: OptionButton
var infoEffects: CustomListV2Base
var infoConditions: CustomListV2Base

var skill_loaded: BaseSkill = null

var clicked_on: Node = null

signal skill_renamed()

# Get all the nodes from the scene
func _ready():
	infoID = %infoID
	infoName = %infoName
	infoDesc = %infoDesc
	infoType = %infoType
	infoTarget = %infoTarget
	infoCooldown = %infoCooldown
	infoCooldownType = %cooldown_time_type
	infoEffects = %infoEffects
	infoConditions = %infoConditions

	init_nodes()

# Initialize the nodes
func init_nodes():

	init_options()

# Set the options for the infoItemOptions nodes (infoType & InfoTarget)
func init_options():

	var target_option = {}
	for enum_name in SkillTargets:
		var enum_key: int = SkillTargets[enum_name]
		target_option[enum_key] = {"name": tr(enum_name), "uid": enum_key}
	
	infoTarget.set_options(target_option)

	var type_option = {}
	for enum_name in SkillTypes:
		var enum_key: int = SkillTypes[enum_name]
		type_option[enum_key] = {"name": tr(enum_name), "uid": enum_key}
	
	infoType.set_options(type_option)

	# Set the cooldown time type
	for enum_name in EnumRegister.SkillCooldownType:
		infoCooldownType.add_item(tr(enum_name), EnumRegister.SkillCooldownType[enum_name])

# Load a skill
func _on_load_skill(skill: BaseSkill):
	
	if skill == null:
		return
	
	skill_loaded = skill

	infoID.set_content(skill.id)
	infoName.set_content(skill.name)
	infoDesc.set_content(skill.description)
	infoType.set_content(skill.skill_type)
	infoTarget.set_content(skill.skill_target)
	infoCooldown.set_content(skill.skill_cooldown)
	infoCooldownType.select(skill.skill_cooldown_type)

	_on_cooldown_time_type_item_selected(skill.skill_cooldown_type)

	# infoEffects.load_items(skill.effects) ## TODO: Implement the "load_items" inside the CustomListV2Base then in the custom script
	# infoConditions.load_items(skill.conditions) ## TODO: Implement the "load_items" inside the CustomListV2Base then in the custom script


func _on_skill_list_item_clicked(_node:Node, _name:String, _metadata:Dictionary):
	
	print("skill_idx: ", _metadata['skill_idx'])
	print("skill: ", SkillRegister.get_skill(_metadata['skill_idx']))


	# Check if the ignore exist
	if _metadata.has('ignore'):
		return

	if !_metadata.has('skill_idx'):
		push_error("No skill index found in the metadata")
		return

	var skill_idx: String = _metadata['skill_idx']

	# Check if the skill exists
	if !SkillRegister.has_skill(skill_idx):
		push_error("Skill not found")
		return

	var skill:BaseSkill = SkillRegister.get_skill(skill_idx)
	_on_load_skill(skill)
	skill_loaded = skill
	print("Skill loaded: ", skill.name)


func _on_cooldown_time_type_item_selected(index:int):

	match(index):
		0:
			infoCooldown.suffix = "MS"
		1:
			infoCooldown.suffix = "SEC"
		2:
			infoCooldown.suffix = "MIN"
		_:
			push_error("Invalid index selected")
			infoCooldown.suffix = "MS"
		
	if skill_loaded == null:
		return
	
	_add_to_history({"skill_cooldown_type": skill_loaded.skill_cooldown_type})
	skill_loaded.skill_cooldown_type = Utils.cast_to_enum(index, EnumRegister.SkillCooldownType)

	pass # Replace with function body.

# When the target type is changed (ex: Self, Enemy, etc)
func _on_info_target_content_changed(content:String):
	
	if skill_loaded == null:
		return

	_add_to_history({"skill_target": skill_loaded.skill_target})

	skill_loaded.skill_target = Utils.cast_to_enum(content.to_int(), EnumRegister.SkillTarget)

# when the skill type is changed (ex: Active, Passive, etc)
func _on_info_type_content_changed(content:String):
	
	if skill_loaded == null:
		return

	_add_to_history({"skill_type": skill_loaded.skill_type})

	skill_loaded.skill_type = Utils.cast_to_enum(content.to_int(), EnumRegister.SkillTypes)

# When the cooldown is changed
func _on_info_cooldown_content_changed(content:int):
	
	if skill_loaded == null:
		return

	_add_to_history({"skill_cooldown": skill_loaded.skill_cooldown})

	skill_loaded.skill_cooldown = content

# when the skill description is changed
func _on_info_desc_content_changed(content:String):
	
	if skill_loaded == null:
		return

	_add_to_history({"description": skill_loaded.description})

	skill_loaded.description = content

# when the skill name is changed
func _on_info_name_content_changed(content:String):
	
	if skill_loaded == null:
		return

	_add_to_history({"name": skill_loaded.name})

	skill_loaded.name = content
	skill_renamed.emit()


func _add_to_history(history_data: Dictionary) -> void:
	var history = HistorySkill.new(EnumRegister.HistoryAction.CHANGE, skill_loaded.id, history_data)
	HistoryRegister.add_to_history(history)
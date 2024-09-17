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

var windowEffects: ConfirmationDialog
var windowConditions: ConfirmationDialog

var windowEditCondition: ConfirmationDialog
var windowEditEffect: ConfirmationDialog

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

	windowEffects = %add_effects
	windowConditions = %add_conditions

	windowEditCondition = %edit_condition
	windowEditEffect = %edit_effect

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

func reload_conditions():
	infoConditions.clear()

	for i in skill_loaded.skill_conditions:
		infoConditions.add_item({"val": i.name, "idx": i.id, "hint": i.description})

func reload_effects():
	
	infoEffects.clear()
	
	for i in skill_loaded.skill_effects:
		infoEffects.add_item({"val": i.name, "idx": i.id, "hint": i.description})

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

	# Reload the conditions
	reload_conditions()
	reload_effects()

	# infoEffects.load_items(skill.effects) ## TODO: Implement the "load_items" inside the CustomListV2Base then in the custom script
	# infoConditions.load_items(skill.conditions) ## TODO: Implement the "load_items" inside the CustomListV2Base then in the custom script


func _on_skill_list_item_clicked(_node:Node, _name:String, _metadata:Dictionary):

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

func _on_info_conditions_create_pressed():
	print("Create condition")

func _on_info_conditions_add_pressed():
	windowConditions.popup_centered()
	windowConditions.set_checked(skill_loaded.skill_conditions)

func _on_info_effects_create_pressed():
	print("Create effect")

func _on_info_effects_add_pressed():
	windowEffects.popup_centered()
	windowEffects.set_checked(skill_loaded.skill_effects)

## When the user clicks on the "Add conditions" button[br]
## Get all the checked conditions[br]
## Then create list of id of the conditions that are already in the skill[br]
## Then create a list of the conditions that are not in the skill[br]
## Then remove the conditions that are not in the checked list[br]
## Then add the conditions that are not in the skill, in the skill[br]
func _on_add_conditions_confirmed():
	# print("Add conditions confirmed") # Debug, uncomment to test
	# print("checked_conditions: ", windowConditions.conditions_checked) # Debug, uncomment to test

	if skill_loaded == null:
		return
	
	var skill_condition_has_id = {}

	for i in skill_loaded.skill_conditions:
		skill_condition_has_id[i.id] = i

	# print("has_id: ", skill_condition_has_id) # Debug, uncomment to test
	# print("checked: ", windowConditions.conditions_checked) # Debug, uncomment to test

	# check if the key are the same, if it is, do nothing - If not, erase the conditions that aren't in the checked and add the new ones
	if skill_condition_has_id == windowConditions.conditions_checked:
		# print("Same conditions") # Debug, uncomment to test
		return
	
	# print("Different conditions") # Debug, uncomment to test

	var new_conditions: Array = []

	for i in windowConditions.conditions_checked.keys():
		if !skill_condition_has_id.has(i):
			new_conditions.append(windowConditions.conditions_checked[i])
	
	# print("New conditions: ", new_conditions) # Debug, uncomment to test

	# Create a list of the old conditions that will be removed
	var old_conditions: Array = []

	for i in skill_loaded.skill_conditions:
		if !windowConditions.conditions_checked.has(i.id):
			old_conditions.append(i)
	
	# Remove the old conditions
	for i in old_conditions:
		skill_loaded.skill_conditions.erase(i)
	
	# Add the new one
	for i in new_conditions:
		skill_loaded.skill_conditions.push_back(i.duplicate())
	
	# print("Skill conditions: ", var_to_str(skill_loaded.skill_conditions)) # Debug, uncomment to test

	# Reload the conditions list
	reload_conditions()

func _on_add_conditions_canceled():
	# print("Add conditions canceled")
	pass

func _on_add_effects_confirmed():
	print("Add effects confirmed") # Debug, uncomment to test
	print("checked_effects: ", windowEffects.effects_checked) # Debug, uncomment to test

	if skill_loaded == null:
		return
	
	var skill_effect_has_id = {}

	for i in skill_loaded.skill_effects:
		skill_effect_has_id[i.id] = i

	print("has_id: ", skill_effect_has_id) # Debug, uncomment to test
	print("checked: ", windowEffects.effects_checked) # Debug, uncomment to test

	# check if the key are the same, if it is, do nothing - If not, erase the conditions that aren't in the checked and add the new ones
	if skill_effect_has_id == windowEffects.effects_checked:
		print("Same effects") # Debug, uncomment to test
		return
	
	print("Different effects") # Debug, uncomment to test

	var new_effects: Array = []

	for i in windowEffects.effects_checked.keys():
		if !skill_effect_has_id.has(i):
			new_effects.append(windowEffects.effects_checked[i])
	
	print("New effects: ", new_effects) # Debug, uncomment to test

	# Create a list of the old effects that will be removed
	var old_effects: Array = []

	for i in skill_loaded.skill_effects:
		if !windowEffects.effects_checked.has(i.id):
			old_effects.append(i)
	
	# Remove the old effects
	for i in old_effects:
		skill_loaded.skill_effects.erase(i)
	
	# Add the new one
	for i in new_effects:
		skill_loaded.skill_effects.push_back(i.duplicate())
	
	print("Skill effects: ", var_to_str(skill_loaded.skill_effects)) # Debug, uncomment to test

	# Reload the conditions list
	reload_effects()

func _on_add_effects_canceled():
	print("Add effects canceled")
	pass


func _on_info_conditions_edit_condition(condition_idx:String):
	print("Edit condition: ", condition_idx)

	var condition: BaseSkillCondition = skill_loaded.get_condition(condition_idx)

	if condition == null:
		push_error("Condition not found")
		return
	
	await windowEditCondition.edit(condition)

	windowEditCondition.popup_centered()

func _on_edit_condition_canceled():
	# print("Edit condition canceled")
	pass

func _on_edit_condition_confirmed():
	
	var condition: BaseSkillCondition = windowEditCondition.get_condition()

	if condition == null:
		push_error("Condition not found")
		return

	# Find the condition in the skill
	for i in skill_loaded.skill_conditions:
		if i.id == condition.id:
			# Remove the condition
			skill_loaded.skill_conditions.erase(i)
			# Add the new condition
			skill_loaded.skill_conditions.push_back(condition)
			break
	# print("Condition edited: ", var_to_str(skill_loaded.skill_conditions)) # Debug, uncomment to test
	
	# Reload the conditions list
	reload_conditions()


func _on_info_effects_edit_effect(effect_idx: String):
	print("Edit effect: ", effect_idx)

	var effect: BaseEffect = skill_loaded.get_effect(effect_idx)

	if effect == null:
		push_error("Effect not found")
		return
	
	await windowEditEffect.edit(effect)

	windowEditEffect.popup_centered()


func _on_edit_effect_canceled():
	pass # Replace with function body.


func _on_edit_effect_confirmed():
	
	var effect: BaseEffect = windowEditEffect.get_effect()

	if effect == null:
		push_error("Effect not found")
		return

	# Find the condition in the skill
	for i in skill_loaded.skill_effects:
		if i.id == effect.id:
			# Remove the condition
			skill_loaded.skill_effects.erase(i)
			# Add the new condition
			skill_loaded.skill_effects.push_back(effect)
			break
	print("Effect edited: ", var_to_str(skill_loaded.skill_effects)) # Debug, uncomment to test
	
	# Reload the effects list
	reload_effects()

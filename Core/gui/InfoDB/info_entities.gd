extends VBoxContainer
class_name InfoEntities

# Import the necessary enums
const CharacterGender = EnumRegister.AnimalsGender
const CharacterDiet = EnumRegister.AnimalsDiet
const CharacterMovementType = EnumRegister.AnimalsMovementType

# Character that will be shown / edited
var character: Character = null

# Nodes
## Details
var node_name: InfoItemLineEdit
var node_surname: InfoItemLineEdit
var node_age: InfoItemSpinBox
var node_race: InfoItemSpinBox
var node_lvl: InfoItemSpinBox
var node_lvl_max: InfoItemSpinBox
var node_gender: InfoItemOptions
var node_history: InfoItemTextedit

## Stats
### PV
var node_pv: InfoItemSpinBox
var node_pv_max: InfoItemSpinBox
var chart_pv: Control
### MP
var node_mp: InfoItemSpinBox
var node_mp_max: InfoItemSpinBox
var chart_mp: Control
### STR
var node_strength: InfoItemSpinBox
var node_strength_max: InfoItemSpinBox
var chart_strength: Control
### DEX
var node_dexterity: InfoItemSpinBox
var node_dexterity_max: InfoItemSpinBox
var chart_dexterity: Control
### CON
var node_constitution: InfoItemSpinBox
var node_constitution_max: InfoItemSpinBox
var chart_constitution: Control
### INT
var node_intelligence: InfoItemSpinBox
var node_intelligence_max: InfoItemSpinBox
var chart_intelligence: Control
### WIS
var node_wisdom: InfoItemSpinBox
var node_wisdom_max: InfoItemSpinBox
var chart_wisdom: Control
### CHA
var node_charisma: InfoItemSpinBox
var node_charisma_max: InfoItemSpinBox
var chart_charisma: Control
### LUCK
var node_luck: InfoItemSpinBox
var node_luck_max: InfoItemSpinBox
var chart_luck: Control

## Skills
var node_max_skills: InfoItemSpinBox
var node_add_skill: Button
var skill_list: CustomList

## Inventory

## Settings
var is_npc: InfoItemCheck
var is_merchant: InfoItemCheck
var is_jobMaster: InfoItemCheck
var is_skillMaster: InfoItemCheck
var is_spellMaster: InfoItemCheck
var is_petMaster: InfoItemCheck
var is_guildMaster: InfoItemCheck
var is_factionMaster: InfoItemCheck
var is_killable: InfoItemCheck
var is_lootable: InfoItemCheck
var is_invincible: InfoItemCheck

# Box
var box_datails: MarginContainer
var box_stats: MarginContainer
var box_skills: MarginContainer
var box_inventory: MarginContainer
var box_settings: MarginContainer

# Called when the node enters the scene tree for the first time.
func _ready():

	EnumRegister.register_base_enum()

	# Get the details tab nodes
	node_name = %infoName
	node_surname = %infoSurname
	node_age = %infoAge
	node_race = %infoRace
	node_lvl = %infoLVL
	node_lvl_max = %infoLVLMax
	node_gender = %infoGender
	node_history = %infoHistory

	# Get the stats tab nodes
	## PV
	node_pv = %infoPV
	node_pv_max = %infoPVMax
	chart_pv = %chartHP
	## MP
	node_mp = %infoMP
	node_mp_max = %infoMPMax
	chart_mp = %chartMP
	## STR
	node_strength = %infoSTR
	node_strength_max = %infoSTRMax
	chart_strength = %chartSTR
	## DEX
	node_dexterity = %infoDEX
	node_dexterity_max = %infoDEXMax
	chart_dexterity = %chartDEX
	## CON
	node_constitution = %infoCON
	node_constitution_max = %infoCONMax
	chart_constitution = %chartCON
	## INT
	node_intelligence = %infoINT
	node_intelligence_max = %infoINTMax
	chart_intelligence = %chartINT
	## WIS
	node_wisdom = %infoWIS
	node_wisdom_max = %infoWISMax
	chart_wisdom = %chartWIS
	## CHA
	node_charisma = %infoCHA
	node_charisma_max = %infoCHAMax
	chart_charisma = %chartCHA
	## LUCK
	node_luck = %infoLUCK
	node_luck_max = %infoLUCKMax
	chart_luck = %chartLUCK

	# Get the skills tab nodes
	node_max_skills = %infoMaxSkill
	node_add_skill = %addNewSkill
	skill_list = %skillList

	# Get the settings tab nodes
	is_merchant = %isMerchant
	is_jobMaster = %isJobMaster
	is_skillMaster = %isSkillMaster
	is_spellMaster = %isSpellMaster
	is_petMaster = %isPetMaster
	is_guildMaster = %isGuildMaster
	is_factionMaster = %isFactionMaster
	is_killable = %isKillable
	is_lootable = %isLootable
	is_invincible = %isInvincible

	# Get the boxes
	box_datails = %DETAILS
	box_stats = %STATS
	box_skills = %SKILLS
	box_inventory = %INVENTORY
	box_settings = %SETTINGS

	node_name.set_max_length(20)
	node_surname.set_max_length(20)

	# Add the options to the infoItems that need it
	add_options()

	# Link the signals
	link_signals()

	# Translate the box name
	translate_box_name()

	# Set list properties
	set_list_prop()

# Add the options to each infoItemOptions
func add_options():
	
	# Gender
	var gender_options = {}
	for enum_name in CharacterGender:
		var enum_key = CharacterGender[enum_name] # Get the key (value) of the enum (e.g. 0, 1,2,3,..,n)
		gender_options[enum_key] = {"name": tr(enum_name), "uid": enum_key}
	
	node_gender.set_options(gender_options)

# Translates the box name
func translate_box_name():
	box_datails.set_name(tr("DETAILS"))
	box_stats.set_name(tr("STATS"))
	box_skills.set_name(tr("SKILLS"))
	box_inventory.set_name(tr("INVENTORY"))
	box_settings.set_name(tr("SETTINGS"))

func set_list_prop():
	skill_list.set_db_data("char_idx", "box_entities", "characters_list")

	skill_list.origin = "BoxEntities"

	skill_list.add_row("SKILL_NAME", List.ItemType.OPTION_BUTTON)

# Set the entities to show
func set_entities(_character: Character):
	character = _character

	# Refresh the info
	refresh_info()

# Refresh the info
func refresh_info():

	# Check if the entities are not null
	if character == null:
		return
	
	# Set the details tab
	init_details()

	# Set the stats tab
	init_stats()

	# Set the skills tab
	init_skills()

	# Set the inventory tab
	init_inventory()

	# Set the settings tab
	init_settings()

	pass

# Init the details tab
func init_details():
	node_name.set_content(character.name)
	node_surname.set_content(character.surname)
	node_age.set_content(character.age)
	node_race.set_content(character.race)
	node_lvl.set_content(character.level)
	node_lvl_max.set_content(character.max_level)
	node_gender.set_content(character.gender)
	node_history.set_content(character.history_text)

# Init the stats tab
func init_stats():
	## PV
	node_pv.set_content(character.min_health)
	node_pv_max.set_content(character.max_health)
	chart_pv.f1.name = tr("HP")
	chart_pv.easing = character.health_easing
	## MP
	node_mp.set_content(character.min_mana)
	node_mp_max.set_content(character.max_mana)
	chart_mp.f1.name = tr("MP")
	chart_mp.easing = character.mana_easing
	## STR
	node_strength.set_content(character.strength)
	node_strength_max.set_content(character.max_strength)
	chart_strength.f1.name = tr("STR")
	chart_strength.easing = character.strength_easing
	## DEX
	node_dexterity.set_content(character.dexterity)
	node_dexterity_max.set_content(character.max_dexterity)
	chart_dexterity.f1.name = tr("DEX")
	chart_dexterity.easing = character.dexterity_easing
	## CON
	node_constitution.set_content(character.constitution)
	node_constitution_max.set_content(character.max_constitution)
	chart_constitution.f1.name = tr("CON")
	chart_constitution.easing = character.constitution_easing
	## INT
	node_intelligence.set_content(character.intelligence)
	node_intelligence_max.set_content(character.max_intelligence)
	chart_intelligence.f1.name = tr("INT")
	chart_intelligence.easing = character.intelligence_easing
	## WIS
	node_wisdom.set_content(character.wisdom)
	node_wisdom_max.set_content(character.max_wisdom)
	chart_wisdom.f1.name = tr("WIS")
	chart_wisdom.easing = character.wisdom_easing
	## CHA
	node_charisma.set_content(character.charisma)
	node_charisma_max.set_content(character.max_charisma)
	chart_charisma.f1.name = tr("CHA")
	chart_charisma.easing = character.charisma_easing
	## LUCK
	node_luck.set_content(character.luck)
	node_luck_max.set_content(character.max_luck)
	chart_luck.f1.name = tr("LUCK")
	chart_luck.easing = character.luck_easing

# Init the skills tab
func init_skills():
	node_max_skills.set_content(character.max_skills)

	# Clear the list
	skill_list.clear_items()

	# Add the skills to the list
	var skills = character.skills

	for skill in skills:
		skill_list.add_item([skill.id], {"char_idx": character.id, "options": SkillRegister.get_all_skills_name_by_ids()})

# Init the inventory tab
func init_inventory():
	pass

# Init the settings tab
func init_settings():
	is_merchant.set_content(character.is_merchant)
	is_jobMaster.set_content(character.is_job_master)
	is_skillMaster.set_content(character.is_skill_master)
	is_spellMaster.set_content(character.is_spell_master)
	is_petMaster.set_content(character.is_pet_master)
	is_guildMaster.set_content(character.is_guild_master)
	is_factionMaster.set_content(character.is_faction_master)
	is_killable.set_content(character.is_killable)
	is_lootable.set_content(character.is_lootable)
	is_invincible.set_content(character.is_invicible)

# Links the signal of the entities to the info
func link_signals():
	link_details_signal()
	link_stats_signal()
	link_skills_signal()
	link_inventory_signal()
	link_settings_signal()

# Link the details tab signals
func link_details_signal():
	node_name.content_changed.connect(_on_node_name_content_changed)
	node_surname.content_changed.connect(_on_node_surname_content_changed)
	node_age.content_changed.connect(_on_node_age_content_changed)
	node_race.content_changed.connect(_on_node_race_content_changed)
	node_lvl.content_changed.connect(_on_nodelvl_content_changed)
	node_lvl_max.content_changed.connect(_on_node_lvl_max_content_changed)
	node_gender.content_changed.connect(_on_node_gender_content_changed)
	node_history.content_changed.connect(_on_node_history_content_changed)

# Link the stats tab signals
func link_stats_signal():

	# Link the PV signals
	node_pv.content_changed.connect(_on_node_pv_content_changed)
	node_pv_max.content_changed.connect(_on_node_pv_max_content_changed)
	# Link the MP signals
	node_mp.content_changed.connect(_on_node_mp_content_changed)
	node_mp_max.content_changed.connect(_on_node_mp_max_content_changed)
	# Link the STR signals
	node_strength.content_changed.connect(_on_node_strength_content_changed)
	node_strength_max.content_changed.connect(_on_node_strength_max_content_changed)
	# Link the DEX signals
	node_dexterity.content_changed.connect(_on_node_dexterity_content_changed)
	node_dexterity_max.content_changed.connect(_on_node_dexterity_max_content_changed)
	# Link the CON signals
	node_constitution.content_changed.connect(_on_node_constitution_content_changed)
	node_constitution_max.content_changed.connect(_on_node_constitution_max_content_changed)
	# Link the INT signals
	node_intelligence.content_changed.connect(_on_node_intelligence_content_changed)
	node_intelligence_max.content_changed.connect(_on_node_intelligence_max_content_changed)
	# Link the WIS signals
	node_wisdom.content_changed.connect(_on_node_wisdom_content_changed)
	node_wisdom_max.content_changed.connect(_on_node_wisdom_max_content_changed)
	# Link the CHA signals
	node_charisma.content_changed.connect(_on_node_charisma_content_changed)
	node_charisma_max.content_changed.connect(_on_node_charisma_max_content_changed)
	# Link the LUCK signals
	node_luck.content_changed.connect(_on_node_luck_content_changed)
	node_luck_max.content_changed.connect(_on_node_luck_max_content_changed)

# Link the skills tab signals
func link_skills_signal():
	node_max_skills.content_changed.connect(_on_node_max_skills_content_changed)
	node_add_skill.pressed.connect(_on_node_add_skill_pressed)

# Link the inventory tab signals
func link_inventory_signal():
	pass

# Link the settings tab signals
func link_settings_signal():

	is_merchant.content_changed.connect(_on_is_merchant_content_changed)
	is_jobMaster.content_changed.connect(_on_is_jobMaster_content_changed)
	is_skillMaster.content_changed.connect(_on_is_skillMaster_content_changed)
	is_spellMaster.content_changed.connect(_on_is_spellMaster_content_changed)
	is_petMaster.content_changed.connect(_on_is_petMaster_content_changed)
	is_guildMaster.content_changed.connect(_on_is_guildMaster_content_changed)
	is_factionMaster.content_changed.connect(_on_is_factionMaster_content_changed)
	is_killable.content_changed.connect(_on_is_killable_content_changed)
	is_lootable.content_changed.connect(_on_is_lootable_content_changed)
	is_invincible.content_changed.connect(_on_is_invincible_content_changed)


############################################
# Signals for the details tab
############################################


func _on_nodelvl_content_changed(content: int) -> void:
	if character == null:
		# For testing purposes only
		if content > node_lvl_max.content:
			node_lvl_max.set_content(content)
		
		return
		
	_add_to_history({"level": character.level})

	character.level = content
	node_lvl_max.set_content(character.max_level)

func _on_node_lvl_max_content_changed(content: int) -> void:
	if character == null:

		# For testing purposes only
		if content < node_lvl.content:
			node_lvl.set_content(content)

		return
		
	_add_to_history({"max_level": character.max_level})

	character.max_level = content
	node_lvl.set_content(character.level)

func _on_node_name_content_changed(content: String) -> void:
	if character == null:
		return
		
	_add_to_history({"name": character.name})

	character.name = content

	# Edit the name in the list on the left
	var char_list = DataRegister.get_data("box_entities", "characters_list")

	if char_list.has(character.id):
		char_list[character.id] = [character.name, character.surname, char_list[character.id][2]]
		DataRegister.register_data("box_entities", "characters_list", char_list)

func _on_node_surname_content_changed(content: String) -> void:
	if character == null:
		return
		
	_add_to_history({"surname": character.surname})

	character.surname = content

	# Edit the name in the list on the left
	var char_list = DataRegister.get_data("box_entities", "characters_list")

	if char_list.has(character.id):
		char_list[character.id] = [character.name, character.surname, char_list[character.id][2]]
		DataRegister.register_data("box_entities", "characters_list", char_list)

func _on_node_age_content_changed(content: int) -> void:
	if character == null:
		return
		
	_add_to_history({"age": character.age})

	character.age = content

func _on_node_race_content_changed(content: int) -> void:
	if character == null:
		return
		
	_add_to_history({"race": character.race})

	character.race = content

func _on_node_gender_content_changed(content: int) -> void:
	if character == null:
		return
		
	_add_to_history({"gender": character.gender})

	character.gender = Utils.cast_to_enum(content, CharacterGender)

func _on_node_history_content_changed(content: String) -> void:
	if character == null:
		return

	_add_to_history({"history_text": character.history_text})

	character.history_text = content

############################################
# Signals for the stats tab
############################################

## PV

func _on_node_pv_content_changed(content: int) -> void:
	if character == null:
		return

	_add_to_history({"min_health": character.min_health})

	character.min_health = content

func _on_node_pv_max_content_changed(content: int) -> void:
	if character == null:
		return

	_add_to_history({"max_health": character.max_health})

	character.max_health = content

## MP

func _on_node_mp_content_changed(content: int) -> void:
	if character == null:
		return

	_add_to_history({"min_mana": character.min_mana})

	character.min_mana = content

func _on_node_mp_max_content_changed(content: int) -> void:
	if character == null:
		return

	_add_to_history({"max_mana": character.max_mana})

	character.max_mana = content

## STR

func _on_node_strength_content_changed(content: int) -> void:
	if character == null:
		return

	_add_to_history({"strength": character.strength})

	character.strength = content

func _on_node_strength_max_content_changed(content: int) -> void:
	if character == null:
		return

	_add_to_history({"max_strength": character.max_strength})

	character.max_strength = content

## DEX

func _on_node_dexterity_content_changed(content: int) -> void:
	if character == null:
		return

	_add_to_history({"dexterity": character.dexterity})

	character.dexterity = content

func _on_node_dexterity_max_content_changed(content: int) -> void:
	if character == null:
		return

	_add_to_history({"max_dexterity": character.max_dexterity})

	character.max_dexterity = content

## CON

func _on_node_constitution_content_changed(content: int) -> void:
	if character == null:
		return

	_add_to_history({"constitution": character.constitution})

	character.constitution = content

func _on_node_constitution_max_content_changed(content: int) -> void:
	if character == null:
		return

	_add_to_history({"max_constitution": character.max_constitution})

	character.max_constitution = content

## INT

func _on_node_intelligence_content_changed(content: int) -> void:
	if character == null:
		return

	_add_to_history({"intelligence": character.intelligence})

	character.intelligence = content

func _on_node_intelligence_max_content_changed(content: int) -> void:
	if character == null:
		return

	_add_to_history({"max_intelligence": character.max_intelligence})

	character.max_intelligence = content

## WIS

func _on_node_wisdom_content_changed(content: int) -> void:
	if character == null:
		return

	_add_to_history({"wisdom": character.wisdom})

	character.wisdom = content

func _on_node_wisdom_max_content_changed(content: int) -> void:
	if character == null:
		return

	_add_to_history({"max_wisdom": character.max_wisdom})

	character.max_wisdom = content

## CHA

func _on_node_charisma_content_changed(content: int) -> void:
	if character == null:
		return

	_add_to_history({"charisma": character.charisma})

	character.charisma = content

func _on_node_charisma_max_content_changed(content: int) -> void:
	if character == null:
		return

	_add_to_history({"max_charisma": character.max_charisma})

	character.max_charisma = content

## LUCK

func _on_node_luck_content_changed(content: int) -> void:
	if character == null:
		return

	_add_to_history({"luck": character.luck})

	character.luck = content

func _on_node_luck_max_content_changed(content: int) -> void:
	if character == null:
		return

	_add_to_history({"max_luck": character.max_luck})

	character.max_luck = content


############################################
# Signals for the skills tab
############################################


func _on_node_max_skills_content_changed(content: int) -> void:
	if character == null:
		return

	_add_to_history({"max_skills": character.max_skills})

	character.max_skills = content

func _on_node_add_skill_pressed() -> void:

	if character == null:
		return

	# Check if the character has reached the maximum number of skills
	if character.skills.size() >= character.max_skills:
		return
	
	# Check if the database has any skill
	if SkillRegister.get_all_skills_ids().size() == 0:
		return

	_add_to_history({"skills": character.skills})

	# Add a new skill to the character
	character.add_skill(SkillRegister.get_all_skills_ids()[0])

	%skillList.add_item([SkillRegister.get_all_skills_ids()[0]], {"char_idx": character.id, "options": SkillRegister.get_all_skills_name_by_ids()})


############################################
# Signals for the settings tab
############################################


func _on_is_merchant_content_changed(content: bool) -> void:
	if character == null:
		return

	_add_to_history({"is_merchant": character.is_merchant})

	character.is_merchant = content

func _on_is_jobMaster_content_changed(content: bool) -> void:
	if character == null:
		return

	_add_to_history({"is_job_master": character.is_job_master})

	character.is_job_master = content

func _on_is_skillMaster_content_changed(content: bool) -> void:
	if character == null:
		return

	_add_to_history({"is_skillMaster": character.is_skill_master})

	character.is_skill_master = content

func _on_is_spellMaster_content_changed(content: bool) -> void:
	if character == null:
		return

	_add_to_history({"is_spellMaster": character.is_spell_master})

	character.is_spell_master = content

func _on_is_petMaster_content_changed(content: bool) -> void:
	if character == null:
		return

	_add_to_history({"is_petMaster": character.is_pet_master})

	character.is_pet_master = content

func _on_is_guildMaster_content_changed(content: bool) -> void:
	if character == null:
		return

	_add_to_history({"is_guildMaster": character.is_guild_master})

	character.is_guild_master = content

func _on_is_factionMaster_content_changed(content: bool) -> void:
	if character == null:
		return

	_add_to_history({"is_factionMaster": character.is_faction_master})

	character.is_faction_master = content

func _on_is_killable_content_changed(content: bool) -> void:
	if character == null:
		return

	_add_to_history({"is_killable": character.is_killable})

	character.is_killable = content

func _on_is_lootable_content_changed(content: bool) -> void:
	if character == null:
		return

	_add_to_history({"is_lootable": character.is_lootable})

	character.is_lootable = content

func _on_is_invincible_content_changed(content: bool) -> void:
	if character == null:
		return

	_add_to_history({"is_invincible": character.is_invicible})

	character.is_invicible = content



func _add_to_history(history_data: Dictionary) -> void:
	var history = HistoryCharacter.new(EnumRegister.HistoryAction.CHANGE, character.id, history_data)
	HistoryRegister.add_to_history(history)

extends VBoxContainer
class_name InfoEntities

# Import the necessary enums
const CharacterGender = EnumRegister.AnimalsGender
const CharacterDiet = EnumRegister.AnimalsDiet
const CharacterMovementType = EnumRegister.AnimalsMovementType

var character: Character = null

# Nodes
var node_name: InfoItemLineEdit
var node_surname: InfoItemLineEdit
var node_age: InfoItemSpinBox
var node_race: InfoItemSpinBox
var node_pv: InfoItemSpinBox
var node_mp: InfoItemSpinBox
var node_lvl: InfoItemSpinBox
var node_lvl_max: InfoItemSpinBox
var node_gender: InfoItemOptions
var node_history: InfoItemTextedit

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
	node_pv = %infoPV
	node_mp = %infoMP

	# Get the boxes
	box_datails = %DETAILS
	box_stats = %STATS
	box_skills = %SKILLS
	box_inventory = %INVENTORY
	box_settings = %SETTINGS

	node_name.set_max_length(20)
	node_surname.set_max_length(20)

	# Add the options
	add_options()

	# Link the signals
	link_signals()

	# Translate the box name
	translate_box_name()

# Add the options to each infoItemOptions
func add_options():
	
	# Gender
	var gender_options = {}
	for enum_name in CharacterGender:
		var enum_key = CharacterGender[enum_name] # Get the key (value) of the enum (e.g. 0, 1,2,3,..,n)
		gender_options[enum_key] = {"name": enum_name, "uid": enum_key}
	
	node_gender.set_options(gender_options)

# Translates the box name
func translate_box_name():
	box_datails.set_name(tr("DETAILS"))
	box_stats.set_name(tr("STATS"))
	box_skills.set_name(tr("SKILLS"))
	box_inventory.set_name(tr("INVENTORY"))
	box_settings.set_name(tr("SETTINGS"))

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
	node_name.set_content(character.name)
	node_surname.set_content(character.surname)
	node_age.set_content(character.age)
	node_race.set_content(character.race)
	node_lvl.set_content(character.level)
	node_lvl_max.set_content(character.max_level)
	node_gender.set_content(character.gender)
	node_history.set_content(character.history_text)

	node_pv.set_content(character.max_health)
	node_mp.set_content(character.max_mana)

	pass

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
	pass

# Link the stats tab signals
func link_stats_signal():

	node_pv.content_changed.connect(_on_node_pv_content_changed)
	node_mp.content_changed.connect(_on_node_mp_content_changed)

	pass

# Link the skills tab signals
func link_skills_signal():
	pass

# Link the inventory tab signals
func link_inventory_signal():
	pass

# Link the settings tab signals
func link_settings_signal():
	pass

############################################
# Signals for the details tab
############################################

func _on_nodelvl_content_changed(content: int) -> void:
	if character == null:
		# For testing purposes only
		if content > node_lvl_max.content:
			node_lvl_max.set_content(content)
		
		return
	character.level = content
	node_lvl_max.set_content(character.max_level)

func _on_node_lvl_max_content_changed(content: int) -> void:
	if character == null:

		# For testing purposes only
		if content < node_lvl.content:
			node_lvl.set_content(content)

		return
	character.max_level = content
	node_lvl.set_content(character.level)

func _on_node_name_content_changed(content: String) -> void:
	if character == null:
		return
	character.name = content

	# Edit the name in the list on the left
	var char_list = DataRegister.get_data("box_entities", "characters_list")

	if char_list.has(character.id):
		char_list[character.id] = [character.name, character.surname, char_list[character.id][2]]
		DataRegister.register_data("box_entities", "characters_list", char_list)

func _on_node_surname_content_changed(content: String) -> void:
	if character == null:
		return
	character.surname = content

	# Edit the name in the list on the left
	var char_list = DataRegister.get_data("box_entities", "characters_list")

	if char_list.has(character.id):
		char_list[character.id] = [character.name, character.surname, char_list[character.id][2]]
		DataRegister.register_data("box_entities", "characters_list", char_list)

func _on_node_age_content_changed(content: int) -> void:
	if character == null:
		return
	character.age = content

func _on_node_race_content_changed(content: int) -> void:
	if character == null:
		return
	character.race = content

func _on_node_gender_content_changed(content: int) -> void:
	if character == null:
		return
	character.gender = Utils.cast_to_enum(content, CharacterGender)

func _on_node_history_content_changed(content: String) -> void:
	if character == null:
		return
	character.history_text = content

############################################
# Signals for the stats tab
############################################

func _on_node_pv_content_changed(content: int) -> void:
	if character == null:
		return
	character.max_health = content

func _on_node_mp_content_changed(content: int) -> void:
	if character == null:
		return
	character.max_mana = content
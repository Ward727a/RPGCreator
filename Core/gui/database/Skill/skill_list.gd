extends TabContainer

var skill_list: Array[BaseSkill] = []

@export var all_list: CustomList
@export var active_list: CustomList
@export var passive_list: CustomList
@export var reactive_list: CustomList
@export var special_list: CustomList

var clicked_on: Node = null
var clicked_skill_idx: String = ""

## Emit it when the list needs to be fully reloaded. (Rows and items will be cleaned)
signal need_fullreload()
## Emit it when the list needs to be reloaded. (Items will be cleaned, not the rows)
signal need_reload()
## Connect to this signal to get the item clicked.
signal item_clicked(node: Node, name: String, metadata: Dictionary)

# Called when the node enters the scene tree for the first time.
func _ready():

	# Check if one of the nodes is missing.
	if all_list == null or active_list == null or passive_list == null:
		print("One of the nodes is missing.")
		# Disable the node.
		self.queue_free()
		return

	# Set DB data & body.
	_init_list()

	# Link us to the need_fullreload signal.
	need_fullreload.connect(_fullreload_list)
	# Link us to the need_reload signal.
	need_reload.connect(_reload_list)

	# Link us to the item_clicked signal of all lists.
	all_list.item_clicked.connect(_click_on_item)
	active_list.item_clicked.connect(_click_on_item)
	passive_list.item_clicked.connect(_click_on_item)
	reactive_list.item_clicked.connect(_click_on_item)
	special_list.item_clicked.connect(_click_on_item)

	_set_rows()

	# Emit the need_reload signal one time to load the skills.
	need_reload.emit()

# Clean all lists of all items and rows.
func _fullclean_all():
	_fullclean_all_list()
	_fullclean_active_list()
	_fullclean_passive_list()
	_fullclean_reactive_list()
	_fullclean_special_list()

func _fullclean_all_list():
	_clean_all_list()
	all_list.clear_rows()

func _fullclean_active_list():
	_clean_active_list()
	active_list.clear_rows()

func _fullclean_passive_list():
	_clean_passive_list()
	passive_list.clear_rows()

func _fullclean_reactive_list():
	_clean_reactive_list()
	reactive_list.clear_rows()

func _fullclean_special_list():
	_clean_special_list()
	special_list.clear_rows()

func _clean_all():
	_clean_all_list()
	_clean_active_list()
	_clean_passive_list()
	_clean_reactive_list()
	_clean_special_list()

func _clean_all_list():
	all_list.clear_items()

func _clean_active_list():
	active_list.clear_items()

func _clean_passive_list():
	passive_list.clear_items()

func _clean_reactive_list():
	reactive_list.clear_items()

func _clean_special_list():
	special_list.clear_items()

func _init_list():
	_init_list_all()
	_init_list_active()
	_init_list_passive()
	_init_list_reactive()
	_init_list_special()

func _init_list_all():
	all_list.set_db_data("skill_idx", "box_skills", "skills_list")
	all_list.origin = "list_skills"

func _init_list_active():
	active_list.set_db_data("skill_idx", "box_skills", "skills_list")
	active_list.origin = "list_skills"

func _init_list_passive():
	passive_list.set_db_data("skill_idx", "box_skills", "skills_list")
	passive_list.origin = "list_skills"

func _init_list_reactive():
	reactive_list.set_db_data("skill_idx", "box_skills", "skills_list")
	reactive_list.origin = "list_skills"

func _init_list_special():
	special_list.set_db_data("skill_idx", "box_skills", "skills_list")
	special_list.origin = "list_skills"

func _set_rows():
	_set_all_list_rows()
	_set_active_list_rows()
	_set_passive_list_rows()
	_set_reactive_list_rows()
	_set_special_list_rows()

func _set_all_list_rows():
	all_list.add_row("SKILL_NAME", List.ItemType.TEXT)

func _set_active_list_rows():
	active_list.add_row("SKILL_NAME", List.ItemType.TEXT)

func _set_passive_list_rows():
	passive_list.add_row("SKILL_NAME", List.ItemType.TEXT)

func _set_reactive_list_rows():
	reactive_list.add_row("SKILL_NAME", List.ItemType.TEXT)

func _set_special_list_rows():
	special_list.add_row("SKILL_NAME", List.ItemType.TEXT)

func _add_skill_to_list(list: CustomList, skill: BaseSkill):
	var data = list.add_item([skill.name], { "skill_idx": skill.id, "clicked_on": (clicked_skill_idx)})

	# set if the skill is clicked.
	if clicked_skill_idx == skill.id:
		clicked_on = data["node"]
		# Edit the theme of the last clicked node to set it back to the default
		var base_theme: StyleBoxFlat = clicked_on.base_theme.duplicate()

		base_theme.border_width_bottom = 2
		base_theme.border_width_top = 2
		base_theme.border_width_left = 2
		base_theme.border_width_right = 2
		base_theme.border_color = Color(0.4, 0.4, 0.4, 1.0)
		clicked_on.get_node("Panel").add_theme_stylebox_override("panel", base_theme)

# Fully reload all lists.
func _fullreload_list():

	# Clean all lists.
	_fullclean_all()
	# Set all rows.
	_set_rows()

	# Get skills from register.
	_get_skills_from_register()

	# Get all skills.
	var sorted_skills = _sort_skills_all()
	# Get active skills.
	var active_skills = _sort_skills_by_type(EnumRegister.SkillTypes.ACTIVE)
	# Get passive skills.
	var passive_skills = _sort_skills_by_type(EnumRegister.SkillTypes.PASSIVE)
	# Get reactive skills.
	var reactive_skills = _sort_skills_by_type(EnumRegister.SkillTypes.REACTIVE)
	# Get special skills.
	var special_skills = _sort_skills_by_type(EnumRegister.SkillTypes.SPECIAL)

	# Add all skills to the all list.
	for skill in sorted_skills:
		_add_skill_to_list(all_list, skill)
	
	# Add active skills to the active list.
	for skill in active_skills:
		_add_skill_to_list(active_list, skill)
	
	# Add passive skills to the passive list.
	for skill in passive_skills:
		_add_skill_to_list(passive_list, skill)
	
	# Add reactive skills to the reactive list.
	for skill in reactive_skills:
		_add_skill_to_list(reactive_list, skill)
	
	# Add special skills to the special list.
	for skill in special_skills:
		_add_skill_to_list(special_list, skill)

	pass

func _reload_list():

	# Clean items
	_clean_all()

	# Get skills from register.
	_get_skills_from_register()

	# Get all skills.
	var sorted_skills = _sort_skills_all()
	# Get active skills.
	var active_skills = _sort_skills_by_type(EnumRegister.SkillTypes.ACTIVE)
	# Get passive skills.
	var passive_skills = _sort_skills_by_type(EnumRegister.SkillTypes.PASSIVE)
	# Get reactive skills.
	var reactive_skills = _sort_skills_by_type(EnumRegister.SkillTypes.REACTIVE)
	# Get special skills.
	var special_skills = _sort_skills_by_type(EnumRegister.SkillTypes.SPECIAL)

	# Add all skills to the all list.
	for skill in sorted_skills:
		_add_skill_to_list(all_list, skill)
	
	# Add active skills to the active list.
	for skill in active_skills:
		_add_skill_to_list(active_list, skill)
	
	# Add passive skills to the passive list.
	for skill in passive_skills:
		_add_skill_to_list(passive_list, skill)
	
	# Add reactive skills to the reactive list.
	for skill in reactive_skills:
		_add_skill_to_list(reactive_list, skill)
	
	# Add special skills to the special list.
	for skill in special_skills:
		_add_skill_to_list(special_list, skill)

	


func _get_skills_from_register():
	var skills = SkillRegister.get_all_skills()

	# Reset the skill list.
	skill_list = []

	for skill in skills.values():
		skill_list.append(skill)

func _sort_skills_all() -> Array[BaseSkill]:
	return skill_list

func _sort_skills_by_type(type: EnumRegister.SkillTypes) -> Array[BaseSkill]:
	var sorted_skills: Array[BaseSkill] = []
	for skill in skill_list:
		if skill.skill_type == type:
			sorted_skills.append(skill)
	return sorted_skills

func _click_on_item(_node: Node, _name: String, _metadata: Dictionary):
	item_clicked.emit(_node, _name, _metadata)

	clicked_skill_idx = _metadata['skill_idx']

	if clicked_on != null and clicked_on != _node:
		# Edit the theme of the last clicked node to set it back to the default
		var old_theme: StyleBoxFlat = clicked_on.base_theme
		clicked_on.get_node("Panel").add_theme_stylebox_override("panel", old_theme)
	
	clicked_on = _node

	# Edit the theme of the last clicked node to set it back to the default
	var base_theme: StyleBoxFlat = clicked_on.base_theme.duplicate()

	base_theme.border_width_bottom = 2
	base_theme.border_width_top = 2
	base_theme.border_width_left = 2
	base_theme.border_width_right = 2
	base_theme.border_color = Color(0.4, 0.4, 0.4, 1.0)

	clicked_on.get_node("Panel").add_theme_stylebox_override("panel", base_theme)

func _on_add_skill_pressed():
	
	SkillRegister.new_skill()

	# Emit the reload signal.
	need_reload.emit()
	


func _on_info_skills_skill_renamed():
	# Emit the reload signal.
	need_reload.emit()


func _on_info_skills_skill_type_changed():
	# Emit the reload signal.
	need_reload.emit()

extends Base
class_name Entities
# This is the base class for all entities in the game (e.g. animals, players, npcs, monsters, etc.)

var id: String = "" # Character's id

var name: String = "" # Character's name

var health: int = 100: # Character's health
	set(new):
		health = new

		if max_health < health:
			max_health = health

var max_health: int = 100: # Character's maximum health
	set(new):
		max_health = new

		if health > max_health:
			health = max_health

var strength: int = 10 # Character's strength
var max_strength: int = 10 # Character's maximum strength

var dexterity: int = 10 # Character's dexterity
var max_dexterity: int = 10 # Character's maximum dexterity

var constitution: int = 10 # Character's constitution
var max_constitution: int = 10 # Character's maximum constitution

var attack: int = 0 # Character's attack
var max_attack: int = 0 # Character's maximum attack

var defense: int = 0 # Character's defense
var max_defense: int = 0 # Character's maximum defense

var speed: int = 0 # Character's speed
var max_speed: int = 0 # Character's maximum speed


var crit_damage: int = 0 # Character's critical hit damage

var crit_chance: int = 0 # Character's critical hit chance

var dodge_chance: int = 0 # Character's dodge chance

var block_chance: int = 0 # Character's block chance

var parry_chance: int = 0 # Character's parry chance

var resist_chance: int = 0 # Character's resist chance

var attack_speed: int = 0 # Character's attack speed

var passive_hp_regen: int = 0 # Character's passive health regeneration

var passive_mp_regen: int = 0 # Character's passive mana regeneration

var status_vulnerabilities: Array = [] # Character's status vulnerabilities
var element_vulnerabilities: Array = [] # Character's element vulnerabilities
var stat_vulnerabilities: Array = [] # Character's stat vulnerabilities

var status_resistance: Array = [] # Character's status resistances
var element_resistance: Array = [] # Character's element resistances
var stat_resistance: Array = [] # Character's stat resistances

var status_immunity: Array = [] # Character's status immunities
var element_immunity: Array = [] # Character's element immunities
var stat_immunity: Array = [] # Character's stat immunities

var status_inflict_chance: Array = [] # Character's chance to inflict status effects
var element_inflict_chance: Array = [] # Character's chance to inflict elemental status effects
var stats_inflict_chance: Array = [] # Character's chance to inflict stat status effects

var aggro_chance: int = 0 # Character's chance to aggro

var is_alive: bool = true # Character's alive status

var entities_type: int = 0 # Character's entities type (animal, player, npc, monster...)

var is_hostile: bool = false # Character's hostile status
var is_friendly: bool = false # Character's friendly status

var is_invicible: bool = false # Character's invicible status - on true: can't be hit or die
var is_killable: bool = false # Character's killable status - on false: can be hit but can't die
var is_lootable: bool = false # Character's lootable status

var status_effects: Array = [] # Character's status effects
var element_effects: Array = [] # Character's element effects
var stat_effects: Array = [] # Character's stat effects

# Entity's functions
func _ready() -> void:
	# Called when the node is added to the scene for the first time.
	pass

func _process(_delta: float) -> void:
	# Called every frame. Delta is the elapsed time since the previous frame.
	pass

func _on_damage_taken(damage: int) -> void:
	# Called when the entity takes damage.
	
	if is_invicible:
		return

	health -= damage
	if health <= 0:
		if is_killable:
			health = 0
			is_alive = false
		else:
			health = 1

func _on_healing_taken(healing: int) -> void:
	# Called when the entity receives healing.
	health += healing
	if health > max_health:
		health = max_health

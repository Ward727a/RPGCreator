extends Base
class_name EnumRegister
## Register for the enums
## You can register the enums in it, and then use them in the project
## [br]
## This is useful to avoid having to preload the enums in every script that needs them.
## [br] - Please read the comments in the script to understand how to use it. - [br]
## I try to use this class functions in all of the engine scripts to allow the user to easily edit/add new enums.
## [br]
## But some scripts require that I use the constant directly so you may have to edit this script to edit the
## engine enums without taking the risk of breaking it.

##!       !!!IMPORTANT!!!        PLEASE READ - if you want to make a pull request:
##!
##! Please, when you add a new enum to the engine (via const), add 1 to the "Const numbers" below & add 1 to the "Dict numbers" below
##! If you don't do this, and you do a pull request, it will be rejected until you fix this. (unless you have a good reason to not do it)
##! This is to avoid having to check all the enums in the engine to see if they are all registered in this script.
##!

## TODO: Maybe separate each enum constant in some categories (e.g. Animals.Enum, Effects.Enum, etc.), so it will be easier to find them (but maybe it's not necessary? tbh I don't know)

static var enums: Dictionary = {}
static var registered_base: bool = false

# First creating all constants - You can use this to set the type of a var (e.g. var my_var: AnimalsDiet)
## Animals
const AnimalsDiet = preload("res://Core/Enums/animals/AnimalsDiet.gd").AnimalsDiet
const AnimalsEasingType = preload("res://Core/Enums/animals/AnimalsEasingType.gd").AnimalsEasingType
const AnimalsGender = preload("res://Core/Enums/animals/AnimalsGender.gd").AnimalsGender
const AnimalsMovementType = preload("res://Core/Enums/animals/AnimalsMovementType.gd").AnimalsMovementType
## Effects
const EffectTypes = preload("res://Core/Enums/effects/effect_type.gd").EffectTypes
const EffectOutcome = preload("res://Core/Enums/effects/effect_outcome.gd").EffectOutcome
const EffectTime = preload("res://Core/Enums/effects/effect_time.gd").EffectTime
const EffectTarget = preload("res://Core/Enums/effects/effect_target.gd").EffectTarget
## Skills
const SkillTypes = preload("res://Core/Enums/skills/skill_types.gd").SkillTypes
const SkillTarget = preload("res://Core/Enums/skills/skill_target.gd").SkillTarget
const SkillCooldownType = preload("res://Core/Enums/skills/skill_cooldown_type.gd").SkillCooldownType
## Objects
const ObjectType = preload("res://Core/Enums/objects/object_type.gd").ObjectType
## Items
const ItemObtainability = preload("res://Core/Enums/objects/items/item_obtainability.gd").ItemObtainability
const ItemRarity = preload("res://Core/Enums/objects/items/item_rarity.gd").ItemRarity
const ItemUsability = preload("res://Core/Enums/objects/items/item_usability.gd").ItemUsability
const ItemDurabilityType = preload("res://Core/Enums/objects/items/item_durability_type.gd").ItemDurabilityType
const ItemRepairability = preload("res://Core/Enums/objects/items/item_repairability.gd").ItemRepairability
const ItemDropability = preload("res://Core/Enums/objects/items/item_dropability.gd").ItemDropability
const ItemType = preload("res://Core/Enums/objects/items/item_type.gd").ItemType
## Quests
const QuestStatus = preload("res://Core/Enums/quests/quest_status.gd").QuestStatus
const QuestType = preload("res://Core/Enums/quests/quest_type.gd").QuestType
## Quests Objectives
const ObjectiveType = preload("res://Core/Enums/quests/objective_type.gd").ObjectiveType
## GUI
### BoxEntities
const BoxEntitiesChange = preload("res://Core/Enums/gui/entities/box_entities_change.gd").BoxEntitiesChange
### History
const HistoryAction = preload("res://Core/Enums/gui/history/history_action.gd").HistoryAction
const HistoryLocation = preload("res://Core/Enums/gui/history/history_location.gd").HistoryLocation

##### Const numbers: 24 ##### This number NEED to be the equal or less (Const<=Dict) than the "Dict numbers" below

## Register all the basics engine enums [b](!! If you don't know what you are doing, don't edit this function !!)[/b][br]
## This function NEEDS to be called at the beginning of the engine, otherwise you could have some issues.
static func register_base_enum() -> void:

	if registered_base:
		return
	
	registered_base = true

	# Creating an array with all the enums
	# This is useful to avoid having to register each enum one by one
	# You can add your custom enums here but don't touch the already existing ones (unless you know what you are doing)
	#
	# If your scared to break the engine, you can just call the function "register_enum" for each enum you want to register
	# Check the function "register_enum" for more information
	var enums_array: Dictionary = {
		# Start of the engine enums - The name of the key should be the same as the enum name
		## Animals
		"AnimalsDiet": AnimalsDiet,
		"AnimalsEasingType": AnimalsEasingType,
		"AnimalsGender": AnimalsGender,
		"AnimalsMovementType": AnimalsMovementType,
		## Skills
		"SkillTypes": SkillTypes,
		"SkillTarget": SkillTarget,
		## Effects
		"EffectTypes": EffectTypes,
		"EffectOutcome": EffectOutcome,
		"EffectTime": EffectTime,
		"EffectTarget": EffectTarget,
		"ObjectType": ObjectType,
		## Items
		"ItemObtainability": ItemObtainability,
		"ItemRarity": ItemRarity,
		"ItemUsability": ItemUsability,
		"ItemDurabilityType": ItemDurabilityType,
		"ItemRepairability": ItemRepairability,
		"ItemDropability": ItemDropability,
		"ItemType": ItemType,
		## Quests
		"QuestStatus": QuestStatus,
		"QuestType": QuestType,
		## Quests Objectives
		"ObjectiveType": ObjectiveType,
		## GUI - BoxEntities
		"BoxEntitiesChange": BoxEntitiesChange,
		## History
		"HistoryAction": HistoryAction,
		"HistoryLocation": HistoryLocation,
		# End of the engine enums

		# Add your custom enums after this comment
	}

	##### Dict numbers: 24 ##### This number NEED to be equal or more (Dict>=Const)than the "Const numbers" above

	# Then registering them
	for enum_name in enums_array:
		register_enum(enum_name, enums_array[enum_name])


## Register an enum, this can be used to register any enum (engine or custom)
## [br]
## @param enum_name: the name of the enum [br]
## @param enum_constant: the constant of the enum (e.g. 'const AnimalsDiet = preload("res://Core/Enums/animals/AnimalsDiet.gd").AnimalsDiet')
## @param add_salt_if_already_exist: if the enum already exist, add a salt to the name (default: false), If you set this to true, please be sure to get the enum with the new name (the one returned by this same function)
## [br]
## Example:
## [codeblock]
## # [file: Plugins/MyPlugin/MyEnumDeclaration.gd]
## class_name MyEnumDeclaration
##
## const AnimalsDiet = preload("res://Core/Enums/animals/AnimalsDiet.gd").AnimalsDiet
##
## func _init():
##     EnumRegister.register_enum("MyEnum", AnimalsDiet) # Registering the AnimalsDiet enum
##
## # [file: Plugins/MyPlugin/MyPlugin.gd]
## func _init():
##     MyEnumDeclaration.new() # Registering the enum
##
##     # Then you can get the enum like this
##     const diet = EnumRegister.get_enum("MyEnum") # Getting the AnimalsDiet enum
## [/codeblock]
static func register_enum(enum_name: String, enum_constant: Variant, add_salt_if_already_exist: bool = false) -> String:
	print("Registering enum: " + enum_name)

	if enums.has(enum_name):
		if add_salt_if_already_exist:
			print("Enum already registered: " + enum_name + ", adding salt to the name")

			# Adding a salt to the name
			# We create 2 random numbers to reduce the chance of having the same salt
			var salt1 = randi()%1000
			var salt2 = randi()%1000

			while enums.has(enum_name + "_S" + str(salt1 + salt2)):
				salt1 = randi()%1000
				salt2 = randi()%1000

			enum_name += str("_S", str(salt1 + salt2))
		else:
			print("Enum already registered: " + enum_name)
			return ""

	enums[enum_name] = enum_constant
	return enum_name

## Get an enum, this can be used to get any registered enum (engine or custom)
## [br]
## @param enum_name: the name of the enum
static func get_enum(enum_name: String) -> Variant:
	return enums[enum_name]

## Return all registered enums, it will returns ALL enums (including the engine ones) so be careful!
static func get_all_enums() -> Dictionary:
	return enums

## Unregister an enum, yes, like any other function in this, you can unregister the engine enums too.
## [br]
## @param enum_name: the name of the enum
## [br]
## @return bool: true if the enum was unregistered, false otherwise
static func unregister_enum(enum_name: String) -> bool:
	return enums.erase(enum_name)

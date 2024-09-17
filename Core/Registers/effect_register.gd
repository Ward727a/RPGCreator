extends Base
class_name EffectRegister
# This class is used to register all the effects in the game

func _init():
	need_register = false # Set it to false to avoid registering this class in the class register

	set_custom_name("EffectRegister")

static var data: Dictionary = {}

static func set_effects(effects: Dictionary) -> void:
	# Set the effects data
	data = effects

static func set_effect(effect_id: String, effect: BaseEffect) -> void:
	# Set a effect data
	data[effect_id] = effect

static func get_effect(effect_id: String) -> BaseEffect:
	# Get a effect data
	return data[effect_id].duplicate()

static func get_pure_effect(effect_id: String) -> BaseEffect:
	# Get a effect data without duplicating it - WARNING: Use this function only if you know what you are doing
	return data[effect_id]

static func get_all_effects() -> Dictionary:
	# Get all effects data
	return data

static func get_all_effects_names() -> Array:
	# Get all effects names
	var names = []
	for effect in data.values():
		names.append(effect.name)
	return names

static func get_all_effects_ids() -> Array:
	# Get all effects ids
	return data.keys()

static func get_all_effects_name_by_ids() -> Array:
	# Get all effects names by ids
	var names = []
	for effect_id in data.keys():
		names.append({"name": data[effect_id].name, "value": effect_id})
	return names

static func remove_effect(effect_id: String) -> bool:
	# Remove a effect data
	return data.erase(effect_id)

static func add_effect(effect: BaseEffect) -> String:
	# Add a effect data

	# Check if the effect has an id, if not, generate one
	if effect.id == "":
		effect.id = IdGenerator.new().generate_id()

	var effect_id = effect.id
	data[effect_id] = effect
	return effect_id

static func new_effect() -> BaseEffect:
	# Create a new effect

	var id = IdGenerator.new().generate_id()

	var effect = BaseEffect.new()

	effect.id = id
	effect.name = "New Effect"
	effect.set_custom_name("Effect")

	add_effect(effect)

	return effect

static func get_effect_by_name(name: String) -> BaseEffect:
	# Get a effect by name
	for effect in data.values():
		if effect.name == name:
			return effect
	return null

static func effects_to_string() -> String:
	# Convert the skills data to a string
	return var_to_str(data)

static func has_effect(effect_id: String) -> bool:
	# Check if the effect exists
	return data.has(effect_id)

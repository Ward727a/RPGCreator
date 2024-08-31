extends Entities
class_name Animals

const AnimalsDiet = EnumRegister.AnimalsDiet
const AnimalsGender = EnumRegister.AnimalsGender
const AnimalsMovementType = EnumRegister.AnimalsMovementType

# Character's level
# 
# IMPORTANT - PLEASE READ
# If the level is higher than the max_level, the max_level will be set to the level
# For example, if the max_level is 10 and we set his level to 15, the max_level will be set to 15
# This is why if the player levels up, we don't want to just set the level to the new level, 
# but we want first to check if the new level is higher than the max_level.
# If it is, we stop at the max_level, otherwise, we set the level to the new level.
# For this please use the appropriate function to set the new level in-game.
var level: int = 1: 
	set(new):
		if max_level > 0:
			if new > max_level:
				level = max_level
			else:
				level = new
		else:
			level = new
var max_level: int = 1:
	set(new):
		if new < level:
			level = new
		max_level = new

var race: int = 0 # Animal's species
var age: int = 0 # Animal's age
var movement_type: AnimalsMovementType = AnimalsMovementType.WALK # Animal's movement type (0 = walk, 1 = fly, 2 = swim)
var diet: AnimalsDiet = AnimalsDiet.HERBIVORE # Animal's diet (0 = herbivore, 1 = carnivore, 2 = omnivore)
var gender: AnimalsGender = AnimalsGender.MALE # Animal's gender (0 = male, 1 = female)

var mana: int = 100 # Character's mana
var max_mana: int = 100 # Character's maximum mana

func _on_mana_spent(mana_spent: int) -> void:
	# Called when the entity spends mana.
	mana -= mana_spent
	if mana < 0:
		mana = 0

func _on_mana_restored(mana_restored: int) -> void:
	# Called when the entity restores mana.
	mana += mana_restored
	if mana > max_mana:
		mana = max_mana
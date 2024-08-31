extends Base
class_name _Object
# This is the base class for all objects in the game (e.g. items, money, exp, etc.)
# The name of the class is _Object because Object is a reserved word in Godot (it's a built-in class)

const ObjectType = preload("res://Core/Enums/objects/object_type.gd").ObjectType

var id: int = 0 # Object's ID
var name: String = "" # Object's name
var description: String = "" # Object's description

var type: ObjectType = ObjectType.ITEM # Object's type

# function that need to be implemented in the child classes
# This function is called when the object is used by the player
func use():
    write("'use' function not implemented")
    pass

# Function that need to be implemented in the child classes
# This function is called when the object is storred by the player (e.g. in a chest)
func store():
    write("'store' function not implemented")
    pass

# Function that need to be implemented in the child classes
# This function is called when the object is taken by the player
func take():
    write("'take' function not implemented")
    pass
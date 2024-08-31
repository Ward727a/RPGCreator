extends _Object
class_name money

var currency: String = "Gold" # Currency type
var value: int = 0 # Money value

# Override the use function
func use():
    pass

func store():
    pass

# Return the money value
func get_value():
    return value
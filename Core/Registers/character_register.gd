extends Base
class_name CharacterRegister
# This class is used to register all the characters of the project.
# TODO: Optimize the class by saving data in a tempfile?

# Characters template name:
const CHAR_TEMPLATE_NAME: Array = ["Alfred", "Alice", "Bob", "Charlie", "David", "Eve", "Frank", "Grace", "Heidi", "Isaac", "Judy", "Kevin", "Linda", "Michael", "Nancy", "Oliver", "Pamela", "Quentin", "Rachel", "Steve", "Tina", "Ursula", "Victor", "Wendy", "Xander", "Yvonne", "Zach"]
const CHAR_TEMPLATE_SURNAME: Array = ["Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor", "Anderson", "Thomas", "Jackson", "White", "Harris", "Martin", "Thompson", "Garcia", "Martinez", "Robinson", "Clark", "Rodriguez", "Lewis", "Lee", "Walker", "Hall", "Allen", "Young", "Hernandez", "King", "Wright", "Lopez", "Hill", "Scott", "Green", "Adams", "Baker", "Gonzalez", "Nelson", "Carter", "Mitchell", "Perez", "Roberts", "Turner", "Phillips", "Campbell", "Parker", "Evans", "Edwards", "Collins", "Stewart", "Sanchez", "Morris", "Rogers", "Reed", "Cook", "Morgan", "Bell", "Murphy", "Bailey", "Rivera", "Cooper", "Richardson", "Cox", "Howard", "Ward", "Torres", "Peterson", "Gray", "Ramirez", "James", "Watson", "Brooks", "Kelly", "Sanders", "Price", "Bennett", "Wood", "Barnes", "Ross", "Henderson", "Cole", "Jenkins", "Perry", "Powell", "Long", "Patterson", "Hughes", "Flores", "Washington", "Butler", "Simmons", "Foster", "Gonzales", "Bryant", "Alexander", "Russell", "Griffin", "Diaz", "Hayes"]

func _init():
    set_custom_name("CharacterRegister")

static var data: Dictionary = {}

static func set_characters(characters: Dictionary) -> void:
    # Set the characters data
    data = characters

static func set_character(character_id: String, character: Character) -> void:
    # Set a character data
    data[character_id] = character

static func get_character(character_id: String) -> Character:
    # Get a character data
    return data[character_id]

static func get_all_characters() -> Dictionary:
    # Get all characters data
    return data

static func remove_character(character_id: String) -> bool:
    # Remove a character data
    return data.erase(character_id)

static func add_character(character: Character) -> String:
    # Add a character data

    # Check if the character has an id, if not, generate one
    if character.id == "":
        character.id = IdGenerator.new().generate_id()

    var character_id = character.id
    data[character_id] = character
    return character_id

static func new_character() -> Character:
    # Create a new character

    var id: String = IdGenerator.new().generate_id()

    var character = Character.new()

    character.id = id
    character.name = CHAR_TEMPLATE_NAME.pick_random()
    character.surname = CHAR_TEMPLATE_SURNAME.pick_random()
    character.set_custom_name(id)

    add_character(character)

    return character

static func characters_to_string() -> String:
    # Convert the characters data to a string
    return var_to_str(data)

static func has_character(character_id: String) -> bool:
    # Check if a character exists
    return data.has(character_id)
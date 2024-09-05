extends Base
class_name BaseSkill

const SkillTypes = EnumRegister.SkillTypes
const SkillTarget = EnumRegister.SkillTarget

var name: String = "" # Skill's name
var description: String = "" # Skill's description
var level: int = 1 # Skill's level (For now, it's not used - It's different from the character's level!)

var skill_type: SkillTypes = SkillTypes.ACTIVE # Skill's type
var skill_target: SkillTarget = SkillTarget.SELF # Skill's target (self, ally, enemy, all)
var skill_effects: Array[BaseEffect] = [] # Skill's effect
var skill_cost: Dictionary = {} # Skill's cost (ex: { "hp": 10, "mp": 5})
var skill_conditions: Array[BaseSkillCondition] = [] # Skill's conditions
var skill_caster: Character = null # Skill's caster
var skill_cooldown: int = 0 # Skill's cooldown

# Function to use the skill
# It should return true if the skill was used successfully, and false if it wasn't
func use_skill(_character: Character, _target: Character) -> bool:
    if !can_use_skill(_character):
        return false

    return true

# Function to apply the effects of the skill
# It should apply all the effects of the skill to the target
func apply_effects(_target: Character) -> void:
    for effect in skill_effects:
        effect.apply_effect(_target)

## Function to calculate the damage of the skill[br]
## It should return the damage that the skill will do to the target[br]
## TODO: Implement this function
## [br]
##! This function doesn't do anything yet
func calculate_damage(_target: Character) -> int:
    return 0

# Function to check if the skill can be used
# It should return true if the skill can be used, and false if it can't
func can_use_skill(_character: Character) -> bool:
    return check_conditions(_character)

## Function to check if the target is valid[br]
## It should return [b]true[/b] if the target is valid, and [b]false[/b] if it isn't[br]
func is_valid_target(_target: Character) -> bool:
    match skill_target:
        SkillTarget.SELF:
            return _target == skill_caster
        SkillTarget.ALLY:
            return _target.is_ally(skill_caster)
        SkillTarget.ENEMY:
            return _target.is_enemy(skill_caster)
        SkillTarget.ALL:
            return true
    return false

## Function to check if the conditions are met [br]
## It will check if all the conditions are met [br]
## It should return [b]true[/b] if all the conditions are met, and [b]false[/b] if they aren't
func check_conditions(_character: Character) -> bool:
    for condition in skill_conditions:
        if !condition.check_condition(_character):
            return false
    return true

func set_cooldown(_cooldown: int) -> void:
    skill_cooldown = _cooldown

func set_cost(_cost: Dictionary) -> void:
    skill_cost = _cost

func set_effects(_effects: Array[BaseEffect]) -> void:
    skill_effects = _effects

func set_conditions(_conditions: Array[BaseSkillCondition]) -> void:
    skill_conditions = _conditions
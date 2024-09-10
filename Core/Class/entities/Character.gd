extends Animals
class_name Character

var surname: String = "" # Character's surname

var religion: int = 0 # Character's religion
var reputation: Array = [] # Character's reputation

var classes: int = 0 # Character's class
var subclass: int = -1 # Character's subclass

var jobs: Array = [] # Character's jobs
var titles: Array = [] # Character's titles
var guilds: Array = [] # Character's guild

var inventory: Array = [] # Character's inventory
var max_inventory: int = 0 # Character's maximum inventory
var equipment: Array = [] # Character's equipment
var max_equipment: int = 0 # Character's maximum equipment

var mount: Object = null # Character's mount
var mount_list: Array = [] # Character's mount list

var pet: Object = null # Character's pet
var pet_list: Array = [] # Character's pet list

var intelligence: int = 10 # Character's intelligence
var max_intelligence: int = 10 # Character's maximum intelligence
var intelligence_easing: EnumRegister.AnimalsEasingType = EnumRegister.AnimalsEasingType.EASE_OUT_SINE

var wisdom: int = 10 # Character's wisdom
var max_wisdom: int = 10 # Character's maximum wisdom
var wisdom_easing: EnumRegister.AnimalsEasingType = EnumRegister.AnimalsEasingType.EASE_OUT_SINE

var charisma: int = 10 # Character's charisma
var max_charisma: int = 10 # Character's maximum charisma
var charisma_easing: EnumRegister.AnimalsEasingType = EnumRegister.AnimalsEasingType.EASE_OUT_SINE

var cast_speed: int = 0 # Character's cast speed
var max_cast_speed: int = 0 # Character's maximum cast speed

var skills: Array = [] # Character's skills
var max_skills: int = 0 # Character's maximum skills

var spells: Array = [] # Character's spells
var max_spells: int = 0 # Character's maximum spells

var luck: int = 0 # Character's luck
var max_luck: int = 0 # Character's maximum luck
var luck_easing: EnumRegister.AnimalsEasingType = EnumRegister.AnimalsEasingType.EASE_OUT_SINE

var is_merchant: bool = false # Character's merchant status
var is_job_master: bool = false # Character's job master status
var is_skill_master: bool = false # Character's skill master status
var is_spell_master: bool = false # Character's spell master status
var is_mount_master: bool = false # Character's mount master status
var is_pet_master: bool = false # Character's pet master status
var is_guild_master: bool = false # Character's guild master status
var is_faction_master: bool = false # Character's faction master status

var on_mount: bool = false # Character's mount status

var gold: int = 0 # Character's gold

var history_text: String = "\ntest\n\r\t" # Character's history

func is_ally(_character: Character) -> bool:
    if _character == self:
        return true
    return false

func is_enemy(_character: Character) -> bool:
    if _character != self:
        return true
    return false

func add_skill(_skill_name: String) -> void:
    var skill = SkillRegister.get_skill(_skill_name)
    if skill == null:
        return
    skills.append(skill)
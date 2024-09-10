extends HistoryBase
class_name HistorySkill

var skill_idx: String = ""
var edited_data: Dictionary = {}

func _init(_action: EnumRegister.HistoryAction, _skill_idx: String, _edited_data: Dictionary = {}):
    location = EnumRegister.HistoryLocation.DB_SKILL
    action = _action
    skill_idx = _skill_idx
    edited_data = _edited_data

func undo_change() -> void:
    print("Undo changement to skill")
    var skill: BaseSkill = SkillRegister.get_skill(skill_idx)

    for key in edited_data:
        skill.set(key, edited_data[key])
    
    SkillRegister.set_skill(skill_idx, skill)

func redo_change() -> void:
    print("Redo changement to skill")
    var skill: BaseSkill = SkillRegister.get_skill(skill_idx)

    for key in edited_data:
        skill.set(key, edited_data[key])
    
    SkillRegister.set_skill(skill_idx, skill)
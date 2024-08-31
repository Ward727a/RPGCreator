extends quests_objectives
class_name objectives_talk

var target: Entities = null
var amount: int = 0
var current_amount: int = 0

func _init(_target: Entities, _amount: int):
    target = _target
    amount = _amount

    allowed_events.append(["pre_talk", "post_talk"])

# When the player talks to the target
func talk(_entity: Entities):
    # Emit the pre_talk event
    call_event("pre_talk", [_entity])
    
    var success = false

    if _entity == target:
        current_amount += 1
        if current_amount >= amount:
            success = true
            complete()
    
    # Emit the post_talk event
    call_event("post_talk", [_entity, success])
    pass
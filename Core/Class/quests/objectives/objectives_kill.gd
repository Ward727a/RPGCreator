extends quests_objectives
class_name objectives_kill

var target: Entities = null
var amount: int = 0
var current_amount: int = 0

func _init(_target: Entities, _amount: int):
    target = _target
    amount = _amount

    allowed_events.append(["pre_kill", "post_kill"])

# When the player kills the target
func kill(_entity: Entities):
    # Emit the pre_kill event
    call_event("pre_kill", [_entity])
    
    var success = false

    if _entity == target:
        current_amount += 1
        if current_amount >= amount:
            success = true
            complete()
    
    # Emit the post_kill event
    call_event("post_kill", [_entity, success])
    pass
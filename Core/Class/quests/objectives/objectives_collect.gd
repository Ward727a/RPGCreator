extends quests_objectives
class_name objectives_collect

var target: _Object = null
var amount: int = 0
var current_amount: int = 0

func _init(_target: _Object, _amount: int):
    target = _target
    amount = _amount

    allowed_events.append(["pre_collect", "post_collect"])

# When the player collects the target
func collect(_item: _Object):
    # Emit the pre_collect event
    call_event("pre_collect", [_item])
    
    
    var success: bool = false

    if _item == target:
        current_amount += 1
        if current_amount >= amount:
            success = true
            complete()
    
    # Emit the post_collect event
    call_event("post_collect", [_item, success])
    pass

# Return the current amount
func get_current_amount():
    return current_amount
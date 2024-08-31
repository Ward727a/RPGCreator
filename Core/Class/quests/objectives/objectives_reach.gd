extends quests_objectives
class_name objectives_reach

var target: Vector2 = Vector2(0, 0)
var radius: int = 0

func _init(_target: Vector2, _radius: int):
    target = _target
    radius = _radius

    allowed_events.append(["pre_reach", "post_reach"])

# When the player reaches the target
func reach(_position: Vector2):
    # Emit the pre_reach event
    call_event("pre_reach", [_position])
    
    var success: bool = (_position.distance_to(target) <= radius)

    if success:
        complete()
    
    # Emit the post_reach event
    call_event("post_reach", [_position, success])
    pass
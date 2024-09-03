class_name HistoryRegister

static var history: Array = []

## Add a history to the history list
static func add_to_history(history_data: HistoryBase) -> void:

    # Add to history with max 30 elements
    if history.size() >= 30:
        history.pop_front()

    history.append(history_data)

    print("History added: ", var_to_str(history_data))

## Get the history list
static func get_history() -> Array:
    return history

## Pop the last history from the history list
static func pop_history() -> void:
    history.pop_back()
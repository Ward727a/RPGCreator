class_name HistoryRegister

static var history: Array[HistoryBase] = [] # Used for undo
static var future: Array[HistoryBase] = [] # Used for redo
static var registered: bool = true # If the modified data was registered (true = yes, false = no)

static var AddedToHistory: Signal = Utils.make_static_signal(HistoryRegister, "AddedToHistory")

## Add a history to the history list
static func add_to_history(history_data: HistoryBase) -> void:

    # Add to history with max 30 elements
    if history.size() >= 30:
        history.pop_front()

    history.append(history_data)

    registered = false # The data is not registered yet (used for warning the user if he tries to leave the menu without saving)

    print("History added: ", var_to_str(history_data))
    AddedToHistory.emit(history_data)

## Get the history list
static func get_history() -> Array[HistoryBase]:
    return history

## Pop the last history from the history list (used for undo)
static func pop_history() -> HistoryBase:
    var futur: HistoryBase = history.pop_back()

    # We re-add the popped history to the future
    future.append(futur)

    ## Return the object
    return futur

## Get the future list
static func get_future() -> Array[HistoryBase]:
    return future

## Pop the last future from the future list (used for redo)
static func pop_future() -> HistoryBase:
    var past: HistoryBase = future.pop_back()

    # We re-add the popped future to the history
    history.append(past)

    ## Return the object
    return past
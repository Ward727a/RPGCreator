class_name Utils
# This is the base class for all utils scripts in the game

## Casts an integer to an enum
## You can give directly the enum as a parameter or the path to the enum
static func cast_to_enum(value: int, _enum: Variant) -> Variant:

    if value < 0:
        return _enum.values()[0]
    
    if value >= len(_enum.values()):
        return _enum.values()[len(_enum.values()) - 1]

    return _enum.values()[value]

## This function is used to create a signal [br]
## It need to be used only in the case that you want to create a STATIC signal [br]
## If you want to create a non-static signal, just use the classic "signal" keyword [br]
## [br] [br][b]Important - About parameters:[/b] [br]
## If you want to assign a parameters, you just need to emit the signal with the parameters (no need to define them in the signal creation) [br]
## But if you want to define the parameters in the signal creation (ex: your_signal.emit("1", 2)), your function that will receive the signal should be: [br]
## [codeblock]
## # We respect the number of parameters and their types
## func your_function(_param1: String, _param2: int):
##     pass
## [/codeblock]
## Or else you will not be able to receive the signal
static func make_static_signal(object, signal_name: String):
    if object.has_user_signal(signal_name): # Check if the signal already exists
        return Signal(object, signal_name) # If it exists, return the signal
    object.add_user_signal(signal_name) # If it doesn't exist, create the signal on the object
    return Signal(object, signal_name) # Return the signal

static func to_hex_color(color: Color) -> String:
    return str("#", color.to_html())

# Source for the following functions: https://easings.net/
class Math:
    # In
    static func ease_in_circ(x: float) -> float:
        return (1 - sqrt(1 - pow(x, 2)))
    
    static func ease_in_sine(x: float) -> float:
        return (1 - cos((x * PI) / 2 ))
    
    static func ease_in_quint(x: float) -> float:
        return pow(x, 5)
    
    static func ease_in_cubic(x: float) -> float:
        return pow(x, 3)
    
    static func ease_in_quad(x: float) -> float:
        return pow(x, 2)
    
    static func ease_in_quart(x: float) -> float:
        return pow(x, 4)
    
    static func ease_in_expo(x: float) -> float:
        return  float(0) if (x == 0) else pow(2, 10 * (x - 1))
    
    # Out  
    static func ease_out_circ(x: float) -> float:
        return sqrt(1 - pow(x - 1, 2))
    
    static func ease_out_sine(x: float) -> float:
        return sin((x * PI) / 2)
    
    static func ease_out_quint(x: float) -> float:
        return pow(x - 1, 5) + 1
    
    static func ease_out_cubic(x: float) -> float:
        return pow(x - 1, 3) + 1
    
    static func ease_out_quad(x: float) -> float:
        return 1 - pow(1 - x, 2)
    
    static func ease_out_quart(x: float) -> float:
        return 1 - pow(1 - x, 4)
    
    static func ease_out_expo(x: float) -> float:
        return float(1) if (x == 1) else 1 - pow(2, -10 * x)

    static func ease_none(x: float) -> float:
        return x

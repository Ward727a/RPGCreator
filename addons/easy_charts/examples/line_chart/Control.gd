extends Control

@onready var chart: Chart = $Panel/MarginContainer/VBoxContainer/Chart
@onready var optionEasing: OptionButton = %easingOption

@export var title: String = "Air Quality Monitoring"
@export var interactive: bool = true

enum easing_type {
    EASE_IN_CIRC,
    EASE_IN_SINE,
    EASE_IN_QUINT,
    EASE_IN_CUBIC,
    EASE_IN_QUAD,
    EASE_IN_QUART,
    EASE_IN_EXPO,
    EASE_OUT_CIRC,
    EASE_OUT_SINE,
    EASE_OUT_QUINT,
    EASE_OUT_CUBIC,
    EASE_OUT_QUAD,
    EASE_OUT_QUART,
    EASE_OUT_EXPO,
    NONE
}

@export_group("data")
@export
var x: Array = []
@export
# Generate some random values for the y axis (100 values)
var y: Array = []
@export var easing: easing_type = easing_type.EASE_OUT_SINE
@export var min_value_node: Node = null
@export var max_value_node: Node = null

@export_group("graph")
@export var marker_type: Function.Marker = Function.Marker.NONE
@export var line_type: Function.Type = Function.Type.LINE
@export var interpolation_type: Function.Interpolation = Function.Interpolation.LINEAR
@export var line_width: float = 1.0
@export var graph_unit_name: String = "Pressure"

@export_group("colors")
@export var background_color: Color = Color.TRANSPARENT
@export var frame_color: Color = Color("#161a1d")
@export var grid_color: Color = Color("#283442")
@export var text_color: Color = Color.WHITE_SMOKE
@export var ticks_color: Color = Color("#283442")
@export var graph_color: Color = Color("#eb5463")

@export_group("elements visibility")
@export var draw_background: bool = true
@export var draw_borders: bool = false
@export var draw_bounding_box: bool = false
@export var draw_frame: bool = true
@export var draw_grid_box: bool = false
@export var draw_origin: bool = false
@export var draw_ticks: bool = false
@export var show_legend: bool = false
@export var show_tick_labels: bool = false
@export var show_title: bool = false
@export var show_x_label: bool = false
@export var show_y_label: bool = false

@export_group("x-y datas")
@export var x_label: String = ""
@export var x_scale: int = 10
@export var x_tick_size: int = 0
@export var x_ticklabel_space: int = 0
@export var y_label: String = ""
@export var y_scale: int = 10
@export var y_tick_size: int = 0
@export var y_ticklabel_space: int = 0



# This Chart will plot 3 different functions
var f1: Function

# This function will return the easing function based on the selected easing type
func get_easing() -> Callable:
    match easing:
        easing_type.EASE_IN_CIRC:
            return Utils.Math.ease_in_circ
        easing_type.EASE_IN_SINE:
            return Utils.Math.ease_in_sine
        easing_type.EASE_IN_QUINT:
            return Utils.Math.ease_in_quint
        easing_type.EASE_IN_CUBIC:
            return Utils.Math.ease_in_cubic
        easing_type.EASE_IN_QUAD:
            return Utils.Math.ease_in_quad
        easing_type.EASE_IN_QUART:
            return Utils.Math.ease_in_quart
        easing_type.EASE_IN_EXPO:
            return Utils.Math.ease_in_expo
        easing_type.EASE_OUT_CIRC:
            return Utils.Math.ease_out_circ
        easing_type.EASE_OUT_SINE:
            return Utils.Math.ease_out_sine
        easing_type.EASE_OUT_QUINT:
            return Utils.Math.ease_out_quint
        easing_type.EASE_OUT_CUBIC:
            return Utils.Math.ease_out_cubic
        easing_type.EASE_OUT_QUAD:
            return Utils.Math.ease_out_quad
        easing_type.EASE_OUT_QUART:
            return Utils.Math.ease_out_quart
        easing_type.EASE_OUT_EXPO:
            return Utils.Math.ease_out_expo
        easing_type.NONE:
            return Utils.Math.ease_none
        _:
            return Utils.Math.ease_none

func _ready():

    if min_value_node == null or max_value_node == null:
        printerr("Please assign the min and max value nodes")
        return
    
    var min_v : float = min_value_node.content -1
    var max_v : float = max_value_node.content

    # Let's create our @x values
    # var x: PackedFloat32Array = ArrayOperations.multiply_float(range(-10, 11, 1), 0.5)
    for a in range(0, 101):
        x.append(a)
    
    var easing_func: Callable = get_easing()
    # And our y values. It can be an n-size array of arrays.
    # NOTE: `x.size() == y.size()` or `x.size() == y[n].size()`
    # var y: Array = ArrayOperations.multiply_int(ArrayOperations.cos(x), 20)
    for a in range(0, 101):
        # Generate random value that will be exponentially increasing
        var x: float = float(a / 100.0)

        
        # remove number after the .000 to simplify it
        var y_var =  int((float(int(easing_func.call(x)*1000)))/1000 * (max_v - min_v) + min_v)
        
        # clamp the value (can't be lower that min_v and higher than max_v)
        y_var = clamp(y_var, min_v, max_v)


        y.append(y_var)
    
    # Let's customize the chart properties, which specify how the chart
    # should look, plus some additional elements like labels, the scale, etc...
    var cp: ChartProperties = ChartProperties.new()
    # Colors elements
    cp.colors.background = background_color
    cp.colors.frame = frame_color
    cp.colors.grid = grid_color
    cp.colors.text = text_color
    cp.colors.ticks = ticks_color
    # Text elements
    cp.title = title
    cp.x_label = x_label
    cp.y_label = y_label
    # Scale elements
    cp.x_scale = x_scale
    cp.x_tick_size = x_tick_size
    cp.x_ticklabel_space = x_ticklabel_space
    cp.y_scale = y_scale
    cp.y_tick_size = y_tick_size
    cp.y_ticklabel_space = y_ticklabel_space
    # We just want to show the graph, so we will disable all the other elements
    cp.draw_background = draw_background
    cp.draw_borders = draw_borders
    cp.draw_bounding_box = draw_bounding_box
    cp.draw_frame = draw_frame
    cp.draw_grid_box = draw_grid_box
    cp.draw_origin = draw_origin
    cp.draw_ticks = draw_ticks
    cp.show_legend = show_legend
    cp.show_tick_labels = show_tick_labels
    cp.show_title = show_title
    cp.show_x_label = show_x_label
    cp.show_y_label = show_y_label
    cp.interactive = interactive # false by default, it allows the chart to create a tooltip to show point values
    # and interecept clicks on the plot
    
    # Let's add values to our functions
    f1 = Function.new(
        x, y, graph_unit_name, # This will create a function with x and y values taken by the Arrays 
                        # we have created previously. This function will also be named "Pressure"
                        # as it contains 'pressure' values.
                        # If set, the name of a function will be used both in the Legend
                        # (if enabled thourgh ChartProperties) and on the Tooltip (if enabled).
        # Let's also provide a dictionary of configuration parameters for this specific function.
        { 
            color = graph_color, 		# The color associated to this function
            marker = marker_type, 	# The marker that will be displayed for each drawn point (x,y)
                                            # since it is `NONE`, no marker will be shown.
            type = line_type, 		# This defines what kind of plotting will be used, 
                                            # in this case it will be a Linear Chart.
            interpolation = interpolation_type,	# Interpolation mode, only used for 
                                                            # Line Charts and Area Charts.
            line_width = line_width, 				# The width of the line that will be drawn
        }
    )
    
    # Now let's plot our data
    chart.plot([f1], cp)
    
    # Uncommenting this line will show how real time data plotting works
    set_process(false)

    optionEasing.selected = easing

    min_value_node.content_changed.connect(_on_content_changed)
    max_value_node.content_changed.connect(_on_content_changed)
    optionEasing.item_selected.connect(_on_content_changed)


var new_val: float = 4.5

func _process(delta: float):
    # This function updates the values of a function and then updates the plot
    new_val += 5

    if min_value_node == null or max_value_node == null:
        return

    # we can use the `Function.add_point(x, y)` method to update a function
    f1.add_point(new_val, cos(new_val) * 20)
    f1.remove_point(0)
    chart.queue_redraw() # This will force the Chart to be updated

func _on_content_changed(content):

    var min_v : float = min_value_node.content -1
    var max_v : float = max_value_node.content

    easing = Utils.cast_to_enum(optionEasing.selected,easing_type)

    # Let's create our @x values
    # var x: PackedFloat32Array = ArrayOperations.multiply_float(range(-10, 11, 1), 0.5)
    for a in range(0, 101):
        x.append(a)
    
    var easing_func: Callable = get_easing()
    # And our y values. It can be an n-size array of arrays.
    # NOTE: `x.size() == y.size()` or `x.size() == y[n].size()`
    # var y: Array = ArrayOperations.multiply_int(ArrayOperations.cos(x), 20)
    for a in range(0, 101):
        # Generate random value that will be exponentially increasing
        var x: float = float(a / 100.0)

        
        # remove number after the .000 to simplify it
        var y_var =  int((float(int(easing_func.call(x)*1000)))/1000 * (max_v - min_v) + min_v)

        # clamp the value (can't be lower that min_v and higher than max_v)
        y_var = clamp(y_var, min_v, max_v)

        y.append(y_var)
    
    f1.set_points(x, y)

    chart.queue_redraw()

    pass

func _on_CheckButton_pressed():
    set_process(not is_processing())

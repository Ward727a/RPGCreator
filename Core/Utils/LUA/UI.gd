extends Object
class_name LuaUI

static var UI_Events: Dictionary = {}

static var UI_Components: Dictionary = {
	"Label": Label,
	"RichTextLabel": RichTextLabel,
	"Control": Control,
	"Window": Window,
	"ConfirmationDialog": ConfirmationDialog,
	"AcceptDialog": AcceptDialog,
	"HSeparator": HSeparator,
	"VSeparator": VSeparator,
	"HSlider": HSlider,
	"VSlider": VSlider,
	"HScrollBar": HScrollBar,
	"VScrollBar": VScrollBar,
	"Button": LUA_Button,
	"Buttons": {
		"Link": LUA_Link,
		"Menu": MenuButton,
		"Check": CheckButton,
		"CheckBox": CheckBox,
		"Option": OptionButton,
		"Texture": TextureButton,
		"ColorPicker": ColorPickerButton,
	},
	"ColorPicker": ColorPicker,
	"TextEdit": TextEdit,
	"LineEdit": LineEdit,
	"Container": {
		"Center": CenterContainer,
		"Tab": TabContainer,
		"Flow": FlowContainer,
		"Grid": GridContainer,
		"HFlow": HFlowContainer,
		"Panel": PanelContainer,
		"Split": SplitContainer,
		"VFlow": VFlowContainer,
		"Margin": MarginContainer,
		"Scroll": ScrollContainer,
		"AspectRatio": AspectRatioContainer,
		"SubViewport": SubViewportContainer,
		"HSplit": HSplitContainer,
		"VSplit": VSplitContainer,
		"VBox": VBoxContainer,
		"HBox": HBoxContainer,
		"Box": BoxContainer,
	}
}

static var UI_Anchors: Dictionary = {
	"begin": Control.ANCHOR_BEGIN,
	"end": Control.ANCHOR_END
}

static var UI_Cursor: Dictionary = {
	"default": Control.CURSOR_ARROW,
	"IBeam": Control.CURSOR_IBEAM,
	"pointing_hand": Control.CURSOR_POINTING_HAND,
	"cross": Control.CURSOR_CROSS,
	"wait": Control.CURSOR_WAIT,
	"busy": Control.CURSOR_BUSY,
	"drag": Control.CURSOR_DRAG,
	"can_drop": Control.CURSOR_CAN_DROP,
	"forbidden": Control.CURSOR_FORBIDDEN,
	"vsize": Control.CURSOR_VSIZE,
	"hsize": Control.CURSOR_HSIZE,
	"bdiagsize": Control.CURSOR_BDIAGSIZE,
	"fdiagsize": Control.CURSOR_FDIAGSIZE,
	"move": Control.CURSOR_MOVE,
	"vsplit": Control.CURSOR_VSPLIT,
	"hsplit": Control.CURSOR_HSPLIT,
	"help": Control.CURSOR_HELP
}

static var UI_LayoutPreset: Dictionary = {
	"top_left": Control.PRESET_TOP_LEFT,
	"top_right": Control.PRESET_TOP_RIGHT,
	"bottom_left": Control.PRESET_BOTTOM_LEFT,
	"bottom_right": Control.PRESET_BOTTOM_RIGHT,
	"center_left": Control.PRESET_CENTER_LEFT,
	"center_top": Control.PRESET_CENTER_TOP,
	"center_right": Control.PRESET_CENTER_RIGHT,
	"center_bottom": Control.PRESET_CENTER_BOTTOM,
	"center": Control.PRESET_CENTER,
	"left_wide": Control.PRESET_LEFT_WIDE,
	"top_wide": Control.PRESET_TOP_WIDE,
	"right_wide": Control.PRESET_RIGHT_WIDE,
	"bottom_wide": Control.PRESET_BOTTOM_WIDE,
	"vcenter_wide": Control.PRESET_VCENTER_WIDE,
	"hcenter_wide": Control.PRESET_HCENTER_WIDE,
	"full_rect": Control.PRESET_FULL_RECT
}

static var UI_LayoutMode: Dictionary = {
	"min_size": Control.PRESET_MODE_MINSIZE,
	"keep_width": Control.PRESET_MODE_KEEP_WIDTH,
	"keep_height": Control.PRESET_MODE_KEEP_HEIGHT,
	"keep_size": Control.PRESET_MODE_KEEP_SIZE
}

static var UI_Size: Dictionary = {
	"shrink_begin": Control.SIZE_SHRINK_BEGIN,
	"fill": Control.SIZE_FILL,
	"expand": Control.SIZE_EXPAND,
	"expand_fill": Control.SIZE_EXPAND_FILL,
	"shrink_center": Control.SIZE_SHRINK_CENTER,
	"shrink_end": Control.SIZE_SHRINK_END
}

static var UI_MouseFilter: Dictionary = {
	"stop": Control.MOUSE_FILTER_STOP,
	"pass": Control.MOUSE_FILTER_PASS,
	"ignore": Control.MOUSE_FILTER_IGNORE
}

static var UI_GrowDirection: Dictionary = {
	"begin": Control.GROW_DIRECTION_BEGIN,
	"end": Control.GROW_DIRECTION_END,
	"both": Control.GROW_DIRECTION_BOTH
}

static var UI_LayoutDirection: Dictionary = {
	"inherited": Control.LAYOUT_DIRECTION_INHERITED,
	"locale": Control.LAYOUT_DIRECTION_LOCALE,
	"ltr": Control.LAYOUT_DIRECTION_LTR,
	"rtl": Control.LAYOUT_DIRECTION_RTL
}

static var UI_TextDirection: Dictionary = {
	"inherited": Control.TEXT_DIRECTION_INHERITED,
	"auto": Control.TEXT_DIRECTION_AUTO,
	"ltr": Control.TEXT_DIRECTION_LTR,
	"rtl": Control.TEXT_DIRECTION_RTL
}

static var UI_Enums: Dictionary = {
	"Cursor": UI_Cursor,
	"LayoutPreset": UI_LayoutPreset,
	"LayoutMode": UI_LayoutMode,
	"Size": UI_Size,
	"MouseFilter": UI_MouseFilter,
	"GrowDirection": UI_GrowDirection,
	"LayoutDirection": UI_LayoutDirection,
	"TextDirection": UI_TextDirection 
}

static var UI: Dictionary = {
	'Components': UI_Components,
	"Enums": UI_Enums
}

static func bind_UI_var(lua: LuaAPI) -> LuaAPI:
	
	lua.push_variant('UI', UI)
	
	return lua

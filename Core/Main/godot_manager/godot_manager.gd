extends Node
class_name GodotManager

# WIP Class
# Not sure if it will really contains "all" nodes from Godot

static var singleton: GodotManager

var logger: Logger

var windows_list: Array[Window]

static func get_singleton() -> GodotManager:
	return singleton

static func is_valid() -> bool:
	return (GodotManager.get_singleton() != null)

func _init():
	singleton = self
	logger = Logger.new("GodotManager")

func link_base(plugin: ResPlugin):
	
	var possible_permission = {
		"Node": Node,
		"Object": Object
	}

func link_server(plugin: ResPlugin):
	
	var possible_permission = {
		"AudioServer": AudioServer,
		"CameraServer": CameraServer,
		"DisplayServer": DisplayServer,
		"XRServer": XRServer,
		"TCPServer": TCPServer,
		"UDPServer": UDPServer,
		"DTLSServer": DTLSServer,
		"TextServer": TextServer,
		"TextServerAdvanced": TextServerAdvanced,
		"TextServerDummy": TextServerDummy,
		"TextServerExtension": TextServerExtension,
		"TextServerManager": TextServerManager,
		"PhysicsServer2D": PhysicsServer2D,
		"PhysicsServer2DExtension": PhysicsServer2DExtension,
		"PhysicsServer2DManager": PhysicsServer2DManager,
		"PhysicsServer3D": PhysicsServer3D,
		"PhysicsServer3DExtension": PhysicsServer3DExtension,
		"PhysicsServer3DManager": PhysicsServer3DManager,
		"PhysicsServer3DRenderingServerHandler": PhysicsServer3DRenderingServerHandler,
		"RenderingServer": RenderingServer,
		"NavigationServer2D": NavigationServer2D,
		"NavigationServer3D": NavigationServer3D,
		"TranslationServer": TranslationServer,
		"GodotPhysicsServer2D": GodotPhysicsServer2D,
		"GodotPhysicsServer3D": GodotPhysicsServer3D,
		"GodotNavigationServer2D": GodotNavigationServer2D
	}

func link_misc(plugin: ResPlugin):
	
	var possible_permission = {
		"CanvasLayer": CanvasLayer,
		"ParallaxBackground": ParallaxBackground
	}

func link_plugin_2D(plugin: ResPlugin):
	
	var possible_permission = {
		"Node2D": Node2D,
		"AnimatedSprite2D": AnimatedSprite2D,
		"AudioListener2D": AudioListener2D,
		"AudioStreamPlayer2D": AudioStreamPlayer2D,
		"BackBufferCopy": BackBufferCopy,
		"Bone2D": Bone2D,
		"Camera2D": Camera2D,
		"CanvasGroup": CanvasGroup,
		"CanvasModulate": CanvasModulate,
		"CollisionObject2D": CollisionObject2D,
		"CollisionPolygon2D": CollisionPolygon2D,
		"CollisionShape2D": CollisionShape2D,
		"CPUParticles2D": CPUParticles2D,
		"GPUParticles2D": GPUParticles2D,
		"Joint2D": Joint2D,
		"Light2D": Light2D,
		"LightOccluder2D": LightOccluder2D,
		"Line2D": Line2D,
		"Marker2D": Marker2D,
		"MeshInstance2D": MeshInstance2D,
		"MultiMeshInstance2D": MultiMeshInstance2D,
		"NavigationLink2D": NavigationLink2D,
		"NavigationObstacle2D": NavigationObstacle2D,
		"NavigationRegion2D": NavigationRegion2D,
		"ParallaxLayer": ParallaxLayer,
		"Path2D": Path2D,
		"PathFollow2D": PathFollow2D,
		"Polygon2D": Polygon2D,
		"RayCast2D": RayCast2D,
		"RemoteTransform2D": RemoteTransform2D,
		"ShapeCast2D": ShapeCast2D,
		"Skeleton2D": Skeleton2D,
		"Sprite2D": Sprite2D,
		"TileMap": TileMap,
		"TouchScreenButton": TouchScreenButton,
		"VisibleOnScreenNotifier2D": VisibleOnScreenNotifier2D,
		"Area2D": Area2D,
		"CharacterBody2D": CharacterBody2D,
		"RigidBody2D": RigidBody2D,
		"PhysicalBone2D": PhysicalBone2D,
		"StaticBody2D": StaticBody2D,
		"AnimatableBody2D": AnimatableBody2D,
		"DampedSpringJoint2D": DampedSpringJoint2D,
		"GrooveJoint2D": GrooveJoint2D,
		"PinJoint2D": PinJoint2D,
		"DirectionalLight2D": DirectionalLight2D,
		"PointLight2D": PointLight2D,
		"VisibleOnScreenEnabler2D": VisibleOnScreenEnabler2D
	}

func link_plugin_material(plugin: ResPlugin):
	
	var possible_permission = {
		"FogMaterial": FogMaterial,
		"ORMMaterial3D": ORMMaterial3D,
		"BaseMaterial3D": BaseMaterial3D,
		"ShaderMaterial": ShaderMaterial,
		"Material": Material,
		"PhysicsMaterial": PhysicsMaterial,
		"StandardMaterial3D": StandardMaterial3D,
		"CanvasItemMaterial": CanvasItemMaterial,
		"PanoramaSkyMaterial": PanoramaSkyMaterial,
		"PhysicalSkyMaterial": PhysicalSkyMaterial,
		"PlaceholderMaterial": PlaceholderMaterial,
		"ProceduralSkyMaterial": ProceduralSkyMaterial,
		"ParticleProcessMaterial": ParticleProcessMaterial
	}
	
	var base_permission_root = "GD:MATERIAL"
	
	for permission in possible_permission.keys():
		
		var object = possible_permission[permission]
		
		if plugin.has_allowed(base_permission_root):
			plugin.add_link(permission, object)
			continue
		
		if plugin.has_allowed(str(base_permission_root, ":", permission)):
			plugin.add_link(permission, object)
			continue

func link_plugin_texture(plugin: ResPlugin):
	
	var possible_permission = {
		"AnimatedTexture": AnimatedTexture,
		"AtlasTexture": AtlasTexture,
		"CameraTexture": CameraTexture,
		"CanvasTexture": CanvasTexture,
		"CompressedTexture2D": CompressedTexture2D,
		"CompressedTexture2DArray": CompressedTexture2DArray,
		"CompressedTexture3D": CompressedTexture3D,
		"CompressedTextureLayered": CompressedTextureLayered,
		"CurveTexture": CurveTexture,
		"CurveXYZTexture": CurveXYZTexture,
		"GLTFTexture": GLTFTexture,
		"GLTFTextureSampler": GLTFTextureSampler,
		"GradientTexture1D": GradientTexture1D,
		"GradientTexture2D": GradientTexture2D,
		"ImageTexture": ImageTexture,
		"ImageTexture3D": ImageTexture3D,
		"ImageTextureLayered": ImageTextureLayered,
		"MeshTexture": MeshTexture,
		"NoiseTexture2D": NoiseTexture2D,
		"NoiseTexture3D": NoiseTexture3D,
		"PlaceholderTexture2D": PlaceholderTexture2D,
		"PlaceholderTexture2DArray": PlaceholderTexture2DArray,
		"PlaceholderTexture3D": PlaceholderTexture3D,
		"RDTextureFormat": RDTextureFormat,
		"RDTextureView": RDTextureView,
		"Texture": Texture,
		"Texture2D": Texture2D,
		"Texture2DArray": Texture2DArray,
		"Texture2DArrayRD": Texture2DArrayRD,
		"Texture2DRD": Texture2DRD,
		"Texture3D": Texture3D,
		"TextureCubemapArrayRD": TextureCubemapArrayRD,
		"TextureCubemapRD": TextureCubemapRD,
		"TextureLayered": TextureLayered,
		"TextureLayeredRD": TextureLayeredRD,
		"ViewportTexture": ViewportTexture
	}
	var base_permission_root = "GD:TEXTURE"
	
	for permission in possible_permission.keys():
		
		var object = possible_permission[permission]
		
		if plugin.has_allowed(base_permission_root):
			plugin.add_link(permission, object)
			continue
		
		if plugin.has_allowed(str(base_permission_root, ":", permission)):
			plugin.add_link(permission, object)
			continue

func link_plugin_style(plugin: ResPlugin):
	
	var possible_permission = {
		"Theme": Theme,
		"StyleBoxEmpty": StyleBoxEmpty,
		"StyleBoxFlat": StyleBoxFlat,
		"StyleBoxLine": StyleBoxLine,
		"StyleBoxTexture": StyleBoxTexture,
		"Image": Image,
		"Font": Font,
		"FontFile": FontFile,
		"FontVariation": FontVariation,
		"SystemFont": SystemFont,
		"LabelSettings": LabelSettings,
		"RichTextEffect": RichTextEffect
	}
	var base_permission_root = "GD:STYLE"
	
	for permission in possible_permission.keys():
		
		var object = possible_permission[permission]
		
		if plugin.has_allowed(base_permission_root):
			plugin.add_link(permission, object)
			continue
		
		if plugin.has_allowed(str(base_permission_root, ":", permission)):
			plugin.add_link(permission, object)
			continue


func link_plugin_ui(plugin: ResPlugin):
	
	var possible_permission = {
		"Window": Window,
		"Control": Control,
		"CanvasItem": CanvasItem,
		"HScrollBar": HScrollBar,
		"HSeparator": HSeparator,
		"HSlider": HSlider,
		"Container": Container,
		"CenterContainer": CenterContainer,
		"BoxContainer": BoxContainer,
		"TabContainer": TabContainer,
		"FlowContainer": FlowContainer,
		"GridContainer": GridContainer,
		"HBoxContainer": HBoxContainer,
		"VBoxContainer": VBoxContainer,
		"HFlowContainer": HFlowContainer,
		"PanelContainer": PanelContainer,
		"SplitContainer": SplitContainer,
		"VFlowContainer": VFlowContainer,
		"HSplitContainer": HSplitContainer,
		"MarginContainer": MarginContainer,
		"ScrollContainer": ScrollContainer,
		"VSplitContainer": VSplitContainer,
		"AspectRatioContainer": AspectRatioContainer,
		"SubViewportContainer": SubViewportContainer,
		"Button": Button,
		"BaseButton": BaseButton,
		"LinkButton": LinkButton,
		"MenuButton": MenuButton,
		"CheckButton": CheckButton,
		"OptionButton": OptionButton,
		"TextureButton": TextureButton,
		"ColorPickerButton": ColorPickerButton,
		"ButtonGroup": ButtonGroup,
		"Panel": Panel,
		"PopupPanel": PopupMenu,
		"Popup": Popup,
		"PopupMenu": PopupMenu,
		"FileDialog": FileDialog,
		"AcceptDialog": AcceptDialog,
		"ConfirmationDialog": ConfirmationDialog,
		"VideoStreamPlayer": VideoStreamPlayer,
		"VScrollBar": VScrollBar,
		"VSeparator": VSeparator,
		"VSlider": VSlider,
		"CodeEdit": CodeEdit,
		"LineEdit": LineEdit,
		"TextEdit": TextEdit,
		"Label": Label,
		"LabelSettings": LabelSettings,
		"RichTextLabel": RichTextLabel,
		"TextureProgressBar": TextureProgressBar,
		"TextureRect": TextureRect,
		"GraphEdit": GraphEdit,
		"GraphElement": GraphElement,
		"GraphNode": GraphNode,
		"ReferenceRect": ReferenceRect,
		"ColorRect": ColorRect,
		"NinePatchRect": NinePatchRect,
		"Range": Range,
		"ProgressBar": ProgressBar,
		"SpinBox": SpinBox,
		"ItemList": ItemList,
		"TabBar": TabBar,
		"Tree": Tree
	}
	
	var base_permission_root = "GD:INTERFACE"
	
	for permission in possible_permission.keys():
		
		var object = possible_permission[permission]
		
		if plugin.has_allowed(base_permission_root):
			plugin.add_link(permission, object)
			continue
		
		if plugin.has_allowed(str(base_permission_root, ":", permission)):
			plugin.add_link(permission, object)
			continue






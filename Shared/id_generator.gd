class_name IDGenerator

static func generate_id(min_size: int = 10, max_size: int = 20) -> String:
	
	# Minimum key size
	if min_size < 8:
		min_size = 8
	
	# Set max_size to min_size if it's inferior to min_size
	if max_size < min_size:
		max_size = min_size
	
	var id := PackedByteArray()
	@warning_ignore("integer_division")
	var size = randi_range(min_size, max_size)/2
	
	id.resize(size)
	
	for i in size:
		id[i] = randi_range(0, 255)
	
	return id.hex_encode()

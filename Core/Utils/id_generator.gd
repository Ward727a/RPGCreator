extends RefCounted
class_name IdGenerator
# This is a simple ID generator that generates a random ID encrypts it using RSA (256 bits)
# It is encrypted to be sure that the ID is unique and secure.
# IDK if this is the best way to generate an ID, but it is a way to do it.

static var crypto = Crypto.new()
static var crypto_key = crypto.generate_rsa(256)
var cert = X509Certificate.new()

func generate_id() -> String:
	
	var id = str("UniqueID",randi()%10_000)

	var crypted_id: PackedByteArray = crypto.encrypt(crypto_key, id.to_utf8_buffer())

	var id_string = str(crypted_id).replace(",", "").replace("[", "").replace("]", "").replace(" ", "")

	return id_string

func generate_medium_id() -> String:
	var id = str(randi()%1_000_000_000)
	
	return id

# This is a more simpler way to generate an ID, it is less secure but it is faster.
func generate_small_id() -> String:
	
	var id = str(randi()%10_000)

	return id

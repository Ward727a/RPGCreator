extends RefCounted
class_name IdGenerator
# This is a simple ID generator that generates a random ID encrypts it using RSA (256 bits)
# It is encrypted to be sure that the ID is unique and secure.
# IDK if this is the best way to generate an ID, but it is a way to do it.

var crypto = Crypto.new()
var crypto_key = crypto.generate_rsa(256)
var cert = X509Certificate.new()

func generate_id() -> String:
    
    var id = str("UniqueID",randi()%10000)

    var crypted_id: PackedByteArray = crypto.encrypt(crypto_key, id.to_utf8_buffer())

    var id_string = str(crypted_id).replace(",", "").replace("[", "").replace("]", "").replace(" ", "")

    return id_string
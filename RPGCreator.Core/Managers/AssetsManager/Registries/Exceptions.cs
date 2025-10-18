namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public class IdAlreadyInRegistry(string objectType, string id)
    : Exception($"[Registry Error] {objectType} with ID {id} already exists in the registry.");

public class UrnAlreadyInRegistry(string objectType, string urn)
    : Exception($"[Registry Error] {objectType} with URN {urn} already exists in the registry.");

public class IdNotFoundInRegistry(string objectType, string id)
    : Exception($"[Registry Error] {objectType} with ID {id} not found in the registry.");

public class UrnNotFoundInRegistry(string objectType, string urn)
    : Exception($"[Registry Error] {objectType} with URN {urn} not found in the registry.");


namespace RPGCreator.SDK.Serializer;

/// <summary>
/// This interface should be implemented by classes that need to be serialized.
/// </summary>
/// <remarks>
/// To use this interface, the class must implement a constructor that takes no parameters.
/// </remarks>
public interface ISerializable
{
    /// <summary>
    /// This method should return a <see cref="SerializationInfo"/> object that contains the data to be serialized.<br/>
    /// It should include all the properties and fields that need to be serialized.
    /// </summary>
    /// <returns><see cref="SerializationInfo"/> object containing the data to be serialized</returns>
    public SerializationInfo GetObjectData();

    /// <summary>
    /// If this class has any references to assets, this method should return a list of their IDs.
    /// </summary>
    /// <returns></returns>
    public List<Ulid> GetReferencedAssetIds();
}
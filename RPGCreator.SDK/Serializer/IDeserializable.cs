namespace RPGCreator.SDK.Serializer;

/// <summary>
/// This interface should be implemented by classes that need to be deserialized.
/// </summary>
/// <remarks>
/// To use this interface, the class must implement a constructor that takes no parameters.
/// </remarks>
public interface IDeserializable
{
    /// <summary>
    /// This method should set the object data from the <see cref="SerializationInfo"/> object.<br/>
    /// It should work in conjunction with <see cref="ISerializable.GetObjectData"/>.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> object containing the data to be set, which should be the same as the one returned by <see cref="ISerializable.GetObjectData"/>.</param>
    public void SetObjectData(DeserializationInfo info);
}
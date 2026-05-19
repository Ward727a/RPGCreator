using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using PropertyChanged;
using PropertyChanging;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Internals;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Assets.Definitions;

public interface IBaseAssetDef : IEngineClass
{
    Ulid Unique { get; set; }
    URN ClassUrn { get; set; }
    string Name { get; set; }
}

public abstract class BaseObservableAssetDef : ObservableObject, IBaseAssetDef
{
    public Ulid Unique { get; set; }
    public URN ClassUrn { get; set; }
    public string Name { get; set; } = string.Empty;
}

[AddINotifyPropertyChangedInterface]
[ImplementPropertyChanging]
public abstract class BaseAssetDef : IBaseAssetDef
{
    
    [JsonInclude]
    public string Name { get; set; } = string.Empty;
    [JsonInclude]
    public Ulid Unique { get; set; }

    [JsonInclude]
    public URN ClassUrn { get; set; }
}
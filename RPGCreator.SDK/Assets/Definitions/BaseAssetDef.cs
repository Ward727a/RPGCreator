using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using PropertyChanged;
using PropertyChanging;
using RPGCreator.SDK.Assets.MetaData;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Assets.Definitions;

public interface IBaseAssetDef : IHasUniqueId
{
    public string Name { get; set; }
    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; }
    
    public virtual UrnNamespace UrnNamespace => "rpgc".ToUrnNamespace();
    public abstract UrnSingleModule UrnModule { get; }

    public void SuspendTracking();
    public void ResumeTracking();

    public void UpdateUrn();
}

public abstract class BaseObservableAssetDef : ObservableObject, IBaseAssetDef
{
    // We use a flag to avoid updating the URN when we are initializing the object.
    // By default, a bool is false, so we consider that tracking is deactivated until ResumeTracking is called for the first time.
    private bool _isTrackingActivated; 
    public Ulid Unique { get; protected set; }
    public URN Urn
    {
        get;
        protected set
        {
            if (RegistryServices.UrnRegistry.IsUrnRegistered(field))
            {
                RegistryServices.UrnRegistry.UnregisterUrn(field);
            }
            
            field = value;
            
            if (field != URN.Empty)
            {
                RegistryServices.UrnRegistry.RegisterUrn(ref field);
            }
        }
    } = URN.Empty;

    public string Name
    {
        get;
        set
        {
            field = value;
            UpdateUrn();
        }
    }

    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; }
    public virtual UrnNamespace UrnNamespace => "rpgc".ToUrnNamespace();
    public abstract UrnSingleModule UrnModule { get; }
    public void SuspendTracking()
    {
        _isTrackingActivated = false;
    }

    public void ResumeTracking()
    {
        _isTrackingActivated = true;
        UpdateUrn();
    }

    public void UpdateUrn()
    {
        var identifier = !string.IsNullOrWhiteSpace(Name) ? Name : Unique.ToString();
        var urn = UrnNamespace.ToUrnModule(UrnModule).ToUrn($"{identifier ?? $"NO_IDENTIFIER-{Ulid.NewUlid()}"}");
        Urn = urn;
    }

    public void Init(Ulid id)
    {
        if (Unique != Ulid.Empty) return;
        Unique = id;
        UpdateUrn();
    }
}

[AddINotifyPropertyChangedInterface]
[ImplementPropertyChanging]
public abstract class BaseAssetDef : IBaseAssetDef, IHasMetadata
{
    private bool _isTrackingActivated;
    [JsonProperty("Name")]
    public string Name
    {
        get;
        set
        {
            field = value;
            UpdateUrn();
        }
    }
    
    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; }

    [JsonProperty("Unique")]
    public Ulid Unique { get; protected set; }

    public virtual UrnNamespace UrnNamespace => "rpgc".ToUrnNamespace();
    protected virtual bool ShouldUrnBeRegistered => true;
    
    public abstract UrnSingleModule UrnModule { get; }
    public void SuspendTracking()
    {
        _isTrackingActivated = false;
    }

    public void ResumeTracking()
    {
        _isTrackingActivated = true;
        UpdateUrn();
        SaveMetaData();
    }
    
    protected void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(Unique)) return; // In fact, this should NEVER happen (except on first init).
        SaveMetaData();
    }

    private void SaveMetaData()
    {
        if (!_isTrackingActivated) return; // We don't want to update the metadata if the tracking is not activated (meaning that the object is still being initialized).

        var meta = GetMetaData();
        
        if (meta == null)
            return;
        
        if(RegistryServices.AssetsMetaDataRegistry.ContainsMetaData(Unique))
            RegistryServices.AssetsMetaDataRegistry.UpdateMetaData(meta);
        else
            RegistryServices.AssetsMetaDataRegistry.RegisterMetaData(meta);
    }

    public void UpdateUrn()
    {
        var identifier = !string.IsNullOrWhiteSpace(Name) ? Name : Unique.ToString();
        var urn = UrnNamespace.ToUrnModule(UrnModule).ToUrn($"{identifier ?? $"NO_IDENTIFIER-{Ulid.NewUlid()}"}");
        Urn = urn;
    }

    public URN Urn
    {
        get;
        protected set
        {
            if (ShouldUrnBeRegistered && RegistryServices.UrnRegistry.IsUrnRegistered(field))
            {
                RegistryServices.UrnRegistry.UnregisterUrn(field);
            }
            
            field = value;
            
            if (field != URN.Empty)
            {
                if(ShouldUrnBeRegistered)
                    RegistryServices.UrnRegistry.RegisterUrn(ref field);
            }
        }
    } = URN.Empty;

    public void Init(Ulid id)
    {
        if (Unique != Ulid.Empty) return;
        Unique = id;
    }

    
    public virtual BaseMetaData? GetMetaData()
    {
        return null;
    }
}
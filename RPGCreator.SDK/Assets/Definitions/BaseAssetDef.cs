using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using RPGCreator.SDK.EngineService;
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
        if (!_isTrackingActivated || string.IsNullOrWhiteSpace(Name)) return;
        var urn = UrnNamespace.ToUrnModule(UrnModule).ToUrn($"{Name}");
        Urn = urn;
    }

    public void Init(Ulid id)
    {
        if (Unique != Ulid.Empty) return;
        Unique = id;
    }
}

public abstract class BaseAssetDef : IBaseAssetDef
{
    private bool _isTrackingActivated;
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
        if (!_isTrackingActivated || string.IsNullOrWhiteSpace(Name)) return;
        var urn = UrnNamespace.ToUrnModule(UrnModule).ToUrn($"{Name}");
        
        if(Urn == urn) return;
        
        Urn = urn;
    }

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

    public void Init(Ulid id)
    {
        if (Unique != Ulid.Empty) return;
        Unique = id;
    }
}
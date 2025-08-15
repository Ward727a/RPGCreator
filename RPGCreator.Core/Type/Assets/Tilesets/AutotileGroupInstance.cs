using RPGCreator.Core.Type.Internal;
using RPGCreator.Core.Type.Map;

namespace RPGCreator.Core.Type.Assets.Tilesets;

public class AutotileGroupInstance
{
    
    public Ulid RuntimeUnique = Ulid.NewUlid();
    public AutotileGroupDef Definition { get; set; } = null!;
    
    public AutotileGroupInstance(AutotileGroupDef def)
    {
        Definition = def ?? throw new ArgumentNullException(nameof(def), "AutotileGroupDef cannot be null.");
    }
    

}
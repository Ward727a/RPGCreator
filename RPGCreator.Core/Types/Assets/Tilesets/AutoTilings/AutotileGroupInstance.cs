using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Map;

namespace RPGCreator.Core.Types.Assets.Tilesets;

public class AutotileGroupInstance
{
    
    public Ulid RuntimeUnique = Ulid.NewUlid();
    public AutotileGroupDef Definition { get; set; } = null!;
    
    public AutotileGroupInstance(AutotileGroupDef def)
    {
        Definition = def ?? throw new ArgumentNullException(nameof(def), "AutotileGroupDef cannot be null.");
    }
    

}
using System.Text.Json.Serialization;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.SDK.Assets.SpriteAnimation;

[TypeDiscriminator("rpgc", "sdk", "definitions", "sprite_animation")]
[AssetType("sprite_animations", "rsanim")]
public class SpriteAnimationDefinition : IAssetDto
{
    private const int DefaultFrameSize = 32;
    private const float DefaultFps = 8f;

    public Ulid AssetId { get; init; } = Ulid.NewUlid();

    public List<Ulid> Dependencies { get; init; } = [];

    public string Name { get; set; } = string.Empty;

    public Ulid SpriteSheetResource { get; set; } = Ulid.Empty;

    public int FrameWidth { get; set; } = DefaultFrameSize;
    public int FrameHeight { get; set; } = DefaultFrameSize;

    public List<AnimationFrame> Frames { get; set; } = [];
    
    public float Fps { get; set; } = DefaultFps;

    public AnimationPlaybackMode PlaybackMode { get; set; } = AnimationPlaybackMode.Loop;

    /// <summary>
    /// Duration in seconds. Derived from <see cref="Fps"/> (0 if fps is invalid).
    /// </summary>
    [JsonIgnore]
    public float FrameDuration => Fps > 0f ? 1f / Fps : 0f;
}
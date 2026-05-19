namespace RPGCreator.EngineLib.Types.Internal
{
    internal interface IHasUIVisual<TUI> where TUI : UIVisual
    {
        public TUI Visual { get; }
    }

    public abstract class UIVisual
    { }
}

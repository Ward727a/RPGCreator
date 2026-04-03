namespace RPGCreator.Core.Types.Blueprint;

public class BooleanPort : Port
{
    public BooleanPort()
    {
        Kind = PortKind.Boolean;
        ValueType = "bool";
        Value = false;
    }
    
    private bool _value = false;
    public new bool Value
    {
        get => _value;
        set
        {
            if (value != _value)
            {
                _value = value;
            }
        }
    }
}
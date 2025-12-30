namespace RPGCreator.Core.Types.Blueprint;

public class NumberPort : Port
{
    public NumberPort()
    {
        Kind = PortKind.Number;
        ValueType = "double";
        Value = 0D;
    }
    
    private double _value = 0D;
    public new double Value
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
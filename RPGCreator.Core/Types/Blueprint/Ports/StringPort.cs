namespace RPGCreator.Core.Types.Blueprint;

public class StringPort : Port
{
    public StringPort()
    {
        Kind = PortKind.String;
        ValueType = "string";
        Value = "";
    }
    
    private string _value = string.Empty;
    public new string Value
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
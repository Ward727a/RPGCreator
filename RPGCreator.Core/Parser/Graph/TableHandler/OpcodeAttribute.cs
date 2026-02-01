using RPGCreator.SDK.Graph;

namespace RPGCreator.Core.Parser.Graph.TableHandler;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class OpcodeAttribute : Attribute
{
    public EGraphOpCode OpCode { get; }
    public EOpCodeHandlerType HandlerType { get; set; } = EOpCodeHandlerType.Public;
    public OpcodeAttribute(EGraphOpCode opcode) => OpCode = opcode;
    public OpcodeAttribute(EGraphOpCode opcode, EOpCodeHandlerType handlerType)
    {
        OpCode = opcode;
        HandlerType = handlerType;
    }
}
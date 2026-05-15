using RPGCreator.SDK.Assets;

namespace RPGCreator.Tests;

public class TypeRegistryMapping
{
    private const string ValueTupleUrn = "rpgc://sdk/common/tuple/";
    private const string StringUrn = "rpgc://sdk/common/string";
    private const string ListUrn = "rpgc://sdk/common/list";
    private const string DictionaryUrn = "rpgc://sdk/common/dictionary";
    private const string IntUrn = "rpgc://sdk/common/int";
    private const string BoolUrn = "rpgc://sdk/common/bool";
    
    private readonly ITypesRegistry _typeMapping = new TypesRegistryDiscriminator();
    
    [Fact(DisplayName = "Test ValueTuple<string, int>")]
    public void TestValueTuple()
    {
        var testType = _typeMapping.GetType($"{ValueTupleUrn}2<{StringUrn}, {IntUrn}>");
        
        Assert.True(testType.IsSuccess);
        Assert.True(testType.Value == typeof(ValueTuple<string, int>));
    }

    [Fact(DisplayName = "Test dictionary<int, bool>")]
    public void TestDictionary()
    {
        var dicTypeResult = _typeMapping.GetType($"{DictionaryUrn}<{IntUrn}, {BoolUrn}>");
        
        Assert.True(dicTypeResult.IsSuccess);
        Assert.True(dicTypeResult.Value == typeof(Dictionary<int, bool>));
    }

    [Fact(DisplayName = "Test List<string>")]
    public void TestList()
    {
        var listTypeResult = _typeMapping.GetType($"{ListUrn}<{StringUrn}>");
        
        Assert.True(listTypeResult.IsSuccess);
        Assert.True(listTypeResult.Value == typeof(List<string>));
    }

    [Fact(DisplayName = "Test List<ValueTuple<string, int, Dictionary<string, bool>>>")]
    public void TestFull()
    {
        var fullTypeResult =
            _typeMapping.GetType(
                $"{ListUrn}<{ValueTupleUrn}3<{StringUrn}, {IntUrn}, {DictionaryUrn}<{StringUrn}, {BoolUrn}>>>");
        
        Assert.True(fullTypeResult.IsSuccess);
        Assert.True(fullTypeResult.Value == typeof(List<ValueTuple<string, int, Dictionary<string, bool>>>));
    }
    
    [Fact(DisplayName = "Test bad formatted urn")]
    public void TestBadFormattedUrn()
    {
        var badUrn = $"rpgc://sdk/common/tuple/2<{StringUrn} {IntUrn}>";
        var badUrnResult = _typeMapping.GetType(badUrn);
        
        Assert.False(badUrnResult.IsSuccess);
    }
}
namespace RPGCreator.Core.Parser.PRATT;

public sealed class PrattCompiler
{
    private readonly PrattTable _table;

    public PrattCompiler(PrattTable? table = null)
    {
        table ??= PrattTable.DefaultPrattTable();
        _table = table;
    }

    public PrattCompiledFormula Compile(string formulaText)
    {
        var tokens = new PrattLexer(formulaText).Tokenize();
        var ast = new PrattParser(tokens, _table).ParseExpression(0);
        var foldedAst = PrattConstantFolder.Fold(ast);
        return new PrattCompiledFormula(foldedAst);
    }
}
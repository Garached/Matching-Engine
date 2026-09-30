using System.Globalization;

public class InputParserTests
{
    [Fact]
    public void Price_ComPonto_LeCorretamenteEmMaquinaBrasileira()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");

        Assert.True(InputParser.TryParsePrice("9.98", out var price));
        Assert.Equal(9.98m, price);
    }

    [Fact]
    public void Price_ComVirgula_Recusa()
    {
        Assert.False(InputParser.TryParsePrice("9,98", out _));
    }

    [Fact]
    public void Price_Negativo_Recusa()
    {
        Assert.False(InputParser.TryParsePrice("-10", out _));
    }

    [Fact]
    public void Side_Invalido_Recusa()
    {
        Assert.False(InputParser.TryParseSide("bye", out _));
    }

    [Fact]
    public void Qty_Zero_Recusa()
    {
        Assert.False(InputParser.TryParseQty("0", out _));
    }

    [Fact]
    public void Qty_Negativa_Recusa()
    {
        Assert.False(InputParser.TryParseQty("-5", out _));
    }

    [Fact]
    public void Id_ComPrefixo_LeCorretamente()
    {
        Assert.True(InputParser.TryParseId("order_1", out var id));
        Assert.Equal(1, id);
    }
}
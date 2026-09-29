using System.Globalization;

public class OrderBookTests
{
    private static string Run(Action<OrderBook> actions)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        var output = new StringWriter();
        var original = Console.Out;
        Console.SetOut(output);

        try
        {
            actions(new OrderBook());
        }
        finally
        {
            Console.SetOut(original);
        }

        return output.ToString();
    }

    private static string LastLine(string output)
    {
        var lines = output.Trim().Split(Environment.NewLine);
        return lines[^1];
    }

    // Ajuste 1: preço do trade

    [Fact]
    public void Trade_UsaPrecoDaOrdemPassiva_QuandoVendaAgride()
    {
        var output = Run(book =>
        {
            book.AddLimitOrder(Side.Buy, 15, 100);
            book.AddLimitOrder(Side.Sell, 10, 50);
        });

        Assert.Contains("Trade, price: 15, qty: 50", output);
    }

    [Fact]
    public void Trade_UsaPrecoDaOrdemPassiva_QuandoCompraAgride()
    {
        var output = Run(book =>
        {
            book.AddLimitOrder(Side.Sell, 10, 50);
            book.AddLimitOrder(Side.Buy, 15, 100);
        });

        Assert.Contains("Trade, price: 10, qty: 50", output);
    }

    // Ajuste 2: peg ignora ordens canceladas

    [Fact]
    public void Peg_IgnoraOrdemCancelada()
    {
        var output = Run(book =>
        {
            book.AddLimitOrder(Side.Buy, 10, 100);
            book.AddLimitOrder(Side.Buy, 9, 100);
            book.CancelOrder(1);
            book.AddPeggedOrder(Side.Buy, 50);
        });

        Assert.Contains("pegged buy 50 @ 9 order_3", output);
    }

    // Ajuste 3: ordens executadas saem do dicionário

    [Fact]
    public void Cancel_OrdemTotalmenteExecutada_NaoEncontrada()
    {
        var output = Run(book =>
        {
            book.AddLimitOrder(Side.Buy, 10, 100);
            book.AddMarketOrder(Side.Sell, 100);
            book.CancelOrder(1);
        });

        Assert.Equal("Order not found", LastLine(output));
    }

    [Fact]
    public void Cancel_OrdemParcialmenteExecutada_Cancela()
    {
        var output = Run(book =>
        {
            book.AddLimitOrder(Side.Buy, 10, 100);
            book.AddMarketOrder(Side.Sell, 40);
            book.CancelOrder(1);
        });

        Assert.Equal("Order cancelled", LastLine(output));
    }

    // Ajuste 4: modify

    [Fact]
    public void Modify_SoPreco_MantemQuantidade()
    {
        var output = Run(book =>
        {
            book.AddLimitOrder(Side.Buy, 10, 100);
            book.ModifyOrder(1, 9.98m, null);
        });

        Assert.Contains("buy 100 @ 9.98", output);
    }

    [Fact]
    public void Modify_ReduzirQuantidade_MantemPrioridade()
    {
        var output = Run(book =>
        {
            book.AddLimitOrder(Side.Buy, 10, 100);
            book.AddLimitOrder(Side.Buy, 10, 100);
            book.ModifyOrder(1, null, 50);
            book.AddMarketOrder(Side.Sell, 50);
            book.CancelOrder(1);
        });

        Assert.Equal("Order not found", LastLine(output));
    }

    [Fact]
    public void Modify_AumentarQuantidade_PerdePrioridade()
    {
        var output = Run(book =>
        {
            book.AddLimitOrder(Side.Buy, 10, 100);
            book.AddLimitOrder(Side.Buy, 10, 100);
            book.ModifyOrder(1, null, 150);
            book.AddMarketOrder(Side.Sell, 100);
            book.CancelOrder(2);
        });

        Assert.Equal("Order not found", LastLine(output));
    }

    [Fact]
    public void Modify_QuantidadeZero_Recusa()
    {
        var output = Run(book =>
        {
            book.AddLimitOrder(Side.Buy, 10, 100);
            book.ModifyOrder(1, 10m, 0);
        });

        Assert.Equal("Quantidade precisa ser maior que zero", LastLine(output));
    }

    // Ajuste 6: peg informa o Id

    [Fact]
    public void Peg_Reposicionado_InformaNovoId()
    {
        var output = Run(book =>
        {
            book.AddLimitOrder(Side.Buy, 10, 100);
            book.AddPeggedOrder(Side.Buy, 50);
            book.AddLimitOrder(Side.Buy, 11, 100);
        });

        Assert.Contains("Peg order_2 repriced to 11, now order_4", output);
    }
}
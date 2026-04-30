var book = new OrderBook();

while (true)
{
    Console.Write(">>> ");
    var input = Console.ReadLine();
    
    if (string.IsNullOrEmpty(input)) continue;
    
    var parts = input.Split(' ');
    
    switch (parts[0])
    {
        case "limit":
            if (parts.Length < 4) { Console.WriteLine("Uso: limit buy/sell <preco> <quantidade>"); break; }
            if (!decimal.TryParse(parts[2], out var price) || !int.TryParse(parts[3], out var qty)) { Console.WriteLine("Preço e quantidade precisam ser números"); break; }
            var side = parts[1] == "buy" ? Side.Buy : Side.Sell;
            book.AddLimitOrder(side, price, qty);
            break;

        case "market":
            if (parts.Length < 3) { Console.WriteLine("Uso: market buy/sell <quantidade>"); break; }
            if (!int.TryParse(parts[2], out var mQty)) { Console.WriteLine("Quantidade precisa ser um número"); break; }
            var mSide = parts[1] == "buy" ? Side.Buy : Side.Sell;
            book.AddMarketOrder(mSide, mQty);
            break;

        case "cancel":
            if (parts.Length < 3) { Console.WriteLine("Uso: cancel order <id>"); break; }
            if (!int.TryParse(parts[2], out var id)) { Console.WriteLine("Id precisa ser um número"); break; }
            book.CancelOrder(id);
            break;

        case "print":
            book.PrintBook();
            break;

        case "modify":
            if (parts.Length < 4) { Console.WriteLine("Uso: modify <id> <novoPreco> <novaQty>"); break; }
            if (!int.TryParse(parts[1], out var mId)) { Console.WriteLine("Id precisa ser um número"); break; }
            decimal? newPrice = decimal.TryParse(parts[2], out var np) ? np : null;
            int? newQty = int.TryParse(parts[3], out var nq) ? nq : null;
            book.ModifyOrder(mId, newPrice, newQty);
            break;

        case "peg":
            if (parts.Length < 4) { Console.WriteLine("Uso: peg bid/offer buy/sell <quantidade>"); break; }
            if (!int.TryParse(parts[3], out var pQty)) { Console.WriteLine("Quantidade precisa ser um número"); break; }
            var pSide = parts[2] == "buy" ? Side.Buy : Side.Sell;
            book.AddPeggedOrder(pSide, pQty);
            break;

        default:
            Console.WriteLine("Comando inválido");
            break;
    }
}
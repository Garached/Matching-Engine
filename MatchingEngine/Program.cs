using System.Globalization;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

using static InputParser;

var book = new OrderBook();

while (true)
{
    Console.Write(">>> ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input)) continue;

    var parts = input.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

    switch (parts[0])
    {
        case "limit":
            if (parts.Length < 4) { Console.WriteLine("Uso: limit buy/sell <preco> <quantidade>"); break; }
            if (!TryParseSide(parts[1], out var side)) { Console.WriteLine("Lado precisa ser buy ou sell"); break; }
            if (!TryParsePrice(parts[2], out var price)) { Console.WriteLine("Preço precisa ser um número maior que zero (use ponto: 9.98)"); break; }
            if (!TryParseQty(parts[3], out var qty)) { Console.WriteLine("Quantidade precisa ser um inteiro maior que zero"); break; }
            book.AddLimitOrder(side, price, qty);
            break;

        case "market":
            if (parts.Length < 3) { Console.WriteLine("Uso: market buy/sell <quantidade>"); break; }
            if (!TryParseSide(parts[1], out var mSide)) { Console.WriteLine("Lado precisa ser buy ou sell"); break; }
            if (!TryParseQty(parts[2], out var mQty)) { Console.WriteLine("Quantidade precisa ser um inteiro maior que zero"); break; }
            book.AddMarketOrder(mSide, mQty);
            break;

        case "cancel":
            if (parts.Length < 3) { Console.WriteLine("Uso: cancel order <id>"); break; }
            if (!TryParseId(parts[2], out var id)) { Console.WriteLine("Id inválido (ex: 1 ou order_1)"); break; }
            book.CancelOrder(id);
            break;

        case "print":
            book.PrintBook();
            break;

        case "modify":
            if (parts.Length < 4) { Console.WriteLine("Uso: modify <id> <novoPreco ou -> <novaQty ou ->"); break; }
            if (!TryParseId(parts[1], out var mId)) { Console.WriteLine("Id inválido (ex: 1 ou order_1)"); break; }

            decimal? newPrice = null;
            if (parts[2] != "-")
            {
                if (!TryParsePrice(parts[2], out var np)) { Console.WriteLine("Preço inválido (use - para manter)"); break; }
                newPrice = np;
            }

            int? newQty = null;
            if (parts[3] != "-")
            {
                if (!TryParseQty(parts[3], out var nq)) { Console.WriteLine("Quantidade inválida (use - para manter)"); break; }
                newQty = nq;
            }

            book.ModifyOrder(mId, newPrice, newQty);
            break;

        case "peg":
            if (parts.Length < 4) { Console.WriteLine("Uso: peg bid buy <quantidade> ou peg offer sell <quantidade>"); break; }
            if (!TryParseSide(parts[2], out var pSide)) { Console.WriteLine("Lado precisa ser buy ou sell"); break; }
            bool validPeg = (parts[1] == "bid" && pSide == Side.Buy) || (parts[1] == "offer" && pSide == Side.Sell);
            if (!validPeg) { Console.WriteLine("Use peg bid buy <quantidade> ou peg offer sell <quantidade>"); break; }
            if (!TryParseQty(parts[3], out var pQty)) { Console.WriteLine("Quantidade precisa ser um inteiro maior que zero"); break; }
            book.AddPeggedOrder(pSide, pQty);
            break;

        case "exit":
            Console.WriteLine("Programa encerrado");
            return;

        default:
            Console.WriteLine("Comando inválido");
            break;
    }
}
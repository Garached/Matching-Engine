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

public enum OrderType { Limit, Market }
public enum Side { Buy, Sell }

public class Order
{
    public int Id { get; set; }
    public OrderType Type { get; set; }
    public Side Side { get; set; }
    public decimal Price { get; set; }
    public int Qty { get; set; }
}

public class OrderBook
{
    private PriorityQueue<Order, (decimal Price, int Id)> _bids = new();
    private PriorityQueue<Order, (decimal Price, int Id)> _asks = new();
    private int _nextId = 1;
    private Dictionary<int, Order> _orders = new();
    private List<Order> _peggedOrders = new();


    public void AddLimitOrder(Side side, decimal price, int qty)
    {
        var order = new Order
        {
            Id = _nextId++,
            Type = OrderType.Limit,
            Side = side,
            Price = price,
            Qty = qty
        };

        _orders[order.Id] = order; 

        if (side == Side.Buy)
            _bids.Enqueue(order, (-price, order.Id));
        else
            _asks.Enqueue(order, (price, order.Id));

        Match();
    }

    public void AddMarketOrder(Side side, int qty)
    {
        var order = new Order
        {
            Id = _nextId++,
            Type = OrderType.Market,
            Side = side,
            Price = 0,
            Qty = qty
        };

        _orders[order.Id] = order; 

        Match(order);
    }

    public void Match()
    {
        while (_bids.Count > 0 && _asks.Count > 0)
        {
            var bid = _bids.Peek();
            var ask = _asks.Peek();

            if (bid.Qty == 0) { _bids.Dequeue(); continue; }
            if (ask.Qty == 0) { _asks.Dequeue(); continue; }

            if (bid.Price >= ask.Price)
            {
                int qty = Math.Min(bid.Qty, ask.Qty);
                Console.WriteLine($"Trade, price: {ask.Price}, qty: {qty}");

                bid.Qty -= qty;
                ask.Qty -= qty;

                if (bid.Qty == 0) _bids.Dequeue();
                if (ask.Qty == 0) _asks.Dequeue();
            }
            else
            {
                break;
            }
        }
    }

    public void Match(Order marketOrder)
    {
        var oppositeBook = marketOrder.Side == Side.Buy ? _asks : _bids;

        while (marketOrder.Qty > 0 && oppositeBook.Count > 0)
        {
            var best = oppositeBook.Peek();

            if (best.Qty == 0) { oppositeBook.Dequeue(); continue; }

            int qty = Math.Min(marketOrder.Qty, best.Qty);
            Console.WriteLine($"Trade, price: {best.Price}, qty: {qty}");

            marketOrder.Qty -= qty;
            best.Qty -= qty;

            if (best.Qty == 0) oppositeBook.Dequeue();
        }
    }

    public void PrintBook()
    {
        var buys = _orders.Values
            .Where(o => o.Side == Side.Buy && o.Qty > 0)
            .OrderByDescending(o => o.Price);

        var sells = _orders.Values
            .Where(o => o.Side == Side.Sell && o.Qty > 0)
            .OrderBy(o => o.Price);

        Console.WriteLine("Ordens de Compra");
        foreach (var o in buys)
            Console.WriteLine($"{o.Qty} @ {o.Price}");

        Console.WriteLine("Ordens de Venda");
        foreach (var o in sells)
            Console.WriteLine($"{o.Qty} @ {o.Price}");
    }

    public void CancelOrder(int id)
    {
        if (_orders.TryGetValue(id, out var order))
        {
            order.Qty = 0;
            _orders.Remove(id);
            Console.WriteLine("Order cancelled");
        }
        else
        {
            Console.WriteLine("Order not found");
        }
    }

    public void ModifyOrder(int id, decimal? newPrice, int? newQty) 
    {
            if (!_orders.TryGetValue(id, out var order))
        {
            Console.WriteLine("Order not found");
            return;
        }

        if (newQty.HasValue)
            order.Qty = newQty.Value;

        if (newPrice.HasValue)
        {
            order.Qty = 0;
            _orders.Remove(id);

            AddLimitOrder(order.Side, newPrice.Value, newQty ?? order.Qty); //zera a ordem antiga e coloca na nova fila com novo id
        }

        Console.WriteLine($"Order modified");
    }

    public void AddPeggedOrder(Side side, int qty) 
    {
        if (side == Side.Buy && _bids.Count == 0)
        {
            Console.WriteLine("No bids available to peg to");
            return;
        }
        if (side == Side.Sell && _asks.Count == 0)
        {
            Console.WriteLine("No asks available to peg to");
            return;
        }

        var referencePrice = side == Side.Buy
        ? _bids.Peek().Price
        : _asks.Peek().Price;

        var order = new Order
        {
            Id = _nextId++,
            Type = OrderType.Limit,
            Side = side,
            Price = referencePrice,
            Qty = qty
        };

        _orders[order.Id] = order;
        _peggedOrders.Add(order);

        if (side == Side.Buy)
            _bids.Enqueue(order, (-referencePrice, order.Id));
        else
            _asks.Enqueue(order, (referencePrice, order.Id));
    }
}


// OrderBook precisa de:
// AddLimitOrder OK (match ainda n existe, sem parâmetro)
// AddMarketOrder Ok - PrintBook, bonus 1; CancelOrder, bonus 3; Modify, bonus 4; Add, bonus 5
// CancelOrder OK - dentro do Match(), antes de processar faz uym while e descarta ordens canceladas
// ModifyOrder OK - newQty.HasValue checa se foi passado valor ou não 
// AddPeggedOrder OK - acrash arrumado ja 
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
// AddPeggedOrder
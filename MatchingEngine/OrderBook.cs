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

        Console.WriteLine($"Order created: {side.ToString().ToLower()} {qty} @ {price} order_{order.Id}");

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
        _orders.Remove(order.Id);
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
                var tradePrice = bid.Id < ask.Id ? bid.Price : ask.Price;
                Console.WriteLine($"Trade, price: {tradePrice}, qty: {qty}");

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

        UpdatePeggedOrders();
    }

    public void Match(Order marketOrder)
    {
        int originalQty = marketOrder.Qty;
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
        UpdatePeggedOrders();

        if (marketOrder.Qty == originalQty)
            Console.WriteLine("No liquidity available");
        else if (marketOrder.Qty > 0)
            Console.WriteLine($"Partially filled: {originalQty - marketOrder.Qty}/{originalQty}");
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

        if (buys.Any() && sells.Any())
        {
            var spread = sells.First().Price - buys.First().Price;
            Console.WriteLine($"Spread: {spread}");
        }
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

            AddLimitOrder(order.Side, newPrice.Value, newQty ?? order.Qty); 
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

    private void UpdatePeggedOrders()
    {
        foreach (var order in _peggedOrders.ToList())
        {
            if (order.Qty == 0) { _peggedOrders.Remove(order); continue; }

            var newPrice = order.Side == Side.Buy
                ? (_bids.Count > 0 ? _bids.Peek().Price : 0)
                : (_asks.Count > 0 ? _asks.Peek().Price : 0);

            if (newPrice == 0 || newPrice == order.Price) continue;

            int savedQty = order.Qty;
            order.Qty = 0;
            _peggedOrders.Remove(order);
            AddPeggedOrder(order.Side, savedQty);
        }
    }
}
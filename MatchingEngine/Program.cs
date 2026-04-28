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

    public void Match() { }

    public void Match(Order marketOrder) { }

    public void PrintBook() { }

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

    public void ModifyOrder(int id, decimal? newPrice, int? newQty) { }

    public void AddPeggedOrder(Side side, int qty) { }
}


// OrderBook precisa de:
// AddLimitOrder OK (match ainda n existe, sem parâmetro)
// AddMarketOrder Ok - PrintBook, bonus 1; CancelOrder, bonus 3; Modify, bonus 4; Add, bonus 5
// CancelOrder OK - dentro do Match(), antes de processar faz uym while e descarta ordens canceladas
// ModifyOrder
// AddPeggedOrder
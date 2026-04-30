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
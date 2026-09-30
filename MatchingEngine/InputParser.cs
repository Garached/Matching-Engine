using System.Globalization;

public static class InputParser
{
    public static bool TryParseSide(string text, out Side side)
    {
        side = default;
        if (text == "buy") { side = Side.Buy; return true; }
        if (text == "sell") { side = Side.Sell; return true; }
        return false;
    }

    public static bool TryParsePrice(string text, out decimal price)
    {
        return decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out price) && price > 0;
    }

    public static bool TryParseQty(string text, out int qty)
    {
        return int.TryParse(text, out qty) && qty > 0;
    }

    public static bool TryParseId(string text, out int id)
    {
        if (text.StartsWith("order_")) text = text.Substring("order_".Length);
        return int.TryParse(text, out id);
    }
}
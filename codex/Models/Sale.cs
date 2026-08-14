namespace codex.Models
{
    /// <summary>
    /// DTO для факта продажи
    /// </summary>
    /// <param name="order_id"></param>
    /// <param name="order_date"></param>
    /// <param name="customer_id"></param>
    /// <param name="product_category"></param>
    /// <param name="region"></param>
    /// <param name="quantity"></param>
    /// <param name="unit_price"></param>
    /// <param name="discount"></param>
    /// <param name="payment_method"></param>
    /// <param name="delivery_days"></param>
    /// <param name="customer_rating"></param>
    /// <param name="revenue"></param>
    public record Sale(
        ulong order_id,
        DateOnly order_date,
        ulong customer_id,
        string product_category,
        string region,
        uint quantity,
        decimal unit_price,
        decimal discount,
        string payment_method,
        ushort delivery_days,
        double customer_rating,
        double revenue
    );

}
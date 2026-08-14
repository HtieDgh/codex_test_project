using codex.Models;
namespace codex.Services
{
    public abstract class ISalesParser
    {
        protected IReader reader_;
        protected static readonly string[] headerShema_ = { "order_id", "order_date", "customer_id", "product_category", "region", "quantity", "unit_price", "discount", "payment_method", "delivery_days", "customer_rating", "revenue" };
        protected static readonly string[] dateFormats_ = { "M/d/yyyy" };

        public ISalesParser(IReader reader)
        {
            reader_ = reader;
        }
        /// <summary>
        /// Возвращает список с покупками
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public abstract List<Sale> GetAll();

        /// <summary>
        /// Вариант асинхроного парсера
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>

        public abstract IAsyncEnumerable<Sale> GetAllAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Пыпытка получить Sale из массива ячеек
        /// </summary>
        /// <param name="cells"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Если ячеек не 12</exception>
        protected abstract Sale Parse(string[] cells);
        protected abstract Task<Sale> ParseAsync(Task<string[]> t);
    }
}
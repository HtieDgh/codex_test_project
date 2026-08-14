using codex.Shared;
namespace codex.Services
{
    /// <summary>
    /// Топ (по умолчанию из 5) покупателей по рейтингу. Берется рейтинг, который был последним на момент покупки, в заданный период.
    /// </summary>
    public class CustomerAnalyticsService : IAnalyticsService
    {
        private ushort topCount_;

        public CustomerAnalyticsService(IWriter writer, ISalesParser parser, ushort topCount, MODE mode, DateOnly start, DateOnly end) : base(writer, parser, mode, start, end)
        { topCount_ = topCount; }
        /// <summary>
        /// Билдер для CustomerAnalyticsService
        /// </summary>
        public class Builder : IAnalyticsServiceBuilder<Builder>
        {
            private ushort topCount_ = 5;

            public Builder AddTopCount(ushort topCount)
            {
                topCount_ = topCount;
                return this;
            }
            public override CustomerAnalyticsService Build()
            {
                if (writer_ is null || parser_ is null)
                    throw new Exception("Parser or writer is not defined");
                return new CustomerAnalyticsService(writer_, parser_, topCount_, mode_, startDate_, endDate_);
            }
        }
        /// <summary>
        /// DTO для покупателей, хранящее результат groupBy вызваный в Run()
        /// </summary>
        /// <param name="customerId">Индентификатор покупателя</param>
        /// <param name="rating">Последний рейтинг при покупке</param>
        public record Customer(ulong customerId, double rating);
        /// <summary>
        /// Результат-отчет работы сервиса, передаваемый в writer_
        /// </summary>
        public record Report
        {
            public IEnumerable<Customer> customers;
            public ulong topCount;

            public Report(IEnumerable<Customer> customers, ulong topCount)
            {
                this.customers = customers;
                this.topCount = topCount;
            }
        }
        /// <summary>
        /// Синхронная обработка
        /// </summary>
        public override void Run()
        {
            IEnumerable<Customer>? customers = null;

            customers = parser_
                    .GetAll()
                    .Where(
                        s => s.order_date.CompareTo(startDate_) >= 0 && s.order_date.CompareTo(endDate_) == -1
                    )
                    .GroupBy(s => s.customer_id)//Сгрупировать по покупателям
                    .Select(g => new Customer(//Для каждого покупателя
                        customerId: g.Key,
                        rating: g.Where(s => s.order_date == g.Max(s => s.order_date))//Выбрать его последнюю покупку
                                 .ElementAt(0)
                                 .customer_rating//и выбрать рейтинг этого покупателя на момент покупки
                        )
                    )
                .OrderByDescending(c => c.rating)//Отсортировать покупателей по рейтингу по убыванию
                .ThenBy(c => c.customerId)
                .Take(topCount_);//Выбрать первые [topCount_]
            writer_.AddReport(new Report(customers, topCount_));
        }
        /// <summary>
        /// Асинхронная обработка
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public override async Task RunAsync(CancellationToken cancellation = default)
        {
            var customers = parser_
                .GetAllAsync()
                .Where(
                    s => s.order_date.CompareTo(startDate_) >= 0 && s.order_date.CompareTo(endDate_) == -1
                )
                .GroupBy(s => s.customer_id)//Сгрупировать по покупателям
                .Select(g => new Customer(//Для каждого покупателя
                    customerId: g.Key,
                    rating: g.Where(s => s.order_date == g.Max(s => s.order_date))//Выбрать его последнюю покупку
                             .ElementAt(0)
                             .customer_rating//и выбрать рейтинг этого покупателя на момент покупки
                    )
                )
                .OrderByDescending(c => c.rating)//Отсортировать покупателей по рейтингу по убыванию
                .ThenBy(c => c.customerId)
                .Take(topCount_);//Выбрать первые [topCount_]

            writer_.AddReport(new Report(await customers.ToListAsync(cancellation), topCount_));
        }
    }
}

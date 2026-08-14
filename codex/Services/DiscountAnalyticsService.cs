using codex.Shared;
namespace codex.Services
{
    /// <summary>
    /// Средняя скидка, по категории (группировка по месяцу).
    /// </summary>
    public class DiscountAnalyticsService : IAnalyticsService
    {
        public DiscountAnalyticsService(IWriter writer, ISalesParser parser, MODE mode, DateOnly start, DateOnly end) : base(writer, parser, mode, start, end)
        { }
        /// <summary>
        /// Билдер для TopAnalyticsService
        /// </summary>
        public class Builder : IAnalyticsServiceBuilder<Builder>
        {
            public override DiscountAnalyticsService Build()
            {
                if (writer_ is null || parser_ is null)
                    throw new Exception("Parser or writer is not defined");
                return new DiscountAnalyticsService(writer_, parser_, mode_, startDate_, endDate_);
            }
        }
        /// <summary>
        /// DTO для категорий, хранящее результат groupBy вызваный в Run()
        /// </summary>
        /// <param name="month"></param>
        /// <param name="name"></param>
        /// <param name="avg"></param>
        public record Category(string name, decimal avg);
        public record Month(DateOnly date, IEnumerable<Category> categories);
        /// <summary>
        /// Результат-отчет работы сервиса, передаваемый в writer_
        /// </summary>
        public record Report
        {
            public IEnumerable<Month> months;

            public Report(IEnumerable<Month> months)
            {
                this.months = months;
            }
        }

        /// <summary>
        /// Синхронная обработка
        /// </summary>
        /// <returns></returns>
        public override void Run()
        {
            var months = parser_
                .GetAll()
                .Where(
                    s => s.order_date.CompareTo(startDate_) >= 0 && s.order_date.CompareTo(endDate_) == -1
                )
                .GroupBy(s => new DateOnly(s.order_date.Year, s.order_date.Month, 1))//Сгрупировать по месяцам и годам
                .Select(//Для каждого месяца
                    g => new Month(
                            date: g.Key,
                            categories: g.GroupBy(s => s.product_category)
                                .Select(//Для каждой категории в ммесяце
                                    ng => new Category(
                                        name: ng.Key,
                                        avg: Math.Round( //Посчитать среднюю скидку и округлить до 2 чисел после запятой
                                                ng.Average(s => s.discount),
                                                2
                                             )
                                        )
                                )
                        )
                );

            writer_.AddReport(new Report(months));
        }
        /// <summary>
        /// Асинхронная обработка
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public override async Task RunAsync(CancellationToken cancellation = default)
        {
            var months = parser_
                .GetAllAsync()
                .Where(
                    s => s.order_date.CompareTo(startDate_) >= 0 && s.order_date.CompareTo(endDate_) == -1
                )
                .GroupBy(s => new DateOnly(s.order_date.Year, s.order_date.Month, 1))//Сгрупировать по месяцам и годам
                .Select(//Для каждого месяца
                    g => new Month(
                            date: g.Key,
                            categories: g.GroupBy(s => s.product_category)
                                .Select(//Для каждой категории в ммесяце
                                    ng => new Category(
                                        name: ng.Key,
                                        avg: Math.Round( //Посчитать среднюю скидку и округлить до 2 чисел после запятой
                                                ng.Average(s => s.discount),
                                                2
                                             )
                                        )
                                )
                        )
                );

            writer_.AddReport(new Report(await months.ToListAsync(cancellation)));
        }

    }
}

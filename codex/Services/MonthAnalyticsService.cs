using codex.Shared;

namespace codex.Services
{
    /// <summary>
    /// Средняя цена за каждый месяц (группировка по месяцу)
    /// </summary>
    public class MonthAnalyticsService : IAnalyticsService
    {
        public MonthAnalyticsService(IWriter writer, ISalesParser parser, MODE mode, DateOnly start, DateOnly end) : base(writer, parser, mode, start, end)
        { }
        /// <summary>
        /// Билдер для MonthAnalyticsService
        /// </summary>
        public class Builder : IAnalyticsServiceBuilder<Builder>
        {

            public override MonthAnalyticsService Build()
            {
                if (writer_ is null || parser_ is null)
                    throw new Exception("Parser or writer is not defined");
                return new MonthAnalyticsService(writer_, parser_, mode_, startDate_, endDate_);
            }
        }
        /// <summary>
        /// DTO для категорий, хранящее результат groupBy вызваный в Run()
        /// </summary>
        /// <param name="month">Месяц и год</param>
        /// <param name="sum">средняя цена за месяц</param>
        public record Category(DateOnly month, decimal avg);
        /// <summary>
        /// Результат-отчет работы сервиса, передаваемый в writer_
        /// </summary>
        public record Report
        {
            public IEnumerable<Category> categories;

            public Report(IEnumerable<Category> categories)
            {
                this.categories = categories;
            }
        }
        /// <summary>
        /// Синхронная обработка
        /// </summary>
        public override void Run()
        {
            var categories = parser_
                .GetAll()
                .Where(
                    s => s.order_date.CompareTo(startDate_) >= 0 && s.order_date.CompareTo(endDate_) == -1
                )
                .GroupBy(s => new DateOnly(s.order_date.Year, s.order_date.Month, 1))//Сгрупировать по месяцам и годам
                .Select(//Для каждого месяца
                    g => new Category(
                        month: g.Key,
                        avg: Math.Round( //Посчитать среднюю  цену и округления до 2 чисел после запятой
                                g.Average(s => s.unit_price),
                                2
                            )
                        )
                );

            writer_.AddReport(new Report(categories));
        }
        /// <summary>
        /// Асинхронная обработка
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public override async Task RunAsync(CancellationToken cancellation = default)
        {
            var categories = parser_
                .GetAllAsync()
                .Where(
                    s => s.order_date.CompareTo(startDate_) >= 0 && s.order_date.CompareTo(endDate_) == -1
                )
                .GroupBy(s => new DateOnly(s.order_date.Year, s.order_date.Month, 1))//Сгрупировать по месяцам и годам
                .Select(//Для каждого месяца
                    g => new Category(
                        month: g.Key,
                        avg: Math.Round( //Посчитать среднюю  цену и округления до 2 чисел после запятой
                                g.Average(s => s.unit_price),
                                2
                            )
                        )
                );

            writer_.AddReport(new Report(await categories.ToListAsync(cancellation)));
        }
    }
}

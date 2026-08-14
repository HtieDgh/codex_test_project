using codex.Shared;
using System.Collections.Concurrent;
namespace codex.Services
{
    /// <summary>
    /// Сервис по подсчету общей суммы продаж по категориям
    /// </summary>
    public class SumAnalyticsService : IAnalyticsService
    {
        public SumAnalyticsService(IWriter writer, ISalesParser parser, MODE mode, DateOnly start, DateOnly end) : base(writer, parser, mode, start, end)
        { }
        /// <summary>
        /// Билдер для SumAnalyticsService
        /// </summary>
        public class Builder : IAnalyticsServiceBuilder<Builder>
        {
            public override SumAnalyticsService Build()
            {
                if (writer_ is null || parser_ is null)
                    throw new Exception("Parser or writer is not defined");
                return new SumAnalyticsService(writer_, parser_, mode_, startDate_, endDate_);
            }
        }
        /// <summary>
        /// DTO для категорий, хранящее результат groupBy вызваный в Run()
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sum"></param>
        public record Category(string name, decimal sum);
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
        /// Реализация синхронного и параллельного режима
        /// </summary>
        public override void Run()
        {
            IEnumerable<Category>? categories = null;
            if (mode_ == MODE.PARALLEL)
            {
                var categoriesDict = new ConcurrentDictionary<string, decimal>();

                parser_
                    .GetAll()
                    .AsParallel()
                    .Where(
                        s => s.order_date.CompareTo(startDate_) >= 0 && s.order_date.CompareTo(endDate_) == -1
                    )
                    .ForAll(s =>
                    {
                        var category = s.product_category;
                        var sum = s.unit_price * s.quantity * (1 - s.discount);

                        categoriesDict.AddOrUpdate(//Сгрупировать по категориям
                            category,
                            sum,
                            (key, existingValue) => existingValue + sum
                        );
                    });

                categories = categoriesDict
                    .Select(kvp => new Category(
                        name: kvp.Key,
                        sum: Math.Round(kvp.Value, 2)//Посчитать сумму с округлением до 2 чисел после запятой
                    ));
            }
            else
            {
                categories = parser_
                .GetAll()
                .Where(
                    s => s.order_date.CompareTo(startDate_) >= 0 && s.order_date.CompareTo(endDate_) == -1
                )
                .GroupBy(s => s.product_category)//Сгрупировать по категориям
                .Select(//Для каждой категории
                    g => new Category(
                        name: g.Key,
                        sum: Math.Round( //Посчитать сумму с округлением до 2 чисел после запятой
                                g.Sum(s => s.unit_price * s.quantity * (1 - s.discount)),
                                2
                            )
                        )
                );
            }
            writer_.AddReport(new Report(categories));
        }
        /// <summary>
        /// Асинхроная обработка
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public override async Task RunAsync(CancellationToken cancellation = default)
        {
            var categories = parser_
                .GetAllAsync(cancellation)
                .Where(
                    s => s.order_date.CompareTo(startDate_) >= 0 && s.order_date.CompareTo(endDate_) == -1
                )
                .GroupBy(s => s.product_category)//Сгрупировать по категориям
                .Select(//Для каждой категории
                    g => new Category(
                        name: g.Key,
                        sum: Math.Round( //Посчитать сумму с округлением до 2 чисел после запятой
                                g.Sum(s => s.unit_price * s.quantity * (1 - s.discount)),
                                2
                            )
                        )
                );
       
            writer_.AddReport(new Report(await categories.ToListAsync(cancellation)));
        }

    }
}

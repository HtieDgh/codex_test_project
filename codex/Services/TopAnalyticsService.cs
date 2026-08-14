using codex.Shared;
using System.Collections.Concurrent;
namespace codex.Services
{
    /// <summary>
    /// Сервис формирующий Топ (по умолчанию из 5) товаров (категорий) по общему количеству проданных единиц
    /// </summary>
    public class TopAnalyticsService : IAnalyticsService
    {

        private readonly ushort topCount_;
        public TopAnalyticsService(IWriter writer, ISalesParser parser, ushort topCount, MODE mode, DateOnly start, DateOnly end) : base(writer, parser, mode, start, end)
        { topCount_ = topCount; }
        /// <summary>
        /// Билдер для TopAnalyticsService
        /// </summary>
        public class Builder : IAnalyticsServiceBuilder<Builder>
        {
            private ushort topCount_ = 5;
            public Builder AddTopCount(ushort topCount)
            {
                topCount_ = topCount;
                return this;

            }
            public override TopAnalyticsService Build()
            {
                if (writer_ is null || parser_ is null)
                    throw new Exception("Parser or writer is not defined");
                return new TopAnalyticsService(writer_, parser_, topCount_, mode_, startDate_, endDate_);
            }
        }
        /// <summary>
        /// DTO для категорий, хранящее результат groupBy вызваный в Run()
        /// </summary>
        /// <param name="name"></param>
        /// <param name="count"></param>
        public record Category(string name, long count);
        /// <summary>
        /// Результат-отчет работы сервиса, передаваемый в writer_
        /// </summary>
        public record Report
        {
            public IEnumerable<Category> categories;
            public ushort topCount;

            public Report(IEnumerable<Category> categories, ushort topCount)
            {
                this.categories = categories;
                this.topCount = topCount;
            }
        }

        /// <summary>
        /// Реализация синхронного и параллельного режима
        /// </summary>
        /// <returns></returns>
        public override void Run()
        {
            IEnumerable<Category>? categories = null;
            if (mode_ == MODE.PARALLEL)
            {
                var categoriesDict = new ConcurrentDictionary<string, long>();
                parser_
                   .GetAll()
                   .AsParallel()
                   .Where(
                        s => s.order_date.CompareTo(startDate_) >= 0 && s.order_date.CompareTo(endDate_) == -1
                    )
                    .ForAll(s =>
                     {
                         var category = s.product_category;
                         var count = s.quantity;

                         categoriesDict.AddOrUpdate(//Сгрупировать по категориям
                             category,
                             count,
                             (key, existingValue) => existingValue + count
                         );
                     });

                categories = categoriesDict
                    .Select(kvp => new Category(
                        name: kvp.Key,
                        count: kvp.Value
                    ));
            }
            else
            {
                categories = parser_
                   .GetAll()
                   .Where(
                       s => s.order_date.CompareTo(startDate_) >= 0 && s.order_date.CompareTo(endDate_) == -1
                   )
                   .GroupBy(s => s.product_category)
                   .Select(g => new Category(name: g.Key, count: g.Sum(s => s.quantity)));
            }
            categories = categories
                   .OrderByDescending(c => c.count)
                   .ThenBy(c => c.name)
                   .Take(topCount_);
            writer_.AddReport(new Report(categories, topCount_));
        }
        /// <summary>
        /// Асинхронная обработка
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public override async Task RunAsync(CancellationToken cancellation = default)
        {
            var categories = parser_.GetAllAsync(cancellation)
                .Where(
                    s => s.order_date.CompareTo(startDate_) >= 0 && s.order_date.CompareTo(endDate_) == -1
                )
                .GroupBy(s => s.product_category)
                .Select(g => new Category(name: g.Key, count: g.Sum(s => s.quantity)))
                .OrderByDescending(c => c.count)
                .ThenBy(c => c.name)
                .Take(topCount_);

            writer_.AddReport(new Report(await categories.ToListAsync(cancellation), topCount_));
        }
    }
}


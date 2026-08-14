using codex.Shared;
namespace codex.Services
{
    /// <summary>
    /// Среднее время доставки за указанный период
    /// </summary>
    public class DeliveryAnalyticsService : IAnalyticsService
    {
        public DeliveryAnalyticsService(IWriter writer, ISalesParser parser, MODE mode, DateOnly start, DateOnly end) : base(writer, parser, mode, start, end)
        { }
        /// <summary>
        /// Билдер для DeliveryAnalyticsService
        /// </summary>
        public class Builder : IAnalyticsServiceBuilder<Builder>
        {
            public override DeliveryAnalyticsService Build()
            {
                if (writer_ is null || parser_ is null)
                    throw new Exception("Parser or writer is not defined");
                return new DeliveryAnalyticsService(writer_, parser_, mode_, startDate_, endDate_);
            }
        }

        /// <summary>
        /// Результат-отчет работы сервиса, передаваемый в writer_
        /// </summary>
        public record Report
        {
            public double avgTime;

            public Report(double avgTime)
            {
                this.avgTime = avgTime;
            }
        }
        /// <summary>
        /// Синхронная обработка
        /// </summary>
        public override void Run()
        {
            var avgTime = parser_
                .GetAll()
                .Where(
                    s => s.order_date.CompareTo(startDate_) >= 0 && s.order_date.CompareTo(endDate_) == -1
                )
                .Average(s => s.delivery_days);//Посчитать среднее кол-во дней по доставке товара

            writer_.AddReport(new Report(avgTime));
        }
        /// <summary>
        /// Асинхронная обработка
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public override async Task RunAsync(CancellationToken cancellation = default)
        {
            var avgTime = await parser_
                .GetAllAsync()
                .Where(
                    s => s.order_date.CompareTo(startDate_) >= 0 && s.order_date.CompareTo(endDate_) == -1
                )
                .Select( s => new decimal(s.delivery_days))//Конвертанция в decimal для AverageAsync()
                .AverageAsync();//Посчитать среднее кол-во дней по доставке товара

            writer_.AddReport( new Report(Convert.ToDouble(avgTime)) );
        }
    }
}

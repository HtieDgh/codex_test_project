using System.Text.Json;

namespace codex.Services
{
    /// <summary>
    /// Запись JSON в файл
    /// </summary>
    public class JSONWriter : IWriter
    {
        class Data
        {
            public IEnumerable<SumAnalyticsService.Category>? SumReport { get; set; }
            public IEnumerable<TopAnalyticsService.Category>? TopReport { get; set; }
            public IEnumerable<MonthAnalyticsService.Category>? MonthReport { get; set; }
            public IEnumerable<CustomerAnalyticsService.Customer>? CustomerReport { get; set; }
            public IEnumerable<DiscountAnalyticsService.Month>? DiscountReport { get; set; }
            public double? DeliverReport { get; set; }
            /// <summary>
            /// Указаное число [topCount] при получени [TopReport]
            /// </summary>
            public ulong TopReportCount { get; set; }
            /// <summary>
            /// Указаное число [topCount] при получени [CustomerReport]
            /// </summary>
            public ulong CustomerReportCount { get; set; }
        }
        private Data data_ = new Data();
        private string filePath_;

        private JSONWriter()
        {
            filePath_ = "output.json";
        }
        public JSONWriter(string fileName)
        {
            filePath_ = fileName;
        }
        public IWriter AddReport(SumAnalyticsService.Report r)
        {
            data_.SumReport = r.categories;
            return this;
        }

        public IWriter AddReport(TopAnalyticsService.Report r)
        {
            data_.TopReport = r.categories;
            data_.TopReportCount = r.topCount;
            return this;
        }
        public IWriter AddReport(MonthAnalyticsService.Report r)
        {
            data_.MonthReport = r.categories;
            return this;
        }
        public IWriter AddReport(CustomerAnalyticsService.Report r)
        {
            data_.CustomerReport = r.customers;
            data_.CustomerReportCount = r.topCount;
            return this;
        }
        public IWriter AddReport(DeliveryAnalyticsService.Report r)
        {
            data_.DeliverReport = r.avgTime;
            return this;
        }
        public IWriter AddReport(DiscountAnalyticsService.Report r)
        {
            data_.DiscountReport = r.months;
            return this;
        }

        public void DoWrite()
        {
            using (FileStream fs = new FileStream(filePath_, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(fs, data_);
                Console.WriteLine($"Data has been saved to file ({filePath_})");
            }
        }
        public async Task DoWriteAsync(CancellationToken cancellation=default)
        {
            using (FileStream fs = new FileStream(filePath_, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                await JsonSerializer.SerializeAsync(fs, data_, JsonSerializerOptions.Default,cancellation);
                Console.WriteLine($"Data has been saved to file ({filePath_})");
            }
        }
    }
}

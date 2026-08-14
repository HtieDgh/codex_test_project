namespace codex.Services
{
    public interface IWriter
    {
        /// <summary>
        /// Добавить Report от [SumAnalyticsService]
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public IWriter AddReport(SumAnalyticsService.Report r);
        /// <summary>
        /// Добавить репорт от [TopAnalyticsService]
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public IWriter AddReport(TopAnalyticsService.Report r);
        /// <summary>
        /// Добавить репорт от [MonthAnalyticsService]
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public IWriter AddReport(MonthAnalyticsService.Report r);
        /// <summary>
        /// Добавить репорт от [CustomerAnalyticsService]
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public IWriter AddReport(CustomerAnalyticsService.Report r);
        /// <summary>
        /// Добавить репорт от [DeliveryAnalyticsService]
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public IWriter AddReport(DeliveryAnalyticsService.Report r);
        /// <summary>
        /// Добавить репорт от [DiscountAnalyticsService]
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public IWriter AddReport(DiscountAnalyticsService.Report r);

        public void DoWrite();
        public Task DoWriteAsync(CancellationToken cancellation=default);
    }
}

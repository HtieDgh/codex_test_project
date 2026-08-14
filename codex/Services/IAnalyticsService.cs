namespace codex.Services
{
    public abstract class IAnalyticsService
    {
        protected const ushort TOP_COUNT_DEFAULT = 5;
        public enum MODE
        {
            SYNCRONOUS,
            PARALLEL
        }
        
        protected readonly MODE mode_;
        protected readonly DateOnly startDate_;
        protected readonly DateOnly endDate_;
        protected readonly IWriter writer_;
        protected readonly ISalesParser parser_;

        
        public IAnalyticsService(IWriter writer, ISalesParser parser,MODE mode, DateOnly start, DateOnly end)
        {
            mode_ = mode;
            startDate_ = start;
            endDate_ = end;
            writer_ = writer;
            parser_ = parser;
        }

        public abstract void Run();
        public abstract Task RunAsync(CancellationToken cancellation = default);
    }
}

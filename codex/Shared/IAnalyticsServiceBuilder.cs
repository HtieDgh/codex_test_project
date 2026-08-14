using codex.Services;
using static codex.Services.IAnalyticsService;

namespace codex.Shared
{
    /// <summary>
    /// Билдер для сервисов. Использует такую конструкцию чтобы возвращать ссылку на обьект класса-наследника, который реализует логику инициализации.
    /// </summary>
    public abstract class IAnalyticsServiceBuilder<T> where T : IAnalyticsServiceBuilder<T>
    {
        protected MODE mode_;
        protected DateOnly startDate_;
        protected DateOnly endDate_;
        protected IWriter? writer_;
        protected ISalesParser? parser_;

        public IAnalyticsServiceBuilder()
        {
            mode_ = MODE.SYNCRONOUS;
            startDate_ = DateOnly.MinValue;
            endDate_ = DateOnly.MaxValue;
            writer_ = null;
            parser_ = null;
        }
        public virtual T AddMode(MODE mode)
        {
            mode_ = mode;
            return (T)this;
        }
        public virtual T AddStartDate(DateOnly start)
        {
            startDate_ = start;
            return (T)this;
        }
        public virtual T AddEndDate(DateOnly end)
        {
            endDate_ = end;
            return (T)this;
        }
        public virtual T AddWriter(IWriter writer)
        {
            writer_ = writer;
            return (T)this;
        }
        public virtual T AddParser(ISalesParser parser)
        {
            parser_ = parser;
            return (T)this;
        }
        public abstract IAnalyticsService Build();
    }
}

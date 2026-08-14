using codex.Services;
using System.Globalization;
using static codex.Program;

namespace codex.Shared
{

    /// <summary>
    /// Фбрика для сервисов. Возвращающает сервис, используемый в ServiceCollection
    /// </summary>
    public abstract class AnalyticsServiceFactory<TBuilder> where TBuilder : IAnalyticsServiceBuilder<TBuilder>, new()
    {
        protected abstract IAnalyticsService CreateService_(Options opts);

        protected TBuilder ConfigureCommonBuilder(Options opts)
        {
            var builder = new TBuilder();
           

            if (opts.endDate != null)
            {
                builder.AddEndDate(DateOnly.ParseExact(opts.endDate, "M/d/yyyy", CultureInfo.InvariantCulture));
            }
            if (opts.startDate != null)
            {
                builder.AddStartDate(DateOnly.ParseExact(opts.startDate, "M/d/yyyy", CultureInfo.InvariantCulture));
            }

            IWriter writer;
            IAnalyticsService.MODE serviceMode = opts.mode == "parallel"
                    ? IAnalyticsService.MODE.PARALLEL
                    : IAnalyticsService.MODE.SYNCRONOUS;
            if (opts.OutputFilePath != null)//Проверка на доступ к записи
            {
                var res = FileAccessChecker.CheckWriteAccess(opts.OutputFilePath);
                if (res != FileAccessChecker.WriteAccessResult.Success)
                    throw new ArgumentException($"No access to ({opts.OutputFilePath}): {res}");
            }

            switch (opts.mode)
            {
                case "console"://режим выввода резальтата в консоль
                    writer = WriterConfigurator.GetConsoleWriter();
                    break;

                case "file":
                    if (opts.OutputFilePath == null)//Проверка на наличие --output опции для --mode=file
                        throw new Exception("Please, define output file path, i.e. codex --mode=file, --input=\"./path/to/input.csv\" --output=\"./path/to/output.json\"");
                    
                    writer = WriterConfigurator.GetJsonWriter(opts.OutputFilePath);
                    break;

                case "parallel":
                case "di":
                case "async":
                case "full":
                    if (opts.OutputFilePath != null)
                        writer = WriterConfigurator.GetJsonWriter(opts.OutputFilePath);
                    else
                        writer = WriterConfigurator.GetConsoleWriter();
                    break;

                default:
                    throw new ArgumentException($"Mode \"{opts.mode}\" is not recognised");

            }

            return builder
                .AddWriter(writer)
                .AddMode(serviceMode)
                .AddParser(new SalesParser(new CsvReader(opts.InputFilePath ?? throw new ArgumentException("No input file path provided, see --help"))));
        }
    }
    /// <summary>
    /// Набор конкретных файбрик
    /// </summary>
    public class SumAnalyticsServiceFactory : AnalyticsServiceFactory<SumAnalyticsService.Builder>
    {
        protected static SumAnalyticsServiceFactory? instance_;
        SumAnalyticsServiceFactory() { }
        public static SumAnalyticsService CreateService(Options opts)
        {
            if (instance_ is null)
                instance_ = new SumAnalyticsServiceFactory();
            return instance_.CreateService_(opts);
        }
        protected override SumAnalyticsService CreateService_(Options opts)
        {
            var builder = ConfigureCommonBuilder(opts);
            return builder.Build();
        }
    }
    public class MonthAnalyticsServiceFactory : AnalyticsServiceFactory<MonthAnalyticsService.Builder>
    {
        protected static MonthAnalyticsServiceFactory? instance_;
        MonthAnalyticsServiceFactory() { }
        public static MonthAnalyticsService CreateService(Options opts)
        {
            if (instance_ is null)
                instance_ = new MonthAnalyticsServiceFactory();
            return instance_.CreateService_(opts);
        }
        protected override MonthAnalyticsService CreateService_(Options opts)
        {
            var builder = ConfigureCommonBuilder(opts);
            return builder.Build();
        }
    }
    public class DeliveryAnalyticsServiceFactory : AnalyticsServiceFactory<DeliveryAnalyticsService.Builder>
    {
        protected static DeliveryAnalyticsServiceFactory? instance_;
        DeliveryAnalyticsServiceFactory() { }
        public static DeliveryAnalyticsService CreateService(Options opts)
        {
            if (instance_ is null)
                instance_ = new DeliveryAnalyticsServiceFactory();
            return instance_.CreateService_(opts);
        }
        protected override DeliveryAnalyticsService CreateService_(Options opts)
        {
            var builder = ConfigureCommonBuilder(opts);
            return builder.Build();
        }
    }
    public class DiscountAnalyticsServiceFactory : AnalyticsServiceFactory<DiscountAnalyticsService.Builder>
    {
        protected static DiscountAnalyticsServiceFactory? instance_;
        DiscountAnalyticsServiceFactory() { }
        public static DiscountAnalyticsService CreateService(Options opts)
        {
            if (instance_ is null)
                instance_ = new DiscountAnalyticsServiceFactory();
            return instance_.CreateService_(opts);
        }
        protected override DiscountAnalyticsService CreateService_(Options opts)
        {
            var builder = ConfigureCommonBuilder(opts);
            return builder.Build();
        }
    }

    public class TopAnalyticsServiceFactory : AnalyticsServiceFactory<TopAnalyticsService.Builder>
    {
        protected static TopAnalyticsServiceFactory? instance_;
        TopAnalyticsServiceFactory() { }
        public static TopAnalyticsService CreateService(Options opts)
        {
            if (instance_ is null)
                instance_ = new TopAnalyticsServiceFactory();
            return instance_.CreateService_(opts);
        }
        protected override TopAnalyticsService CreateService_(Options opts)
        {
            var builder = ConfigureCommonBuilder(opts);
            //Ограничение сверху для Топ- Аналитик
            if (opts.topCount != null)
            {
                builder.AddTopCount(ushort.Parse(opts.topCount));
            }

            return builder.Build();
        }
    }
    public class CustomerAnalyticsServiceFactory : AnalyticsServiceFactory<CustomerAnalyticsService.Builder>
    {
        protected static CustomerAnalyticsServiceFactory? instance_;
        CustomerAnalyticsServiceFactory() { }
        public static CustomerAnalyticsService CreateService(Options opts)
        {
            if (instance_ is null)
                instance_ = new CustomerAnalyticsServiceFactory();
            return instance_.CreateService_(opts);
        }
        protected override CustomerAnalyticsService CreateService_(Options opts)
        {
            var builder = ConfigureCommonBuilder(opts);
            //Ограничение сверху для Топ- Аналитик
            if (opts.topCount != null)
            {
                builder.AddTopCount(ushort.Parse(opts.topCount));
            }

            return builder.Build();
        }
    }
}

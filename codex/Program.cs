using codex.Services;
using codex.Shared;
using Fclp;
using Fclp.Internals;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using static codex.Program.Options;

namespace codex
{
    public class Program
    {
        /// <summary>
        /// Описание возможных аргументов командной строки
        /// </summary>
        public class Options
        {
            public string? mode { get; set; }
            public string? InputFilePath { get; set; }
            public string? OutputFilePath { get; set; }
            public string? startDate { get; set; }
            public string? endDate { get; set; }
            public string? topCount { get; set; }

            public static readonly string mode_d = "Available run mode: 'console', 'file', 'di', 'async";
            public static readonly string InputFilePath_d = "Input file in csv format";
            public static readonly string OutputFilePath_d = "Output file. File wil be overitten.";
            public static readonly string startDate_d = "The date from which to take statistics into account";
            public static readonly string endDate_d = "The date up to which statistics should be taken into account, not inclusive";
            public static readonly string topCount_d = "Top limitter for Analytics. Default is 5.";
            public static readonly string helpText =
                """
                MODES:

                  --mode=console             Writes result to console.
                  --mode=file                Writes result in json to specified file.
                  --mode=di                  Uses Microsoft.Extensions.DependencyInjection to 
                                             execute analytics. If --output is specified,
                                             result will be written to file in json format.
                  --mode=async               Uses async methods to preform exectutions.
                                             If --output is specified, result will be written 
                                             to file in json format.
                  --mode=parallel            Configeres PARALLEL mode for Analytics. If --output 
                                             is specified, result will be written to file in 
                                             json format
                  --mode=full                Combined mode: Asynchronous I/O +
                                             + parallel computing + DI. If --output is specified,
                                             result will be written to file in json format
                """;
            public class Formater : ICommandLineOptionFormatter
            {
                private readonly int _optionNamePadding;

                /// <summary>
                /// Создает экземпляр форматтера с заданным отступом для названий опций.
                /// </summary>
                /// <param name="optionNamePadding">Минимальное количество символов для выравнивания названий опций.</param>
                public Formater(int optionNamePadding = 25)
                {
                    _optionNamePadding = optionNamePadding;
                }

                /// <summary>
                /// Основной метод форматирования. Принимает список опций и возвращает строку справки.
                /// </summary>
                public string Format(IEnumerable<ICommandLineOption> options)
                {
                    if (options == null || !options.Any())
                        return "  Нет доступных опций.";

                    var sb = new StringBuilder();
                    sb.AppendLine("OPTIONS:");
                    sb.AppendLine("");

                    // Группируем опции: сначала обязательные, потом необязательные для наглядности
                    var sortedOptions = options
                        .OrderByDescending(o => o.IsRequired)
                        .ThenBy(o => o.LongName ?? o.ShortName);

                    foreach (var option in sortedOptions)
                    {
                        AppendOptionLine(sb, option);
                    }
                    sb.AppendLine("");
                    sb.Append(helpText);
                    return sb.ToString();
                }
                /// <summary>
                /// Форматирует и добавляет в StringBuilder одну строку для опции
                /// </summary>
                private void AppendOptionLine(StringBuilder sb, ICommandLineOption option)
                {
                    // --- 1. Формируем название опции (ShortName/LongName) ---
                    var nameBuilder = new StringBuilder();
                    if (!string.IsNullOrEmpty(option.ShortName))
                        nameBuilder.Append($"-{option.ShortName}");

                    if (!string.IsNullOrEmpty(option.LongName))
                    {
                        if (nameBuilder.Length > 0)
                            nameBuilder.Append(", ");
                        nameBuilder.Append($"--{option.LongName}");
                    }

                    // Добавляем признак обязательности
                    if (option.IsRequired)
                        nameBuilder.Append(" (required)");

                    var optionName = nameBuilder.ToString();
                    var description = option.Description ?? "No description.";

                    // --- 2. Выравнивание строк ---
                    // Первая строка: название опции + описание (если помещается)
                    int paddingNeeded = Math.Max(0, _optionNamePadding - optionName.Length);
                    string paddedName = optionName + new string(' ', paddingNeeded);

                    // Формируем итоговую строку
                    sb.Append($"  {paddedName}  {description}");
                    
                    sb.AppendLine();
                }
            }

        }

        public static int Main(string[] args)
        {
            // create a generic parser for the ApplicationArguments type
            var p = new FluentCommandLineParser<Options>();

            // Настройка парсера
            p.Setup(arg => arg.mode)
             .As('m', "mode") // Короткое и длинное имя.
             .WithDescription(mode_d)//Описание к параметру
             .Required(); // Отметка что параметр Required.

            p.Setup(arg => arg.InputFilePath)
             .As('i', "input")
             .WithDescription(InputFilePath_d)
             .Required();

            p.Setup(arg => arg.OutputFilePath)
             .As('o', "output")
             .WithDescription(OutputFilePath_d);

            p.Setup(arg => arg.startDate)
             .As('s', "start-date")
             .WithDescription(startDate_d);

            p.Setup(arg => arg.endDate)
             .As('e', "end-date")
             .WithDescription(endDate_d);

            p.Setup(arg => arg.topCount)
             .As('t', "top")
             .WithDescription(topCount_d)
             .SetDefault("5");

            p.SetupHelp("?", "help")
              .WithCustomFormatter(new Formater())
              .Callback(text => Console.WriteLine(text));

            var result = p.Parse(args);

            if (result.HasErrors)
            {
                p.HelpOption.ShowHelp(p.Options);
                return 1;
            }
               
            if (result.HelpCalled)
                return 0;

            var resCode = OptionsHandler(p.Object);

            if (resCode != 0)
                p.HelpOption.ShowHelp(p.Options);
            
            return resCode;
        }

        /// <summary>
        /// Обработчик переданных аргументов. Запускает стратегии(сервисы) реалищующие аналитику
        /// </summary>
        /// <param name="opts"></param>
        /// <returns></returns>
        public static int OptionsHandler(Options opts)
        {
            int resultCode = 1;//возвращаемый код
            try
            {
                if (opts.mode == "di"|| opts.mode == "full")
                {
                    //Реализация c помощью Microsoft.Extensions.DependencyInjection
                    var services = new ServiceCollection();

                    //Регистрация сервисов
                    //Использование AddSingleton() следует из условий задачи,
                    //так как требуется вычислить аналитику по одному набору данных за один запуск программы.
                    //Использование других методов, таких как AddScoped() и/или AddTransient(), не даст преимуществ
                    // в данной задаче, а скорее будет требовать дополнительных ресурсов 
                    // для управления другими копиями сервисов
                    services.AddSingleton<IAnalyticsService, SumAnalyticsService>(
                            provider => SumAnalyticsServiceFactory.CreateService(opts)
                        );
                    services.AddSingleton<IAnalyticsService, TopAnalyticsService>(
                            provider => TopAnalyticsServiceFactory.CreateService(opts)
                        );
                    services.AddSingleton<IAnalyticsService, MonthAnalyticsService>(
                            provider => MonthAnalyticsServiceFactory.CreateService(opts)
                        );
                    services.AddSingleton<IAnalyticsService, CustomerAnalyticsService>(
                            provider => CustomerAnalyticsServiceFactory.CreateService(opts)
                        );
                    services.AddSingleton<IAnalyticsService, DeliveryAnalyticsService>(
                            provider => DeliveryAnalyticsServiceFactory.CreateService(opts)
                        );
                    services.AddSingleton<IAnalyticsService, DiscountAnalyticsService>(
                            provider => DiscountAnalyticsServiceFactory.CreateService(opts)
                        );

                    var serviceProvider = services.BuildServiceProvider();

                    //Запуск и вывод пограммы
                    // Запустить все аналитики параллельно, которые будут асинхроно читать и обрабатывать набор данных,
                    // дождаться завершения записи в консоль или в файл,
                    if (opts.mode == "full")
                    {
                        var pTask=Parallel.ForEachAsync(
                            serviceProvider.GetServices<IAnalyticsService>(),                            
                            async(s,cancellationToken)=>await s.RunAsync(cancellationToken)
                        );

                        pTask.Wait();//Ожидание конца обработки всех аналитик. Это нужно чтоб все аналитики успели предоставить свои отчеты перед итоговой записью в файл.

                        WriterConfigurator.GetWriter()
                            ?.DoWriteAsync()
                            .Wait();//Ожидание завершения вывода;
                    }
                    else//Простой DI режим
                    {
                        serviceProvider.GetServices<IAnalyticsService>()
                            .ToList()
                            .ForEach( s => s.Run() );
                        WriterConfigurator.GetWriter()?.DoWrite();
                    }
                }
                else
                {
                    //Реализация без Microsoft.Extensions.DependencyInjection
                    var controller = new Controller();

                    //Регистрация сервисов
                    controller.addService(SumAnalyticsServiceFactory.CreateService(opts));
                    controller.addService(TopAnalyticsServiceFactory.CreateService(opts));
                    controller.addService(MonthAnalyticsServiceFactory.CreateService(opts));
                    controller.addService(CustomerAnalyticsServiceFactory.CreateService(opts));
                    controller.addService(DeliveryAnalyticsServiceFactory.CreateService(opts));
                    controller.addService(DiscountAnalyticsServiceFactory.CreateService(opts));

                    //Запуск
                    if (opts.mode == "async")
                    {
                        var tasks = controller.RunAsync();

                        tasks.Wait();//Ожидание того, что все аналитики предоставили свои отчеты

                        WriterConfigurator.GetWriter()
                            ?.DoWriteAsync()//Вывод пограммы асинхронно
                            .Wait();//Ожидание завершения вывода
                    }
                    else
                    {
                        controller.Run();
                        WriterConfigurator.GetWriter()?.DoWrite();//Вывод пограммы
                    }
                }
                resultCode = 0;
            }
            catch (AggregateException ex)
            {
                // Обработка всех исключений в режиме full
                foreach (var innerEx in ex.InnerExceptions)
                {
                    Console.WriteLine($"Error: {innerEx.Message}");
                }
                Console.Error.WriteLine("");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine("");
            }
            return resultCode;
        }
    }
}

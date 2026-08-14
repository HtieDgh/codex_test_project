using System.Collections.Concurrent;
using System.Text;

namespace codex.Services
{
    /// <summary>
    /// Реализует вывод в консоль результатов обработки сервисов .
    /// </summary>
    public class ConsoleWriter : IWriter
    {
        // lines_ является критическим ресурсом, требующим внимание к потокобезопасности,
        // Поэтому используеся ConcurrentBag
        private ConcurrentBag<StringBuilder> lines_ = new ConcurrentBag<StringBuilder>();
        
        public IWriter AddReport(SumAnalyticsService.Report r)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"## Общая сумма продаж по категориям");
            sb.AppendLine($"| Категория | Сумма |");
            sb.AppendLine($"|---|---|");
            foreach (var category in r.categories)
            {
                sb.AppendLine($"{category.name}: {category.sum}");
            }
            r.GetHashCode();
            lines_.Add(sb);
            return this;
        }

        public IWriter AddReport(TopAnalyticsService.Report r)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"## Топ-{r.topCount} товаров (категорий) по общему количеству проданных единиц");
            sb.AppendLine($"| Категория | Единиц |");
            sb.AppendLine($"|---|---|");
            foreach (var category in r.categories)
            {
                sb.AppendLine($"| {category.name} | {category.count} |");
            }
            lines_.Add(sb);
            return this;
        }
        public IWriter AddReport(MonthAnalyticsService.Report r)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"## Средняя цена за каждый месяц (группировка по месяцу)");
            sb.AppendLine($"| Месяц | Средняя цена |");
            sb.AppendLine($"|---|---|");
            foreach (var category in r.categories)
            {
                sb.AppendLine($"| {category.month.ToString("Y")} | {category.avg} |");
            }
            lines_.Add(sb);
            return this;
        }
        public IWriter AddReport(CustomerAnalyticsService.Report r)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"## Топ-{r.topCount} покупателей по рейтингу");
            sb.AppendLine($"| ID | Рейтинг |");
            sb.AppendLine($"|---|---|");
            foreach (var c in r.customers)
            {
                sb.AppendLine($"| {c.customerId} | {c.rating} |");
            }
            lines_.Add(sb);
            return this;
        }

        public IWriter AddReport(DeliveryAnalyticsService.Report r)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"## Среднее время доставки товара");
            sb.AppendLine($"Составило {r.avgTime.ToString()} дней" );
            lines_.Add(sb);
            return this;
        }
        public IWriter AddReport(DiscountAnalyticsService.Report r)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"## Средняя скидка, по категории (группировка по месяцам) ");
            foreach (var month in r.months)
            {
                sb.AppendLine($"### {month.date.ToString("Y")}");
                sb.AppendLine($"| Имя категориии | Скидка|");
                sb.AppendLine($"|---|---|");
                foreach (var category in month.categories)
                {
                    sb.AppendLine($"| {category.name} | {category.avg} |");
                }
            }
            lines_.Add(sb);
            return this;
        }

        public void DoWrite()
        {
            foreach (var sb in lines_) {
                Console.Out.Write(sb);
            }
        }
        public async Task DoWriteAsync(CancellationToken cancellation)
        {
            foreach (var sb in lines_)
            {
                await Console.Out.WriteAsync(sb, cancellation);
            }
        }

    }
}

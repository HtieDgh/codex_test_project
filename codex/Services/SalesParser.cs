using codex.Models;
using System.Globalization;

namespace codex.Services
{
    public class SalesParser:ISalesParser
    {
        public SalesParser(IReader reader) : base(reader) 
        { }
        public override List<Sale> GetAll()
        {
            var res = new List<Sale>();

            var headerChecked = false;//Чтение и проверка заголовка     

            foreach (var cells in reader_.Read())
            {
                if (!headerChecked)
                {
                    if (!cells.SequenceEqual(headerShema_))
                    {
                        throw new ArgumentException("CSV cells can't be parsed: Unacceptable header");//заголовок не соответствует схеме
                    }
                    else
                    {
                        headerChecked = true;
                        continue;
                    }
                }

                res.Add(Parse(cells));
            }
            return res;
        }

        public override async IAsyncEnumerable<Sale> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var headerChecked = false;//Чтение и проверка заголовка
            await foreach (var cells in reader_.ReadAsync(cancellationToken))
            {
                if (!headerChecked)
                {
                    if (!cells.SequenceEqual(headerShema_))
                    {
                        throw new ArgumentException($"CSV cells can't be parsed: Unacceptable header");//заголовок не соответствует схеме
                    }
                    else
                    {
                        headerChecked = true;
                        continue;
                    }
                }

                yield return Parse(cells);
            }
            yield break;
        }

        protected override Sale Parse(string[] cells)
        {
            if(cells is null)
                throw new ArgumentException($"CSV cells can't be parsed, found (0) in line");

            if (cells.Length != 12)
                throw new ArgumentException($"CSV cells can't be parsed, found ({cells.Length}) in line");

            return new Sale(
                ulong.Parse(cells[0]),                                //order_id
                                                                      //order_date
                DateOnly.ParseExact(cells[1], dateFormats_, CultureInfo.InvariantCulture, DateTimeStyles.None),
                ulong.Parse(cells[2]),                                //customer_id
                cells[3],                                             //product_category
                cells[4],                                             //region
                uint.Parse(cells[5]),                                 //quantity
                decimal.Parse(cells[6], CultureInfo.InvariantCulture), //unit_price
                decimal.Parse(cells[7], CultureInfo.InvariantCulture), //discount
                cells[8],                                             //payment_method
                ushort.Parse(cells[9]),                               //delivery_days
                double.Parse(cells[10], CultureInfo.InvariantCulture),//customer_rating
                double.Parse(cells[11], CultureInfo.InvariantCulture) //revenue
            );
        }

        protected override async Task<Sale> ParseAsync(Task<string[]> t)
        {
            return Parse(t.Result);
        }
    }
}

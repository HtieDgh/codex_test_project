using codex.Models;
using codex.Services;
using Moq;

namespace test.Unit
{
    public class SalesParserShould
    {
        //Наборы верных (валидных) данных
        public static IEnumerable<object[]> GetValidTestData()
        {
            
            yield return new object[] {//Первый набор
                    new List<string[]>{ //data
                        new[] { "order_id", "order_date", "customer_id", "product_category", "region", "quantity", "unit_price", "discount", "payment_method", "delivery_days", "customer_rating", "revenue" },
                        new[] { "10001", "1/1/2022", "1102", "Beauty", "South", "7", "373.65", "0.28", "Wallet", "10", "4.7", "1883.2" }
                    },
                    new Sale(10001,new DateOnly(2022,1,1) ,1102,"Beauty","South",7,373.65m,0.28m,"Wallet",10,4.7,1883.2) //expected
            };
            yield return new object[] {//Второй набор
                    new List<string[]>{//data
                        new[] { "order_id", "order_date", "customer_id", "product_category", "region", "quantity", "unit_price", "discount", "payment_method", "delivery_days", "customer_rating", "revenue" },
                        new[] { "1", "1/1/0001", "1", "s", "w", "1", "1.0", "1.0", "p", "1", "1.0", "1.0" }
                    },
                    new Sale(1,new DateOnly(1,1,1) ,1,"s","w",1,1.0m,1.0m,"p",1,1.0,1.0) //expected
            };
        }

        [Theory]
        [MemberData(nameof(GetValidTestData))]
        public void ParseCorrectOnValid(List<string[]> data, Sale expected)
        {
            //Arrange
            var mockStreamReader = new Mock<StreamReader>(Stream.Null);
            var mockReader = new Mock<ICsvReader>(mockStreamReader.Object);
            mockReader.Setup(r => r.Read())
                .Returns(data);

            SalesParser parser = new SalesParser(mockReader.Object);

            // Act
            var result = parser.GetAll();

            //Assert
            Assert.Collection(result, item => Assert.Equal(expected, item));
        }

        //Наборы данных с ошибками (Не валидных)
        public static IEnumerable<object[]> GetInvalidTestData()
        {
            yield return new object[] {//Пустая строка
                    new List<string[]>{
                        new[] { "order_id", "order_date", "customer_id", "product_category", "region", "quantity", "unit_price", "discount", "payment_method", "delivery_days", "customer_rating", "revenue" },
                        new[] { "10001", "1/1/2022", "1102", "Beauty", "South", "7", "373.65", "0.28", "Wallet", "10", "4.7", "1883.2" },
                        null,//BAD
                        new[] { "10001", "1/1/2022", "1102", "Beauty", "South", "7", "373.65", "0.28", "Wallet", "10", "4.7", "1883.2" }
                    },
                    typeof(ArgumentException) //expected
            };
            yield return new object[] {//Не всем полям предоставлено значение
                    new List<string[]>{
                        new[] { "order_id", "order_date", "customer_id", "product_category", "region", "quantity", "unit_price", "discount", "payment_method", "delivery_days", "customer_rating", "revenue" },
                        new[] { "10001", "1/1/2022", "1102", "Beauty", "373.65", "0.28", "Wallet", "10", "4.7", "1883.2" },//BAD
                        new[] { "10001", "1/1/2022", "1102", "Beauty", "South", "7", "373.65", "0.28", "Wallet", "10", "4.7", "1883.2" }
                    },
                    typeof(ArgumentException) //expected
            };
            yield return new object[] {//Значений больше чем нужно полей
                    new List<string[]>{
                        new[] { "order_id", "order_date", "customer_id", "product_category", "region", "quantity", "unit_price", "discount", "payment_method", "delivery_days", "customer_rating", "revenue" },
                        new[] { "10001", "1/1/2022", "1102", "Beauty", "South", "7", "373.65", "0.28", "Wallet", "10", "4.7", "1883.2" },
                        new[] { "10001", "1/1/2022", "1102", "Beauty", "South", "7", "373.65", "0.28", "Wallet", "10", "4.7", "1883.2", "1883.2", "1883.2", "1883.2", "1883.2", "1883.2", "1883.2", "1883.2" }//BAD
                    },
                    typeof(ArgumentException) //expected
            };
            yield return new object[] {//Отсутствует заголовок
                    new List<string[]>{
                        new[] { "10001", "1/1/2022", "1102", "Beauty", "South", "7", "373.65", "0.28", "Wallet", "10", "4.7", "1883.2" }
                    },
                    typeof(ArgumentException) //expected
            };
            yield return new object[] {//Заголовок есть, но не все поля
                    new List<string[]>{
                        new[] { "order_id", "order_date", "customer_id", "product_category", "region", "quantity", "unit_price"},//BAD
                        new[] { "10001", "1/1/2022", "1102", "Beauty", "South", "7", "373.65", "0.28", "Wallet", "10", "4.7", "1883.2" }
                    },
                    typeof(ArgumentException) //expected
            };
            yield return new object[] {//Заголовок есть, все поля не верные
                    new List<string[]>{
                        new[] { "__ERRORFIELD__", "__ERRORFIELD__", "__ERRORFIELD__", "__ERRORFIELD__", "__ERRORFIELD__", "__ERRORFIELD__", "__ERRORFIELD__", "__ERRORFIELD__", "__ERRORFIELD__", "__ERRORFIELD__", "__ERRORFIELD__", "__ERRORFIELD__" },//BAD
                        new[] { "10001", "1/1/2022", "1102", "Beauty", "South", "7", "373.65", "0.28", "Wallet", "10", "4.7", "1883.2" }
                    },
                    typeof(ArgumentException) //expected
            };
            yield return new object[] {//Заголовок есть, но одно поле не верное
                    new List<string[]>{
                        new[] { "order_id", "order_date", "customer_id", "product_category", "region", "quantity", "unit_price", "__ERRORFIELD__", "payment_method", "delivery_days", "customer_rating", "revenue" },//BAD
                        new[] { "10001", "1/1/2022", "1102", "Beauty", "South", "7", "373.65", "0.28", "Wallet", "10", "4.7", "1883.2" }
                    },
                    typeof(ArgumentException) //expected
            };
            yield return new object[] {// Не верная дата
                    new List<string[]>{
                        new[] { "order_id", "order_date", "customer_id", "product_category", "region", "quantity", "unit_price", "discount", "payment_method", "delivery_days", "customer_rating", "revenue" },
                        new[] { "10002", "1/1/22", "1102", "Beauty", "South", "7", "373.65", "0.28", "Wallet", "10", "4.7", "1883.2" }//BAD
                    },
                    typeof(FormatException)//expected
            };
            yield return new object[] {// Не верная цена
                    new List<string[]>{
                        new[] { "order_id", "order_date", "customer_id", "product_category", "region", "quantity", "unit_price", "discount", "payment_method", "delivery_days", "customer_rating", "revenue" },
                        new[] { "10003", "1/1/2022", "1102", "Beauty", "South", "7", "373.6FFF", "0.28", "Wallet", "10", "4.7", "1883.2" }//BAD
                    },
                    typeof(FormatException)//expected
            };
            yield return new object[] {// Не верное количество
                    new List<string[]>{
                        new[] { "order_id", "order_date", "customer_id", "product_category", "region", "quantity", "unit_price", "discount", "payment_method", "delivery_days", "customer_rating", "revenue" },
                        new[] { "10003", "1/1/2022", "1102", "Beauty", "South", "5.5", "373.6", "0.28", "Wallet", "10", "4.7", "1883.2" }//BAD
                    },
                    typeof(FormatException)//expected
            };
        }

        [Theory]
        [MemberData(nameof(GetInvalidTestData))]
        public void ThrowExeptionOnInvalid(List<string[]> data, Type expected)
        {
            //Arrange
            var mockStreamReader = new Mock<StreamReader>(Stream.Null);
            var mockReader = new Mock<ICsvReader>(mockStreamReader.Object);
            mockReader.Setup(r => r.Read())
                .Returns(data);

            SalesParser parser = new SalesParser(mockReader.Object);

            //Act & Assert
            var e = Assert.Throws(expected, parser.GetAll);

            Assert.IsType(expected, e);
        }
        [Fact]
        public async Task GetAllAsync_ReturnsTaskCompatibleResult()
        {
            // Arrange
            var mockStreamReader = new Mock<StreamReader>(Stream.Null);
            var mockReader = new Mock<ICsvReader>(mockStreamReader.Object);
                mockReader.Setup(r => r.ReadAsync())
                    .Returns(ProvideAsync_);

            SalesParser parser = new SalesParser(mockReader.Object);

            // Act
            var task = Task.Run(async () =>
            {
                var result = new List<Sale>();
                await foreach (var sale in parser.GetAllAsync())
                {
                    result.Add(sale);
                }
                return result;
            });

            // Assert
            var completed = await Task.WhenAny(task, Task.Delay(100));
            Assert.Equal(task, completed); // Задача должна завершиться успешно 

            var ActualOutput = await task;
        }

        private static async IAsyncEnumerable<string[]> ProvideAsync_()
        {
            var data_= (List<string[]>)GetValidTestData().First()[0];

            for (int i = 0; i < data_.Count(); i++)
            {
                await Task.Delay(1);//долгое чтение
                yield return data_[i];
            }
            yield break;
        }
    }
}

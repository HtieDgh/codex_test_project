using codex.Models;
using codex.Services;
using Moq;
using test.Helpers;

namespace test.Unit
{
    public class DiscountAnalyticsServiceShould
    {
        //Наборы верных (валидных) данных
        public static IEnumerable<object[]> GetValidTestData()
        {
            var data = new List<Sale>//data
            {
                new Sale(10001,new DateOnly(2022, 1 , 19) ,1102,"Beauty","South",1,373.65m,    0.28m,"Wallet",10,4.7,1883.2),//Первый месяц),
                new Sale(10002,new DateOnly(2022, 2 , 16) ,1435,"Clothing","South",1,47.74m,   0.09m,"Card"  ,6,3.9,304.1 ),//Второй месяц
                new Sale(10003,new DateOnly(2022, 2 , 19) ,1860,"Beauty","East",1,311.28m,     0.31m,"COD"   ,6,2.5,644.35),
                new Sale(10004,new DateOnly(2022, 4 , 2 ) ,1270,"Electronics","West",1,524.47m,0.02m,"Wallet",6,1.6,2569.9),//Третий месяц,
                new Sale(10005,new DateOnly(2022, 4 , 22) ,1106,"Clothing","West",1,139.87m,   0.33m,"Wallet",4,4.9,468.56),
                new Sale(10006,new DateOnly(2022, 4 , 16) ,1071,"Electronics","West",2,264.44m,0.2m,"Card"   ,3,3.3,423.1),
                new Sale(10007,new DateOnly(2022, 4 , 23) ,1700,"Clothing","South",1,564.6m,   0.23m,"Card"  ,1,1.8,2608.45),
                new Sale(10008,new DateOnly(2022, 4 , 15) ,1020,"Beauty","North",1,265.71m,    0.13m,"Card"  ,2,3.7,1387.01),
                new Sale(10009,new DateOnly(2022, 4 , 12) ,1614,"Clothing","South",1,224.65m,  0.27m,"Card"  ,2,1.4,983.97),
                new Sale(10010,new DateOnly(2022, 4 , 1 ) ,1121,"Clothing","West",1,272.71m,   0.06m,"Card"  ,10,4.5,1025.39)
            };

            yield return new object[] {//Набор 1 (Весь диапазон дат)
                data,
                DateOnly.MinValue, //startDate, 
                DateOnly.MaxValue, //endDate,  
                new List<DiscountAnalyticsService.Month>()
                {
                    new (
                        date: new DateOnly(2022,1,1),
                        categories: new List<DiscountAnalyticsService.Category>()
                                    {
                                        new DiscountAnalyticsService.Category("Beauty",0.28m)
                                    }

                    ),
                    new (
                        date: new DateOnly(2022, 2, 1),
                        categories: new List<DiscountAnalyticsService.Category>()
                                    {
                                        new DiscountAnalyticsService.Category("Beauty",0.31m),
                                        new DiscountAnalyticsService.Category("Clothing",0.09m)
                                    }
                    ),
                    new (
                        date: new DateOnly(2022, 4, 1),
                        categories: new List<DiscountAnalyticsService.Category>()
                                    {
                                        new DiscountAnalyticsService.Category("Beauty",0.13m),
                                        new DiscountAnalyticsService.Category("Clothing",0.22m),
                                        new DiscountAnalyticsService.Category("Electronics",0.11m)
                                    }
                    ),
                 }//expectedOutput
            };
            yield return new object[] {//Набор 2 (ограничение по дате снизу и сверху)
                data,
                new DateOnly(2022,2,1), //startDate, 
                new DateOnly(2022, 4, 1), //endDate,  
                new List<DiscountAnalyticsService.Month>()
                {
                    new (
                        date: new DateOnly(2022, 2, 1),
                        categories: new List<DiscountAnalyticsService.Category>()
                                    {
                                        new DiscountAnalyticsService.Category("Beauty",0.31m),
                                        new DiscountAnalyticsService.Category("Clothing",0.09m)
                                    }
                    )
                 }//expectedOutput
            };
            yield return new object[] {//Набор 3 (ограничение по дате сверху)
                data,
                DateOnly.MinValue, //startDate, 
                new DateOnly(2022, 4, 1), //endDate,  
                new List<DiscountAnalyticsService.Month>()
                {
                    new (
                        date: new DateOnly(2022,1,1),
                        categories: new List<DiscountAnalyticsService.Category>()
                                    {
                                        new DiscountAnalyticsService.Category("Beauty",0.28m)
                                    }

                    ),
                    new (
                        date: new DateOnly(2022, 2, 1),
                        categories: new List<DiscountAnalyticsService.Category>()
                                    {
                                        new DiscountAnalyticsService.Category("Beauty",0.31m),
                                        new DiscountAnalyticsService.Category("Clothing",0.09m)
                                    }
                    ),
                 }//expectedOutput
            };
            yield return new object[] {//Набор 4 (ограничение по дате снизу)
                data,
                new DateOnly(2022,2,1), //startDate, 
                DateOnly.MaxValue, //endDate,  
                new List<DiscountAnalyticsService.Month>()
                {
                    new (
                        date: new DateOnly(2022, 2, 1),
                        categories: new List<DiscountAnalyticsService.Category>()
                                    {
                                        new DiscountAnalyticsService.Category("Beauty",0.31m),
                                        new DiscountAnalyticsService.Category("Clothing",0.09m)
                                    }
                    ),
                    new (
                        date: new DateOnly(2022, 4, 1),
                        categories: new List<DiscountAnalyticsService.Category>()
                                    {
                                        new DiscountAnalyticsService.Category("Beauty",0.13m),
                                        new DiscountAnalyticsService.Category("Clothing",0.22m),
                                        new DiscountAnalyticsService.Category("Electronics",0.11m)
                                    }
                    )
                 }//expectedOutput
            };
        }
        [Theory]
        [MemberData(nameof(GetValidTestData))]
        public void ReturnCorrectOnValid(List<Sale> data, DateOnly startDate, DateOnly endDate, List<DiscountAnalyticsService.Month> expectedOutput)
        {
            var mockStreamReader = new Mock<StreamReader>(Stream.Null);
            var mockReader = new Mock<ICsvReader>(mockStreamReader.Object);
            var mockSalesParser = new Mock<ISalesParser>(mockReader.Object);
            mockSalesParser.Setup(sp => sp.GetAll())
                .Returns(data);

            var actualOutput = new List<DiscountAnalyticsService.Month>();//Для получения вывода

            var mockWriter = new Mock<IWriter>();
            mockWriter.Setup(w => w.AddReport(It.IsAny<DiscountAnalyticsService.Report>()))
                .Callback<DiscountAnalyticsService.Report>(report =>
                {
                    foreach (var month in report.months)
                    {
                        actualOutput.Add(new DiscountAnalyticsService.Month(month.date, month.categories.ToList()));
                    }
                }
             );//Полученный Report добавлять в actualOutput


            var service = new DiscountAnalyticsService(mockWriter.Object, mockSalesParser.Object, IAnalyticsService.MODE.SYNCRONOUS, startDate, endDate);

            //Act
            service.Run();

            //Assert
            Assert.Equal(expectedOutput.Count, actualOutput.Count);
            for (ushort i = 0; i < actualOutput.Count; i++)
            {
                foreach (var c in expectedOutput[i].categories)
                {
                    Assert.Contains(c, actualOutput[i].categories);
                }
            }
        }
        [Fact]
        public async Task ReturnCorrectOnValid_Async()
        {
            var mockStreamReader = new Mock<StreamReader>(Stream.Null);
            var mockReader = new Mock<ICsvReader>(mockStreamReader.Object);
            var mockSalesParser = new Mock<ISalesParser>(mockReader.Object);

            mockSalesParser.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                      .Returns(ProvideAsync_);

            var actualOutput = new List<DiscountAnalyticsService.Month>();//Для получения вывода
            var expectedOutput = new List<DiscountAnalyticsService.Month>()
                {
                    new (
                        date: new DateOnly(2022,1,1),
                        categories: new List<DiscountAnalyticsService.Category>()
                                    {
                                        new DiscountAnalyticsService.Category("Beauty",0.28m)
                                    }

                    ),
                    new (
                        date: new DateOnly(2022, 2, 1),
                        categories: new List<DiscountAnalyticsService.Category>()
                                    {
                                        new DiscountAnalyticsService.Category("Beauty",0.31m),
                                        new DiscountAnalyticsService.Category("Clothing",0.09m)
                                    }
                    ),
                    new (
                        date: new DateOnly(2022, 4, 1),
                        categories: new List<DiscountAnalyticsService.Category>()
                                    {
                                        new DiscountAnalyticsService.Category("Beauty",0.13m),
                                        new DiscountAnalyticsService.Category("Clothing",0.22m),
                                        new DiscountAnalyticsService.Category("Electronics",0.11m)
                                    }
                    ),
                 };


            var mockWriter = new Mock<IWriter>();
            mockWriter.Setup(w => w.AddReport(It.IsAny<DiscountAnalyticsService.Report>()))
                .Callback<DiscountAnalyticsService.Report>(report =>
                {
                    foreach (var month in report.months)
                    {
                        actualOutput.Add(new DiscountAnalyticsService.Month(month.date, month.categories.ToList()));
                    }
                }
             );

            var service = new DiscountAnalyticsService(mockWriter.Object, mockSalesParser.Object, IAnalyticsService.MODE.SYNCRONOUS, DateOnly.MinValue, DateOnly.MaxValue);

            //Act
            var task = service.RunAsync();

            //Assert
            var completed = await Task.WhenAny(task, Task.Delay(1000));
            Assert.Equal(task, completed); // Задача должна завершиться успешно за установленый Delay

            Assert.Equal(expectedOutput.Count, actualOutput.Count);
            for (ushort i = 0; i < actualOutput.Count; i++)
            {
                foreach (var c in expectedOutput[i].categories)
                {
                    Assert.Contains(c, actualOutput[i].categories);
                }
            }
        }
        [Fact]
        public void BuildYourself_WithHelper()
        {
            // Arrange & Act
            var mockStreamReader = new Mock<StreamReader>(Stream.Null);
            var mockReader = new Mock<ICsvReader>(mockStreamReader.Object);
            var mockSalesParser = new Mock<ISalesParser>(mockReader.Object);

            var mockWriter = new Mock<IWriter>();

            var service = new DiscountAnalyticsService.Builder()
                .AddParser(mockSalesParser.Object)
                .AddWriter(mockWriter.Object)
                .Build();


            var parser = ObjectAssert.GetPrivateField<ISalesParser>(service, "parser_");
            var writer = ObjectAssert.GetPrivateField<IWriter>(service, "writer_");
            var startDate = ObjectAssert.GetPrivateField<DateOnly>(service, "startDate_");
            var endDate = ObjectAssert.GetPrivateField<DateOnly>(service, "endDate_");
            var mode = ObjectAssert.GetPrivateField<IAnalyticsService.MODE>(service, "mode_");

            // Assert
            Assert.Equal(mockWriter.Object, writer);
            Assert.Equal(mockSalesParser.Object, parser);
            Assert.Equal(DateOnly.MinValue, startDate);
            Assert.Equal(DateOnly.MaxValue, endDate);
            Assert.Equal(IAnalyticsService.MODE.SYNCRONOUS, mode);
        }
        [Fact]
        public async Task RunAsyncTruly()
        {
            // Arrange
            var mockStreamReader = new Mock<StreamReader>(Stream.Null);
            var mockReader = new Mock<ICsvReader>(mockStreamReader.Object);
            var mockParser = new Mock<ISalesParser>(mockReader.Object);
            var mockWriter = new Mock<IWriter>();


            // Настраиваем моки на возврат Task.FromResult (это синхронный Task)
            mockParser.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                      .Returns(ProvideAsync_);

            mockWriter.Setup(x => x.DoWriteAsync(It.IsAny<CancellationToken>()))
                      .Returns(Task.FromResult(0));

            var service = new DiscountAnalyticsService(mockWriter.Object, mockParser.Object, IAnalyticsService.MODE.SYNCRONOUS, DateOnly.MinValue, DateOnly.MaxValue);

            // Acе & Assert
            Assert.True(await AsyncVerifier.IsMethodAsync(service.RunAsync));
        }

        //Данные для метода ProvideAsync_()
        private static List<Sale> data_ = new List<Sale>
            {
                new Sale(10001,new DateOnly(2022, 1 , 19) ,1102,"Beauty","South",1,373.65m,    0.28m,"Wallet",10,4.7,1883.2),//Первый месяц),
                new Sale(10002,new DateOnly(2022, 2 , 16) ,1435,"Clothing","South",1,47.74m,   0.09m,"Card"  ,6,3.9,304.1 ),//Второй месяц
                new Sale(10003,new DateOnly(2022, 2 , 19) ,1860,"Beauty","East",1,311.28m,     0.31m,"COD"   ,6,2.5,644.35),
                new Sale(10004,new DateOnly(2022, 4 , 2 ) ,1270,"Electronics","West",1,524.47m,0.02m,"Wallet",6,1.6,2569.9),//Третий месяц,
                new Sale(10005,new DateOnly(2022, 4 , 22) ,1106,"Clothing","West",1,139.87m,   0.33m,"Wallet",4,4.9,468.56),
                new Sale(10006,new DateOnly(2022, 4 , 16) ,1071,"Electronics","West",2,264.44m,0.2m,"Card"   ,3,3.3,423.1),
                new Sale(10007,new DateOnly(2022, 4 , 23) ,1700,"Clothing","South",1,564.6m,   0.23m,"Card"  ,1,1.8,2608.45),
                new Sale(10008,new DateOnly(2022, 4 , 15) ,1020,"Beauty","North",1,265.71m,    0.13m,"Card"  ,2,3.7,1387.01),
                new Sale(10009,new DateOnly(2022, 4 , 12) ,1614,"Clothing","South",1,224.65m,  0.27m,"Card"  ,2,1.4,983.97),
                new Sale(10010,new DateOnly(2022, 4 , 1 ) ,1121,"Clothing","West",1,272.71m,   0.06m,"Card"  ,10,4.5,1025.39)
            };
        private static async IAsyncEnumerable<Sale> ProvideAsync_()
        {
            await Task.Delay(1);//долгое чтение
            for (int i = 0; i < data_.Count; i++)
                yield return data_[i];
            yield break;
        }
    }
}


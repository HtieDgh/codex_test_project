using codex.Models;
using codex.Services;
using Moq;
using test.Helpers;

namespace test
{
    public class CustomerAnalyticsServiceShould
    {
        //Наборы верных (валидных) данных
        public static IEnumerable<object[]> GetValidTestData()
        {
            var data = new List<Sale>//data
            {
                new Sale(10001,new DateOnly(2022,1,1) ,1102,"Beauty","South",1,373.65m,0.28m,"Wallet",10,4.7,1883.2),
                new Sale(10002,new DateOnly(2022,1,2) ,1102,"Clothing","South",1,47.74m,0.09m,"Card",6,3.9,304.1 ),
                new Sale(10003,new DateOnly(2022,1,3) ,1102,"Beauty","East",1,311.28m,0.31m,"COD",6,2.5,644.35),
                new Sale(10004,new DateOnly(2022,1,4) ,1270,"Electronics","West",1,524.47m,0.02m,"Wallet",6,1.6,2569.9),
                new Sale(10005,new DateOnly(2022,1,5) ,1270,"Clothing","West",1,139.87m,0.33m,"Wallet",4,4.9,468.56),
                new Sale(10006,new DateOnly(2022,1,6) ,1270,"Electronics","West",2,264.44m,0.2m,"Card",3,3.3,423.1),
                new Sale(10007,new DateOnly(2022,1,7) ,1700,"Clothing","South",1,564.6m,0.23m,"Card",1,1.8,2608.45),
                new Sale(10008,new DateOnly(2022,1,8) ,1102,"Beauty","North",1,265.71m,0.13m,"Card",2,3.7,1387.01),
                new Sale(10009,new DateOnly(2022,1,9) ,1614,"Clothing","South",1,224.65m,0.27m,"Card",2,1.4,983.97),
                new Sale(10010,new DateOnly(2022,1,10),1614,"Clothing","West",1,272.71m,0.06m,"Card",10,4.5,1025.39)
            };
            yield return new object[] {//Набор 1 (Весь диапазон дат)
                data,
                DateOnly.MinValue, //startDate, 
                DateOnly.MaxValue, //endDate,  
                new List<CustomerAnalyticsService.Customer>()
                    {
                        new CustomerAnalyticsService.Customer(1614,4.5),
                        new CustomerAnalyticsService.Customer(1102,3.7),
                        new CustomerAnalyticsService.Customer(1270,3.3),
                        new CustomerAnalyticsService.Customer(1700,1.8)
                    }//expectedOutput
            };
            yield return new object[] {//Набор 2 (ограничение по дате снизу и сверху)
                data,
                new DateOnly(2022, 1, 2), //startDate, 
                new DateOnly(2022, 1, 6), //endDate,  
                new List<CustomerAnalyticsService.Customer>()
                {
                    new CustomerAnalyticsService.Customer(1270,4.9),
                    new CustomerAnalyticsService.Customer(1102,2.5)
                }//expectedOutput
            };
            yield return new object[] {//Набор 3 (ограничение по дате сверху)
                data,
                DateOnly.MinValue, //startDate, 
                new DateOnly(2022, 1, 6), //endDate,  
                new List<CustomerAnalyticsService.Customer>()
                {
                    new CustomerAnalyticsService.Customer(1270,4.9),
                    new CustomerAnalyticsService.Customer(1102,2.5)
                }//expectedOutput
            };
            yield return new object[] {//Набор 4 (ограничение по дате снизу)
                data,
                new DateOnly(2022, 1, 7), //startDate, 
                DateOnly.MaxValue, //endDate,  
                new List<CustomerAnalyticsService.Customer>()
                {
                    new CustomerAnalyticsService.Customer(1614,4.5),
                    new CustomerAnalyticsService.Customer(1102,3.7),
                    new CustomerAnalyticsService.Customer(1700,1.8)
                }//expectedOutput
            };
        }

        [Theory]
        [MemberData(nameof(GetValidTestData))]
        public void ReturnCorrectOnValid(List<Sale> data, DateOnly startDate, DateOnly endDate, List<CustomerAnalyticsService.Customer> expectedOutput)
        {
            var mockStreamReader = new Mock<StreamReader>(Stream.Null);
            var mockReader = new Mock<ICsvReader>(mockStreamReader.Object);
            var mockSalesParser = new Mock<ISalesParser>(mockReader.Object);
            mockSalesParser.Setup(sp => sp.GetAll())
                .Returns(data);

            var actualOutput = new List<CustomerAnalyticsService.Customer>();//Для получения вывода

            var mockWriter = new Mock<IWriter>();
            mockWriter.Setup(w => w.AddReport(It.IsAny<CustomerAnalyticsService.Report>()))
                .Callback<CustomerAnalyticsService.Report>(report => actualOutput.AddRange(report.customers));//Полученный Report добавлять в actualOutput


            var service = new CustomerAnalyticsService(mockWriter.Object, mockSalesParser.Object, 5, IAnalyticsService.MODE.SYNCRONOUS, startDate, endDate);

            //Act
            service.Run();

            //Assert
            Assert.Equal(expectedOutput, actualOutput);
        }
/*        [Fact]
        public async Task ReturnCorrectOnValid_Async()
        {
            var mockStreamReader = new Mock<StreamReader>(Stream.Null);
            var mockReader = new Mock<ICsvReader>(mockStreamReader.Object);
            var mockSalesParser = new Mock<ISalesParser>(mockReader.Object);
            mockSalesParser.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                      .Returns(ProvideAsync_);

            var actualOutput = new List<CustomerAnalyticsService.Customer>();//Для получения вывода
            var expectedOutput = new List<CustomerAnalyticsService.Customer>()
                {
                    new CustomerAnalyticsService.Customer(1614,4.5),
                    new CustomerAnalyticsService.Customer(1102,3.7),
                    new CustomerAnalyticsService.Customer(1270,3.3),
                    new CustomerAnalyticsService.Customer(1700,1.8)
                };


            var mockWriter = new Mock<IWriter>();
            mockWriter.Setup(w => w.AddReport(It.IsAny<CustomerAnalyticsService.Report>()))
                .Callback<CustomerAnalyticsService.Report>(report => actualOutput.AddRange(report.customers));//Полученный Report добавлять в actualOutput


            var service = new CustomerAnalyticsService(mockWriter.Object, mockSalesParser.Object, 5, IAnalyticsService.MODE.SYNCRONOUS, DateOnly.MinValue, DateOnly.MaxValue);

            //Act
            var task = Task.Run(async () =>
            {
                return service.RunAsync();
            });

            //Assert
            var completed = await Task.WhenAny(task, Task.Delay(100));
            Assert.Equal(task, completed); // Задача должна завершиться успешно 

            Assert.Equal(expectedOutput, actualOutput);
        }*/
        [Theory]
        [MemberData(nameof(GetValidTestData))]
        public void ReturnCorrectOnValid_ParallelMode(List<Sale> data, DateOnly startDate, DateOnly endDate, List<CustomerAnalyticsService.Customer> expectedOutput)
        {
            var mockStreamReader = new Mock<StreamReader>(Stream.Null);
            var mockReader = new Mock<ICsvReader>(mockStreamReader.Object);
            var mockSalesParser = new Mock<ISalesParser>(mockReader.Object);
            mockSalesParser.Setup(sp => sp.GetAll())
                .Returns(data);

            var actualOutput = new List<CustomerAnalyticsService.Customer>();//Для получения вывода

            var mockWriter = new Mock<IWriter>();
            mockWriter.Setup(w => w.AddReport(It.IsAny<CustomerAnalyticsService.Report>()))
                .Callback<CustomerAnalyticsService.Report>(report => actualOutput.AddRange(report.customers));//Полученный Report добавлять в actualOutput


            var service = new CustomerAnalyticsService(mockWriter.Object, mockSalesParser.Object, 5, IAnalyticsService.MODE.PARALLEL, startDate, endDate);

            //Act
            service.Run();

            //Assert
            Assert.Equal(expectedOutput, actualOutput);
        }
        [Fact]
        public void BuildYourself_WithHelper()
        {
            // Arrange & Act
            var mockStreamReader = new Mock<StreamReader>(Stream.Null);
            var mockReader = new Mock<ICsvReader>(mockStreamReader.Object);
            var mockSalesParser = new Mock<ISalesParser>(mockReader.Object);

            var mockWriter = new Mock<IWriter>();

            var service = new CustomerAnalyticsService.Builder()
                .AddParser(mockSalesParser.Object)
                .AddWriter(mockWriter.Object)
                .AddTopCount(3)
                .Build();


            var topCount = ObjectAssert.GetPrivateField<ushort>(service, "topCount_");
            var parser = ObjectAssert.GetPrivateField<ISalesParser>(service, "parser_");
            var writer = ObjectAssert.GetPrivateField<IWriter>(service, "writer_");
            var startDate = ObjectAssert.GetPrivateField<DateOnly>(service, "startDate_");
            var endDate = ObjectAssert.GetPrivateField<DateOnly>(service, "endDate_");
            var mode = ObjectAssert.GetPrivateField<IAnalyticsService.MODE>(service, "mode_");

            // Assert
            Assert.Equal(3, topCount);
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

            var service = new CustomerAnalyticsService(mockWriter.Object, mockParser.Object, 5, IAnalyticsService.MODE.SYNCRONOUS, DateOnly.MinValue, DateOnly.MaxValue);

            // Acе & Assert
            Assert.True(await AsyncVerifier.IsMethodAsync(service.RunAsync));
        }

        //Данные для метода ProvideAsync_()
        private static List<Sale> data_ = new List<Sale>
            {
                new Sale(10001,new DateOnly(2022,1,1) ,1102,"Beauty","South",1,373.65m,0.28m,"Wallet",10,4.7,1883.2),
                new Sale(10002,new DateOnly(2022,1,2) ,1102,"Clothing","South",1,47.74m,0.09m,"Card",6,3.9,304.1 ),
                new Sale(10003,new DateOnly(2022,1,3) ,1102,"Beauty","East",1,311.28m,0.31m,"COD",6,2.5,644.35),
                new Sale(10004,new DateOnly(2022,1,4) ,1270,"Electronics","West",1,524.47m,0.02m,"Wallet",6,1.6,2569.9),
                new Sale(10005,new DateOnly(2022,1,5) ,1270,"Clothing","West",1,139.87m,0.33m,"Wallet",4,4.9,468.56),
                new Sale(10006,new DateOnly(2022,1,6) ,1270,"Electronics","West",2,264.44m,0.2m,"Card",3,3.3,423.1),
                new Sale(10007,new DateOnly(2022,1,7) ,1700,"Clothing","South",1,564.6m,0.23m,"Card",1,1.8,2608.45),
                new Sale(10008,new DateOnly(2022,1,8) ,1102,"Beauty","North",1,265.71m,0.13m,"Card",2,3.7,1387.01),
                new Sale(10009,new DateOnly(2022,1,9) ,1614,"Clothing","South",1,224.65m,0.27m,"Card",2,1.4,983.97),
                new Sale(10010,new DateOnly(2022,1,10),1614,"Clothing","West",1,272.71m,0.06m,"Card",10,4.5,1025.39)
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

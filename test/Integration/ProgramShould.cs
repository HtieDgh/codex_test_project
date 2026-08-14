using Castle.Core.Logging;
using codex.Models;
using codex.Services;
using codex.Shared;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using static codex.Program;

namespace test.Integration
{
    public class ProgramShould
    {
        [Fact]
        public void ResoleCorrectAllServices()
        {
            // Arrange
            var mockStreamReader = new Mock<StreamReader>(Stream.Null);
            var mockReader = new Mock<ICsvReader>(mockStreamReader.Object);
            var mockParser = new Mock<ISalesParser>(mockReader.Object);

            var mockWriter = new Mock<IWriter>();

            var services = new ServiceCollection();

            services.AddScoped<IAnalyticsService, SumAnalyticsService>(
                            provider => new SumAnalyticsService(mockWriter.Object, mockParser.Object, IAnalyticsService.MODE.SYNCRONOUS, DateOnly.MinValue, DateOnly.MaxValue)
                        );
            services.AddSingleton<ILogger, ConsoleLogger>();

            // Act & Assert
            var exception = Record.Exception(() =>
                AssertServiceCollectionIsValid(services));

            Assert.Null(exception);

        }

        private static void AssertServiceCollectionIsValid(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();

            foreach (var serviceDescriptor in services)
            {
                try
                {
                    // Проверяем, что сервис можно разрешить
                    var service = serviceProvider.GetService(serviceDescriptor.ServiceType);

                    // Для IEnumerable проверяем все зарегистрированные экземпляры
                    if (serviceDescriptor.ServiceType.IsGenericType &&
                        serviceDescriptor.ServiceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        var enumerable = service as System.Collections.IEnumerable;
                        if (enumerable != null)
                        {
                            foreach (var item in enumerable) { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Failed to resolve {serviceDescriptor.ServiceType.FullName}", ex);
                }
            }
        }
        /Данные для метода ProvideAsync_()
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

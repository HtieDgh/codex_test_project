using Castle.Core.Logging;
using codex.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace test.Unit
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
    }
}

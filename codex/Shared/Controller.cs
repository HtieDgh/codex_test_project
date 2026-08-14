using codex.Services;

namespace codex.Shared
{
    public class Controller
    {
        List<IAnalyticsService>? services_;
        public Controller()
        {
            services_ = null;
        }
        public Controller(List<IAnalyticsService> services )
        {
            addServices(services);
        }
        public void addServices(List<IAnalyticsService> services)
        {
            services_ = services;
        }
        public void addService(IAnalyticsService service)
        {
            if(services_ is null)
                services_ = new List<IAnalyticsService>();
            services_.Add(service);
        }
        public void Run() {
            if (services_ is null)
                return;
            foreach (var service in services_)
            {
                service.Run();
            }
        }
        public async Task RunAsync(CancellationToken cancellation=default) {
            if (services_ is null)
                return;

            Task.WaitAll(services_.Select(s => s.RunAsync(cancellation)));
        }
    }
}

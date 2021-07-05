using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FrenziedMarmot.DependencyInjection.Samples.AspNetCore.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, IGreetingService interfaceSvc, InjectedAsSelf implSvc,
            FactoryInjected factoryCreatedSvc)
        {
            _logger = logger;
            InterfaceSvc = interfaceSvc;
            ImplSvc = implSvc;
            FactoryInjectedSvc = factoryCreatedSvc;
        }

        public IGreetingService InterfaceSvc { get; }
        public InjectedAsSelf ImplSvc { get; }
        public FactoryInjected FactoryInjectedSvc { get; set; }

        public void OnGet()
        {
        }
    }
}
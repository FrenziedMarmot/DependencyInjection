using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace FrenziedMarmot.DependencyInjection.Samples.AspNetCore.Pages
{
    public class IndexModel : PageModel
    {
        public IndexModel(IGreetingService interfaceSvc, InjectedAsSelf implSvc,
            FactoryInjected factoryCreatedSvc, IOptions<InjectedOptions> opts)
        {
            InterfaceSvc = interfaceSvc;
            ImplSvc = implSvc;
            FactoryInjectedSvc = factoryCreatedSvc;
            Options = opts;
        }

        public IOptions<InjectedOptions> Options { get; }

        public IGreetingService InterfaceSvc { get; }
        public InjectedAsSelf ImplSvc { get; }
        public FactoryInjected FactoryInjectedSvc { get; set; }

        public void OnGet()
        {
            // no-op
        }
    }
}
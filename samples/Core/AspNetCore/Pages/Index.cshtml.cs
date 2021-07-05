using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrenziedMarmot.DependencyInjection.Samples.AspNetCore.Pages
{
    public class IndexModel : PageModel
    {
        public IndexModel(IGreetingService interfaceSvc, InjectedAsSelf implSvc,
            FactoryInjected factoryCreatedSvc)
        {
            InterfaceSvc = interfaceSvc;
            ImplSvc = implSvc;
            FactoryInjectedSvc = factoryCreatedSvc;
        }

        public IGreetingService InterfaceSvc { get; }
        public InjectedAsSelf ImplSvc { get; }
        public FactoryInjected FactoryInjectedSvc { get; set; }

        public void OnGet()
        {
            // no-op
        }
    }
}
using Omnicare_SureMedPlusRdlc.Models;
using Omnicare_SureMedPlusRdlc.Views;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;

namespace Omnicare_SureMedPlusRdlc
{
    [Module(ModuleName = "771929b1-ecfb-4af0-a84f-584da3109516", OnDemand = true)]
    public class ReportModule : IModule
    {
        private readonly IUnityContainer _container;
        private IRegionManager _regionManager;

        public ReportModule(IRegionManager region, IUnityContainer container) //, BCOAppContextProxy proxy)
        {
            _regionManager = region;
            _container = container;
        }

        public void Initialize()
        {
            _container.RegisterType<object, RDLCReportView>(ReportViewNames.ReportViewName);
        }
    }
}
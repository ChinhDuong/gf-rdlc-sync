using Mercury_v1_SureMedPlusRdlc.Models;
using Mercury_v1_SureMedPlusRdlc.Views;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;

namespace Mercury_v1_SureMedPlusRdlc
{
    [Module(ModuleName = "6b4ddde3-dd5a-44da-8f80-bb6064e0da29", OnDemand = true)]
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
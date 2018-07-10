using BJRX_v4_SureMedPlusRdlc.Models;
using BJRX_v4_SureMedPlusRdlc.Views;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;

namespace BJRX_v4_SureMedPlusRdlc
{
    [Module(ModuleName = "fa24beac-5d05-46aa-8f84-eb07f998ad7c", OnDemand = true)]
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
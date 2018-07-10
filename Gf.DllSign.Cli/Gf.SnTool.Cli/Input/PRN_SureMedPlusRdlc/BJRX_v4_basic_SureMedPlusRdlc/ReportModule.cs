using BJRX_v4_basic_SureMedPlusRdlc.Models;
using BJRX_v4_basic_SureMedPlusRdlc.Views;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;

namespace BJRX_v4_basic_SureMedPlusRdlc
{
    [Module(ModuleName = "d40d1003-a461-482e-bebf-cc4ab5080b77", OnDemand = true)]
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
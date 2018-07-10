using Omnicare_SureMedPlusRdlc.Models;
using Omnicare_SureMedPlusRdlc.Views;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;

namespace Omnicare_SureMedPlusRdlc
{
    [Module(ModuleName = "d4076d15-2d3e-4d3f-90cd-feb88c27d145", OnDemand = true)]
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
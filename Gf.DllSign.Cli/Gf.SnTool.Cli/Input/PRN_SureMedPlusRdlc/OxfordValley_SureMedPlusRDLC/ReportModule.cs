using OxfordValley_SureMedPlusRDLC.Models;
using OxfordValley_SureMedPlusRDLC.Views;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;

namespace OxfordValley_SureMedPlusRDLC
{
    [Module(ModuleName = "9b63cfe6-e85a-462c-8004-a75eb15ec9cc", OnDemand = true)]
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
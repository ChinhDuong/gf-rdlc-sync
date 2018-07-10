using System.Collections.Generic;

namespace Gf.SnTool.Cli
{
    public interface ISignUtil
    {        
        void AddKeyToProject(string keyPath, string projectPath, string xmlNamespace = "");
        void UpdateReferences(string projectPath, Dictionary<string, string> replacedDlls, string xmlNamespace = "");
        void ReplaceInterface(string xamlFile, string oldInterface, string newInterface);
        void Build(string msbuildPath, string solutionPath);

        void ValidateSolutionAndGetProjectName(string solutionPath, out string projectName);
        void UpdateVersion(string versionPath, string version);
        void ValidAssembly(string assemblyPath);
        //void UpdatePackages(string projectFile);
    }
}
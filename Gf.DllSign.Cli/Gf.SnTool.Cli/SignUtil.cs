using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using Serilog;
using System.Text.RegularExpressions;

namespace Gf.SnTool.Cli
{
    public class SignUtil : ISignUtil
    {    
        private const string REFERENCE_KEY = "Reference";
        private const string INCLUDE_KEY = "Include";
        private const string ITEM_GROUP_KEY = "ItemGroup";
        private const string HINTPATH_KEY = "HintPath";
        private const string PROJECT_NAMESPACE = "http://schemas.microsoft.com/developer/msbuild/2003";
        private const string PROPERTY_GROUP_KEY = "PropertyGroup";
        private const string CONDITION_KEY = "Condition";
        private const string ASSEMBLY_ORIGINATOR_KEY_FILE_KEY = "AssemblyOriginatorKeyFile";
        private const string SIGN_ASSEMBLY_KEY = "SignAssembly";

        private const string CONTENT_KEY = "Content";
        private const string PROJECT_KEY = "Project";
        private const string PUBLIC_KEY_TOKEN = "886d19fb6b8dfcb0";
        
        public virtual void UpdateReferences(string projectPath, Dictionary<string, string> newDlls, string xmlNamespace = "")
        {
            Log.Information("UpdateReferences: [projectPath: {0}, newDlls: {1}, xmlNamespace: {2}]", projectPath, newDlls, xmlNamespace);

            Dictionary<string, string> dllsWithFullPath = new Dictionary<string, string>();
            if (!File.Exists(projectPath))
            {
                throw new FileNotFoundException(string.Format("projectPath:{0} is not found", projectPath));
            }
            if (newDlls == null || newDlls.Count == 0)
            {
                throw new ArgumentException("There is no dll to replace");
            }
            foreach (var item in newDlls)
            {
                if (File.Exists(item.Value))
                {
                    string fullPath = Path.GetFullPath(item.Value);
                    dllsWithFullPath.Add(item.Key, fullPath);
                }
                else
                {
                    throw new ArgumentException("{0} is not exist", item.Value);
                }
            }
            XNamespace ns = string.IsNullOrEmpty(xmlNamespace)
                ? PROJECT_NAMESPACE
                : xmlNamespace;

            XElement projectXml = XElement.Load(projectPath);

            string[] replaceDllNames = dllsWithFullPath.Keys.ToArray();
            RemoveLibs(ns, projectXml);
            
            List<XElement> parentNodes = new List<XElement>();
            foreach (string referenceToReplace in replaceDllNames)
            {
                var references =
                    (from elt in projectXml.Descendants(ns + REFERENCE_KEY)
                     where elt.HasAttributes
                         && ((string)elt.Attribute(INCLUDE_KEY)).Contains(referenceToReplace)
                     select elt).ToList();
                if (references != null && references.Count > 0)
                {
                    foreach (var refer in references)
                    {
                        if (!parentNodes.Contains(refer.Parent))
                        {
                            parentNodes.Add(refer.Parent);
                        }
                        var hintPath = refer.Element(ns + HINTPATH_KEY);
                        string newDll = dllsWithFullPath[referenceToReplace];
                        hintPath.SetValue(newDll);

                    }
                }
                else
                {
                    if (parentNodes.Count > 0)
                    {
                        foreach (var parent in parentNodes)
                        {
                            XElement newReference = new XElement(ns + REFERENCE_KEY);
                            XAttribute include = new XAttribute(INCLUDE_KEY, referenceToReplace);
                            newReference.Add(include);
                            XElement hintPath = new XElement(ns + HINTPATH_KEY, dllsWithFullPath[referenceToReplace]);
                            newReference.Add(hintPath);
                            parent.Add(newReference);
                        }
                    }

                }

            }

            projectXml.Save(projectPath);

        }

        private static void RemoveLibs(XNamespace ns, XElement projectXml)
        {
            var itemGroupIncludeLibs =
                            (from ele in projectXml.Descendants(ns + CONTENT_KEY)
                             where ele.HasAttributes
                                && ((string)ele.Attribute(INCLUDE_KEY)).Contains("Libs")
                             select ele
                             );
            if(itemGroupIncludeLibs != null)
            {
                while(itemGroupIncludeLibs.Any())
                {
                    var item = itemGroupIncludeLibs.FirstOrDefault();
                    item.Remove();
                }
            }
                    
        }

        //private void ReplaceTextInFile
        public virtual void ReplaceInterface(string xamlFile, string oldInterface, string newInterface)
        {
            Log.Information("ReplaceInterface: [xamlFile: {0}, oldInterface: {1}, newInterface: {2}]", xamlFile, oldInterface, newInterface);

            if (!File.Exists(xamlFile))
            {
                throw new FileNotFoundException(string.Format("xamlFile:{0} is not found", xamlFile));
            }
            if (string.IsNullOrEmpty(oldInterface) || string.IsNullOrEmpty(newInterface))
            {
                throw new ArgumentException("There is no interface to replace");
            }

            string fileContent = File.ReadAllText(xamlFile);
            var matches = Regex.Matches(fileContent, oldInterface);
            if (matches.Count == 0)
            {
                throw new InvalidDataException("Old interface is not found");
            }
            string newContent = fileContent.Replace(oldInterface, newInterface);

            File.WriteAllText(xamlFile, newContent);
        }

        public virtual void AddKeyToProject(string keyPath, string projectPath, string xmlNamespace = "")
        {
            Log.Information("AddKeyToProject: [keyPath: {0}, projectPath: {1}, xmlNamespace: {2}]", keyPath, projectPath, xmlNamespace);
            if (!File.Exists(keyPath))
            {
                throw new FileNotFoundException(string.Format("keyPath:{0} is not found", keyPath));
            }
            else
            {
                string fullPath = Path.GetFullPath(keyPath);
                keyPath = fullPath;
            }

            if (!File.Exists(projectPath))
            {
                throw new FileNotFoundException(string.Format("projectPath:{0} is not found", projectPath));
            }

            XNamespace ns = string.IsNullOrEmpty(xmlNamespace)
                ? PROJECT_NAMESPACE
                : xmlNamespace;

            XElement projectRoot = XElement.Load(projectPath);

            AddKeyPathToCsproj(keyPath, ns, projectRoot);

            EnableSignDll(ns, projectRoot);

            projectRoot.Save(projectPath);
        }

        private void EnableSignDll(XNamespace ns, XElement projectRoot)
        {
            Log.Information("EnableSignDll .....");
            var sgList = projectRoot.Descendants(ns + SIGN_ASSEMBLY_KEY);
            
            if (sgList == null || !sgList.Any())
            {
                XElement pg = new XElement(ns + PROPERTY_GROUP_KEY);
                XElement sg = new XElement(ns + SIGN_ASSEMBLY_KEY, true);

                pg.Add(sg);
                projectRoot.Add(pg);
            }
            else
            {
                foreach(XElement sg in sgList)
                {
                    sg.SetValue(true);
                }
                
            }

        }

        private void AddKeyPathToCsproj(string keyPath, XNamespace ns, XElement projectRoot)
        {
            Log.Information("AddKeyPathToCsproj .....");
            XElement aokf = projectRoot.Descendants(ns + ASSEMBLY_ORIGINATOR_KEY_FILE_KEY).FirstOrDefault();
            if (aokf == null)
            {
                XElement pg = new XElement(ns + PROPERTY_GROUP_KEY);
                aokf = new XElement(ns + ASSEMBLY_ORIGINATOR_KEY_FILE_KEY, keyPath);
                pg.Add(aokf);
                projectRoot.Add(pg);
            }
            else
            {
                aokf.SetValue(keyPath);
            }

        }

        public virtual void Build(string msbuildPath, string solutionPath)
        {
            Log.Information("Build: [msbuildPath: {0}, solutionPath: {1}]", msbuildPath, solutionPath);
            if (!File.Exists(msbuildPath))
            {
                throw new FileNotFoundException(string.Format("msbuildPath:{0} is not found", msbuildPath));
            }

            if (!File.Exists(solutionPath))
            {
                throw new FileNotFoundException(string.Format("solutionPath:{0} is not found", solutionPath));
            }

            string cmd = string.Format(@"""{0}"" /t:Clean,Build /p:Configuration=Release ""{1}""", msbuildPath, solutionPath);

            string log = "";
            int exitCode = CommonHelper.ExecuteCommand(cmd, out log);
            Log.Information("Build log: \n{0}", log);
            if (exitCode != 0)
            {
                throw new SystemException("Build is failed");
            }

        }

        public virtual void ValidateSolutionAndGetProjectName(string solutionPath, out string projectName)
        {
            Log.Information("ValidateSolutionAndGetProjectName: [solutionPath: {0}]", solutionPath);
            if (!File.Exists(solutionPath))
            {
                throw new FileNotFoundException(string.Format("solutionPath:{0} is not found", solutionPath));
            }

            string content = File.ReadAllText(solutionPath);
            string pattern = @"(?<=Project\().*(?:csproj)";
            MatchCollection projectMatch = Regex.Matches(content, pattern);
            if (projectMatch.Count == 0)
            {
                throw new MissingFieldException("Solution have no project");
            }
            else if (projectMatch.Count > 1)
            {
                throw new MissingFieldException("Solution have too many projects");
            }
            
            string projectValue = projectMatch[0].Value;
            string[] projectInfos = projectValue.Split(",");
            string projectPath = projectInfos[1];
            const string patternForProjectWithoutFolder = @"(?<=[""]).*csproj";
            const string patternForProjectWithFolder = @"(?<=[\\]).*csproj";

            string pattern2 = patternForProjectWithFolder;
            if (!projectPath.Contains("\\"))
            {
                pattern2 = patternForProjectWithoutFolder;
            }
            Match projectNameMatch = Regex.Match(projectPath, pattern2);
            projectName = projectNameMatch.Success ? projectNameMatch.Value : throw new MissingFieldException("Not found *.csproj");

            Log.Information("ValidateSolutionAndGetProjectName: out [projectName: {0}]", projectName);
        }

        public void UpdateVersion(string versionPath, string version)
        {
            Log.Information("UpdateVersion: [versionPath: {0}, version:{1}]", versionPath, version);
            if (!File.Exists(versionPath))
            {
                throw new FileNotFoundException(string.Format("versionPath:{0} is not found", versionPath));
            }

            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentNullException("version");
            }

            File.WriteAllText(versionPath, version);
        }

        public void ValidAssembly(string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
            {
                throw new FileNotFoundException(string.Format("assemblyPath:{0} is not found", assemblyPath));
            }
            var assemblyInfo = System.Reflection.Assembly.LoadFile(assemblyPath);
            string fullName = assemblyInfo.FullName;
            string patternGetPublicKey = @"(?<=PublicKeyToken=)[a-z0-9]*$";
            Match m = Regex.Match(fullName, patternGetPublicKey, RegexOptions.IgnoreCase);
            if (m.Success
                && m.Value == PUBLIC_KEY_TOKEN
                )
            {
                Log.Information("ValidAssembly: [{0} is valid. fullName: {1}]", assemblyPath,fullName);
            }
            else
            {
                throw new BadImageFormatException(string.Format("ValidAssembly: [{0} is not invalid. fullName: {1}]", assemblyPath, fullName));
                
            }
        }

        //public void UpdatePackages(string projectFile)
        //{
            
        //}
    }
}

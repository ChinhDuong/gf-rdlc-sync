using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Gf.SnTool.Cli
{
    public static class Program
    {
        public static int Main(string[] args)
        {

            InitializeLog();
            Log.Information("-------------------------------------------------------------------------");
            Log.Information("Starting {now}", DateTime.Now.ToString("yyyy-MM-dd H:mm:ss zzz"));

            var app = new CommandLineApplication();
            app.Name = "gf.sntool.cli";
            app.HelpOption();
            var optionFileZip = app.Option("-f|--fileZip <SolutionFileZip>", "The Solution File Zip", CommandOptionType.SingleValue);
            var optionOutputFolder = app.Option("-o|--output <Output>", "Assembly output is copied to the output folder after successful build", CommandOptionType.SingleValue);
            var optionVersion = app.Option("-v|--version <Version>", "Use the version to build", CommandOptionType.SingleValue);
            app.OnExecute(() =>
            {
                string zipPath = optionFileZip.HasValue() ? optionFileZip.Value() : "";
                string output = optionOutputFolder.HasValue() ? optionOutputFolder.Value() : "";
                string version = optionVersion.HasValue() ? optionVersion.Value() : "";
                return Execute(zipPath, output, version);
            });

            return app.Execute(args);
        }

        static int Execute(string zipPath, string outputFolder, string newVersion)
        {
            int exitcode = 0;
            if (string.IsNullOrEmpty(zipPath))
            {
                throw new ArgumentNullException("zipPath", "Missing argument. Use -h to show help information");
            }

            IConfigurationRoot configuration = LoadSettings();
            try
            {
                string keyPath = configuration["snKey"];
                string msbuildPath = configuration["msbuild"];
                string dllName = Path.GetFileNameWithoutExtension(zipPath);
                string oldInterface = "IDataReportService";
                string newInterface = "BCO.Infrastructure.Services.IBcoReportService";
                string nameFolder = Path.GetRandomFileName();
                string extractPath = string.Format(@"Tmp\{0}", nameFolder);
                bool isKeep = configuration["IsKeep"] == "true";

                Dictionary<string, string> replacedDlls = new Dictionary<string, string>
                {
                    {"BCO.DataReportModule",@"dlls\BCO.DataReportModule.dll" },
                    {"CommonLib",@"dlls\CommonLib.dll" },
                    {"BCO.Infrastructure",@"dlls\BCO.Infrastructure.dll" },
                    {"GenCode128",@"dlls\GenCode128.dll" }
                };

                Log.Information("Signing {file}", dllName);

                IExtractor extractor = new ZipExtractor();
                extractor.Extract(zipPath, extractPath);

                ISignUtil signUtil = new SignUtil();

                string solutionFile = GetSolutionPath(extractPath);
                string projectName = string.Empty;

                signUtil.ValidateSolutionAndGetProjectName(solutionFile, out projectName);

                string projectFile = GetProjectPath(extractPath, projectName);
                signUtil.UpdateReferences(projectFile, replacedDlls);
                signUtil.AddKeyToProject(keyPath, projectFile);

                string projectFolder = Path.GetDirectoryName(projectFile);
                string xamlFile = GetXamlFile(projectFolder);
                signUtil.ReplaceInterface(xamlFile, oldInterface, newInterface);

                string[] versionFiles = GetVersionFiles(extractPath);
                if(versionFiles == null || versionFiles.Length == 0)
                {
                    Log.Information("Version is not found");
                }
                else
                {
                    foreach (string versionFile in versionFiles)
                    {
                        if (File.Exists(versionFile))
                        {
                            string currentVersion = File.ReadAllText(versionFile);

                            Log.Information("Previous version: {0}", currentVersion);

                            if (!string.IsNullOrEmpty(newVersion))
                            {
                                signUtil.UpdateVersion(versionFile, newVersion);
                            }
                        }
                    }
                }
                signUtil.Build(msbuildPath, solutionFile);
                string assemblyFile = GetDllFile(extractPath, string.Format("{0}*.dll", Path.GetFileNameWithoutExtension(projectFile)));
                string assemblyFileFullPath = Path.GetFullPath(assemblyFile);
                signUtil.ValidAssembly(assemblyFileFullPath);
                Copy(outputFolder, extractPath, projectFile);

                if (!isKeep)
                {
                    try
                    {
                        Directory.Delete(extractPath, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Information(string.Format("Cleaning: failed ....{0}", ex.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception:");
                exitcode = 1;
            }

            Log.Information("End {now}", DateTime.Now.ToString("yyyy-MM-dd H:mm:ss zzz"));
            return exitcode;
        }

        static IConfigurationRoot LoadSettings()
        {
            Log.Information("Load settings");
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();
            return configuration;

        }

        static void Copy(string outputFolder, string extractPath, string projectPath)
        {
            if (!string.IsNullOrEmpty(outputFolder))
            {

                if (!Directory.Exists(outputFolder))
                {
                    throw new DirectoryNotFoundException(outputFolder);
                }
                string fileSource = GetDllFile(extractPath, string.Format("{0}*.dll", Path.GetFileNameWithoutExtension(projectPath)));
                string fileTarget = Path.Combine(outputFolder, Path.GetFileName(fileSource));
                if (!string.IsNullOrEmpty(fileSource))
                {
                    string fullPath = Path.GetFullPath(fileSource);
                    Log.Information("Copying {0} to {1}", fullPath, fileTarget);
                    File.Copy(fullPath, fileTarget, true);
                }

            }
            else
            {
                Log.Information("Copy: ignore");
            }
        }

        static void InitializeLog()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(@"logs\sntool.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

        }

        static string[] GetFileWithPattern(string folderPath, string pattern)
        {
            string[] path;
            if (Directory.Exists(folderPath))
            {
                path = Directory.GetFiles(folderPath, pattern, SearchOption.AllDirectories);
            }
            else
            {
                throw new DirectoryNotFoundException(folderPath);
            }
            return path;
        }

        static string GetProjectPath(string folderPath, string pattern)
        {
            return GetFileWithPattern(folderPath, pattern).FirstOrDefault();
        }

        static string GetSolutionPath(string folderPath)
        {
            return GetFileWithPattern(folderPath, "*.sln").FirstOrDefault();
        }

        static string GetXamlFile(string folderPath)
        {
            return GetFileWithPattern(folderPath, "*.xaml.cs").FirstOrDefault();
        }

        static string GetDllFile(string folderPath, string pattern)
        {
            string path = "";
            if (Directory.Exists(folderPath))
            {
                path = Directory.GetFiles(folderPath, pattern, SearchOption.AllDirectories).FirstOrDefault(x => x.Contains("Release"));
            }
            else
            {
                throw new DirectoryNotFoundException(folderPath);
            }
            return path;
        }

        static string GetVersionFile(string folderPath)
        {
            return GetFileWithPattern(folderPath, "Version.txt").FirstOrDefault();
        }

        static string[] GetVersionFiles(string folderPath)
        {
            return GetFileWithPattern(folderPath, "Version.txt");
        }

    }
}

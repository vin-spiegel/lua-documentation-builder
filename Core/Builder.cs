using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using LDocBuilder.Utility;

namespace LDocBuilder.Core
{
    public class LBuilder
    {
        public string path;
        public string outPath;

        private Assembly _assembly;
        
        public LBuilder(string path, string outPath="")
        {
            this.path = path;
            this.outPath = outPath;
        }

        public Assembly LoadAssembly(string fileName)
        {
            var s = Path.Combine(outPath, fileName);
            if (File.Exists(s)) 
                return _assembly = Assembly.LoadFrom(s);
            Logger.Error($"error .dll file not found {s}");
            return null;
        }

        public void DocumentationBuild()
        {
            if (_assembly == null)
                return;
            var types = _assembly.GetTypes();
            Console.WriteLine(types.Length);
        }

        private async void DeleteCache() => await _DeleteCache();

        private async Task _DeleteCache() =>
            await Task.Run(() =>
            {
                Directory.Delete(outPath, true);
            });

        public string RunMsBuild()
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            DeleteCache();
            new DirectoryInfo(outPath).Create();
            
            var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                // Arguments = $@"msbuild {path} /p:OutputType=Library /p:OutDir={outPath} /p:DocumentationFile=""{fileName}.xml"" /p:UseResultsCache=""false""",
                // Arguments = $@"C:\""Program Files""\""Microsoft Visual Studio""\2022\Professional\MSBuild\Current\Bin\MSBuild {path} /p:OutputType=Library /p:OutDir={outPath} /p:DocumentationFile=""{fileName}.xml"" /p:UseResultsCache=""false""",
                Arguments = $@"msbuild {path} /t:creator /p:OutDir={outPath} /p:DocumentationFile=""{fileName}.xml"" /p:UseResultsCache=""false""",
                // Arguments = $@"{path} /t:creator /p:OutDir={outPath} /p:DocumentationFile=""{fileName}.xml"" /p:UseResultsCache=""false""",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            //* Set your output and error (asynchronous) handlers
            process.OutputDataReceived += OutputHandler;
            process.ErrorDataReceived += ErrorHandler;
            //* Start process and handlers
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            Logger.Success("  .dll Build Success.");
            return outPath;
        }
        
        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (string.IsNullOrEmpty(outLine.Data))
                return;
            
            if (outLine.Data.Contains("warning"))
            {
                Logger.Warn(outLine.Data);
                return;
            }

            if (outLine.Data.Contains("error"))
            {
                Logger.Error(outLine.Data);
                return;
            }

            Logger.Success(outLine.Data);
        }
        
        private static void ErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Logger.Error(outLine.Data);
        }
    }
}
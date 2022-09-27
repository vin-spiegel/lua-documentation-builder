using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Xml;

namespace LDocBuilder
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            new Program();
        }

        /// <summary>
        /// Dll 파일 및 Xml을 빌딩합니다.
        /// </summary>
        /// <param name="path"></param>
        public static void BuildDllAndXml(string path = "")
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            //* Create your Process
            var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet.exe",
                Arguments = $@"msbuild {path} /p:OutputType=Library /p:DocumentationFile=""{fileName}.xml"" /p:UseResultsCache=""false""",
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
        }

        public Program()
        {
            BuildDllAndXml(@"C:\Projects\LDocBuilder\Target/Target.csproj");
            var assembly = Assembly.LoadFrom(@"C:\Projects\LDocBuilder\Target\bin\Debug\Target.dll");
            //
            var types = assembly.GetTypes();
            //
            foreach (var type in types)
            {
                // Console.WriteLine(type.GetSummary());
                // Console.WriteLine(type.Name);
                var methods = type.GetMethods();
                foreach (var methodInfo in methods)
                {
                    Console.WriteLine(methodInfo.GetSummary());
                }
            }
        }
        
        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //* Do your stuff with the output (write to console/log/StringBuilder)
            Console.WriteLine(outLine.Data);
        }
        
        private static void ErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //* Do your stuff with the output (write to console/log/StringBuilder)
            Console.WriteLine(outLine.Data);
        }

    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LDocBuilder
{
        // var regex = new Regex("network.T[A-Z]");
    internal class Program
    {
        public static void Main(string[] args)
        {
            new Program();
        }

        public Program()
        {
            var assembly = Assembly.LoadFrom(@"C:\Projects\nekorpg-new\Creator\bin\Debug\Creator.exe");
            var list = assembly.GetTypesContainsAttribute("MoonSharp");
            foreach (var type in list)
            {
                var doc = type.GetLuaDocumentation();
                File.WriteAllText($@"d:\LDocs\{type.Name}.lua", $"{doc}", Encoding.UTF8);
            }

            var requires = assembly.GetRequireTypes("MoonSharp");
            foreach (var require in requires)
            {
                // var doc = require.GetLuaDocumentation();
                File.WriteAllText($@"d:\LDocs\{require.Name}.lua", $"---@class {require}", Encoding.UTF8);
            }
        }
    }

    public static class Ext
    {
        
    }
}
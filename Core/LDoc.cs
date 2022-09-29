using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace LDocBuilder.Core
{
    public class LDoc 
    {
        public Type type;
        public StringBuilder script = new StringBuilder();
        public string outPath = @"d:\test.txt";

        public LDoc(Type type)
        {
            this.type = type;
            ToString();
            // ToFile();
        }

        private void ToStringField(FieldInfo[] fields)
        {
            foreach (var fi in fields)
            {
                Console.WriteLine(fi);
            }
        }

        public string ToString()
        {
            script.Append("---@meta\n");
            script.Append($"---{type.GetSummary()}\n");
            script.Append($"---@class {type.Name}\n");
            // Console.WriteLine("---@meta");
            // Console.WriteLine($"---{type.GetSummary()}");
            // Console.WriteLine($"---@class {type.Name}");

            // Console.WriteLine(res);
            
            // var fields = type.GetFields();
            // if (fields.Length > 0)
            // {
            //     ToStringField(fields);
            // }
            Console.WriteLine(script.ToString());
            return script.ToString();
        }

        public void ToFile()
        {
            var fi = new FileInfo(outPath);
            using (fi.Create())
            {
                File.WriteAllText(outPath, ToString(), Encoding.UTF8);
            }
        }
    }
}
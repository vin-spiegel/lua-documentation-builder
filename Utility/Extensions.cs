﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

/// <summary>
/// Utility class to provide documentation for various types where available with the assembly
/// </summary>
public static class DocumentationExtensions
{
    /// <summary>
    /// Provides the documentation comments for a specific type
    /// </summary>
    /// <param name="type">Type to find the documentation for</param>
    /// <returns>The XML fragment that describes the type</returns>
    public static XmlElement GetDocumentation(this Type type)
    {
        // Prefix in type names is T
        return XmlFromName(type, 'T', "");
    }
    
    /// <summary>
    /// Provides the documentation comments for a specific method
    /// </summary>
    /// <param name="methodInfo">The MethodInfo (reflection data ) of the member to find documentation for</param>
    /// <returns>The XML fragment describing the method</returns>
    public static XmlElement GetDocumentation(this MethodInfo methodInfo)
    {
        // Calculate the parameter string as this is in the member name in the XML
        var parametersString = "";
        foreach (var parameterInfo in methodInfo.GetParameters())
        {
            if (parametersString.Length > 0)
            {
                parametersString += ",";
            }

            parametersString += parameterInfo.ParameterType.FullName;
        }

        //AL: 15.04.2008 ==> BUG-FIX remove “()” if parametersString is empty
        if (parametersString.Length > 0)
            return XmlFromName(methodInfo.DeclaringType, 'M', methodInfo.Name + "(" + parametersString + ")");
        else
            return XmlFromName(methodInfo.DeclaringType, 'M', methodInfo.Name);
    }

    /// <summary>
    /// Provides the documentation comments for a specific member
    /// </summary>
    /// <param name="memberInfo">The MemberInfo (reflection data) or the member to find documentation for</param>
    /// <returns>The XML fragment describing the member</returns>
    public static XmlElement GetDocumentation(this MemberInfo memberInfo)
    {
        // First character [0] of member type is prefix character in the name in the XML
        return XmlFromName(memberInfo.DeclaringType, memberInfo.MemberType.ToString()[0], memberInfo.Name);
    }
    
    /// <summary>
    /// Gets the summary portion of a type's documenation or returns an empty string if not available
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetSummary(this Type type)
    {
        var element = type.GetDocumentation();
        var summaryElm = element?.SelectSingleNode("summary");
        if (summaryElm == null) 
            return "";
        
        return Regex.Replace(summaryElm.InnerText.Trim(), "[\n][\\s]+", "---");
    }
    
    /// <summary>
    /// Returns the Xml documenation summary comment for this member
    /// </summary>
    /// <param name="memberInfo"></param>
    /// <returns></returns>
    public static string GetSummary(this MemberInfo memberInfo)
    {
        var element = memberInfo.GetDocumentation();
        var summaryElm = element?.SelectSingleNode("summary");
        if (summaryElm == null) return "";
        return Regex.Replace(summaryElm.InnerText.Trim(), "[\n][\\s]+", "---");
    }

    private static readonly string[] ExcludedMethods = 
    {
        "GetHashCode", "GetType", "ToString", "Equals"
    };
    
    public static string GetSummary(this MethodInfo methodInfo)
    {
        if (ExcludedMethods.Contains(methodInfo.Name))
            return "";
        
        var element = methodInfo.GetDocumentation();
        var summaryElm = element?.SelectSingleNode("summary");
        if (summaryElm == null) return "";
        return summaryElm.InnerText.Trim();
    }
    
    /// <summary>
    /// Returns the Xml documentation summary coment for this parameter
    /// </summary>
    /// <param name="pi"></param>
    /// <returns></returns>
    public static string GetSummary(this ParameterInfo pi)
    {
        var xml = (pi.Member as MethodInfo).GetDocumentation();
        foreach (XmlElement elemental in xml.GetElementsByTagName("param"))
        {
            if (elemental.Attributes?["name"].Value == pi.Name)
                return elemental.InnerText.Trim();
        }

        return "";
    }
    
    public static bool IsExcluded(this MethodInfo memberInfo)
    {
        var element = memberInfo.GetDocumentation();
        var res = element?.SelectSingleNode("exclude");
        return res == null;
    }
    
    public static bool IsExcluded(this Type type)
    {
        var element = type.GetDocumentation();
        var res = element?.SelectSingleNode("exclude");
        return res != null;
    }
    
    public static bool IsExcluded(this PropertyInfo type)
    {
        var element = type.GetDocumentation();
        var res = element?.SelectSingleNode("exclude");
        return res != null;
    }

    /// <summary>
    /// Obtains the XML Element that describes a reflection element by searching the 
    /// members for a member that has a name that describes the element.
    /// </summary>
    /// <param name="type">The type or parent type, used to fetch the assembly</param>
    /// <param name="prefix">The prefix as seen in the name attribute in the documentation XML</param>
    /// <param name="name">Where relevant, the full name qualifier for the element</param>
    /// <returns>The member that has a name that describes the specified reflection element</returns>
    private static XmlElement XmlFromName(this Type type, char prefix, string name)
    {
        string fullName;

        if (string.IsNullOrEmpty(name))
            fullName = prefix + ":" + type.FullName;
        else
            fullName = prefix + ":" + type.FullName + "." + name;

        var xmlDocument = XmlFromAssembly(type.Assembly);

        var matchedElement = xmlDocument["doc"]["members"].SelectSingleNode("member[@name='" + fullName + "']") as XmlElement;

        return matchedElement;
    }

    /// <summary>
    /// A cache used to remember Xml documentation for assemblies
    /// </summary>
    private static readonly Dictionary<Assembly, XmlDocument> Cache = new Dictionary<Assembly, XmlDocument>();

    /// <summary>
    /// A cache used to store failure exceptions for assembly lookups
    /// </summary>
    private static readonly Dictionary<Assembly, Exception> FailCache = new Dictionary<Assembly, Exception>();

    /// <summary>
    /// Obtains the documentation file for the specified assembly
    /// </summary>
    /// <param name="assembly">The assembly to find the XML document for</param>
    /// <returns>The XML document</returns>
    /// <remarks>This version uses a cache to preserve the assemblies, so that 
    /// the XML file is not loaded and parsed on every single lookup</remarks>
    public static XmlDocument XmlFromAssembly(this Assembly assembly)
    {
        if (FailCache.ContainsKey(assembly))
        {
            throw FailCache[assembly];
        }

        try
        {
            if (!Cache.ContainsKey(assembly))
            {
                // load the docuemnt into the cache
                Cache[assembly] = XmlFromAssemblyNonCached(assembly);
            }

            return Cache[assembly];
        }
        catch (Exception exception)
        {
            FailCache[assembly] = exception;
            throw;
        }
    }

    /// <summary>
    /// Loads and parses the documentation file for the specified assembly
    /// </summary>
    /// <param name="assembly">The assembly to find the XML document for</param>
    /// <returns>The XML document</returns>
    private static XmlDocument XmlFromAssemblyNonCached(Assembly assembly)
    {
        var assemblyFilename = assembly.Location;
   
        if (!string.IsNullOrEmpty(assemblyFilename))
        {
            StreamReader streamReader;

            try
            {
                streamReader = new StreamReader(Path.ChangeExtension(assemblyFilename, ".xml"));
            }
            catch (FileNotFoundException exception)
            {
                throw new Exception("XML documentation not present (make sure it is turned on in project properties when building)", exception);
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(streamReader);
            return xmlDocument;
        }
        else
        {
            throw new Exception("Could not ascertain assembly filename", null);
        }
    }
}

public static class ReflectionExtensions
{
    public static MethodInfo[] GetMethodsEx(this Type type)
    {
        var list = new List<MethodInfo>();
        foreach (var mi in type.GetMethods())
        {
            if (mi.Name.StartsWith("get_") || mi.Name.StartsWith("set_"))
                continue;
            if (mi.Name == "Equals" || mi.Name == "GetHashCode" || mi.Name == "GetType" || mi.Name == "ToString")
                continue;
            list.Add(mi);
        }
        return list.ToArray();
    }

    public static string GetLuaDocumentation(this Type type)
    {
        var builder = new StringBuilder();
        builder.Append("---@meta\n");
        builder.Append($"---{type.GetSummary()}\n");
        
        var baseType = type.BaseType != null ? " : " + type.BaseType.Name : "";
        builder.Append($"---@class {type.ToString()}{baseType}\n");

        // Register Fields
        foreach (var prop in type.GetProperties())
        {
            if (prop.IsExcluded())
                continue;
            if (prop.GetSummary() != "")
                builder.Append($"---{prop.GetSummary()}\n");
            var line = $"---@field {prop.Name} {prop.GetLuaType()}\n";
            builder.Append(line);
            // Console.WriteLine($"---@field {prop.Name} {prop.GetLuaType()}");
        }
        
        var className = type.Name.StartsWith("Script") ? type.Name.Replace("Script", "") : type.Name;
        builder.Append($"local {className} = {{}}\n");

        return builder.ToString();
    }

    private static readonly Dictionary<string, string> LuaTypes = new Dictionary<string, string>()
    {
        {"System.Int32","integer"},
        {"System.Int64","integer"},
        {"System.Single","number"},
        {"MoonSharp.Interpreter.Table","table"},
        {"System.Boolean","boolean"},
        {"System.String","string"},
    };

    public static string GetLuaType(this PropertyInfo mi)
    {
        var name = mi.PropertyType.ToString();
        foreach (var pair in LuaTypes.Where(pair => name.Contains(pair.Key)))
        {
            return name.Replace(pair.Key, pair.Value);
        }
        return name;
    }

    public static Type[] GetTypesContainsAttribute(this Assembly assembly, string attribute)
    {
        var list = new List<Type>();
        foreach (var type in assembly.GetTypes())
        {
            if (!type.GetCustomAttributes().ToList().Exists(x => x.ToString().Contains(attribute))) 
                continue;
            if (type.IsExcluded())
                continue;
            list.Add(type);
        }
        return list.ToArray();
    }
}
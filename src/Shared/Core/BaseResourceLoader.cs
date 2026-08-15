using System;
using System.Collections.Generic;
using System.Reflection;

namespace Jamesnet.Foundation;

public abstract class BaseResourceLoader<TItem, TResult>
{
    protected abstract string AssemblyName { get; }
    protected abstract string ResourcePath { get; }

    public TResult LoadAndOrganize()
    {
        Assembly assembly = Assembly.Load(AssemblyName);
        YamlData rawData = LoadYamlData(assembly, ResourcePath);
        IEnumerable<TItem> items = ConvertToItems(rawData);
        return OrganizeItems(items);
    }

    protected abstract IEnumerable<TItem> ConvertToItems(YamlData rawData);
    protected abstract TResult OrganizeItems(IEnumerable<TItem> items);

    private YamlData LoadYamlData(Assembly assembly, string resourcePath)
    {
        YamlData yamlData = new();
        object result = YamlConverter.ParseResource(assembly, resourcePath);

        if (result is not IEnumerable<object> data)
        {
            throw new InvalidOperationException("YamlConverter.ParseResource did not return an IEnumerable<object>");
        }

        foreach (object item in data)
        {
            if (item is IDictionary<string, string> dict)
            {
                yamlData.Add(new(dict));
            }
        }

        return yamlData;
    }
}
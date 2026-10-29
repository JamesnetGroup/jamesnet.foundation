using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Jamesnet.Foundation;

/// <summary>
/// 어셈블리에 포함된 YAML 리소스를 읽어 항목으로 변환하고 정리하는 로더의 기반 클래스.
/// </summary>
public abstract class BaseResourceLoader<TItem, TResult>
{
    protected abstract string AssemblyName { get; }
    protected abstract string ResourcePath { get; }

    public TResult LoadAndOrganize()
    {
        var assembly = Assembly.Load(AssemblyName);
        var rawData = new YamlData(YamlConverter.ParseResource(assembly, ResourcePath)
            .Select(item => new YamlItem(item.ToDictionary(p => p.Key, p => p.Value))));

        return OrganizeItems(ConvertToItems(rawData));
    }

    protected abstract IEnumerable<TItem> ConvertToItems(YamlData rawData);
    protected abstract TResult OrganizeItems(IEnumerable<TItem> items);
}

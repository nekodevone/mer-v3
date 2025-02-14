using System.Reflection;
using YamlDotNet.Serialization;

namespace ProjectMER.Features.Extensions;

public static class ReflectionExtensions
{
	public static IEnumerable<string> GetColoredProperties(this List<PropertyInfo> properties, object instance)
	{
		foreach (PropertyInfo property in properties)
		{
			if (Attribute.IsDefined(property, typeof(YamlIgnoreAttribute)))
				continue;

			if (property.PropertyType == typeof(bool))
				yield return $"{property.Name}: {((bool)property.GetValue(instance) ? "<color=green><b>TRUE</b></color>" : "<color=red><b>FALSE</b></color>")}";
			else
				yield return $"{property.Name}: <color=yellow><b>{property.GetValue(instance) ?? "NULL"}</b></color>";
		}
	}
}

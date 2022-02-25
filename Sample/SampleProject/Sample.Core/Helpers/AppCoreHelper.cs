using System;
using System.Linq;
using System.Reflection;
using Sample.Core.Exceptions;
using Sample.Core.Installers;

namespace Sample.Core.Helpers;

public static class AppCoreHelper
{
	public static Type GetType(string typeName)
	{
		var assembly = typeof(ServiceLayerInstaller).GetTypeInfo().Assembly;
		return GetType(assembly, typeName);
	}

	public static Type GetType(Assembly assembly, string typeName)
	{
		var result = assembly.GetType(typeName);
		if (result != null)
		{
			return result;
		}

		var parts = typeName.Split('.');

		var matchingTypes = assembly.GetTypes().Where(x => x.FullName != null && x.FullName.Contains(typeName) && x.Name == parts.Last()).ToList();
		if (matchingTypes.Count == 1)
		{
			return matchingTypes.First();
		}

		if (matchingTypes.Count > 1)
		{
			throw new AppCoreException($"Ambiguous reference type - Many types found for resource '{typeName}'. Please use a more specific type.");
		}

		if (matchingTypes.Count == 0)
		{
			throw new AppCoreException($"Ambiguous reference type - No type found for resource '{typeName}'. Please use a more specific type.");
		}

		return null;
	}

}
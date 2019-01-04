using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Configuration;
using Microsoft.Extensions.DependencyModel;

namespace AopCaching.Core
{
	public class RegisterDynamicProxy
	{
		/// <summary>
		/// Register dynamic proxy
		/// </summary>
		/// <param name="configurator"></param>
		/// <param name="filter"></param>
		public static void Register(IAspectConfiguration configurator, CacheMethodFilter filter)
		{
			configurator.ThrowAspectException = false;

			//Exclude methods that do not return a value.
			configurator.NonAspectPredicates.Add(method => method.ReturnType == typeof(void) || method.ReturnType == typeof(Task));
			//Add all methods that use custom attributes.
			configurator.Interceptors.AddTyped<AopCachingInterceptor>(method =>
				method.GetCustomAttributes(true).Any(p => p.GetType() == typeof(AopCachingAttribute)));
			//Add services
			if (filter?.IncludeService?.Any() ?? false)
				configurator.Interceptors.AddTyped<AopCachingInterceptor>(filter.IncludeService.Select(Predicates.ForService).ToArray());

			//Exclude services
			if (filter?.ExcludeService?.Any() ?? false)
				foreach (var item in filter.ExcludeService)
				{
					configurator.NonAspectPredicates.AddService(item);
				}
			//Add methods
			if (filter?.IncludeMethod?.Any() ?? false)
				configurator.Interceptors.AddTyped<AopCachingInterceptor>(filter.IncludeMethod.Select(Predicates.ForMethod).ToArray());
			//Exclude methods
			if (filter?.ExcludeMethod?.Any() ?? false)
				foreach (var item in filter.ExcludeMethod)
				{
					configurator.NonAspectPredicates.AddMethod(item.ServiceName, item.MethodName);
				}
			//Add namespaces
			if (filter?.IncludeNameSpace?.Any() ?? false)
				configurator.Interceptors.AddTyped<AopCachingInterceptor>(filter.IncludeNameSpace.Select(Predicates.ForNameSpace).ToArray());
			//Exclude namespaces
			if (filter?.ExcludeNameSpace?.Any() ?? false)
				foreach (var item in filter.ExcludeNameSpace)
				{
					configurator.NonAspectPredicates.AddNamespace(item);
				}

			var assemblies = DependencyContext.Default.RuntimeLibraries.SelectMany(i => i.GetDefaultAssemblyNames(DependencyContext.Default).Where(p => !p.Name.StartsWith("Microsoft", StringComparison.CurrentCultureIgnoreCase) && !p.Name.StartsWith("System", StringComparison.CurrentCultureIgnoreCase) && !p.Name.StartsWith("Aspect", StringComparison.CurrentCultureIgnoreCase)).Select(z => Assembly.Load(new AssemblyName(z.Name)))).Where(p => !p.IsDynamic).ToList();

			foreach (var assembly in assemblies)
			{
				var types = assembly.GetExportedTypes();
				foreach (var type in types)
				{
					var typeAttrs = type.GetCustomAttributes(true);
					if (typeAttrs.Any(p => p.GetType() == typeof(AopCachingAttribute)))
					{
						configurator.Interceptors.AddTyped<AopCachingInterceptor>(Predicates.ForService(type.FullName));
					}

					if (typeAttrs.Any(p => p.GetType() == typeof(NonAopCachingAttribute)))
					{
						configurator.NonAspectPredicates.AddService(type.FullName);
					}
					var methods = type.GetMethods();
					foreach (var method in methods)
					{
						var methodAttrs = method.GetCustomAttributes(true);
						if (methodAttrs.Any(p => p.GetType() == typeof(AopCachingAttribute)))
						{
							configurator.Interceptors.AddTyped<AopCachingInterceptor>(Predicates.ForMethod($"{type.FullName}.{method.Name}"));
						}

						if (methodAttrs.Any(p => p.GetType() == typeof(NonAopCachingAttribute)))
						{
							configurator.NonAspectPredicates.AddMethod(type.FullName, method.Name);
						}
					}
				}
			}
		}
	}
}

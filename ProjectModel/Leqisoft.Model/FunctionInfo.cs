using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Leqisoft.Model;

public class FunctionInfo
{
	public string Name { get; private set; }

	public string Description { get; private set; }

	public string Category { get; private set; }

	public List<ParameterInfo> Parameters { get; private set; }

	public static List<FunctionInfo> AllFunctionInfos { get; } = GetFunctionInfosImpl(typeof(FunctionEvaluator).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));


	public static List<FunctionInfo> PublicFunctionInfos { get; } = GetFunctionInfosImpl(typeof(FunctionEvaluator).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public));


	private static List<FunctionInfo> GetFunctionInfosImpl(MethodInfo[] input)
	{
		return input.Select((MethodInfo f) => new FunctionInfo
		{
			Name = f.Name,
			Description = (Attribute.GetCustomAttribute(f, typeof(DescriptionAttribute)) as DescriptionAttribute).Description,
			Category = (Attribute.GetCustomAttribute(f, typeof(CategoryAttribute)) as CategoryAttribute).Category,
			Parameters = (from p in f.GetParameters()
				select new ParameterInfo
				{
					Name = (Attribute.GetCustomAttribute(p, typeof(ParameterNameAttribute)) as ParameterNameAttribute).Name,
					Description = (Attribute.GetCustomAttribute(p, typeof(DescriptionAttribute)) as DescriptionAttribute).Description
				}).ToList()
		}).ToList();
	}
}

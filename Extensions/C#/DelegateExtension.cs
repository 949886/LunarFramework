// 
// DelegateExtension.cs
// 
// 
// Created by LunarEclipse on 2023-10-18 20:04.
// Copyright © 2023 LunarEclipse. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

// namespace Luna.Extensions
// {
//     public static class DelegateExtension
//     {
//         public static bool InvokeRapidly(this Delegate del, params object[] args)
//         {
//             if (del == null)
//                 throw new ArgumentNullException(nameof(del));
//
//             var type = del.GetType();
//             return TypeToWrapperMap.GetOrAdd(type, t =>
//             {
//                 var method = del.GetMethodInfo();
//                 var wrapper = CreateMethodWrapper(t, method, true);
//                 return new EfficientInvoker(wrapper);
//             });
//             return true;
//         }
//
//         private static Func<object, object[], object> CreateMethodWrapper(Type type, MethodInfo method, bool isDelegate)
//         {
//             CreateParamsExpressions(method, out ParameterExpression argsExp, out Expression[] paramsExps);
//
//             var targetExp = Expression.Parameter(typeof(object), "target");
//             var castTargetExp = Expression.Convert(targetExp, type);
//             var invokeExp = isDelegate
//                 ? (Expression)Expression.Invoke(castTargetExp, paramsExps)
//                 : Expression.Call(castTargetExp, method, paramsExps);
//
//             LambdaExpression lambdaExp;
//
//             if (method.ReturnType != typeof(void))
//             {
//                 var resultExp = Expression.Convert(invokeExp, typeof(object));
//                 lambdaExp = Expression.Lambda(resultExp, targetExp, argsExp);
//             }
//             else
//             {
//                 var constExp = Expression.Constant(null, typeof(object));
//                 var blockExp = Expression.Block(invokeExp, constExp);
//                 lambdaExp = Expression.Lambda(blockExp, targetExp, argsExp);
//             }
//
//             var lambda = lambdaExp.Compile();
//             return (Func<object, object[], object>)lambda;
//         }
//
//         private static void CreateParamsExpressions(MethodBase method, out ParameterExpression argsExp, out Expression[] paramsExps)
//         {
//             var parameters = method.GetParameterTypes();
//
//             argsExp = Expression.Parameter(typeof(object[]), "args");
//             paramsExps = new Expression[parameters.Count];
//
//             for (var i = 0; i < parameters.Count; i++)
//             {
//                 var constExp = Expression.Constant(i, typeof(int));
//                 var argExp = Expression.ArrayIndex(argsExp, constExp);
//                 paramsExps[i] = Expression.Convert(argExp, parameters[i]);
//             }
//         }
//
//     }
//
//     public static class MethodBaseExtensions
//     {
//         public static IReadOnlyList<Type> GetParameterTypes(this MethodBase method)
//         {
//             return TypeExtensions.ParameterMap.GetOrAdd(method, c => c.GetParameters().Select(p => p.ParameterType).ToArray());
//         }
//     }
//
//     public static class TypeExtensions
//     {
//         internal static readonly ConcurrentDictionary<MethodBase, IReadOnlyList<Type>> ParameterMap =
//             new ConcurrentDictionary<MethodBase, IReadOnlyList<Type>>();
//
//         public static EfficientInvoker GetMethodInvoker(this Type type, string methodName)
//         {
//             return EfficientInvoker.ForMethod(type, methodName);
//         }
//
//         public static EfficientInvoker GetPropertyInvoker(this Type type, string propertyName)
//         {
//             return EfficientInvoker.ForProperty(type, propertyName);
//         }
//     }
// }
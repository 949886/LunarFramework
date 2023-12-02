﻿//
//  MethodInfoExtension.cs
//  Luna
//
//  Created by LunarEclipse on 2018-8-8.
//  Copyright © 2018 LunarEclipse. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Luna.Extensions
{
    public static class MethodInfoExtension
    {
        // https://stackoverflow.com/questions/940675/getting-a-delegate-from-methodinfo
        public static Delegate CreateDelegate(this MethodInfo method, object firstArgument = null)
        {
            Func<Type[], Type> getType;
            var types = method.GetParameters().Select(p => p.ParameterType);
            var isAction = method.ReturnType.Equals((typeof(void)));
            if (isAction)
                getType = Expression.GetActionType;
            else
            {
                getType = Expression.GetFuncType;
                types = types.Concat(new[] { method.ReturnType });
            }

            if (firstArgument == null)
                return Delegate.CreateDelegate(getType(types.ToArray()), method);
            return Delegate.CreateDelegate(getType(types.ToArray()), firstArgument, method);
        }

        public static Action<Object> ToLambda(this MethodInfo methodInfo, Object invoker)
        {
            var instance = Expression.Constant(invoker);
            UnaryExpression convertedInstance = Expression.Convert(instance, methodInfo.DeclaringType);

            var parameterType = methodInfo.GetParameters().First().ParameterType;
            ParameterExpression param1 = Expression.Parameter(parameterType);
            MethodCallExpression call = Expression.Call(convertedInstance, methodInfo, param1);
            var lambda = Expression.Lambda<Action<Object>>(call, param1);
            return lambda.Compile();
        }

        public static Action<T> ToLambda<T>(this MethodInfo methodInfo, Object invoker)
        {
            var instance = Expression.Constant(invoker);
            UnaryExpression convertedInstance = Expression.Convert(instance, methodInfo.DeclaringType);

            var parameterType = methodInfo.GetParameters().First().ParameterType;
            ParameterExpression param1 = Expression.Parameter(parameterType);
            MethodCallExpression call = Expression.Call(convertedInstance, methodInfo, param1);
            var lambda = Expression.Lambda<Action<T>>(call, param1);
            return lambda.Compile();
        }

        // https://stackoverflow.com/questions/13041674/create-func-or-action-for-any-method-using-reflection-in-c
        public static T BuildDelegate<T>(this MethodInfo method, params object[] missingParamValues)
        {
            var queueMissingParams = new Queue<object>(missingParamValues);

            var dgtMi = typeof(T).GetMethod("Invoke");
            var dgtRet = dgtMi.ReturnType;
            var dgtParams = dgtMi.GetParameters();

            var paramsOfDelegate = dgtParams
                .Select(tp => Expression.Parameter(tp.ParameterType, tp.Name))
                .ToArray();

            var methodParams = method.GetParameters();

            if (method.IsStatic)
            {
                var paramsToPass = methodParams
                    .Select((p, i) => CreateParam(paramsOfDelegate, i, p, queueMissingParams))
                    .ToArray();

                var expr = Expression.Lambda<T>(
                    Expression.Call(method, paramsToPass),
                    paramsOfDelegate);

                return expr.Compile();
            }
            else
            {
                var paramThis = Expression.Convert(paramsOfDelegate[0], method.DeclaringType);

                var paramsToPass = methodParams
                    .Select((p, i) => CreateParam(paramsOfDelegate, i + 1, p, queueMissingParams))
                    .ToArray();

                var expr = Expression.Lambda<T>(
                    Expression.Call(paramThis, method, paramsToPass),
                    paramsOfDelegate);

                return expr.Compile();
            }
        }

        private static Expression CreateParam(ParameterExpression[] paramsOfDelegate, int i, ParameterInfo callParamType, Queue<object> queueMissingParams)
        {
            if (i < paramsOfDelegate.Length)
                return Expression.Convert(paramsOfDelegate[i], callParamType.ParameterType);

            if (queueMissingParams.Count > 0)
                return Expression.Constant(queueMissingParams.Dequeue());

            if (callParamType.ParameterType.IsValueType)
                return Expression.Constant(Activator.CreateInstance(callParamType.ParameterType));

            return Expression.Constant(null);
        }
    }
}
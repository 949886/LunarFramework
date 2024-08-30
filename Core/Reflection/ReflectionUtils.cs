using System;
using System.Reflection;

namespace Luna.Core
{
    public static class ReflectionUtils
    {
        /// <summary>Gets an instance method with single argument of type <typeparamref
        /// name="TArg0"/> and return type of <typeparamref name="TReturn"/> from <typeparamref
        /// name="TThis"/> and compiles it into a fast open delegate.</summary>
        /// <typeparam name="TThis">Type of the class owning the instance method.</typeparam>
        /// <typeparam name="TArg0">Type of the single parameter to the instance method to
        /// find.</typeparam>
        /// <typeparam name="TReturn">Type of the return for the method</typeparam>
        /// <param name="methodName">The name of the method the compile.</param>
        /// <returns>The compiled delegate, which should be about as fast as calling the function
        /// directly on the instance.</returns>
        /// <exception cref="ArgumentException">If the method can't be found, or it has an
        /// unexpected return type (the return type must match exactly).</exception>
        /// <see href="https://codeblog.jonskeet.uk/2008/08/09/making-reflection-fly-and-exploring-delegates/"/>
        
        public static Func<TThis, TArg0, TReturn> AccessPrivateFunction<TThis, TArg0, TReturn>(string methodName)
        {
            var method = typeof(TThis).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                CallingConventions.Any,
                new[] { typeof(TArg0) },
                null);
     
            if (method == null)
                throw new ArgumentException("Can't find method " + typeof(TThis).FullName + "." + methodName + "(" + typeof(TArg0).FullName + ")");
            else if (method.ReturnType != typeof(TReturn))
                throw new ArgumentException("Expected " + typeof(TThis).FullName + "." + methodName + "(" + typeof(TArg0).FullName + ") to have return type of string but was " + method.ReturnType.FullName);
            return (Func<TThis, TArg0, TReturn>)Delegate.CreateDelegate(typeof(Func<TThis, TArg0, TReturn>), method);
        }
        
        public static Action<TThis, TArg0> AccessPrivateAction<TThis, TArg0>(string methodName)
        {
            var method = typeof(TThis).GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                CallingConventions.Any,
                new[] { typeof(TArg0) },
                null);
     
            if (method == null)
                throw new ArgumentException("Can't find method " + typeof(TThis).FullName + "." + methodName + "(" + typeof(TArg0).FullName + ")");
            return (Action<TThis, TArg0>)Delegate.CreateDelegate(typeof(Action<TThis, TArg0>), method);
        }

        public static Action<TThis, TArg0, TArg1> AccessPrivateAction<TThis, TArg0, TArg1>(string methodName)
        {
            var method = typeof(TThis).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                CallingConventions.Any,
                new[] { typeof(TArg0), typeof(TArg1) },
                null);
     
            if (method == null)
                throw new ArgumentException("Can't find method " + typeof(TThis).FullName + "." + methodName + "(" + typeof(TArg0).FullName + ")");
            return (Action<TThis, TArg0, TArg1>)Delegate.CreateDelegate(typeof(Action<TThis, TArg0, TArg1>), method);
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace SSystem.Data.Compiler
{
    public delegate object GetHandler(object source);
    public delegate void SetHandler(object source, object value);
    public delegate object InstantiateObjectHandler();

    public sealed class DynamicMethodCompiler
    {
        // DynamicMethodCompiler
        private DynamicMethodCompiler() { }

        private static ConcurrentDictionary<string, GetHandler> _CachedGetHandlers = new ConcurrentDictionary<string, GetHandler>();
        private static ConcurrentDictionary<string, SetHandler> _CachedSetHandlers = new ConcurrentDictionary<string, SetHandler>();
        private static ConcurrentDictionary<string, InstantiateObjectHandler> _CachedObjectHandler = new ConcurrentDictionary<string, InstantiateObjectHandler>();
        // CreateInstantiateObjectDelegate
        internal static InstantiateObjectHandler CreateInstantiateObjectHandler(Type type)
        {
            if (_CachedObjectHandler.ContainsKey(type.FullName))
                return _CachedObjectHandler[type.FullName];

            ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null);
            if (constructorInfo == null)
            {
                throw new ApplicationException(string.Format("The type {0} must declare an empty constructor (the constructor may be private, internal, protected, protected internal, or public).", type));
            }

            DynamicMethod dynamicMethod = new DynamicMethod("InstantiateObject", MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(object), null, type, true);
            ILGenerator generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Newobj, constructorInfo);
            generator.Emit(OpCodes.Ret);
            return _CachedObjectHandler.GetOrAdd(type.FullName, (InstantiateObjectHandler)dynamicMethod.CreateDelegate(typeof(InstantiateObjectHandler)));
        }

        // CreateGetDelegate
        internal static GetHandler CreateGetHandler(Type type, PropertyInfo propertyInfo)
        {
            string key = type.FullName + propertyInfo.Name;
            if (_CachedGetHandlers.ContainsKey(key))
                return _CachedGetHandlers[key];

            MethodInfo getMethodInfo = propertyInfo.GetGetMethod(true);
            DynamicMethod dynamicGet = CreateGetDynamicMethod(type);
            ILGenerator getGenerator = dynamicGet.GetILGenerator();

            getGenerator.Emit(OpCodes.Ldarg_0);
            getGenerator.Emit(OpCodes.Call, getMethodInfo);
            BoxIfNeeded(getMethodInfo.ReturnType, getGenerator);
            getGenerator.Emit(OpCodes.Ret);

            var handler = (GetHandler)dynamicGet.CreateDelegate(typeof(GetHandler));
            _CachedGetHandlers.TryAdd(key, handler);
            return handler;
        }

        // CreateGetDelegate
        internal static GetHandler CreateGetHandler(Type type, FieldInfo fieldInfo)
        {
            string key = type.FullName + fieldInfo.Name;
            if (_CachedGetHandlers.ContainsKey(key))
                return _CachedGetHandlers[key];

            DynamicMethod dynamicGet = CreateGetDynamicMethod(type);
            ILGenerator getGenerator = dynamicGet.GetILGenerator();

            getGenerator.Emit(OpCodes.Ldarg_0);
            getGenerator.Emit(OpCodes.Ldfld, fieldInfo);
            BoxIfNeeded(fieldInfo.FieldType, getGenerator);
            getGenerator.Emit(OpCodes.Ret);

            var handler = (GetHandler)dynamicGet.CreateDelegate(typeof(GetHandler));
            _CachedGetHandlers.TryAdd(key, handler);
            return handler;
        }

        // CreateSetDelegate
        internal static SetHandler CreateSetHandler(Type type, PropertyInfo propertyInfo)
        {
            string key = type.FullName + propertyInfo.Name;
            if (_CachedSetHandlers.ContainsKey(key))
                return _CachedSetHandlers[key];

            MethodInfo setMethodInfo = propertyInfo.GetSetMethod(true);
            DynamicMethod dynamicSet = CreateSetDynamicMethod(type);
            ILGenerator setGenerator = dynamicSet.GetILGenerator();

            setGenerator.Emit(OpCodes.Ldarg_0);
            setGenerator.Emit(OpCodes.Ldarg_1);
            UnboxIfNeeded(setMethodInfo.GetParameters()[0].ParameterType, setGenerator);
            setGenerator.Emit(OpCodes.Call, setMethodInfo);
            setGenerator.Emit(OpCodes.Ret);

            var handler = (SetHandler)dynamicSet.CreateDelegate(typeof(SetHandler));
            _CachedSetHandlers.TryAdd(key, handler);
            return handler;
        }

        // CreateSetDelegate
        internal static SetHandler CreateSetHandler(Type type, FieldInfo fieldInfo)
        {
            string key = type.FullName + fieldInfo.Name;
            if (_CachedSetHandlers.ContainsKey(key))
                return _CachedSetHandlers[key];

            DynamicMethod dynamicSet = CreateSetDynamicMethod(type);
            ILGenerator setGenerator = dynamicSet.GetILGenerator();

            setGenerator.Emit(OpCodes.Ldarg_0);
            setGenerator.Emit(OpCodes.Ldarg_1);
            UnboxIfNeeded(fieldInfo.FieldType, setGenerator);
            setGenerator.Emit(OpCodes.Stfld, fieldInfo);
            setGenerator.Emit(OpCodes.Ret);

            var handler = (SetHandler)dynamicSet.CreateDelegate(typeof(SetHandler));
            _CachedSetHandlers.TryAdd(key, handler);
            return handler;
        }

        // CreateGetDynamicMethod
        private static DynamicMethod CreateGetDynamicMethod(Type type)
        {
            return new DynamicMethod("DynamicGet", typeof(object), new Type[] { typeof(object) }, type, true);
        }

        // CreateSetDynamicMethod
        private static DynamicMethod CreateSetDynamicMethod(Type type)
        {
            return new DynamicMethod("DynamicSet", typeof(void), new Type[] { typeof(object), typeof(object) }, type, true);
        }

        // BoxIfNeeded
        private static void BoxIfNeeded(Type type, ILGenerator generator)
        {
            if (type.IsValueType)
            {
                generator.Emit(OpCodes.Box, type);
            }
        }

        // UnboxIfNeeded
        private static void UnboxIfNeeded(Type type, ILGenerator generator)
        {
            if (type.IsValueType)
            {
                generator.Emit(OpCodes.Unbox_Any, type);
            }
        }
    }
}
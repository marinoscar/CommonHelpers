/*

Copyright (C) 2007-2011 by Gustavo Duarte and Bernardo Vieira.
 
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 
*/
using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Common.Helpers
{
    public static class ReflectionHelper
    {
        #region Public members

        public static readonly BindingFlags InstanceAllInclusiveIgnoreCase = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase;
        public static readonly BindingFlags PublicInstanceIgnoreCase = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
        public static readonly BindingFlags PublicStaticIgnoreCase = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;

        public static object ApplyDataRecordToObject(IDataRecord r, object o)
        {
            ArgumentValidator.ThrowIfNull(r, "r");
            ArgumentValidator.ThrowIfNull(o, "o");

            var t = o.GetType();

            for (var i = 0; i < r.FieldCount; i++)
            {
                var fieldName = r.GetName(i);

                var value = r.GetValue(i);
                if (Convert.IsDBNull(value))
                {
                    value = null;
                }

                var property = t.GetProperty(fieldName, InstanceAllInclusiveIgnoreCase);
                if (property != null)
                {
                    try
                    {
                        property.SetValue(o, value, null);
                    }
                    catch
                    {
                        //try to do a convert
                        property.SetValue(o, Convert.ChangeType(value, property.PropertyType));
                    }
                    // We could continue here, since the field lookup below is likely to be useless, but we cater to people who have a property and field
                    // differing only in capitalization
                }

                var field = t.GetField(fieldName, InstanceAllInclusiveIgnoreCase);
                if (field != null)
                {
                    try
                    {
                        field.SetValue(o, value);
                    }
                    catch
                    {
                        field.SetValue(o, Convert.ChangeType(value, field.FieldType));
                    }
                }
            }

            return o;
        }

        public static object ApplyDictionaryToObject(IDictionary d, object o)
        {
            ArgumentValidator.ThrowIfNull(d, "d");
            ArgumentValidator.ThrowIfNull(o, "o");

            var t = o.GetType();

            foreach (object key in d.Keys)
            {
                var property = t.GetProperty(key.ToString(), InstanceAllInclusiveIgnoreCase);
                if (property != null)
                {
                    property.SetValue(o, Convert.ChangeType(d[key], property.PropertyType), null);
                    continue;
                }

                var field = t.GetField(key.ToString(), InstanceAllInclusiveIgnoreCase);
                if (field != null)
                {
                    field.SetValue(o, Convert.ChangeType(d[key], field.FieldType));
                }
            }

            return o;
        }


        public static PropertyInfo GetPropertyOrDie(Type t, string propertyName, BindingFlags flags)
        {
            ArgumentValidator.ThrowIfNull(t, "t");
            ArgumentValidator.ThrowIfNullOrEmpty(propertyName, "propertyName");

            PropertyInfo propertyInfo = t.GetProperty(propertyName, flags);
            if (propertyInfo == null)
            {
                string msg = "Failed to get property {0} on type {1} with binding flags {2}".Fi(propertyName, t.FullName, flags);
                throw new AssertionViolationException(msg);
            }

            return propertyInfo;
        }

        public static PropertyInfo GetPropertyOrDie(Type t, string propertyName)
        {
            return GetPropertyOrDie(t, propertyName, InstanceAllInclusiveIgnoreCase);
        }

        public static object Invoke(Object target, string methodName)
        {
            return Invoke(target, methodName, PublicInstanceIgnoreCase, null);
        }

        public static object Invoke(Object target, string methodName, BindingFlags flags, object[] arguments)
        {
            ArgumentValidator.ThrowIfNull(target, "target");

            bool foundMethod;
            var result = Invoke(target.GetType(), target, methodName, flags, out foundMethod, arguments);

            if (!foundMethod)
            {
                throw new AssertionViolationException("Could not find method {0} in type {1}".Fi(methodName, target.GetType().FullName));
            }

            return result;
        }

        public static bool TryInvoke(Object target, string methodName)
        {
            return TryInvoke(target, methodName, PublicInstanceIgnoreCase, null);
        }

        public static bool TryInvoke(Object target, string methodName, BindingFlags flags, object[] arguments)
        {
            ArgumentValidator.ThrowIfNull(target, "target");

            bool foundMethod;
            Invoke(target.GetType(), target, methodName, flags, out foundMethod, arguments);
            return foundMethod;
        }

        public static object InvokeStatic(Type t, string methodName, params object[] arguments)
        {
            ArgumentValidator.ThrowIfNull(t, "t");
            ArgumentValidator.ThrowIfNullOrEmpty(methodName, "methodName");

            bool foundMethod;
            var r = Invoke(t, null, methodName, PublicStaticIgnoreCase, out foundMethod, arguments);
            if (!foundMethod)
            {
                throw new AssertionViolationException("Could not find static method {0} in type {1}".Fi(methodName, t.FullName));
            }

            return r;
        }

        public static object CreateInstance(Type t)
        {
            ArgumentValidator.ThrowIfNull(t, "t");
            ConstructorInfo ci = t.GetConstructor(Type.EmptyTypes);
            return ci.Invoke(new object[0]);
        }

        public static object Invoke(Type t, object target, string methodName, BindingFlags flags, out bool foundMethod, object[] arguments)
        {
            ArgumentValidator.ThrowIfNull(t, "t");
            ArgumentValidator.ThrowIfNullOrEmpty(methodName, "methodName");
            MethodInfo methodInfo;


            if (arguments != null)
            {
                var types = arguments.Select(p => p.GetType()).ToArray();
                methodInfo = t.GetMethod(methodName, flags, null, types, null);
            }
            else
            {
                methodInfo = t.GetMethod(methodName, flags);
            }

            if (null == methodInfo)
            {
                foundMethod = false;
                return null;
            }

            foundMethod = true;
            return methodInfo.Invoke(target, arguments);
        }

        public static bool IsBool(Type t)
        {
            ArgumentValidator.ThrowIfNull(t, "t");

            t = Nullable.GetUnderlyingType(t) ?? t;
            return t == typeof(bool);
        }

        public static bool IsDateTime(Type t)
        {
            ArgumentValidator.ThrowIfNull(t, "t");

            t = Nullable.GetUnderlyingType(t) ?? t;
            return t == typeof(DateTime);
        }

        public static bool IsNumeric(Type t)
        {
            ArgumentValidator.ThrowIfNull(t, "t");

            return IsIntegralType(t) || IsFloatingPoint(t);
        }

        public static bool IsFloatingPoint(Type t)
        {
            ArgumentValidator.ThrowIfNull(t, "t");

            t = Nullable.GetUnderlyingType(t) ?? t;
            return t == typeof(float) || t == typeof(double) || t == typeof(decimal);
        }

        public static bool IsIntegralType(Type t)
        {
            ArgumentValidator.ThrowIfNull(t, "t");

            t = Nullable.GetUnderlyingType(t) ?? t;

            return (t == typeof(sbyte) || t == typeof(byte) || t == typeof(short) || t == typeof(ushort) || t == typeof(int) || t == typeof(uint)
                    || t == typeof(long) || t == typeof(ulong));
        }

        public static object ReadProperty(object o, string propertyName)
        {
            ArgumentValidator.ThrowIfNull(o, "o");
            ArgumentValidator.ThrowIfNullOrEmpty(propertyName, "propertyName");

            PropertyInfo propertyInfo = GetPropertyOrDie(o.GetType(), propertyName);
            return propertyInfo.GetValue(o, null);
        }

        public static void TrySetProperty(object o, string propertyName, object value)
        {
            ArgumentValidator.ThrowIfNull(o, "o");
            ArgumentValidator.ThrowIfNullOrEmpty(propertyName, "propertyName");

            try
            {
                var propertyInfo = TryGetProperty(o.GetType(), propertyName, InstanceAllInclusiveIgnoreCase);
                if (null == propertyInfo)
                {
                    return;
                }


                SetProperty(o, propertyInfo, value);
            }
            catch { }

        }

        public static void SetProperty(object o, string propertyName, object value)
        {
            ArgumentValidator.ThrowIfNull(o, "o");
            ArgumentValidator.ThrowIfNullOrEmpty(propertyName, "propertyName");

            var propertyInfo = GetPropertyOrDie(o.GetType(), propertyName);
            SetProperty(o, propertyInfo, value);
        }

        public static void SetProperty(object o, PropertyInfo propertyInfo, object value)
        {
            ArgumentValidator.ThrowIfNull(o, "o");

            Type conversionType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;

            if (string.Empty.Equals(value) && conversionType != typeof(string))
            {
                value = null;
            }

            if (value != null)
            {
                value = Convert.ChangeType(value, conversionType);
            }

            propertyInfo.SetValue(o, value, null);
        }


        public static PropertyInfo TryGetProperty(Type t, string propertyName, BindingFlags flags)
        {
            ArgumentValidator.ThrowIfNull(t, "t");
            ArgumentValidator.ThrowIfNullOrEmpty(propertyName, "propertyName");

            return t.GetProperty(propertyName, flags);
        }

        public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }


        #endregion
    }
}
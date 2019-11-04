using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Smidas.Common.Extensions
{
    public static class Enums
    {
        public static string GetDisplayName(this Enum @enum) => GetDisplayAttribute(@enum)?.Name ?? @enum.ToString();

        private static DisplayAttribute GetDisplayAttribute(object value)
        {
            var type = value.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException($"Type {type} is not an enum");
            }

            return type.GetField(value.ToString())?.GetCustomAttribute<DisplayAttribute>();
        }
    }
}

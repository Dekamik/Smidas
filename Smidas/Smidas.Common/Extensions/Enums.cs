using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Smidas.Common.Extensions
{
    public static class Enums
    {
        public static string GetDisplayName(this Enum @enum) => GetAttribute<DisplayAttribute>(@enum)?.Name ?? @enum.ToString();

        public static string GetDescription(this Enum @enum) => GetAttribute<DescriptionAttribute>(@enum).Description;

        private static T GetAttribute<T>(object value) where T : Attribute
        {
            var type = value.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException($"Type {type} is not an enum");
            }

            return type.GetField(value.ToString())?.GetCustomAttribute<T>();
        }
    }
}

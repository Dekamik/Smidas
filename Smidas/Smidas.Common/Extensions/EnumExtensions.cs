using Smidas.Common.StockIndices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Smidas.Common.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum @enum) => GetAttribute<DisplayAttribute>(@enum)?.Name ?? @enum.ToString();

        public static string GetDescription(this Enum @enum) => GetAttribute<DescriptionAttribute>(@enum).Description;

        public static T GetAttribute<T>(object value) where T : Attribute => GetAttributes<T>(value).Single();

        public static IEnumerable<T> GetAttributes<T>(object value) where T : Attribute
        {
            Type type = value.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException($"Type {type} is not an enum");
            }

            return type.GetField(value.ToString())?.GetCustomAttributes<T>();
        }
    }
}

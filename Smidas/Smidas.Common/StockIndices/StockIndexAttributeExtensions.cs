using Smidas.Common.Extensions;
using System;

namespace Smidas.Common.StockIndices
{
    public static class StockIndexAttributeExtensions
    {
        public static AffarsVarldenInfoAttribute GetAffarsVarldenInfo(this Enum @enum) => EnumExtensions.GetAttribute<AffarsVarldenInfoAttribute>(@enum);

        public static DagensIndustriInfoAttribute GetDagensIndustriInfo(this Enum @enum) => EnumExtensions.GetAttribute<DagensIndustriInfoAttribute>(@enum);
    }
}

using System;
using System.Collections.Generic;

namespace YourCompany.Reflection
{
    public static partial class ValueTupleHelper
    {
        public static readonly IReadOnlyList<string> ValueTuplePropertyNames = new string[]
        {
            nameof(ValueTuple<int>.Item1),
            nameof(ValueTuple<int, int>.Item2),
            nameof(ValueTuple<int, int, int>.Item3),
            nameof(ValueTuple<int, int, int, int>.Item4),
            nameof(ValueTuple<int, int, int, int, int>.Item5),
            nameof(ValueTuple<int, int, int, int, int, int>.Item6),
            nameof(ValueTuple<int, int, int, int, int, int, int>.Item7),
            nameof(ValueTuple<int, int, int, int, int, int, int, int>.Rest),
        };

        public static bool TryGetValueTuplePropertyTypes(Type type, out Type[] valuePropertyTypes)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            valuePropertyTypes = CheckIsValueTuple(type) ? type.GetGenericArguments() : null;
            if (valuePropertyTypes?.Length > ValueTuplePropertyNames.Count) throw new ApplicationException("valuePropertyTypes.Length > ValueTuplePropertyNames.Count");
            return valuePropertyTypes != null;
        }

        public static bool CheckIsValueTuple(Type type) => type.Name.StartsWith(nameof(ValueTuple)) && type.IsGenericType;
    }
}
using System;

namespace YourCompany.Reflection
{
    public static class TypeAbstractionsHelper
    {
        public static bool CheckIsAbstraction(Type type)
        {
            return type.IsAbstract || type.IsInterface;
        }

        public static void VisitTypeAbstractions(Type type, Action<Type> abstractionVisitor)
        {
            var interfaces = type.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
                abstractionVisitor(interfaces[i]);

            var baseType = type.BaseType;
            while (baseType != null)
            {
                abstractionVisitor(baseType);
                baseType = baseType.BaseType;
            }
        }
    }
}
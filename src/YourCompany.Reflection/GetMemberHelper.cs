using System;
using System.Linq.Expressions;
using System.Reflection;

namespace YourCompany.Reflection
{
    public static class GetMemberHelper
    {
        public static ConstructorInfo GetConstructorInfo<T>(Expression<Func<T>> expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            var constructorCall = expression.Body as NewExpression;
            return constructorCall?.Constructor ?? throw new ApplicationException("constructorCall?.Constructor == null");
        }

        public static MethodInfo GetMethodInfo<TResult>(Expression<Func<TResult>> expression)
            => GetMethodInfo((LambdaExpression)expression);

        public static MethodInfo GetMethodInfo<TInstance, TResult>(Expression<Func<TInstance, TResult>> expression)
            => GetMethodInfo((LambdaExpression)expression);

        public static MethodInfo GetMethodInfo(Expression<Action> expression)
            => GetMethodInfo((LambdaExpression)expression);

        public static MethodInfo GetMethodInfo<TInstance>(Expression<Action<TInstance>> expression)
            => GetMethodInfo((LambdaExpression)expression);

        public static PropertyInfo GetPropertyInfo<TValue>(Expression<Func<TValue>> expression)
            => GetPropertyInfo((LambdaExpression)expression);

        public static PropertyInfo GetPropertyInfo<TInstance, TValue>(Expression<Func<TInstance, TValue>> expression)
            => GetPropertyInfo((LambdaExpression)expression);

        public static PropertyInfo GetPropertyInfo(Expression<Action> expression)
            => GetPropertyInfo((LambdaExpression)expression);

        public static PropertyInfo GetPropertyInfo<TInstance>(Expression<Action<TInstance>> expression)
            => GetPropertyInfo((LambdaExpression)expression);

        private static MethodInfo GetMethodInfo(LambdaExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            var methodCall = expression.Body as MethodCallExpression;
            return methodCall?.Method ?? throw new ApplicationException("methodCall?.Method == null");
        }

        private static PropertyInfo GetPropertyInfo(LambdaExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            var memberAccess = expression.Body as MemberExpression;
            var propertyInfo = memberAccess?.Member as PropertyInfo ?? throw new ApplicationException("memberAccess?.Member == null");
            if (expression.Body is IndexExpression indexExpression && indexExpression.Indexer != propertyInfo)
                throw new ApplicationException("expression.Body is IndexExpression indexExpression && indexExpression.Indexer != propertyInfo");
            return propertyInfo;
        }
    }
}
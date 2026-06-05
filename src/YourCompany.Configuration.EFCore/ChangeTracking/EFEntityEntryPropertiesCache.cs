using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using YourCompany.Reflection;

namespace YourCompany.Configuration.EFCore.ChangeTracking
{
    internal static class EFEntityEntryPropertiesCache<TProperty>
    {
        private static readonly Expression<Action<EntityEntry<object>>> MethodExpression
            = entry => entry.Property<TProperty>("dummy");
        private static readonly Type[] PropertyMethodGenericArgumentTypes = new Type[] { typeof(TProperty) };
        private static readonly ConcurrentDictionary<(Type, string), Func<EntityEntry, TProperty>> Getters = new();
        private static readonly ConcurrentDictionary<(Type, string), Action<EntityEntry, TProperty>> Setters = new();

        private static readonly Func<(Type EntityType, string PropertyName), Func<EntityEntry, TProperty>>
            CreateGetterNonExclusiveStrategy = key =>
        {
            var entityEntryType = typeof(EntityEntry<>).MakeGenericType(key.EntityType);
            var propertyMethod = entityEntryType
                .GetMethod(GetMemberHelper.GetMethodInfo(MethodExpression).Name, PropertyMethodGenericArgumentTypes)
                .MakeGenericMethod(typeof(TProperty));
            var entityEntry = Expression.Parameter(typeof(EntityEntry), "e");
            var convertedEntityEntry = Expression.Convert(entityEntry, entityEntryType);
            var propertyName = Expression.Constant(key.PropertyName);
            var propertyEntry = Expression.Call(convertedEntityEntry, propertyMethod, propertyName);
            var currentValue = Expression.Property(propertyEntry, nameof(PropertyEntry<object, TProperty>.CurrentValue));
            return Expression.Lambda<Func<EntityEntry, TProperty>>(currentValue, entityEntry).Compile();
        };

        private static readonly Func<(Type EntityType, string PropertyName), Action<EntityEntry, TProperty>>
            CreateSetterNonExclusiveStrategy = key =>
        {
            var entityEntryType = typeof(EntityEntry<>).MakeGenericType(key.EntityType);
            var propertyMethod = entityEntryType
                .GetMethod(GetMemberHelper.GetMethodInfo(MethodExpression).Name, PropertyMethodGenericArgumentTypes)
                .MakeGenericMethod(typeof(TProperty));
            var entityEntry = Expression.Parameter(typeof(EntityEntry), "e");
            var convertedEntityEntry = Expression.Convert(entityEntry, entityEntryType);
            var propertyName = Expression.Constant(key.PropertyName);
            var propertyEntry = Expression.Call(convertedEntityEntry, propertyMethod, propertyName);
            var currentValue = Expression.Property(propertyEntry, nameof(PropertyEntry<object, TProperty>.CurrentValue));
            var setValue = Expression.Parameter(typeof(TProperty), "v");
            var assignment = Expression.Assign(currentValue, setValue);
            return Expression.Lambda<Action<EntityEntry, TProperty>>(assignment, entityEntry, setValue).Compile();
        };

        internal static Func<EntityEntry, TProperty> GetGetter(Type entityType, string propertyName)
        {
            if (entityType == null) throw new ArgumentNullException(nameof(entityType));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            return Getters.GetOrAdd((entityType, propertyName), CreateGetterNonExclusiveStrategy);
        }

        internal static Action<EntityEntry, TProperty> GetSetter(Type entityType, string propertyName)
        {
            if (entityType == null) throw new ArgumentNullException(nameof(entityType));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            return Setters.GetOrAdd((entityType, propertyName), CreateSetterNonExclusiveStrategy);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.CompilerServices;

namespace ObjectPooling
{
    public static class ObjectPoolManager
    {
        private static readonly Dictionary<Type, ObjectPool> typeDictionary = new Dictionary<Type, ObjectPool>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ObjectPool CreatePool<T>() => CreatePool(typeof(T));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ObjectPool CreatePool(Type type)
        {
            var entry = new ObjectPool(type);
            typeDictionary.Add(type, entry);
            return entry;
        }

        /// <summary>
        /// Returns an Object Pool for object T. Object must have a base type ObjectPoolItem.
        /// Optionally enter a number amount to populate the pool.
        /// </summary>
        public static ObjectPool GetPool<T>(int numberOfItems = 0)
        {
            if (!typeDictionary.TryGetValue(typeof(T), out ObjectPool entry))
            {
                entry = CreatePool<T>();
            }

            if (numberOfItems != 0)
            {
                var type = typeof(T);
                for (int i = 0; i < numberOfItems; i++)
                {
                    var item = Activator.CreateInstance(type) as ObjectPoolItem;
                    item.ObjectPool = entry;
                    entry.GiveInstance(item);
                }
            }

            return entry;
        }
    }
}
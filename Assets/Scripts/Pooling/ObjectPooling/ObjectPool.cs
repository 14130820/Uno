using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Concurrent;

namespace ObjectPooling
{
    public class ObjectPool
    {
        private const int CREATE_NEW_INSTANCE_COUNT = 5;

        private readonly ConcurrentBag<ObjectPoolItem> items = new ConcurrentBag<ObjectPoolItem>();

        private Type objectPoolType;

        public ObjectPool(Type objectPoolType)
        {
            this.objectPoolType = objectPoolType;
        }

        public ObjectPoolItem GetInstance()
        {
            const int FIXED_INSTANCE_COUNT = CREATE_NEW_INSTANCE_COUNT - 1;

            if (items.TryTake(out ObjectPoolItem item))
            {
                return item;
            }
            else // Create # more
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning(string.Format("Creating {0} more instances of {1}", CREATE_NEW_INSTANCE_COUNT, objectPoolType.ToString()));
#endif

                ObjectPoolItem newItem;
                for (int i = 0; i < FIXED_INSTANCE_COUNT; i++)
                {
                    newItem = Activator.CreateInstance(objectPoolType) as ObjectPoolItem;
                    newItem.ObjectPool = this;
                    items.Add(newItem);
                }

                newItem = Activator.CreateInstance(objectPoolType) as ObjectPoolItem;
                newItem.ObjectPool = this;
                items.Add(newItem);
                return newItem;
            }
        }

        public void GiveInstance(ObjectPoolItem item) => items.Add(item);
    }


    //public class ObjectPool
    //{
    //    private const int CREATE_NEW_INSTANCE_COUNT = 5;

    //    private readonly ConcurrentBag<object> items = new ConcurrentBag<object>();

    //    public void CreateItems<T>(int numberOfItems) where T : new()
    //    {
    //        for (int i = 0; i < numberOfItems; i++)
    //        {
    //            items.Add(new T());
    //        }
    //    }

    //    public T GetInstance<T>() where T : new()
    //    {
    //        const int FIXED_INSTANCE_COUNT = CREATE_NEW_INSTANCE_COUNT - 1;

    //        if (items.TryTake(out object item))
    //        {
    //            return (T)item;
    //        }
    //        else // Create # more
    //        {

    //            CreateItems<T>(FIXED_INSTANCE_COUNT);

    //            return new T();
    //        }
    //    }

    //    public void GiveInstance<T>(T item) => items.Add(item);
    //}


//    public T GetInstance<T>() where T : ObjectPoolItem, new()
//    {
//        const int FIXED_INSTANCE_COUNT = CREATE_NEW_INSTANCE_COUNT - 1;

//        if (items.TryTake(out ObjectPoolItem item))
//        {
//            return item as T;
//        }
//        else // Create # more
//        {
//#if UNITY_EDITOR
//            UnityEngine.Debug.LogWarning(string.Format("Creating {0} more instances of {1}", CREATE_NEW_INSTANCE_COUNT, typeof(T).ToString()));
//#endif

//            for (int i = 0; i < FIXED_INSTANCE_COUNT; i++)
//            {
//                items.Add(new T() { ObjectPool = this });
//            }

//            return new T() { ObjectPool = this };
//        }
//    }
}

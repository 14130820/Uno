using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPooling
{
    public abstract class ObjectPoolItem
    {
        public ObjectPool ObjectPool { private get; set; }

        public void PoolThis()
        {
            ObjectPool.GiveInstance(this);
        }
    }
}

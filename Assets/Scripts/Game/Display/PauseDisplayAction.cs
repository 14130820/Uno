using ObjectPooling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Uno.Game.Display
{
    public class PauseDisplayAction : DisplayActionBase
    {
        public override void ActionStart() { }

        public override void ActionUpdate()
        {
            pauseTimer += Time.deltaTime;
            if (pauseTimer >= pauseTime)
            {
                SendActionToManager(this);
                this.PoolThis();
            }
        }

        private float pauseTime;
        private float pauseTimer;

        public void CreateAction(float pauseTime)
        {
            this.pauseTime = pauseTime;
            pauseTimer = 0;

            QueueThis();
        }
    }
}

#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the MIT License.
// You are free to use, modify, and distribute this file under the terms of the MIT License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence MIT.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence MIT.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.
// 
// 
#endregion
using RPGCreator.Core.Scheduler.Tasks;
using RPGCreator.Core.Scheduler.Tasks.Condition;
using RPGCreator.Core.Scheduler.Tasks.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Scheduler
{
    /// <summary>
    /// This class is the main scheduler of the engine. <br/>
    /// Right now it's only utility is to wait a few seconds before executing a callback. <br/>
    /// But this is just a proof of concept, and it will (and need to be) improved in the future. <br/><br/>
    /// Example of (future) usages: <br/>
    /// - Wait for an event to be triggered <br/>
    /// - Wait for an object to be loaded <br/>
    /// - Wait for an animation to be completed <br/>
    /// </summary>
    public class EngineScheduler
    {
        private List<BaseTask> _tasks = [];

        public void WaitSecond(float seconds, Action callback)
        {
            var task = new WaitSecondTask(seconds, callback);
            _tasks.Add(task);
        }

        public void WaitUntil(Func<bool> condition, Action callback)
        {
            var task = new WaitUntilTask(condition, callback);
            _tasks.Add(task);
        }

        public int AddTask(BaseTask task)
        {
            _tasks.Add(task);
            return _tasks.Count - 1;
        }

        public void RemoveTask(int index)
        {
            if (index >= 0 && index < _tasks.Count)
            {
                _tasks.RemoveAt(index);
            }
        }

        public void Update(float deltaTime)
        {
            for (int i = _tasks.Count - 1; i >= 0; i--)
            {
                var task = _tasks[i];
                task.Update(deltaTime);
                if (task.IsCompleted())
                {
                    task.Execute();
                    _tasks.RemoveAt(i);
                }
            }
        }
    }
}

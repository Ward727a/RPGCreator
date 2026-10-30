#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
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

using Microsoft.Extensions.DependencyInjection;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Enums;
using Serilog;

namespace RPGCreator.EngineLib.Scheduler
{
    
    public class WaitUntilTask : BaseTask
    {
        private readonly Func<bool> _condition;
        public WaitUntilTask(Func<bool> condition, Action callback)
        {
            _condition = condition;
            Callback = callback;
        }

        public override void Update(float deltaTime)
        {
        }

        public override bool IsCompleted()
        {
            return _condition.Invoke();
        }

        public override void Execute()
        {
            Callback?.Invoke();
        }

        public override bool CanBeRemoved()
        {
            return true;
        }
    }
    
    public class WaitSecondTask : BaseTask
    {
        public float RemainingTime { get; private set; } = 0f;
        private float _time;
        public bool Loop = false;

        public WaitSecondTask(float seconds, Action callback)
        {
            RemainingTime = seconds;
            _time = seconds;
            Callback = callback;
        }

        public override void Update(float deltaTime)
        {
            RemainingTime -= deltaTime;
        }

        public override bool IsCompleted()
        {
            return RemainingTime <= 0f;
        }

        public override void Execute()
        {
            Callback?.Invoke();
        }

        public override bool CanBeRemoved()
        {
            if (Loop)
            {
                RemainingTime = _time;
                return false;
            }

            return true;
        }
    }
    
    /// <summary>
    /// This class is the main scheduler of the engine. <br/>
    /// Right now it's only utility is to wait a few seconds before executing a callback. <br/>
    /// But this is just a proof of concept, and it will (and need to be) improved in the future. <br/><br/>
    /// Example of (future) usages: <br/>
    /// - Wait for an event to be triggered <br/>
    /// - Wait for an object to be loaded <br/>
    /// - Wait for an animation to be completed <br/>
    /// </summary>
    [EngineService(typeof(IScheduler))]
    public class EngineScheduler : IScheduler
    {
        private readonly ScopedLogger _logger = Logger.ForContext<EngineScheduler>();
        
        private readonly List<BaseTask> _tasks = [];
        private readonly List<BaseTask> _tasksToAdd = [];
        private readonly HashSet<Ulid> _tasksToRemove = [];

        private readonly Lock _addLock = new();
        private readonly Lock _removeLock = new();
        
        public EngineScheduler()
        {
            _logger.Info("EngineScheduler initialized.");
        }

        public Ulid WaitSecond(float seconds, Action callback, bool loop = false)
        {
            var task = new WaitSecondTask(seconds, callback);
            task.Loop = loop;
            return AddTask(task);
        }

        public Ulid WaitUntil(Func<bool> condition, Action callback)
        {
            var task = new WaitUntilTask(condition, callback);
            return AddTask(task);
        }

        public Ulid AddTask(BaseTask task)
        {
            _addLock.Enter();
            _tasksToAdd.Add(task);
            _addLock.Exit();
            return task.Id;
        }

        public void CancelTask(Ulid taskId)
        {
            _removeLock.Enter();
            _tasksToRemove.Add(taskId);
            _removeLock.Exit();
        }

        public void Update(float deltaTime)
        {
            if (_tasksToAdd.Count > 0)
            {
                _tasks.AddRange(_tasksToAdd);
                _tasksToAdd.Clear();
            }

            for (int i = _tasks.Count - 1; i >= 0; i--)
            {
                var task = _tasks[i];

                if (_tasksToRemove.Contains(task.Id))
                {
                    _tasks.RemoveAt(i);
                    _removeLock.Enter();
                    _tasksToRemove.Remove(task.Id);
                    _removeLock.Exit();
                    continue;
                }

                task.Update(deltaTime);

                if (task.IsCompleted())
                {
                    try 
                    {
                        task.Execute();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error while executing task {TaskId}", args: task.Id);
                    }
                    
                    if(task.CanBeRemoved())
                        _tasks.RemoveAt(i);
                }
            }
        }
    }
}

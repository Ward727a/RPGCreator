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

namespace RPGCreator.Core.Scheduler.Tasks.Time
{
    public class WaitSecondTask : BaseTask
    {
        private DateTime _startTime = DateTime.Now;
        public float RemainingTime { get; private set; } = 0f;

        public WaitSecondTask(float seconds, Action callback) : base(callback)
        {
            RemainingTime = seconds;
        }

        public override void Update(float deltaTime)
        {
            RemainingTime -= deltaTime;
        }

        public override bool IsCompleted()
        {
            return RemainingTime <= 0f;
        }
    }
}

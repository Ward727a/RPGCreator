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

namespace RPGCreator.Core.Scheduler.Tasks.Condition
{
    public class WaitUntilTask : BaseTask
    {
        private readonly Func<bool> _condition;
        public WaitUntilTask(Func<bool> condition, Action callback) : base(callback)
        {
            _condition = condition;
            Callback = callback;
        }

        public override bool IsCompleted()
        {
            return _condition.Invoke();
        }
    }
}

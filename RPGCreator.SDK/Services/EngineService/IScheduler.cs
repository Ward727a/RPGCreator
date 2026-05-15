// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
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

namespace RPGCreator.SDK.EngineService;

public abstract class BaseTask
{
    public Action Callback;
    public Guid Id { get; } = Guid.NewGuid();
    public abstract void Update(float deltaTime);
    public abstract bool IsCompleted();
    public abstract void Execute();
    public abstract bool CanBeRemoved();
}

public interface IScheduler : IService
{ 
    public Guid WaitSecond(float seconds, Action callback, bool loop = false);
    public Guid WaitUntil(Func<bool> condition, Action callback);
    public Guid AddTask(BaseTask task);
    public void CancelTask(Guid taskId);
    public void Update(float deltaTime);
}
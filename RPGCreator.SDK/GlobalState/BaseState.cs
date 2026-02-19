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

using System.ComponentModel;

namespace RPGCreator.SDK.GlobalState;

public abstract class BaseState : IState
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event PropertyChangingEventHandler? PropertyChanging;
    
    public bool SetProperty<T>(ref T field, T value, string propertyName = "")
    {
        if(EqualityComparer<T>.Default.Equals(field, value))
            return false;
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    public abstract void Reset();
    
    protected void CallPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    protected void CallPropertyChanging(string propertyName)
    {
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }
}
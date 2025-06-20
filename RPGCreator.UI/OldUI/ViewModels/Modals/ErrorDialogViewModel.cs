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
using CommunityToolkit.Mvvm.Input;
using RPGCreator.Core.Internal;
using RPGCreator.UI.Common;
using RPGCreator.UI.OLD.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace RPGCreator.UI.OLD.ViewModels.Modals;

public partial class ErrorDialogViewModel : ViewModelBase
{
    // Model
    //private readonly ErrorModel _launcherModel = new();

    // Commands
    //public ICommand OpenProjectCreator { get; }

    // Public variable
    public string Title { get; set; }
    public string Content { get; set; }
    public string OKButton {  get; set; }
    //public ObservableCollection<Project> ItemsList { get; set; }

    public ErrorDialogViewModel()
    {
        //OpenProjectCreator = new RelayCommand(_launcherModel.OpenSecondWindow);

        Title = "Ho";
        Content = "Hoho";
        OKButton = "OK";
    }

    public ErrorDialogViewModel(string title, string content, string oKButton = "OK")
    {
        Title = title;
        Content = content;
        OKButton = oKButton;
    }
}

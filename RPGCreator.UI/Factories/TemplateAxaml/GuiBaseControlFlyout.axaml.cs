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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using RPGCreator.SDK.GameUI.Controls;
using RPGCreator.UI.Common;

namespace RPGCreator.UI.Factories.TemplateAxaml;

public partial class GuiBaseControlFlyoutVm : ViewModelBase
{
    public BaseControl CurrentControl { get; set; }
    public ObservableCollection<BaseControl> Controls { get; set; } = new();

    public GuiBaseControlFlyoutVm(IEnumerable<BaseControl> controls, BaseControl currentControl)
    {
        CurrentControl = currentControl;
        foreach (var baseControl in controls)
        {
            Controls.Add(baseControl);
        }
    }
}

public partial class GuiBaseControlFlyout : UserControl
{
    public GuiBaseControlFlyout(IEnumerable<BaseControl> controls, BaseControl currentControl)
    {
        InitializeComponent();
        this.DataContext = new GuiBaseControlFlyoutVm(controls, currentControl);
    }
}
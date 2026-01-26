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

using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using RPGCreator.SDK.UiService;
using RPGCreator.UI.Content.Editor;

namespace RPGCreator.UI.Services;


public class DialogService : IDialogService
{
    #region Helpers
    
    protected Window GetParent(Window? manualOwner = null) 
    {
        if (manualOwner != null) return manualOwner;

        var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        if (lifetime == null) return EditorWindow.Instance;

        var active = lifetime.Windows.FirstOrDefault(w => w.IsActive);
        if (active != null) return active;

        var last = lifetime.Windows.LastOrDefault();
        if (last != null) return last;

        return EditorWindow.Instance;
    }
    
    #endregion
    
    public Task<string?> PromptTextAsync(string title, string message, string defaultText = "", DialogStyle style = new(), bool selectAllText = true)
    {
        return PromptTextAsync(title, (object)message, defaultText, style);
    }

    public Task<string?> PromptTextAsync(string title, object content, string defaultText = "", DialogStyle style = new(), bool selectAllText = true)
    {
        var promptWindow = new Window()
        {
            Title = title,
        };
        ApplyStyle(promptWindow, style);
        
        var stackPanel = new StackPanel { Margin = new Thickness(15), Spacing = 10 };
        promptWindow.Content = stackPanel;
        
        var inputBox = new TextBox { Text = defaultText, Watermark = "Enter text here..." };

        if (content is Control avaloniaContent)
        {
            stackPanel.Children.Add(avaloniaContent);
        }
        else
        {
            var textContent =
                new TextBlock()
                {
                    Text = content?.ToString() ?? "EMPTY CONTENT PROVIDED",
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                };
            
            inputBox.InnerLeftContent = textContent;
        }


        stackPanel.Children.Add(inputBox);
        
        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 10 };
        stackPanel.Children.Add(buttonPanel);
        
        var okButton = new Button { Content = "OK", IsDefault = true };
        buttonPanel.Children.Add(okButton);
        
        var cancelButton = new Button { Content = "Cancel", IsCancel = true };
        buttonPanel.Children.Add(cancelButton);
        
        okButton.Click += (_, _) =>
        {
            promptWindow.Close(inputBox.Text);
        };
        cancelButton.Click += (_, _) =>
        {
            promptWindow.Close();
        };
        
        promptWindow.Opened += (_, _) =>
        {
            inputBox.Focus();
            if(selectAllText)
                inputBox.SelectAll();
        };

        return promptWindow.ShowDialog<string?>(GetParent());
    }

    public Task<bool> ConfirmAsync(string title, string message, DialogStyle style = new(), string confirmButtonText = "OK", string cancelButtonText = "Cancel")
    {
        var confirmWindow = new Window { Title = title };
        ApplyStyle(confirmWindow, style);

        var stackPanel = new StackPanel { Margin = new Thickness(15), Spacing = 20 };
        confirmWindow.Content = stackPanel;
        
        var label = new TextBlock { Text = "Please confirm:", FontWeight = Avalonia.Media.FontWeight.Bold };
        stackPanel.Children.Add(label);
        
        var messageBlock = new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap };
        stackPanel.Children.Add(messageBlock);

        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 10 };
        stackPanel.Children.Add(buttonPanel);
        
        var yesButton = new Button { Content = confirmButtonText, IsDefault = true };
        buttonPanel.Children.Add(yesButton);
        
        var noButton = new Button { Content = cancelButtonText, IsCancel = true };
        buttonPanel.Children.Add(noButton);

        yesButton.Click += (_, _) => confirmWindow.Close(true);
        noButton.Click += (_, _) => confirmWindow.Close(false);

        return confirmWindow.ShowDialog<bool>(GetParent());
    }

    public Task<bool> ConfirmAsync(string title, object content, DialogStyle style = new DialogStyle(), string confirmButtonText = "OK", string cancelButtonText = "Cancel")
    {
        var confirmWindow = new Window { Title = title };
        ApplyStyle(confirmWindow, style);

        var stackPanel = new StackPanel { Margin = new Thickness(15), Spacing = 20 };
        confirmWindow.Content = stackPanel;
        
        if (content is Control avaloniaContent)
        {
            stackPanel.Children.Add(avaloniaContent);
        }
        else
        {
            var textContent =
                new TextBlock()
                {
                    Text = content?.ToString() ?? "EMPTY CONTENT PROVIDED",
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                };
            stackPanel.Children.Add(textContent);
        }

        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 10 };
        stackPanel.Children.Add(buttonPanel);
        
        var yesButton = new Button { Content = confirmButtonText, IsDefault = true };
        buttonPanel.Children.Add(yesButton);
        
        var noButton = new Button { Content = cancelButtonText, IsCancel = true };
        buttonPanel.Children.Add(noButton);

        yesButton.Click += (_, _) => confirmWindow.Close(true);
        noButton.Click += (_, _) => confirmWindow.Close(false);

        return confirmWindow.ShowDialog<bool>(GetParent());
    }

    public Task ShowMessageAsync(string title, string message, DialogStyle style = new())
    {
        var messageWindow = new Window { Title = title };
        ApplyStyle(messageWindow, style);

        var stackPanel = new StackPanel { Margin = new Thickness(15), Spacing = 20 };
        messageWindow.Content = stackPanel;
        
        var messageBlock = new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap };
        stackPanel.Children.Add(messageBlock);

        var okButton = new Button { Content = "OK", IsDefault = true, HorizontalAlignment = HorizontalAlignment.Right };
        stackPanel.Children.Add(okButton);

        okButton.Click += (_, _) => messageWindow.Close();

        return messageWindow.ShowDialog(GetParent());
    }

    public Task ShowErrorAsync(string title, string message, DialogStyle style = new())
    {
        var errorWindow = new Window { Title = title };
        ApplyStyle(errorWindow, style);

        var stackPanel = new StackPanel { Margin = new Thickness(15), Spacing = 20 };
        errorWindow.Content = stackPanel;
        
        var messageBlock = new TextBlock 
        { 
            Text = message, 
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Red)
        };
        stackPanel.Children.Add(messageBlock);

        var okButton = new Button { Content = "OK", IsDefault = true, HorizontalAlignment = HorizontalAlignment.Right };
        stackPanel.Children.Add(okButton);

        okButton.Click += (_, _) => errorWindow.Close();

        return errorWindow.ShowDialog(GetParent());
    }

    /// <summary>
    /// Applies the given DialogStyle to the specified Window.
    /// </summary>
    /// <param name="window">The window to style.</param>
    /// <param name="styleToApply">The DialogStyle to apply.</param>
    private void ApplyStyle(Window window, DialogStyle styleToApply)
    {
        if (styleToApply.IsDefault)
        {
            styleToApply = DialogStyle.Default;
        }

        if (styleToApply.SizeToContent == DialogSizeToContent.None)
        {
            window.Width = styleToApply.Width;
            window.Height = styleToApply.Height;
        } else if(styleToApply.SizeToContent == DialogSizeToContent.WidthOnly)
            window.Height = styleToApply.Height;
        else if(styleToApply.SizeToContent == DialogSizeToContent.HeightOnly)
            window.Width = styleToApply.Width;
        
        window.CanResize = styleToApply.CanResize;

        window.SizeToContent = styleToApply.SizeToContent switch
        {
            DialogSizeToContent.None => SizeToContent.Manual,
            DialogSizeToContent.WidthOnly => SizeToContent.Width,
            DialogSizeToContent.HeightOnly => SizeToContent.Height,
            _ => SizeToContent.WidthAndHeight
        };
        
        window.WindowStartupLocation = styleToApply.StartupLocation switch
        {
            DialogStartupLocation.Manual => WindowStartupLocation.Manual,
            DialogStartupLocation.CenterScreen => WindowStartupLocation.CenterScreen,
            _ => WindowStartupLocation.CenterOwner
        };

        window.SystemDecorations = styleToApply.SystemDecorations switch
        {
            DialogSystemDecorations.None => SystemDecorations.None,
            DialogSystemDecorations.BorderOnly => SystemDecorations.BorderOnly,
            _ => SystemDecorations.Full
        };

        if (styleToApply.StartupLocation == DialogStartupLocation.Manual)
        {
            window.Position = new PixelPoint(styleToApply.X, styleToApply.Y);
        }
    }
}
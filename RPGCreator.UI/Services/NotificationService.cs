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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using RPGCreator.SDK.EditorUiService;
using RPGCreator.UI.Content.Editor;
using Notification = Ursa.Controls.Notification;
using NotificationType = RPGCreator.SDK.EditorUiService.NotificationType;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace RPGCreator.UI.Services;

public class NotificationService : INotificationService
{
    public void ShowNotification(string title, string message, NotificationType type = NotificationType.Info,
        NotificationOptions options = default)
    {
        Dispatcher.UIThread.Post(() =>
        {

            options = CheckOptions(options);
            
            if(options.DurationMs == 0) // We don't want to have a duration of 0 without a close button
            {
                options.ShowClose = true;
            }
            
            var manager = GetWindowNotificationManager();

            var expiration = System.TimeSpan.FromMilliseconds(options.DurationMs);
            var notification = new Notification(title, message, ConvertNotificationType(type), expiration,
                options.ShowClose, options.OnClick, options.OnClose);
            notification.ShowIcon = options.ShowIcon;

            manager.Show(notification);
        });
    }

    public void ShowCustomNotification(object? content, NotificationType type = NotificationType.Info,
        NotificationOptions options = default)
    {
        Dispatcher.UIThread.Post(() =>
        {

            if (content == null) return;
            options = CheckOptions(options);

            var manager = GetWindowNotificationManager();

            if(options.DurationMs == 0)
            {
                options.ShowClose = true;
            }
            
            manager.Show(content, ConvertNotificationType(type),  System.TimeSpan.FromMilliseconds(options.DurationMs), options.ShowIcon, options.ShowClose, options.OnClick, options.OnClose, options.Classes);
            
        });
    }
    
    #region helpers

    private Window GetParent(Window? manualOwner = null) 
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

    private WindowNotificationManager GetWindowNotificationManager(Window? manualOwner = null)
    {
        var parent = GetParent(manualOwner);
        
        if (WindowNotificationManager.TryGetNotificationManager(parent, out WindowNotificationManager? manager))
        {
            return manager!;
        }

        manager = new WindowNotificationManager(parent)
        {
            Position = NotificationPosition.BottomRight,
            MaxItems = 5
        };
        return manager;
    }

    private Avalonia.Controls.Notifications.NotificationType ConvertNotificationType(NotificationType type)
    {
        return type switch
        {
            NotificationType.Info => Avalonia.Controls.Notifications.NotificationType.Information,
            NotificationType.Warning => Avalonia.Controls.Notifications.NotificationType.Warning,
            NotificationType.Error => Avalonia.Controls.Notifications.NotificationType.Error,
            NotificationType.Success => Avalonia.Controls.Notifications.NotificationType.Success,
            _ => Avalonia.Controls.Notifications.NotificationType.Information
        };
    }
    
    private NotificationOptions CheckOptions(NotificationOptions optionsToCheck)
    {
        return optionsToCheck.IsDefault ? NotificationOptions.Default : optionsToCheck;
    }
    
    #endregion
}
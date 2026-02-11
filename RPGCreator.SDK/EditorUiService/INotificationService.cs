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

namespace RPGCreator.SDK.EditorUiService;

public enum NotificationType
{
    Info,
    Warning,
    Error,
    Success
}

/// <summary>
/// Options for notifications.
/// </summary>
/// <param name="DurationMs">The duration of the notification in milliseconds.</param>
/// <param name="ShowIcon">True to show the icon, false otherwise.</param>
/// <param name="ShowClose">True to show the close button, false otherwise.</param>
/// <param name="OnClick">The action to perform when the notification is clicked.</param>
/// <param name="OnClose">The action to perform when the notification is closed.</param>
/// <param name="Classes">The additional classes to apply to the notification.</param>
public record struct NotificationOptions(
    int DurationMs = 3000,
    bool ShowIcon = true,
    bool ShowClose = true,
    Action? OnClick = null,
    Action? OnClose = null,
    string[]? Classes = null
)
{
    // ReSharper disable once ConvertToConstant.Local
    private readonly bool _isNotDefault = true;
    public bool IsDefault => !_isNotDefault;
    
    public static NotificationOptions Default => new(3000);
}

/// <summary>
/// A service to show notifications to the user.
/// </summary>
public interface INotificationService : IService
{
    /// <summary>
    /// Shows a notification message.<br/>
    /// This method allows to customize the notification type and options.
    /// </summary>
    /// <param name="title">The notification title.</param>
    /// <param name="message">The notification message.</param>
    /// <param name="type">The notification type.</param>
    /// <param name="options">The notification options.</param>
    void ShowNotification(string title, string message, NotificationType type = NotificationType.Info,
        NotificationOptions options = default);

    /// <summary>
    /// Shows a custom notification message.<br/>
    /// This method allows to provide any content as notification.
    /// </summary>
    /// <param name="content">The notification content.</param>
    /// <param name="type">The notification type.</param>
    /// <param name="options">The notification options.</param>
    void ShowCustomNotification(object? content, NotificationType type = NotificationType.Info,
        NotificationOptions options = default);
    
    /// <summary>
    /// Shows a warning notification message.<br/>
    /// Shortcut for ShowNotification with NotificationType.Warning.
    /// </summary>
    /// <param name="title">The notification title.</param>
    /// <param name="message">The notification message.</param>
    /// <param name="options">The notification options.</param>
    public void Warn(string title, string message, NotificationOptions options = default) =>
        ShowNotification(title, message, NotificationType.Warning, options);
    
    /// <summary>
    /// Shows an error notification message.<br/>
    /// Shortcut for ShowNotification with NotificationType.Error.
    /// </summary>
    /// <param name="title">The notification title.</param>
    /// <param name="message">The notification message.</param>
    /// <param name="options">The notification options.</param>
    public void Error(string title, string message, NotificationOptions options = default) =>
        ShowNotification(title, message, NotificationType.Error, options);
    
    /// <summary>
    /// Shows an info notification message.<br/>
    /// Shortcut for ShowNotification with NotificationType.Info.
    /// </summary>
    /// <param name="title">The notification title.</param>
    /// <param name="message">The notification message.</param>
    /// <param name="options">The notification options.</param>
    public void Info(string title, string message, NotificationOptions options = default) =>
        ShowNotification(title, message, NotificationType.Info, options);
    
    /// <summary>
    /// Shows a success notification message.<br/>
    /// Shortcut for ShowNotification with NotificationType.Success.
    /// </summary>
    /// <param name="title">The notification title.</param>
    /// <param name="message">The notification message.</param>
    /// <param name="options">The notification options.</param>
    public void Success(string title, string message, NotificationOptions options = default) =>
        ShowNotification(title, message, NotificationType.Success, options);
}
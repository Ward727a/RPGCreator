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

/// <summary>
/// Allows to define the startup location of a dialog.
/// </summary>
public enum DialogStartupLocation
{
    /// <summary>
    /// Positioned manually.
    /// </summary>
    Manual,
    /// <summary>
    /// Centered on the screen.
    /// </summary>
    CenterScreen,
    /// <summary>
    /// Centered on the owner window.<br/>
    /// This is the default behavior.
    /// </summary>
    CenterOwner,
}

public enum DialogSizeToContent
{
    /// <summary>
    /// The dialog size is defined by the content size.
    /// </summary>
    WidthAndHeight,
    /// <summary>
    /// The dialog width is defined by the content size.
    /// </summary>
    WidthOnly,
    /// <summary>
    /// The dialog height is defined by the content size.
    /// </summary>
    HeightOnly,
    /// <summary>
    /// The dialog size is not defined by the content size.<br/>
    /// This is the default behavior.
    /// </summary>
    None,
}

/// <summary>
/// Allows to define the system decorations of a dialog.
/// </summary>
public enum DialogSystemDecorations
{
    /// <summary>
    /// No system decorations.
    /// </summary>
    None,
    /// <summary>
    /// Only the border system decorations.
    /// </summary>
    BorderOnly,
    /// <summary>
    /// Full system decorations (e.g., title bar, border, etc.).<br/>
    /// This is the default behavior.
    /// </summary>
    Full,
}

/// <summary>
/// Allows to define the style of a dialog.
/// </summary>
/// <param name="Width">The width of the dialog.</param>
/// <param name="Height">The height of the dialog.</param>
/// <param name="X">The X position of the dialog (Only if StartupLocation is Manual).</param>
/// <param name="Y">The Y position of the dialog (Only if StartupLocation is Manual).</param>
/// <param name="CanResize">Whether the dialog can be resized.</param>
/// <param name="StartupLocation">The startup location of the dialog.</param>
/// <param name="SystemDecorations">The system decorations of the dialog.</param>
public record struct DialogStyle(
    int Width = 300,
    int Height = 200,
    int X = 0,
    int Y = 0,
    bool CanResize = false,
    DialogStartupLocation StartupLocation = DialogStartupLocation.CenterScreen,
    DialogSystemDecorations SystemDecorations = DialogSystemDecorations.Full,
    DialogSizeToContent SizeToContent = DialogSizeToContent.WidthAndHeight)
{
    private bool _isNotDefault = true;
    public bool IsDefault => !_isNotDefault;
    
    public static DialogStyle Default { get; } = new(300);
}

/// <summary>
/// The dialog service interface for showing various types of dialogs to the user.<br/>
/// DevNote: This service is used to interact with the user through dialogs, such as prompts, confirmations, messages, and errors.
/// It provides a simple way to display dialogs and get user input in a consistent manner.
/// The implementation of this service should handle the actual rendering and behavior of the dialogs.
/// </summary>
public interface IDialogService : IService
{
    /// <summary>
    /// Show a prompt dialog to get text input from the user.
    /// </summary>
    /// <param name="title">The title of the prompt dialog.</param>
    /// <param name="message">The message to display in the prompt dialog.</param>
    /// <param name="defaultText">The default text to display in the input field.</param>
    /// <param name="style">The style of the dialog.</param>
    /// <param name="selectAllText">Whether to select all text in the input field when the dialog is shown.</param>
    /// <returns>The text input by the user, or null if the user canceled the prompt.</returns>
    /// <exception cref="NotImplementedException">Thrown if the UI framework does not support this operation.</exception>
    Task<string?> PromptTextAsync(string title, string message, string defaultText = "", DialogStyle style = new(), bool selectAllText = true);

    /// <summary>
    /// Show a prompt dialog to get text input from the user, with custom content.<br/>
    /// As content, you can provide any Avalonia control or object that can be rendered in the dialog.<br/>
    /// In any case, the dialog will have an input field for the user to type text at the bottom.
    /// </summary>
    /// <param name="title">The title of the prompt dialog.</param>
    /// <param name="content">The content to display in the prompt dialog.</param>
    /// <param name="defaultText">The default text to display in the input field.</param>
    /// <param name="style">The style of the dialog.</param>
    /// <param name="selectAllText">Whether to select all text in the input field when the dialog is shown.</param>
    /// <returns>
    /// The text input by the user, or null if the user canceled the prompt.
    /// </returns>
    /// <exception cref="NotImplementedException">Thrown if the UI framework does not support this operation.</exception>
    Task<string?> PromptTextAsync(string title, object content, string defaultText = "", DialogStyle style = new(), bool selectAllText = true);
    
    /// <summary>
    /// Show a confirmation dialog to the user.
    /// </summary>
    /// <param name="title">The title of the confirmation dialog.</param>
    /// <param name="message">The message to display in the confirmation dialog.</param>
    /// <param name="style">The style of the dialog.</param>
    /// <returns>
    /// True if the user confirmed, false if the user canceled.
    /// </returns>
    /// <exception cref="NotImplementedException">Thrown if the UI framework does not support this operation.</exception>
    Task<bool> ConfirmAsync(string title, string message, DialogStyle style = new(), string confirmButtonText = "OK", string cancelButtonText = "Cancel");
    
    /// <summary>
    /// Show a confirmation dialog to the user, with custom content.<br/>
    /// As content, you can provide any Avalonia control or object that can be rendered in the dialog.
    /// </summary>
    /// <param name="title">The title of the confirmation dialog.</param>
    /// <param name="content">The content to display in the confirmation dialog.</param>
    /// <param name="style">The style of the dialog.</param>
    /// <exception cref="NotImplementedException">Thrown if the UI framework does not support this operation.</exception>
    /// <returns>
    /// Returns true if the user confirmed, false if the user canceled.
    /// </returns>
    Task<bool> ConfirmAsync(string title, object content, DialogStyle style = new(), string confirmButtonText = "OK", string cancelButtonText = "Cancel");
    
    /// <summary>
    /// Show a message dialog to the user.
    /// </summary>
    /// <param name="title">The title of the message dialog.</param>
    /// <param name="message">The message to display in the message dialog.</param>
    /// <param name="style">The style of the dialog.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="NotImplementedException">Thrown if the UI framework does not support this operation.</exception>
    Task ShowMessageAsync(string title, string message, DialogStyle style = new());
    
    /// <summary>
    /// Show a message dialog to the user, with custom content.<br/>
    /// As content, you can provide any Avalonia control or object that can be rendered in the dialog.<br/>
    /// </summary>
    /// <param name="title">The title of the message dialog.</param>
    /// <param name="content">The content to display in the message dialog.</param>
    /// <param name="style">The style of the dialog.</param>
    /// <exception cref="NotImplementedException">Thrown if the UI framework does not support this operation.</exception>
    /// <returns></returns>
    Task ShowPromptAsync(string title, object content, DialogStyle style = new());
    
    /// <summary>
    /// Show an error dialog to the user.
    /// </summary>
    /// <param name="title">The title of the error dialog.</param>
    /// <param name="message">The message to display in the error dialog.</param>
    /// <param name="style">The style of the dialog.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="NotImplementedException">Thrown if the UI framework does not support this operation.</exception>
    Task ShowErrorAsync(string title, string message, DialogStyle style = new());
    
    /// <summary>
    /// Show a dialog allowing the user to select an item from a list of items.
    /// </summary>
    /// <param name="title">The title of the selection dialog.</param>
    /// <param name="message">The message to display in the selection dialog.</param>
    /// <param name="items">The list of items to select from.</param>
    /// <param name="labelSelector">A function to select the label to display for each item. If null, the ToString() method of the item will be used.</param>
    /// <param name="style">The style of the dialog.</param>
    /// <param name="confirmButtonText">The text to display on the confirm button.</param>
    /// <param name="cancelButtonText">The text to display on the cancel button.</param>
    /// <typeparam name="T">The type of the items to select from.</typeparam>
    /// <returns>
    /// The item selected by the user, or default(T) if the user canceled the selection.
    /// </returns>
    Task<T?> ShowSelectAsync<T>(string title, string message, IEnumerable<T> items, Func<T, string>? labelSelector = null, DialogStyle style = new(), string confirmButtonText = "OK", string cancelButtonText = "Cancel");
    
}
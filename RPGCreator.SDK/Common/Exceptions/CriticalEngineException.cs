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

using Newtonsoft.Json;

namespace RPGCreator.SDK.Exceptions;

/// <summary>
/// A critical exception that should never happen during normal engine operation.<br/>
/// If you encounter this exception, please report it to the developer with as much information as possible.
/// This helps in identifying and fixing potential bugs in the engine.<br/>
/// This exception can optionally include a dump of an object to provide additional context for debugging.
/// </summary>
public class CriticalEngineException : Exception
{
    
    private const string DefaultPrefix = "[CRITICAL ENGINE EXCEPTION - THIS SHOULD NOT HAPPEN - PLEASE REPORT THIS TO THE DEVELOPER] ";
    
    /// <summary>
    /// Creates a new CriticalEngineException with an optional object dump for debugging purposes.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="dumpObject">An optional object to dump for additional debugging information.</param>
    public CriticalEngineException(string message, object? dumpObject = null) : base(FormatMessage(message, dumpObject))
    {
    }

    /// <summary>
    /// Throws a new CriticalEngineException with an inner exception and an optional object dump for debugging purposes.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception that caused this exception.</param>
    /// <param name="dumpObject">An optional object to dump for additional debugging information.</param>
    public CriticalEngineException(string message, Exception innerException, object? dumpObject = null) : base(FormatMessage(message, dumpObject, innerException), innerException)
    {
    }
    
    /// <summary>
    /// Formats the exception message by adding a default prefix, dumping the provided object (if any), and including inner exception details (if any).<br/>
    /// This helps in providing a comprehensive context for debugging critical engine exceptions.<br/>
    /// It also includes warnings about potential sensitive data in the object dump.
    /// </summary>
    /// <param name="message">The base exception message.</param>
    /// <param name="dumpObject">The object to dump for additional debugging information.</param>
    /// <param name="innerException">The inner exception, if any.</param>
    /// <returns>
    /// The formatted exception message.
    /// </returns>
    private static string FormatMessage(string message, object? dumpObject, Exception? innerException = null)
    {
        var finalMessage = "CAUTION: This is a critical engine exception. Please report this to the developer.";
        
        if(dumpObject != null)
        {
            finalMessage += "\nThe exception includes an object dump for debugging purposes." +
                            "Be cautious as it may contain sensitive data.";
        }
        
        finalMessage += $"{DefaultPrefix}{message}";
        
        if (dumpObject != null)
        {
            finalMessage += "\n\n--- ADDITIONAL DEBUG INFORMATION ---";
            finalMessage += "\n----------------------------------";
            finalMessage += "\n--- PLEASE READ BEFORE SHARING ---";
            finalMessage += "\n----------------------------------";
            finalMessage += "\n/!\\ CAUTION: THE DUMP BELOW MAY CONTAIN SENSITIVE DATA FROM YOUR PROJECT OR SYSTEM PATHS.";
            finalMessage += "\n/!\\ DO NOT SHARE THIS LOG PUBLICLY UNLESS YOU TRUST THE RECIPIENT.";
            try 
            {
                var settings = new JsonSerializerSettings 
                { 
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Error = (sender, args) => { args.ErrorContext.Handled = true; } 
                };
    
                var jsonDump = JsonConvert.SerializeObject(dumpObject, settings);
                finalMessage += $"\n--- OBJECT DUMP ---\n{jsonDump}\n-------------------";
            }
            catch (Exception ex)
            {
                finalMessage += $"\n(Failed to dump object of type {dumpObject.GetType().Name}. Reason: {ex.Message})";
            }
        }
        
        if (innerException != null)
        {
            finalMessage += $"\n--- INNER EXCEPTION ---\n{innerException}\n-----------------------";
        }
        
        return finalMessage;
    }
}
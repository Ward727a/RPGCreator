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

namespace RPGCreator.SDK.Attributes;

/// <summary>
/// Define an engine class.<br/>
/// The engine uses this attribute to identify classes and to provide metadata for them.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class EClassAttribute : Attribute
{
    /// <summary>
    /// (optional - default: Class Name)<br/>
    /// Define the name of the class.<br/>
    /// E.g.: "LavaArmor"
    /// <remarks>
    /// If not set, the engine will use the class name directly (for example, "public class MyLavaArmor" will use "MyLavaArmor" as the <see cref="Name"/>
    /// </remarks>
    /// </summary>
    public string Name { get; set; } = "";
    /// <summary>
    /// (optional - default: <see cref="Name"/>)<br/>
    /// Define the display name of the class.<br/>
    /// E.g.: "Lava Armor"
    /// <remarks>
    /// If not set, the engine will use the <see cref="Name"/> property as the display name.
    /// </remarks>
    /// </summary>
    public string DisplayName { get; set; } = "";
    /// <summary>
    /// (optional - default: empty)<br/>
    /// A short description of the class.<br/>
    /// E.g.: "An item that protects the player from lava damage."
    /// </summary>
    public string Description { get; set; } = "";
    /// <summary>
    /// (optional - default: empty)<br/>
    /// An icon for the class.<br/>
    /// E.g.: "mdi-shield"<br/>
    /// For a complete list of supported icons, see: <a href="https://pictogrammers.com/library/mdi/">Material Design Icons</a>
    /// </summary>
    public string Icon { get; set; } = "";
    /// <summary>
    /// (optional - default: EngineClass)<br/>
    /// Define the category of the class for organization and filtering purposes.<br/>
    /// E.g.: "Items", "Gui"<br/>
    /// It also supports subcategories using |, e.g.: "Items|Weapons"<br/>
    /// E.g.: "Items|Weapons|Swords", "Gui|Inputs"
    /// </summary>
    public string Category { get; set; } = "EngineClass";
    /// <summary>
    /// (optional - require <see cref="EClassAttribute.SupportSerialization">SupportSerialization</see>)<br/>
    /// Define the path to the serialization folder for this class.<br/>
    /// E.g.: "Items/Armor" will be serialized to <c>[PROJECT FOLDER]/Assets/Items/Armor/[ID].json</c><br/>
    /// If set to an empty string, the folder will be <c>[PROJECT FOLDER]/Assets/[ID].json</c>.
    /// <remarks>
    /// For this to work, you need to set <see cref="EClassAttribute.SupportSerialization">SupportSerialization</see> to true.
    /// </remarks>
    /// </summary>
    public string SerializationPath { get; set; } = "";
    /// <summary>
    /// (optional)<br/>
    /// Define if the class support serialization.<br/>
    /// If set to true, the engine will automatically serialize and deserialize the class when saving and loading games.
    /// <remarks>
    /// You can override the serialization path by setting <see cref="EClassAttribute.SerializationPath">SerializationPath</see> to a custom folder.<br/>
    /// You can also override the serialize and deserialize method by implementing the partial methods "Serialize" and "Deserialize".
    /// </remarks>
    /// </summary>
    public bool SupportSerialization { get; set; } = false;
    /// <summary>
    /// (optional)<br/>
    /// Define if the class support dirty flag.<br/>
    /// Important note: This property alone does NOTHING! You need to implement the [EProperty] attribute on each property that need to be tracked for changes.<br/>
    /// E.g.:
    /// <code>
    /// <![CDATA[
    /// [EClass([...], SupportDirtyFlag = true)]
    /// public class MaClass {
    ///     
    ///     [EProperty([...], Dirtying = true)]
    ///     private int _myProperty; // Note: We need to declare it has "private" and WITHOUT {get; set;}!
    ///     // If you really need to edit the "get" or "set", then check the partial void method "Getter[PropertyName]" and "Setter[PropertyName]"!
    ///     
    /// }
    /// ]]>
    /// </code>
    /// Here, MyProperty will be tracked for changes, meaning that when "MyProperty" is modified, the class instance will be marked as dirty.<br/>
    /// To un-mark the class as dirty, either save it or call the "MarkAsClean()" method on the class instance.
    /// </summary>
    public bool SupportDirtyFlag { get; set; } = false;
    
    /// <summary>
    /// (optional - default: C# Project Name)<br/>
    /// Define the namespace of the class.<br/>
    /// E.g.: If the namespace is "RPGCreator.Items", then the full urn will be: "RPGCreator.Items://[Module]/[Name]/[Id]".
    /// <remarks>
    /// By default, the namespace is the same as the C# project name, so if your project is named "RPGCreator.SDK", then the namespace will be "RPGCreator.SDK".
    /// </remarks>
    /// </summary>
    public string UrnNamespace { get; set; } = "";
    
    /// <summary>
    /// (optional - default: <see cref="Category"/>)
    /// Define the module of the class.<br/>
    /// E.g.: If the module is "Items", then the full urn will be: "[Namespace]://Items/[Name]/[Id]".<br/>
    /// E.g.: If the module is "Items/Weapons", then the full urn will be: "[Namespace]://Items/Weapons/[Name]/[Id]".
    /// <remarks>
    /// By default, the module is the same as the <see cref="Category"/> property, so if you do not define a <see cref="Category"/>, the default module will be "EngineClass".
    /// </remarks>
    /// </summary>
    public string UrnModule { get; set; } = "";
}
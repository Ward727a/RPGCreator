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

using RPGCreator.SDK.Types;

namespace RPGCreator.Tests.IO;

public class FilePath
{
    // All those are FAKE paths, they are not real.
    private const string ProjectPath = @"C:\Users\Test\Documents\Project\MyProject";
    private const string AppDataPath = @"C:\Users\Test\AppData\Local\MyProject";
    private const string ExecutablePath = @"C:\Program Files\RPGCreator";
    
    private const string TestPath1 = @"%project%\Assets\test.json";
    private const string TestPath2 = @"%appdata%\test.json";
    private const string TestPath3 = @"%exe%\test.json";
    private const string TestPath4 = @"%temp%\test.json";

    private class MockupPathResolver : IPathResolver
    {
        public string Resolve(SDK.Types.FilePath path)
        {
            return path.LogicalPath
                .Replace("%project%", ProjectPath)
                .Replace("%appdata%", AppDataPath)
                .Replace("%exe%", ExecutablePath)
                .Replace("%temp%", System.IO.Path.GetTempPath());
        }
    }
    
    private static readonly IPathResolver PathResolver = new MockupPathResolver();

    [Fact(DisplayName = "Test %project%")]
    public void TestProjectPath()
    {
        var resolvedPath = PathResolver.Resolve(new SDK.Types.FilePath(TestPath1));
        
        Assert.Equal($@"{ProjectPath}\Assets\test.json", resolvedPath);
    }
    
    [Fact(DisplayName = "Test %appdata%")]
    public void TestAppDataPath()
    {
        var resolvedPath = PathResolver.Resolve(new SDK.Types.FilePath(TestPath2));
        
        Assert.Equal($@"{AppDataPath}\test.json", resolvedPath);
    }
    
    [Fact(DisplayName = "Test %exe%")]
    public void TestExecutablePath()
    {
        var resolvedPath = PathResolver.Resolve(new SDK.Types.FilePath(TestPath3));
        
        Assert.Equal($@"{ExecutablePath}\test.json", resolvedPath);
    }

    [Fact(DisplayName = "Test %temp%")]
    public void TestTempPath()
    {
        var resolvedPath = PathResolver.Resolve(new SDK.Types.FilePath(TestPath4));
        
        Assert.Equal($@"{Path.GetTempPath()}\test.json", resolvedPath);
    }    
}
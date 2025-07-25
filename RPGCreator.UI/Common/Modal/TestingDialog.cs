using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using RPGCreator.Core;
using RPGCreator.Core.Type.Assets.BaseAssetsPack;

namespace RPGCreator.UI.Common.Windows;

public class TestingDialog : Window
{
    private struct TestButton(string name, Action action)
    {
        public string Name = name;
        public Action Action = action;
    }

    private static TestingDialog instance;
    
    private TestButton[] Testbuttons =
    [
        new("Test Save pack", () =>
        {
            // Add your test logic here
            var pack = EngineCore.Instance.Managers.Assets.GetAssetsPacks()[0];
            if (pack == null)
            {
                Console.WriteLine("No assets pack found to test saving.");
                return;
            }
            Console.WriteLine($"Testing save for pack: {pack.Name}");
            pack.Save();
        }),
        new("Test Load pack", () =>
        {
            var textDialog = new TextInputDialog("Enter the path to the pack to load:");
            textDialog.Confirmed += (path) =>
            {
                EngineSerializer.Instance.Deserialize(File.ReadAllText(path), out object? pack, out Type? type);
                if (type == typeof(BaseAssetsPack))
                {
                    Console.WriteLine($"Pack loaded successfully: {((BaseAssetsPack)pack).Name}");
                }
                else
                {
                    Console.WriteLine("Loaded object is not a BaseAssetsPack.");
                }
            };
            textDialog.ShowDialog(instance);
        })
    ];
    
    /// <summary>
    /// This dialog is used for testing purposes only, if you want to test something, you can add it here.
    /// </summary>
    public TestingDialog()
    {
        instance = this;
        Title = "Testing Dialog";
        Width = 400;
        Height = 300;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        var contentPanel = new StackPanel
        {
            Margin = new Thickness(10)
        };
        
        Content = contentPanel;

        foreach (var testButton in Testbuttons)
        {
            var button = new Button
            {
                Content = testButton.Name,
                Margin = new Thickness(5),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Width = 200,
                Height = 40,
                FontSize = 16
            };
            button.Click += (s, e) =>
            {
                try
                {
                    testButton.Action.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing test action: {ex.Message}");
                }
            };
            contentPanel.Children.Add(button);
        }
    }
    
}
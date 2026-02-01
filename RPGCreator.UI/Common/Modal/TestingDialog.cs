using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets;
using RPGCreator.UI;

namespace RPGCreator.Core.Types.Windows;

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
            var pack = EngineServices.AssetsManager.GetLoadedPacks()[0];
            if (pack == null)
            {
                Console.WriteLine("No assets pack found to test saving.");
                return;
            }
            Console.WriteLine($"Testing save for pack: {pack.Name}");
            pack.Save();
        }),
        new("Close Pack", () =>
        {
            var pack = EngineServices.AssetsManager.GetLoadedPacks()[0];
            if (pack == null)
            {
                Console.WriteLine("No assets pack found to test closing.");
                return;
            }
            Console.WriteLine($"Testing close for pack: {pack.Name}");
            EngineServices.AssetsManager.UnregisterPack(pack.Id);
        }),
        new("Test Load pack", () =>
        {
            var textDialog = new TextInputDialog("Enter the path to the pack to load:");
            textDialog.Confirmed += (path) =>
            {
                EngineServices.SerializerService.Deserialize(File.ReadAllText(path), out object? pack, out System.Type? type);
                if (type == typeof(IAssetsPack))
                {
                    Console.WriteLine($"Pack loaded successfully: {((IAssetsPack)pack).Name}");
                }
                else
                {
                    Console.WriteLine("Loaded object is not a BaseAssetsPack.");
                }
            };
            textDialog.ShowDialog(instance);
        }),
        // new("Open icons explorer", () =>
        // {
        //     var iconsExplorer = new IconsExplorer();
        //     iconsExplorer.ShowDialog(instance);
        // })
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
        // TEST ASSETS EXPLORER DIALOG
        #if DEBUG
        
        var buttonTestAssetsExplorer = new Button
        {
            Content = "Test Assets Explorer",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin
        };
        buttonTestAssetsExplorer.Click += (sender, args) =>
        {
            var assetsExplorer = new AssetExplorerDialog();
            assetsExplorer.Show();
        };
        contentPanel.Children.Add(buttonTestAssetsExplorer);
        
        #endif
        
        // TEST FORMULA PRATT PARSE
        #if DEBUG
        
        var testFormula = new TextBox
        {
            Watermark = "(Optional)",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin,
        };
        var inputTestFormula = new InputLabel("Test Formula", testFormula);
        contentPanel.Children.Add(inputTestFormula);
        var buttonTestFormula = new Button
        {
            Content = "Test Formula",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin
        };
        // buttonTestFormula.Click += (sender, args) =>
        // {
        //     if (string.IsNullOrEmpty(testFormula.Text))
        //     {
        //         Logger.Error("Test formula is empty, please enter a valid formula to test.");
        //         return;
        //     }
        //     try
        //     {
        //         PrattCompiler compiler = new PrattCompiler();
        //         var result = compiler.Compile(testFormula.Text);
        //         if (result != null)
        //         {
        //             var value = result.Eval(
        //                 new PrattEvaluationEnvironment()
        //                 {
        //                     Variables = new ReadOnlyDictionary<string, double>(
        //                         new Dictionary<string, double>()
        //                         {
        //                             ["testVar"] = 20.0
        //                         })
        //                 });
        //             Log.Debug($"PrattCompiledFormula: {value}");
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Log.Error("Got error while testing formula: {Message}", e.Message);
        //     }
        // };
        contentPanel.Children.Add(buttonTestFormula);
        
        #endif
    }
    
}
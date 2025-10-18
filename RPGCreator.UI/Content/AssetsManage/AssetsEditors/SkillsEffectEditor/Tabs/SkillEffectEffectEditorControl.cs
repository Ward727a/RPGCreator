using System.Collections.Generic;
using Avalonia.Controls;
using RPGCreator.Core.Parser.Graph;
using RPGCreator.Core.Type.Assets.Skills;
using RPGCreator.Core.Type.Blueprint;
using RPGCreator.Core.Type.Blueprint.Nodes;
using RPGCreator.UI.Common.Blueprint;
using Serilog;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.SkillsEffectEditor;

public class SkillEffectEffectEditorControl : UserControl
{
    
    
    #region Constants
    #endregion
    
    #region Events
    #endregion
    
    #region Properties
    
    public GraphDocumentCompiled CompiledDocument => GraphDocumentCompiler.Compile(_doc);
    
    private List<SkillEffectPropertyDescriptor> _effectProperties;
    private GraphDocument _doc = new();
    
    #endregion
    
    #region Components
    
    private Grid Body { get; set; }
    private GraphView Graph { get; set; }
    
    #endregion
    
    #region Constructors

    public SkillEffectEffectEditorControl(List<SkillEffectPropertyDescriptor> properties)
    {
        _effectProperties = properties;
        // Initialize UI components here
        Content = new TextBlock
        {
            Text = "Effect Editor (Graph) - To be implemented",
            Margin = App.style.Margin,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        CreateComponents();
        RegisterEvents();
        RefreshGraphVariables();

        Content = Body;
    }

    #endregion
    
    #region Methods
    
    public void SetSkillEffectProperties(List<SkillEffectPropertyDescriptor> properties)
    {
        _effectProperties = properties;
        RefreshGraphVariables();
    }
    
    private void CreateComponents()
    {
        Body = new Grid
        {
            Margin = App.style.Margin,
            RowDefinitions = new RowDefinitions("*, Auto")
        };

        Graph = new GraphView
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };

        Body.Children.Add(Graph);
        Grid.SetRow(Graph, 0);
        
        // We move the start and end nodes so it's not hidden by the variables panel
        _doc.AddNode(new NodeStart(){Y = 300});
        _doc.AddNode(new NodeEnd() { Y=300, X = 500 });
        
        Graph.SetDocument(_doc);
    }
    
    private void RegisterEvents()
    {
        // Register event handlers here
    }

    private void RefreshGraphVariables()
    {
        _doc.ClearGraphVariables();
        Log.Debug("Refreshing graph variables...");
        foreach (var propertyDescriptor in _effectProperties)
        {

            var propertyPath = $"skill_effect.props.{propertyDescriptor.Name}";
            
            _doc.AddGraphVariable(propertyPath, typeof(SkillEffectPropertyDescriptor), propertyDescriptor);
            Log.Debug("Added graph variable: {PropertyPath}", propertyPath);
        }
        
        Log.Debug("Total graph variables: {Count} from {Count2} total descriptors.", _doc.GraphVariables.Count, _effectProperties.Count);
    }
    
    #endregion

    #region Events Handlers
    #endregion
    
}
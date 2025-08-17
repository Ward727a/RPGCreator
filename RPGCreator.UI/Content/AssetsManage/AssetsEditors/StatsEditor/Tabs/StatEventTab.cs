
using Avalonia.Controls;
using RPGCreator.Core.Type.Assets.Characters.Stats;
using RPGCreator.UI.Common.Blueprint;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.StatsEditor.Tabs;

/// <summary>
/// This will manage what happens for different "events" related to the stat in the game (e.g. When the stat come to the minimum value, when the stat is modified, etc.).
/// </summary>
public class StatEventTab : UserControl
{
    #region Constants
    #endregion
    
    #region Events
    #endregion
    
    #region Properties
    public IStatDef StatDef { get; private set; }
    private readonly GraphDocument _doc = new();
    #endregion
    
    #region Components

    #endregion
    
    #region Constructors
    public StatEventTab(IStatDef statDef)
    {
        StatDef = statDef;
        CreateComponents();
    }
    #endregion
    
    #region Methods

    private void CreateComponents()
    {
        var graph = new GraphView();
        Content = graph;

        var n1 = new Node { Title = "Start", X = 50, Y = 100 };
        n1.Outputs.Add(new Port { Name = "Out", Kind = PortKind.Exec });
        _doc.AddNode(n1);

        var n2 = new Node { Title = "Print", X = 300, Y = 120 };
        n2.Inputs.Add(new Port { Name = "In", Kind = PortKind.Exec, IsInput = true });
        _doc.AddNode(n2);
        
        // Add a third node for demonstration
        var n3 = new Node { Title = "End", X = 550, Y = 100 };
        n3.Inputs.Add(new Port { Name = "In", Kind = PortKind.Exec, IsInput = true});
        n3.Outputs.Add(new Port { Name = "Out", Kind = PortKind.Exec });
        _doc.AddNode(n3);

        _doc.AddLink(new Link(n1.Id, n1.Outputs[0].Id, n2.Id, n2.Inputs[0].Id));

        graph.SetDocument(_doc);
    }
    #endregion

    #region Events Handlers
    #endregion
}
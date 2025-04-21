using LIB.PanelsModel;

namespace Lib.CutOptimizer
{
  public class Optimizer
  {
    private readonly Panel _basePanel = new();
    private List<Panel> _panelsToBeStowed = new();
    private List<Panel> _freeRectangles = new();

    public Optimizer(int basePanelWidth, int basePanelHeight) 
    {
      _basePanel.Width = basePanelWidth;
      _basePanel.Height = basePanelHeight;

      _freeRectangles.Add(new Panel
      {
        PanelName = "Base",
        Width = basePanelWidth,
        Height = basePanelHeight
      });
    }

    public List<PackedBasePanel> OptimizePanels(bool rotate = false)
    {
      if (!_panelsToBeStowed.Any()) return [];

      var optimizedPanels = new List<PackedBasePanel>();

      var rectsSolver = new MaxRectsSolver(_basePanel.Width, _basePanel.Height, rotate);

      return rectsSolver.Pack(_panelsToBeStowed);
    }

    public void AddPanel(int panelx, int panely, string? panelName = null)
    {
      var name = string.IsNullOrEmpty(panelName) ? (_panelsToBeStowed.Count + 1).ToString() : panelName;
      _panelsToBeStowed.Add(new Panel { PanelName = name, Width = panelx, Height = panely });
    }

    public void RemovePanel(string panelName)
    {
      if (_panelsToBeStowed.Any()) 
      { 
        _panelsToBeStowed.Remove(_panelsToBeStowed.First(x => x.PanelName == panelName));
      }
    }

    public void ModifyPanel(string panelName, int? panelx = null, int? panely = null) 
    {
      if (_panelsToBeStowed.Any(x => x.PanelName == panelName))
      {
        if (panelx != null)
        {
          _panelsToBeStowed.First(x => x.PanelName == panelName).Width = panelx.Value;
        }

        if (panely != null)
        {
          _panelsToBeStowed.First(x => x.PanelName == panelName).Width = panely.Value;
        }
      }
    }
  }
}

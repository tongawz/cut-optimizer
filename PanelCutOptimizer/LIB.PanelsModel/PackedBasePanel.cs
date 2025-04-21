namespace LIB.PanelsModel
{
  public class PackedBasePanel : Panel
  {
    public int Index { get; set; }
    public List<PositionedPanel> PlacedPanels { get; set; } = [];
    public decimal UsagePercentage { get { return PlacedPanels.Sum(x => x.AreaM2) / this.AreaM2; } }
    public string FormattedUsagePercentage => (UsagePercentage*100).ToString("F2");
  }
}

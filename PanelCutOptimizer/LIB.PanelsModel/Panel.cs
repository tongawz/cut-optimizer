namespace LIB.PanelsModel
{
  public class Panel
  {
    public int? index { get; set; }
    public string? PanelName { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public decimal AreaM2 { get { return (Width / 100m) * (Height / 100m); } }
    public int MaxDimension { get { return Width >= Height ? Width : Height; } }
    public int MinDimension { get { return Width >= Height ? Height : Width; } }
  }
}

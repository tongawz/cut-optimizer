namespace CMD.CutOptimizerTest.Model
{
  public class OptimizerTest
  {
    public bool WithRotate {  get; set; }
    public int TotalPanels { get; set; } = 0;
    public int RotatablePanels { get; set; } = 0;
    public int NotRotatablePanels { get; set; } = 0;
    public int TotalBasePanels { get; set; } = 0;
    public float DurationSeconds { get; set; } = 0;
    public List<string> CoverStats { get; set; } = []; 
  }
}

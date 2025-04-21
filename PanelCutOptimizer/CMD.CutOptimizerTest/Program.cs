// See https://aka.ms/new-console-template for more information
using CMD.CutOptimizerTest.Model;
using Lib.CutOptimizer;
using System.Diagnostics;

var random = new Random();

var width = random.Next(100, 301);
var height = random.Next(100, 301);
var optimizer = new Optimizer(width, height);

var tests = new List<OptimizerTest>();

for (int n = 0; n < 4; n++)
{
  var totalPanels = n+1;
  var rotatablePanels = 0;
  var nonRotatablePanels = 0;

  for (int i = 0; i < totalPanels; i++)
  {
    var characterIter = i % 26;
    var character = (char)('A' + characterIter);
    var panelx = random.Next(10, width + 1);
    var panely = random.Next(10, height + 1);
    optimizer.AddPanel(panelx, panely, character.ToString());

    if (panelx == panely || panely > width || panelx > height) nonRotatablePanels++;
    else rotatablePanels++;
  }

  var stopwatch = Stopwatch.StartNew();
  var panels = optimizer.OptimizePanels(rotate: false);
  stopwatch.Stop();
  
  tests.Add(new OptimizerTest
  {
    WithRotate = false,
    TotalPanels = totalPanels,
    NotRotatablePanels = nonRotatablePanels,
    RotatablePanels = rotatablePanels,
    DurationSeconds = (float)stopwatch.Elapsed.TotalMilliseconds / 1000,
    TotalBasePanels = panels.Count,
    CoverStats = panels.Select(x => $"{x.FormattedUsagePercentage}%").ToList(),
  });

  stopwatch = Stopwatch.StartNew();
  panels = optimizer.OptimizePanels(rotate: true);
  stopwatch.Stop();

  tests.Add(new OptimizerTest
  {
    WithRotate = true,
    TotalPanels = totalPanels,
    NotRotatablePanels = nonRotatablePanels,
    RotatablePanels = rotatablePanels,
    DurationSeconds = (float)stopwatch.Elapsed.TotalMilliseconds/1000,
    TotalBasePanels = panels.Count,
    CoverStats = panels.Select(x => $"{x.FormattedUsagePercentage}%").ToList(),
  });
}

// Encabezado
Console.WriteLine($"{"Rotate",7} {"Panels",7} {"Rotatable",5} {"Non-Rotatable",6} {"Bases",6} {"Time(s)",8} {"CoverStats"}");
Console.WriteLine(new string('-', 70));

// Filas
foreach (var r in tests)
{
  var coverStr = string.Join(", ", r.CoverStats);
  Console.WriteLine($"{r.WithRotate,7} {r.TotalPanels,7} {r.RotatablePanels,5} {r.NotRotatablePanels,6} {r.TotalBasePanels,6} {r.DurationSeconds,8:F3} {coverStr}");
}

Console.Read();


/* Para crear las imagenes
var imgPath = @"C:\Users\gsosa\Desktop\Dev\cut-optimizer";
var dateString = DateTime.Now.ToString("yyyyMMddhhmmss");

foreach (var panel in panels)
{
  PanelDrawer.DrawPackedPanel(
    panel,
    Path.Combine(imgPath, dateString + $"_{panel.Index}.png"),
    dpi: 1000,
    marginBottom: 20,
    marginRight: 20,
    marginTop: 35,
    marginLeft: 35,
    scale: 5f,
    panelFontSize: 7f,
    tableFontSize: 6.5f);
}
*/
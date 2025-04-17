// See https://aka.ms/new-console-template for more information
using Lib.CutOptimizer;

var optimizer = new Optimizer(240, 120);

optimizer.AddPanel(panelx: 80, panely: 100, "A");
optimizer.AddPanel(panelx: 100, panely: 10, "B");
optimizer.AddPanel(panelx: 120, panely: 40, "C");
optimizer.AddPanel(panelx: 120, panely: 10, "D");
optimizer.AddPanel(panelx: 110, panely: 20, "E");
optimizer.AddPanel(panelx: 30, panely: 15, "F");
optimizer.AddPanel(panelx: 20, panely: 100, "G");
//optimizer.AddPanel(panelx: 20, panely: 20, "H");
//optimizer.AddPanel(panelx: 10, panely: 25, "I");
//optimizer.AddPanel(panelx: 80, panely: 100, "J");
//optimizer.AddPanel(panelx: 100, panely: 35, "K");


var panels = optimizer.OptimizePanels(rotate: false);

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
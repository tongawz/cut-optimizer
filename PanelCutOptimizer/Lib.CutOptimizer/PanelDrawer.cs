using System.Drawing;
using System.Drawing.Imaging;
using LIB.CutOptimizer.Model;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

public static class PanelDrawer
{
  public static void DrawPackedPanel(
      PackedBasePanel packedPanel,
      string outputPath,
      int dpi = 400,
      int marginPx = 60,
      float scale = 2.0f,
      float panelFontSize = 5f,
      float tableFontSize = 4f)
  {
    var panelWidthPx = (int)(packedPanel.Width * scale);
    var panelHeightPx = (int)(packedPanel.Height * scale);

    var font = new Font("Arial", panelFontSize, GraphicsUnit.Point);
    var tableFont = new Font("Arial", tableFontSize, GraphicsUnit.Point);
    var brush = Brushes.Black;

    // Crear imagen temporal para medir texto
    using var tmpBmp = new Bitmap(1, 1);
    using var tmpG = Graphics.FromImage(tmpBmp);
    tmpG.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

    // Medir columnas con textos máximos
    var col1Width = (int)tmpG.MeasureString("Panel", tableFont).Width + 6;
    var col2Width = (int)tmpG.MeasureString("9999cm", tableFont).Width + 6;
    var col3Width = (int)tmpG.MeasureString("9999cm", tableFont).Width + 6;

    var rowHeight = (int)tmpG.MeasureString("Alto", tableFont).Height + 4;
    var tableWidth = col1Width + col2Width + col3Width;
    var tableRows = packedPanel.PlacedPanels.Count + 1;
    var tableHeight = tableRows * rowHeight;

    // Mitad del margen entre dibujo y tabla
    var tableOffset = marginPx / 2;

    var bmpWidth = panelWidthPx + marginPx * 2 + tableWidth + tableOffset;
    var bmpHeight = Math.Max(panelHeightPx + marginPx * 2, tableHeight + marginPx * 2);

    using var bmp = new Bitmap(bmpWidth, bmpHeight);
    using var g = Graphics.FromImage(bmp);
    g.Clear(Color.White);
    g.SmoothingMode = SmoothingMode.AntiAlias;
    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

    var basePanelPen = new Pen(Color.Black, 1); // Borde fino para el panel base
    var placedPanelPen = new Pen(Color.Black, 3); // Borde grueso para los paneles recortados
    var borderPen = new Pen(Color.Black, 1);

    // Relleno con diagonales para el panel base
    using (var hatchBrush = new HatchBrush(HatchStyle.ForwardDiagonal, Color.LightGray, Color.White))
    {
      var baseRect = new Rectangle(marginPx, marginPx, panelWidthPx, panelHeightPx);
      g.FillRectangle(hatchBrush, baseRect);
    }

    // Panel base
    g.DrawRectangle(basePanelPen, marginPx, marginPx, panelWidthPx, panelHeightPx);

    foreach (var p in packedPanel.PlacedPanels)
    {
      var x = marginPx + (int)(p.X * scale);
      var y = marginPx + (int)(p.Y * scale);
      var w = (int)(p.Width * scale);
      var h = (int)(p.Height * scale);

      var panelRect = new Rectangle(x, y, w, h);
      g.FillRectangle(Brushes.White, panelRect); // Fondo blanco
      g.DrawRectangle(placedPanelPen, panelRect); // Borde grueso para paneles recortados

      string name = p.PanelName ?? "";

      var nameSize = g.MeasureString(name, font);
      float nameX = x + (w - nameSize.Width) / 2;
      float nameY = y + (h - nameSize.Height) / 2;

      g.DrawString(name, font, brush, nameX, nameY);
    }

    // Tabla
    var tableLeft = marginPx + panelWidthPx + tableOffset;
    var tableTop = marginPx;

    int col1 = tableLeft;
    int col2 = col1 + col1Width;
    int col3 = col2 + col2Width;

    // Header
    g.DrawRectangle(borderPen, col1, tableTop, tableWidth, rowHeight);
    g.DrawLine(borderPen, col2, tableTop, col2, tableTop + rowHeight);
    g.DrawLine(borderPen, col3, tableTop, col3, tableTop + rowHeight);
    g.DrawString("Panel", tableFont, brush, col1 + 2, tableTop + 2);
    g.DrawString("Largo", tableFont, brush, col2 + 2, tableTop + 2);
    g.DrawString("Alto", tableFont, brush, col3 + 2, tableTop + 2);

    // Filas
    int i = 1;
    foreach (var p in packedPanel.PlacedPanels)
    {
      int rowY = tableTop + i * rowHeight;
      g.DrawRectangle(borderPen, col1, rowY, tableWidth, rowHeight);
      g.DrawLine(borderPen, col2, rowY, col2, rowY + rowHeight);
      g.DrawLine(borderPen, col3, rowY, col3, rowY + rowHeight);

      g.DrawString(p.PanelName ?? "-", tableFont, brush, col1 + 2, rowY + 2);
      g.DrawString($"{p.Width}cm", tableFont, brush, col2 + 2, rowY + 2);
      g.DrawString($"{p.Height}cm", tableFont, brush, col3 + 2, rowY + 2);
      i++;
    }

    // Agregar el porcentaje de uso
    var usageText = $"Uso: {packedPanel.FormattedUsagePercentage:F2}%";
    var usageSize = g.MeasureString(usageText, tableFont);
    // Ubicarlo a la derecha de la tabla
    float usageX = tableLeft; // Un pequeño margen después de la tabla
    float usageY = tableTop + tableHeight + 5; // Ubicarlo debajo de la tabla

    g.DrawString(usageText, tableFont, brush, usageX, usageY);

    // Guardar como PNG de alta resolución
    bmp.SetResolution(dpi, dpi);
    bmp.Save(outputPath, ImageFormat.Png);
  }
}
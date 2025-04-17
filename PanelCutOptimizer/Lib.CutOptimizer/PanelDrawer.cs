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
      int marginTop = 80,     // margen aumentado arriba
      int marginLeft = 80,    // margen aumentado a la izquierda
      int marginRight = 20,
      int marginBottom = 20,
      float scale = 2.0f,
      float panelFontSize = 5f,
      float tableFontSize = 4f)
  {
    var panelWidthPx = (int)(packedPanel.Width * scale);
    var panelHeightPx = (int)(packedPanel.Height * scale);

    var font = new Font("Arial", panelFontSize, GraphicsUnit.Point);
    var tableFont = new Font("Arial", tableFontSize, GraphicsUnit.Point);
    var brush = Brushes.Black;

    using var tmpBmp = new Bitmap(1, 1);
    using var tmpG = Graphics.FromImage(tmpBmp);
    tmpG.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

    var col1Width = (int)tmpG.MeasureString("Panel", tableFont).Width + 6;
    var col2Width = (int)tmpG.MeasureString("9999cm", tableFont).Width + 6;
    var col3Width = (int)tmpG.MeasureString("9999cm", tableFont).Width + 6;

    var rowHeight = (int)tmpG.MeasureString("Alto", tableFont).Height + 4;
    var tableWidth = col1Width + col2Width + col3Width;
    var tableRows = packedPanel.PlacedPanels.Count + 1;
    var tableHeight = tableRows * rowHeight;

    var tableOffset = 10;

    var bmpWidth = marginLeft + panelWidthPx + marginRight + tableWidth + tableOffset;
    var bmpHeight = Math.Max(marginTop + panelHeightPx + marginBottom, tableHeight + marginTop + marginBottom);

    using var bmp = new Bitmap(bmpWidth, bmpHeight);
    using var g = Graphics.FromImage(bmp);
    g.Clear(Color.White);
    g.SmoothingMode = SmoothingMode.AntiAlias;
    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

    var basePanelPen = new Pen(Color.Black, 1);
    var placedPanelPen = new Pen(Color.Black, 3);
    var borderPen = new Pen(Color.Black, 1);

    // Relleno del panel base
    using (var hatchBrush = new HatchBrush(HatchStyle.ForwardDiagonal, Color.LightGray, Color.White))
    {
      var baseRect = new Rectangle(marginLeft, marginTop, panelWidthPx, panelHeightPx);
      g.FillRectangle(hatchBrush, baseRect);
    }

    // Panel base
    g.DrawRectangle(basePanelPen, marginLeft, marginTop, panelWidthPx, panelHeightPx);

    // Cotas externas del panel base
    DrawDimension(g, tableFont, brush, borderPen,
      new Point(marginLeft, marginTop - 10),
      new Point(marginLeft + panelWidthPx, marginTop - 10),
      $"{packedPanel.Width}cm", isVertical: false);

    DrawDimension(g, tableFont, brush, borderPen,
      new Point(marginLeft - 10, marginTop),
      new Point(marginLeft - 10, marginTop + panelHeightPx),
      $"{packedPanel.Height}cm", isVertical: true);

    foreach (var p in packedPanel.PlacedPanels)
    {
      var x = marginLeft + (int)(p.X * scale);
      var y = marginTop + (int)(p.Y * scale);
      var w = (int)(p.Width * scale);
      var h = (int)(p.Height * scale);

      var panelRect = new Rectangle(x, y, w, h);
      g.FillRectangle(Brushes.White, panelRect);
      g.DrawRectangle(placedPanelPen, panelRect);

      string name = p.PanelName ?? "";
      var nameSize = g.MeasureString(name, font);
      float nameX = x + (w - nameSize.Width) / 2;
      float nameY = y + (h - nameSize.Height) / 2;

      g.DrawString(name, font, brush, nameX, nameY);

      // Cotas internas del panel recortado
      DrawDimension(g, tableFont, brush, borderPen,
        new Point(x, y - 6),
        new Point(x + w, y - 6),
        $"{p.Width}cm", isVertical: false);

      DrawDimension(g, tableFont, brush, borderPen,
        new Point(x - 6, y),
        new Point(x - 6, y + h),
        $"{p.Height}cm", isVertical: true);
    }

    // Tabla
    var tableLeft = marginLeft + panelWidthPx + tableOffset;
    var tableTop = marginTop;

    int col1 = tableLeft;
    int col2 = col1 + col1Width;
    int col3 = col2 + col2Width;

    g.DrawRectangle(borderPen, col1, tableTop, tableWidth, rowHeight);
    g.DrawLine(borderPen, col2, tableTop, col2, tableTop + rowHeight);
    g.DrawLine(borderPen, col3, tableTop, col3, tableTop + rowHeight);
    g.DrawString("Panel", tableFont, brush, col1 + 2, tableTop + 2);
    g.DrawString("Largo", tableFont, brush, col2 + 2, tableTop + 2);
    g.DrawString("Alto", tableFont, brush, col3 + 2, tableTop + 2);

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

    var usageText = $"Uso: {packedPanel.FormattedUsagePercentage:F2}%";
    var usageSize = g.MeasureString(usageText, tableFont);
    float usageX = tableLeft;
    float usageY = tableTop + tableHeight + 5;

    g.DrawString(usageText, tableFont, brush, usageX, usageY);

    bmp.SetResolution(dpi, dpi);
    bmp.Save(outputPath, ImageFormat.Png);
  }

  private static void DrawDimension(Graphics g, Font font, Brush brush, Pen pen,
    Point start, Point end, string text, bool isVertical)
  {
    g.DrawLine(pen, start, end);

    var arrowSize = 4;
    if (isVertical)
    {
      g.DrawLine(pen, start, new Point(start.X - arrowSize, start.Y + arrowSize));
      g.DrawLine(pen, start, new Point(start.X + arrowSize, start.Y + arrowSize));
      g.DrawLine(pen, end, new Point(end.X - arrowSize, end.Y - arrowSize));
      g.DrawLine(pen, end, new Point(end.X + arrowSize, end.Y - arrowSize));
    }
    else
    {
      g.DrawLine(pen, start, new Point(start.X + arrowSize, start.Y - arrowSize));
      g.DrawLine(pen, start, new Point(start.X + arrowSize, start.Y + arrowSize));
      g.DrawLine(pen, end, new Point(end.X - arrowSize, end.Y - arrowSize));
      g.DrawLine(pen, end, new Point(end.X - arrowSize, end.Y + arrowSize));
    }

    var textSize = g.MeasureString(text, font);
    float textX = isVertical
        ? start.X - textSize.Width - 4
        : (start.X + end.X) / 2 - textSize.Width / 2;
    float textY = isVertical
        ? (start.Y + end.Y) / 2 - textSize.Height / 2
        : start.Y - textSize.Height - 2;

    g.DrawString(text, font, brush, textX, textY);
  }
}

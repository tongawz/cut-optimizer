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

    // Dibuja el panel base y sus cotas
    DrawBasePanel(g, packedPanel, font, brush, basePanelPen, tableFont, borderPen, marginLeft, marginTop, panelWidthPx, panelHeightPx);

    // Paneles colocados
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


      // Cota horizontal interna (debajo del borde superior del panel)
      DrawInternalPanelDimensions(g, tableFont, brush, borderPen,
          new Point(x + 6, y + 6),  // Dentro del panel, arriba
          new Point(x + w - 6, y + 6),
          $"{p.Width}cm");

      // Cota vertical interna (a la derecha del borde izquierdo del panel)
      DrawInternalPanelDimensions(g, tableFont, brush, borderPen,
          new Point(x + 6, y + 6),
          new Point(x + 6, y + h - 6),
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

  private static void DrawBasePanel(
      Graphics g,
      PackedBasePanel packedPanel,
      Font font,
      Brush brush,
      Pen basePanelPen,
      Font tableFont,
      Pen borderPen,
      int marginLeft,
      int marginTop,
      int panelWidthPx,
      int panelHeightPx)
  {
    // Relleno del panel base
    using (var hatchBrush = new HatchBrush(HatchStyle.ForwardDiagonal, Color.LightGray, Color.White))
    {
      var baseRect = new Rectangle(marginLeft, marginTop, panelWidthPx, panelHeightPx);
      g.FillRectangle(hatchBrush, baseRect);
    }

    // Panel base
    g.DrawRectangle(basePanelPen, marginLeft, marginTop, panelWidthPx, panelHeightPx);

    // Cotas externas del panel base
    DrawBasePanelDimensions(g, tableFont, brush, borderPen,
        new Point(marginLeft, marginTop - 6),    // Separación ajustada
        new Point(marginLeft + panelWidthPx, marginTop - 6),
        $"{packedPanel.Width}cm");

    DrawBasePanelDimensions(g, tableFont, brush, borderPen,
        new Point(marginLeft - 6, marginTop),    // Separación ajustada
        new Point(marginLeft - 6, marginTop + panelHeightPx),
        $"{packedPanel.Height}cm", isVertical: true);
  }

  private static void DrawBasePanelDimensions(
    Graphics g, Font font, Brush brush, Pen pen,
    Point start, Point end, string text, bool isVertical = false)
  {
    // Separar la línea de dimensión del borde 1 píxel
    if (isVertical)
    {
      start = new Point(start.X - 1, start.Y);
      end = new Point(end.X - 1, end.Y);
    }
    else
    {
      start = new Point(start.X, start.Y - 1);
      end = new Point(end.X, end.Y - 1);
    }

    g.DrawLine(pen, start, end);

    var arrowSize = 4;
    if (isVertical)
    {
      // Flechas
      g.DrawLine(pen, start, new Point(start.X - arrowSize, start.Y + arrowSize));
      g.DrawLine(pen, start, new Point(start.X + arrowSize, start.Y + arrowSize));
      g.DrawLine(pen, end, new Point(end.X - arrowSize, end.Y - arrowSize));
      g.DrawLine(pen, end, new Point(end.X + arrowSize, end.Y - arrowSize));

      var textSize = g.MeasureString(text, font);

      // Punto medio de la línea vertical
      float centerX = start.X;
      float centerY = (start.Y + end.Y) / 2;

      // Guardar estado gráfico actual
      GraphicsState state = g.Save();

      // Trasladar al centro y rotar -90°
      g.TranslateTransform(centerX, centerY);
      g.RotateTransform(-90);

      // Posicionar texto (centrado)
      float textX = -textSize.Width / 2;
      float textY = -textSize.Height / 2;

      float padding = 2;
      var backgroundRect = new RectangleF(
          textX - padding,
          textY - padding,
          textSize.Width + 2 * padding,
          textSize.Height + 2 * padding);

      // Fondo blanco y texto
      g.FillRectangle(Brushes.White, backgroundRect);
      g.DrawString(text, font, brush, textX, textY);

      // Restaurar estado gráfico
      g.Restore(state);
    }
    else
    {
      // Flechas
      g.DrawLine(pen, start, new Point(start.X + arrowSize, start.Y - arrowSize));
      g.DrawLine(pen, start, new Point(start.X + arrowSize, start.Y + arrowSize));
      g.DrawLine(pen, end, new Point(end.X - arrowSize, end.Y - arrowSize));
      g.DrawLine(pen, end, new Point(end.X - arrowSize, end.Y + arrowSize));

      var textSize = g.MeasureString(text, font);

      float textX = (start.X + end.X) / 2 - textSize.Width / 2;
      float textY = (start.Y + end.Y) / 2 - textSize.Height / 2;

      float padding = 2;
      var backgroundRect = new RectangleF(
          textX - padding,
          textY - padding,
          textSize.Width + 2 * padding,
          textSize.Height + 2 * padding);

      g.FillRectangle(Brushes.White, backgroundRect);
      g.DrawString(text, font, brush, textX, textY);
    }
  }



  private static void DrawInternalPanelDimensions(
    Graphics g, Font font, Brush brush, Pen pen,
    Point start, Point end, string text, bool isVertical = false)
  {
    var arrowSize = 4;

    if (isVertical)
    {
      // Separar línea 1px a la derecha
      start = new Point(start.X + 3, start.Y + 1);
      end = new Point(end.X + 3, end.Y);

      g.DrawLine(pen, start, end);

      // Flechas verticales
      g.DrawLine(pen, start, new Point(start.X - arrowSize, start.Y + arrowSize));
      g.DrawLine(pen, start, new Point(start.X + arrowSize, start.Y + arrowSize));
      g.DrawLine(pen, end, new Point(end.X - arrowSize, end.Y - arrowSize));
      g.DrawLine(pen, end, new Point(end.X + arrowSize, end.Y - arrowSize));

      var textSize = g.MeasureString(text, font);
      float centerX = start.X;
      float centerY = (start.Y + end.Y) / 2;

      GraphicsState state = g.Save();
      g.TranslateTransform(centerX, centerY);
      g.RotateTransform(-90);

      float textX = -textSize.Width / 2;
      float textY = -textSize.Height / 2;

      float padding = 2;
      var backgroundRect = new RectangleF(
          textX - padding,
          textY - padding,
          textSize.Width + 2 * padding,
          textSize.Height + 2 * padding);

      g.FillRectangle(Brushes.White, backgroundRect);
      g.DrawString(text, font, brush, textX, textY);

      g.Restore(state);
    }
    else
    {
      start = new Point(start.X + 3, start.Y + 2);
      end = new Point(end.X, end.Y + 2);

      // Horizontal sin desplazamiento
      g.DrawLine(pen, start, end);

      g.DrawLine(pen, start, new Point(start.X + arrowSize, start.Y - arrowSize));
      g.DrawLine(pen, start, new Point(start.X + arrowSize, start.Y + arrowSize));
      g.DrawLine(pen, end, new Point(end.X - arrowSize, end.Y - arrowSize));
      g.DrawLine(pen, end, new Point(end.X - arrowSize, end.Y + arrowSize));

      var textSize = g.MeasureString(text, font);
      float textX = (start.X + end.X) / 2 - textSize.Width / 2;
      float textY = start.Y - 4;

      float padding = 2;
      var backgroundRect = new RectangleF(
          textX - padding,
          textY - padding,
          textSize.Width + 1 * padding,
          textSize.Height + 1 * padding);

      g.FillRectangle(Brushes.White, backgroundRect);
      g.DrawString(text, font, brush, textX, textY);
    }
  }
}

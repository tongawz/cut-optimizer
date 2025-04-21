using LIB.CutOptimizer.Model;
using RectpackSharp;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class MaxRectsSolver
{
  private readonly int _basePanelWidth;
  private readonly int _basePanelHeight;
  private decimal _baseAreaM2;
  private bool _rotate;

  public MaxRectsSolver(int basePanelWidth, int basePanelHeight, bool rotate)
  {
    _basePanelWidth = basePanelWidth;
    _basePanelHeight = basePanelHeight;
    _baseAreaM2 = (_basePanelWidth / 100m) * (_basePanelHeight / 100m);
    _rotate = rotate;
  }

  public List<PackedBasePanel> Pack(List<Panel> panels)
  {
    var remainPanels = panels;
    var result = new List<PackedBasePanel>();
    int basePanelIndex = 0;

    while (remainPanels.Any())
    {
      var combinations = GetPanelCombinations(remainPanels); // ordenadas por área descendente
      var totalPanelCombinations = combinations.Count;
      List<PositionedPanel>? bestResult = null;
      int bestArea = 0;

      int falseCount = 0;

      while (combinations.Any())
      {
        int mid = combinations.Count / 2;

        bool packSuccess = false;

        while (!packSuccess)
        {
          var comb = combinations[mid];

          var packingRects = comb.Select((x, idx) =>
          {
            x.index = idx;
            return new PackingRectangle
            {
              Id = idx,
              Width = (uint)x.Width,
              Height = (uint)x.Height
            };
          }).ToArray();

          int totalArea = packingRects.Sum(r => (int)(r.Width * r.Height));

          try
          {
            RectanglePacker.Pack(
                rectangles: packingRects,
                out PackingRectangle bounds,
                packingHint: PackingHints.FindBest,
                maxBoundsWidth: (uint)_basePanelWidth,
                maxBoundsHeight: (uint)_basePanelHeight
            );

            falseCount = 0;
            packSuccess = true;

            if (totalArea > bestArea)
            {
              bestArea = totalArea;
              bestResult = packingRects.Select(x => new PositionedPanel
              {
                Width = (int)x.Width,
                Height = (int)x.Height,
                X = (int)x.X,
                Y = (int)x.Y,
                PanelName = comb.First(z => z.index == x.Id).PanelName
              }).ToList();

              Console.WriteLine($"✔ Analizando [{mid}/{combinations.Count}] -> Total area: {totalArea}, Pack: true");

              // Se eliminan combinaciones con área menor o igual a la actual
              combinations = combinations
                .Where(c => CalculateArea(c) > bestArea)
                .ToList();

              // Reiniciar la búsqueda desde el nuevo medio
              continue;
            }
          }
          catch
          {
            Console.WriteLine($"✘ Analizando [{mid}/{combinations.Count}] -> Total area: {totalArea}, Pack: false");
            combinations.Remove(comb);
            falseCount++;

            if (!combinations.Any()) break;

            mid = combinations.Count - 1;

            if (falseCount == totalPanelCombinations/100)
            {
              var fallback = TryPackInParallelWithPruning(combinations);

              if (fallback != null)
              {
                bestArea = fallback.Value.Item1;
                bestResult = fallback.Value.Item2;

                Console.WriteLine($"⚡ Pack paralelo encontrado -> Total area: {bestArea}");
              }

              combinations.Clear(); // salir del while
              break; // ninguno funcionó
            }

          }
        }
      }

      if (bestResult == null)
        break;

      remainPanels = remainPanels
          .Where(x => !bestResult.Any(z => z.PanelName == x.PanelName))
          .ToList();

      result.Add(new PackedBasePanel
      {
        Index = ++basePanelIndex,
        Width = _basePanelWidth,
        Height = _basePanelHeight,
        PlacedPanels = bestResult
      });
    }

    return result;
  }

  private (int, List<PositionedPanel>)? TryPackInParallelWithPruning(List<List<Panel>> combinations)
  {
    object locker = new();
    (int, List<PositionedPanel>)? bestResult = null;
    int bestArea = 0;

    var parallelOptions = new ParallelOptions
    {
      MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2)
    };

    while (combinations.Any())
    {
      int index = 0;
      int total = combinations.Count;

      var newBestResult = (area: 0, positioned: (List<PositionedPanel>)null);
      bool stopRequested = false;

      Parallel.ForEach(combinations, parallelOptions, (comb, state) =>
      {
        if (stopRequested) return;

        int localIndex = Interlocked.Increment(ref index);
        int area = 0;

        try
        {
          var packingRects = comb.Select((x, idx) =>
          {
            x.index = idx;
            return new PackingRectangle
            {
              Id = idx,
              Width = (uint)x.Width,
              Height = (uint)x.Height
            };
          }).ToArray();

          area = packingRects.Sum(r => (int)(r.Width * r.Height));
          
          RectanglePacker.Pack(
              rectangles: packingRects,
              out PackingRectangle bounds,
              packingHint: PackingHints.FindBest,
              maxBoundsWidth: (uint)_basePanelWidth,
              maxBoundsHeight: (uint)_basePanelHeight
          );


          var positioned = packingRects.Select(x => new PositionedPanel
          {
            Width = (int)x.Width,
            Height = (int)x.Height,
            X = (int)x.X,
            Y = (int)x.Y,
            PanelName = comb.First(z => z.index == x.Id).PanelName
          }).ToList();

          lock (locker)
          {
            if (area > newBestResult.area)
            {
              newBestResult = (area, positioned);
              stopRequested = true;
              state.Stop(); // 🛑 Cortar todo el loop en cuanto se encuentra un nuevo mejor
            }
          }

          Console.WriteLine($"✔ Analizando [{localIndex}/{total}] -> Total area: {area}, Pack: true");
        }
        catch
        {
          Console.WriteLine($"✘ Analizando [{localIndex}/{total}] -> Total area: {area}, Pack: false");
        }
      });

      if (newBestResult.area > bestArea)
      {
        bestArea = newBestResult.area;
        bestResult = (bestArea, newBestResult.positioned);
      }

      if (bestArea == 0 || newBestResult.area == 0) break;

      // Poda
      combinations = combinations
        .Where(c => CalculateArea(c) > bestArea)
        .ToList();
    }

    return bestResult;
  }



  private int CalculateArea(List<Panel> comb)
  {
    return comb.Sum(p => p.Width * p.Height);
  }

  private List<List<Panel>> GetPanelCombinations(List<Panel> panels)
  {
    var plusRotatedPanels = panels;

    if (_rotate)
    {
      var rotatedPanels = panels
          .Where(x => x.Width != x.Height)
          .Where(x => x.Width <= _basePanelHeight && x.Height <= _basePanelWidth)
          .Select(x => new Panel { PanelName = x.PanelName, Height = x.Width, Width = x.Height })
          .ToList();

      plusRotatedPanels = panels.Concat(rotatedPanels).ToList();
    }

    plusRotatedPanels = plusRotatedPanels.OrderByDescending(x => x.AreaM2).ToList();
    int n = plusRotatedPanels.Count;

    var result = new ConcurrentBag<List<Panel>>();
    int maxMask = (1 << n);

    int threadsToUse = Math.Max(1, Environment.ProcessorCount / 2);

    Parallel.For(1, maxMask, new ParallelOptions { MaxDegreeOfParallelism = threadsToUse }, mask =>
    {
      var combo = new List<Panel>();
      decimal totalArea = 0;
      var usedNames = new HashSet<string>();

      for (int i = 0; i < n; i++)
      {
        if ((mask & (1 << i)) != 0)
        {
          var panel = plusRotatedPanels[i];

          if (!usedNames.Add(panel.PanelName)) continue;

          totalArea += panel.AreaM2;
          if (totalArea > _baseAreaM2) return; // salir de este thread

          combo.Add(panel);
        }
      }

      if (combo.Count > 0 && totalArea <= _baseAreaM2)
      {
        result.Add(combo);
      }
    });

    return result
        .OrderByDescending(c => c.Sum(p => p.AreaM2))
        .ThenByDescending(c => c.Count)
        .ToList();
  }


}
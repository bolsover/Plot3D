using System;
using System.Text;
using Plot3D.Core;
using Plot3D.Figure;
using Plot3D.Projection;

namespace Plot3D.Rendering
{
    /// <summary>
    /// Renders 3D curves into ASCII / Unicode text for terminal and CLI visualization.
    /// </summary>
    public sealed class ConsoleRenderer3D
    {
        public int Columns { get; set; } = 78;
        public int Rows { get; set; } = 24;

        public ConsoleRenderer3D(int columns = 78, int rows = 24)
        {
            Columns = Math.Max(20, columns);
            Rows = Math.Max(10, rows);
        }

        public string Render(Axes3D axes)
        {
            if (axes == null) throw new ArgumentNullException(nameof(axes));

            char[,] grid = new char[Rows, Columns];
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    grid[r, c] = ' ';
                }
            }

            var bounds = axes.GetEffectiveBounds();
            var camera = axes.Camera;
            camera.Setup(bounds, Columns - 4, Rows - 4, margin: 4.0);

            // Draw bounding box wireframe
            DrawBox(grid, camera, bounds);

            // Rasterize curve lines
            char[] symbols = new[] { '*', '#', '+', 'o', 'x', '@', '%' };
            int lineIdx = 0;

            foreach (var line in axes.Lines)
            {
                if (!line.Visible) continue;
                char sym = symbols[lineIdx % symbols.Length];
                lineIdx++;

                foreach (var segment in line.Segments)
                {
                    for (int i = 0; i < segment.Count - 1; i++)
                    {
                        var p1 = camera.Project(segment[i], Columns - 4, Rows - 4);
                        var p2 = camera.Project(segment[i + 1], Columns - 4, Rows - 4);

                        if (double.IsNaN(p1.ScreenX) || double.IsNaN(p2.ScreenX))
                            continue;

                        int x1 = (int)Math.Round(p1.ScreenX) + 2;
                        int y1 = (int)Math.Round(p1.ScreenY) + 2;
                        int x2 = (int)Math.Round(p2.ScreenX) + 2;
                        int y2 = (int)Math.Round(p2.ScreenY) + 2;

                        DrawLineBresenham(grid, x1, y1, x2, y2, sym);
                    }
                }
            }

            // Build string
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(axes.Title))
            {
                sb.AppendLine("=== " + axes.Title + " ===");
            }

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    sb.Append(grid[r, c]);
                }
                sb.AppendLine();
            }

            sb.AppendFormat("X:[{0:F2}, {1:F2}]  Y:[{2:F2}, {3:F2}]  Z:[{4:F2}, {5:F2}]\n",
                bounds.XMin, bounds.XMax, bounds.YMin, bounds.YMax, bounds.ZMin, bounds.ZMax);

            return sb.ToString();
        }

        private void DrawBox(char[,] grid, Camera3D camera, Bounds3D bounds)
        {
            Point3D[] corners = new Point3D[]
            {
                new Point3D(bounds.XMin, bounds.YMin, bounds.ZMin),
                new Point3D(bounds.XMax, bounds.YMin, bounds.ZMin),
                new Point3D(bounds.XMax, bounds.YMax, bounds.ZMin),
                new Point3D(bounds.XMin, bounds.YMax, bounds.ZMin),
                new Point3D(bounds.XMin, bounds.YMin, bounds.ZMax),
                new Point3D(bounds.XMax, bounds.YMin, bounds.ZMax),
                new Point3D(bounds.XMax, bounds.YMax, bounds.ZMax),
                new Point3D(bounds.XMin, bounds.YMax, bounds.ZMax)
            };

            int[][] edges = new int[][]
            {
                new[] {0, 1}, new[] {1, 2}, new[] {2, 3}, new[] {3, 0},
                new[] {4, 5}, new[] {5, 6}, new[] {6, 7}, new[] {7, 4},
                new[] {0, 4}, new[] {1, 5}, new[] {2, 6}, new[] {3, 7}
            };

            foreach (var edge in edges)
            {
                var p1 = camera.Project(corners[edge[0]], Columns - 4, Rows - 4);
                var p2 = camera.Project(corners[edge[1]], Columns - 4, Rows - 4);

                if (double.IsNaN(p1.ScreenX) || double.IsNaN(p2.ScreenX))
                    continue;

                int x1 = (int)Math.Round(p1.ScreenX) + 2;
                int y1 = (int)Math.Round(p1.ScreenY) + 2;
                int x2 = (int)Math.Round(p2.ScreenX) + 2;
                int y2 = (int)Math.Round(p2.ScreenY) + 2;

                DrawLineBresenham(grid, x1, y1, x2, y2, '.');
            }
        }

        private void DrawLineBresenham(char[,] grid, int x0, int y0, int x1, int y1, char sym)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                if (x0 >= 0 && x0 < Columns && y0 >= 0 && y0 < Rows)
                {
                    if (grid[y0, x0] == ' ' || grid[y0, x0] == '.' || sym != '.')
                    {
                        grid[y0, x0] = sym;
                    }
                }

                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }
    }
}

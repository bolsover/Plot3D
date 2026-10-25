using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Plot3D.Core;
using Plot3D.Figure;
using Plot3D.Objects;
using Plot3D.Projection;

namespace Plot3D.Rendering
{
    /// <summary>
    /// Renders 3D figures into scalable vector graphics (SVG) with depth-sorted polylines, axis grids, ticks, and markers.
    /// </summary>
    public sealed class SvgRenderer3D
    {
        public int Width { get; set; } = 800;
        public int Height { get; set; } = 600;

        public SvgRenderer3D(int width = 800, int height = 600)
        {
            Width = width;
            Height = height;
        }

        public string Render(Axes3D axes)
        {
            if (axes == null) throw new ArgumentNullException(nameof(axes));

            var bounds = axes.GetEffectiveBounds();
            var camera = axes.Camera;
            camera.Setup(bounds, Width, Height, margin: 80.0);

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendFormat(CultureInfo.InvariantCulture,
                "<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" width=\"{0}\" height=\"{1}\" viewBox=\"0 0 {0} {1}\">\n",
                Width, Height);

            // Background
            sb.AppendFormat(CultureInfo.InvariantCulture,
                "  <rect width=\"100%\" height=\"100%\" fill=\"{0}\"/>\n",
                axes.BackgroundColor.ToHex());

            // Embedded CSS for clean typography and graphics styling
            sb.AppendLine("  <style>");
            sb.AppendLine("    .title { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; font-size: 16px; font-weight: bold; text-anchor: middle; fill: #222; }");
            sb.AppendLine("    .axis-label { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; font-size: 13px; font-weight: 600; text-anchor: middle; fill: #333; }");
            sb.AppendLine("    .tick-label { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; font-size: 10px; fill: #666; text-anchor: middle; dominant-baseline: middle; }");
            sb.AppendLine("    .grid-line { stroke: #e0e0e0; stroke-width: 0.75; fill: none; }");
            sb.AppendLine("    .box-edge { stroke: #888888; stroke-width: 1.0; fill: none; }");
            sb.AppendLine("  </style>");

            // Render Title
            if (!string.IsNullOrWhiteSpace(axes.Title))
            {
                sb.AppendFormat(CultureInfo.InvariantCulture,
                    "  <text x=\"{0:F1}\" y=\"30\" class=\"title\">{1}</text>\n",
                    Width * 0.5, EscapeXml(axes.Title));
            }

            // Render 3D Bounding Box and Grids
            RenderAxesBoxAndGrids(sb, axes, bounds);

            // Render Plot Lines with Depth Sorting
            RenderLines(sb, axes);

            // Render Legend if multiple named lines exist
            RenderLegend(sb, axes);

            sb.AppendLine("</svg>");
            return sb.ToString();
        }

        private void RenderAxesBoxAndGrids(StringBuilder sb, Axes3D axes, Bounds3D bounds)
        {
            var camera = axes.Camera;

            // 8 bounding box corners
            Point3D[] corners = new Point3D[]
            {
                new Point3D(bounds.XMin, bounds.YMin, bounds.ZMin), // 0: 000
                new Point3D(bounds.XMax, bounds.YMin, bounds.ZMin), // 1: 100
                new Point3D(bounds.XMax, bounds.YMax, bounds.ZMin), // 2: 110
                new Point3D(bounds.XMin, bounds.YMax, bounds.ZMin), // 3: 010
                new Point3D(bounds.XMin, bounds.YMin, bounds.ZMax), // 4: 001
                new Point3D(bounds.XMax, bounds.YMin, bounds.ZMax), // 5: 101
                new Point3D(bounds.XMax, bounds.YMax, bounds.ZMax), // 6: 111
                new Point3D(bounds.XMin, bounds.YMax, bounds.ZMax)  // 7: 011
            };

            // Edges of the 3D box
            int[][] edges = new int[][]
            {
                new[] {0, 1}, new[] {1, 2}, new[] {2, 3}, new[] {3, 0}, // Bottom
                new[] {4, 5}, new[] {5, 6}, new[] {6, 7}, new[] {7, 4}, // Top
                new[] {0, 4}, new[] {1, 5}, new[] {2, 6}, new[] {3, 7}  // Vertical
            };

            var (xTicks, yTicks, zTicks) = axes.CalculateTicks(bounds);

            // Draw grid lines
            if (axes.GridOn)
            {
                sb.AppendLine("  <g class=\"grid-lines\">");

                // Grid lines on bottom plane (Z = ZMin)
                foreach (var x in xTicks)
                {
                    var p1 = camera.Project(new Point3D(x, bounds.YMin, bounds.ZMin), Width, Height);
                    var p2 = camera.Project(new Point3D(x, bounds.YMax, bounds.ZMin), Width, Height);
                    if (!double.IsNaN(p1.ScreenX) && !double.IsNaN(p2.ScreenX))
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture,
                            "    <line x1=\"{0:F2}\" y1=\"{1:F2}\" x2=\"{2:F2}\" y2=\"{3:F2}\" class=\"grid-line\"/>\n",
                            p1.ScreenX, p1.ScreenY, p2.ScreenX, p2.ScreenY);
                    }
                }
                foreach (var y in yTicks)
                {
                    var p1 = camera.Project(new Point3D(bounds.XMin, y, bounds.ZMin), Width, Height);
                    var p2 = camera.Project(new Point3D(bounds.XMax, y, bounds.ZMin), Width, Height);
                    if (!double.IsNaN(p1.ScreenX) && !double.IsNaN(p2.ScreenX))
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture,
                            "    <line x1=\"{0:F2}\" y1=\"{1:F2}\" x2=\"{2:F2}\" y2=\"{3:F2}\" class=\"grid-line\"/>\n",
                            p1.ScreenX, p1.ScreenY, p2.ScreenX, p2.ScreenY);
                    }
                }

                // Grid lines on back wall planes
                foreach (var z in zTicks)
                {
                    var p1 = camera.Project(new Point3D(bounds.XMin, bounds.YMax, z), Width, Height);
                    var p2 = camera.Project(new Point3D(bounds.XMax, bounds.YMax, z), Width, Height);
                    if (!double.IsNaN(p1.ScreenX) && !double.IsNaN(p2.ScreenX))
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture,
                            "    <line x1=\"{0:F2}\" y1=\"{1:F2}\" x2=\"{2:F2}\" y2=\"{3:F2}\" class=\"grid-line\"/>\n",
                            p1.ScreenX, p1.ScreenY, p2.ScreenX, p2.ScreenY);
                    }
                }

                sb.AppendLine("  </g>");
            }

            // Draw box edges
            if (axes.BoxOn)
            {
                sb.AppendLine("  <g class=\"box-edges\">");
                foreach (var edge in edges)
                {
                    var p1 = camera.Project(corners[edge[0]], Width, Height);
                    var p2 = camera.Project(corners[edge[1]], Width, Height);
                    if (!double.IsNaN(p1.ScreenX) && !double.IsNaN(p2.ScreenX))
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture,
                            "    <line x1=\"{0:F2}\" y1=\"{1:F2}\" x2=\"{2:F2}\" y2=\"{3:F2}\" class=\"box-edge\"/>\n",
                            p1.ScreenX, p1.ScreenY, p2.ScreenX, p2.ScreenY);
                    }
                }
                sb.AppendLine("  </g>");
            }

            // Draw Tick Marks and Labels
            sb.AppendLine("  <g class=\"ticks\">");
            foreach (var x in xTicks)
            {
                var pt = camera.Project(new Point3D(x, bounds.YMin, bounds.ZMin), Width, Height);
                if (!double.IsNaN(pt.ScreenX))
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture,
                        "    <text x=\"{0:F1}\" y=\"{1:F1}\" class=\"tick-label\">{2:G4}</text>\n",
                        pt.ScreenX, pt.ScreenY + 12, x);
                }
            }
            foreach (var y in yTicks)
            {
                var pt = camera.Project(new Point3D(bounds.XMax, y, bounds.ZMin), Width, Height);
                if (!double.IsNaN(pt.ScreenX))
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture,
                        "    <text x=\"{0:F1}\" y=\"{1:F1}\" class=\"tick-label\">{2:G4}</text>\n",
                        pt.ScreenX + 14, pt.ScreenY, y);
                }
            }
            foreach (var z in zTicks)
            {
                var pt = camera.Project(new Point3D(bounds.XMin, bounds.YMax, z), Width, Height);
                if (!double.IsNaN(pt.ScreenX))
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture,
                        "    <text x=\"{0:F1}\" y=\"{1:F1}\" class=\"tick-label\">{2:G4}</text>\n",
                        pt.ScreenX - 16, pt.ScreenY, z);
                }
            }
            sb.AppendLine("  </g>");

            // Axis labels
            if (!string.IsNullOrWhiteSpace(axes.XLabel))
            {
                var xMid = camera.Project(new Point3D(bounds.Center.X, bounds.YMin, bounds.ZMin), Width, Height);
                if (!double.IsNaN(xMid.ScreenX))
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture,
                        "  <text x=\"{0:F1}\" y=\"{1:F1}\" class=\"axis-label\">{2}</text>\n",
                        xMid.ScreenX, xMid.ScreenY + 28, EscapeXml(axes.XLabel));
                }
            }

            if (!string.IsNullOrWhiteSpace(axes.YLabel))
            {
                var yMid = camera.Project(new Point3D(bounds.XMax, bounds.Center.Y, bounds.ZMin), Width, Height);
                if (!double.IsNaN(yMid.ScreenX))
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture,
                        "  <text x=\"{0:F1}\" y=\"{1:F1}\" class=\"axis-label\">{2}</text>\n",
                        yMid.ScreenX + 30, yMid.ScreenY, EscapeXml(axes.YLabel));
                }
            }

            if (!string.IsNullOrWhiteSpace(axes.ZLabel))
            {
                var zMid = camera.Project(new Point3D(bounds.XMin, bounds.YMax, bounds.Center.Z), Width, Height);
                if (!double.IsNaN(zMid.ScreenX))
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture,
                        "  <text x=\"{0:F1}\" y=\"{1:F1}\" class=\"axis-label\">{2}</text>\n",
                        zMid.ScreenX - 35, zMid.ScreenY, EscapeXml(axes.ZLabel));
                }
            }
        }

        private void RenderLines(StringBuilder sb, Axes3D axes)
        {
            var camera = axes.Camera;

            sb.AppendLine("  <g class=\"plot-curves\">");

            foreach (var line in axes.Lines)
            {
                if (!line.Visible) continue;

                string strokeColor = line.Color.ToHex();
                double strokeWidth = line.LineWidth;
                string dashArray = GetDashArray(line.LineStyle, strokeWidth);

                // Render continuous curve segments
                if (line.LineStyle != LineStyle.None)
                {
                    foreach (var segment in line.Segments)
                    {
                        if (segment.Count < 2) continue;

                        sb.Append("    <path d=\"");
                        bool first = true;

                        foreach (var pt in segment)
                        {
                            var proj = camera.Project(pt, Width, Height);
                            if (double.IsNaN(proj.ScreenX) || double.IsNaN(proj.ScreenY))
                                continue;

                            if (first)
                            {
                                sb.AppendFormat(CultureInfo.InvariantCulture, "M {0:F2} {1:F2}", proj.ScreenX, proj.ScreenY);
                                first = false;
                            }
                            else
                            {
                                sb.AppendFormat(CultureInfo.InvariantCulture, " L {0:F2} {1:F2}", proj.ScreenX, proj.ScreenY);
                            }
                        }

                        sb.AppendFormat(CultureInfo.InvariantCulture,
                            "\" fill=\"none\" stroke=\"{0}\" stroke-width=\"{1:F2}\" stroke-linecap=\"round\" stroke-linejoin=\"round\"",
                            strokeColor, strokeWidth);

                        if (!string.IsNullOrEmpty(dashArray))
                        {
                            sb.AppendFormat(CultureInfo.InvariantCulture, " stroke-dasharray=\"{0}\"", dashArray);
                        }

                        sb.AppendLine("/>");
                    }
                }

                // Render Markers
                if (line.Marker != MarkerStyle.None)
                {
                    RenderMarkers(sb, line, camera);
                }
            }

            sb.AppendLine("  </g>");
        }

        private void RenderMarkers(StringBuilder sb, ParameterizedFunctionLine3D line, Camera3D camera)
        {
            string edgeColor = (line.MarkerEdgeColor ?? line.Color).ToHex();
            string faceColor = line.MarkerFaceColor.HasValue ? line.MarkerFaceColor.Value.ToHex() : "none";
            double r = line.MarkerSize * 0.5;

            sb.AppendLine("    <g class=\"markers\">");

            foreach (var pt in line.Points)
            {
                if (!pt.IsValid) continue;
                var proj = camera.Project(pt, Width, Height);
                if (double.IsNaN(proj.ScreenX) || double.IsNaN(proj.ScreenY)) continue;

                double x = proj.ScreenX;
                double y = proj.ScreenY;

                switch (line.Marker)
                {
                    case MarkerStyle.Circle:
                        sb.AppendFormat(CultureInfo.InvariantCulture,
                            "      <circle cx=\"{0:F2}\" cy=\"{1:F2}\" r=\"{2:F2}\" stroke=\"{3}\" fill=\"{4}\" stroke-width=\"1.2\"/>\n",
                            x, y, r, edgeColor, faceColor);
                        break;
                    case MarkerStyle.Square:
                        sb.AppendFormat(CultureInfo.InvariantCulture,
                            "      <rect x=\"{0:F2}\" y=\"{1:F2}\" width=\"{2:F2}\" height=\"{2:F2}\" stroke=\"{3}\" fill=\"{4}\" stroke-width=\"1.2\"/>\n",
                            x - r, y - r, 2 * r, edgeColor, faceColor);
                        break;
                    case MarkerStyle.Diamond:
                        sb.AppendFormat(CultureInfo.InvariantCulture,
                            "      <polygon points=\"{0:F2},{1:F2} {2:F2},{3:F2} {4:F2},{5:F2} {6:F2},{7:F2}\" stroke=\"{8}\" fill=\"{9}\" stroke-width=\"1.2\"/>\n",
                            x, y - r, x + r, y, x, y + r, x - r, y, edgeColor, faceColor);
                        break;
                    case MarkerStyle.Plus:
                        sb.AppendFormat(CultureInfo.InvariantCulture,
                            "      <line x1=\"{0:F2}\" y1=\"{1:F2}\" x2=\"{2:F2}\" y2=\"{3:F2}\" stroke=\"{4}\" stroke-width=\"1.2\"/>" +
                            "<line x1=\"{5:F2}\" y1=\"{6:F2}\" x2=\"{7:F2}\" y2=\"{8:F2}\" stroke=\"{4}\" stroke-width=\"1.2\"/>\n",
                            x - r, y, x + r, y, edgeColor, x, y - r, x, y + r);
                        break;
                    case MarkerStyle.Cross:
                        sb.AppendFormat(CultureInfo.InvariantCulture,
                            "      <line x1=\"{0:F2}\" y1=\"{1:F2}\" x2=\"{2:F2}\" y2=\"{3:F2}\" stroke=\"{4}\" stroke-width=\"1.2\"/>" +
                            "<line x1=\"{5:F2}\" y1=\"{6:F2}\" x2=\"{7:F2}\" y2=\"{8:F2}\" stroke=\"{4}\" stroke-width=\"1.2\"/>\n",
                            x - r, y - r, x + r, y + r, edgeColor, x - r, y + r, x + r, y - r);
                        break;
                    case MarkerStyle.Asterisk:
                        sb.AppendFormat(CultureInfo.InvariantCulture,
                            "      <circle cx=\"{0:F2}\" cy=\"{1:F2}\" r=\"1.5\" fill=\"{2}\"/>\n",
                            x, y, edgeColor);
                        break;
                    case MarkerStyle.Point:
                        sb.AppendFormat(CultureInfo.InvariantCulture,
                            "      <circle cx=\"{0:F2}\" cy=\"{1:F2}\" r=\"1.0\" fill=\"{2}\"/>\n",
                            x, y, edgeColor);
                        break;
                    case MarkerStyle.TriangleUp:
                        sb.AppendFormat(CultureInfo.InvariantCulture,
                            "      <polygon points=\"{0:F2},{1:F2} {2:F2},{3:F2} {4:F2},{5:F2}\" stroke=\"{6}\" fill=\"{7}\" stroke-width=\"1.2\"/>\n",
                            x, y - r, x + r, y + r, x - r, y + r, edgeColor, faceColor);
                        break;
                    case MarkerStyle.TriangleDown:
                        sb.AppendFormat(CultureInfo.InvariantCulture,
                            "      <polygon points=\"{0:F2},{1:F2} {2:F2},{3:F2} {4:F2},{5:F2}\" stroke=\"{6}\" fill=\"{7}\" stroke-width=\"1.2\"/>\n",
                            x, y + r, x + r, y - r, x - r, y - r, edgeColor, faceColor);
                        break;
                }
            }

            sb.AppendLine("    </g>");
        }

        private void RenderLegend(StringBuilder sb, Axes3D axes)
        {
            var namedLines = axes.Lines.Where(l => !string.IsNullOrWhiteSpace(l.DisplayName)).ToList();
            if (namedLines.Count == 0) return;

            double legendX = Width - 180;
            double legendY = 40;
            double legendWidth = 160;
            double itemHeight = 22;
            double legendHeight = namedLines.Count * itemHeight + 12;

            sb.AppendFormat(CultureInfo.InvariantCulture,
                "  <g class=\"legend\">\n" +
                "    <rect x=\"{0:F1}\" y=\"{1:F1}\" width=\"{2:F1}\" height=\"{3:F1}\" fill=\"#ffffff\" stroke=\"#cccccc\" rx=\"4\" opacity=\"0.9\"/>\n",
                legendX, legendY, legendWidth, legendHeight);

            for (int i = 0; i < namedLines.Count; i++)
            {
                var line = namedLines[i];
                double y = legendY + 16 + i * itemHeight;

                sb.AppendFormat(CultureInfo.InvariantCulture,
                    "    <line x1=\"{0:F1}\" y1=\"{1:F1}\" x2=\"{2:F1}\" y2=\"{1:F1}\" stroke=\"{3}\" stroke-width=\"{4:F1}\"/>\n",
                    legendX + 10, y, legendX + 35, line.Color.ToHex(), line.LineWidth);

                sb.AppendFormat(CultureInfo.InvariantCulture,
                    "    <text x=\"{0:F1}\" y=\"{1:F1}\" font-family=\"sans-serif\" font-size=\"11\" fill=\"#333\" dominant-baseline=\"middle\">{2}</text>\n",
                    legendX + 42, y, EscapeXml(line.DisplayName!));
            }

            sb.AppendLine("  </g>");
        }

        private static string GetDashArray(LineStyle style, double lineWidth)
        {
            switch (style)
            {
                case LineStyle.Dashed:
                    return string.Format(CultureInfo.InvariantCulture, "{0:F1},{1:F1}", lineWidth * 4, lineWidth * 3);
                case LineStyle.Dotted:
                    return string.Format(CultureInfo.InvariantCulture, "{0:F1},{1:F1}", lineWidth * 1.5, lineWidth * 2);
                case LineStyle.DashDot:
                    return string.Format(CultureInfo.InvariantCulture, "{0:F1},{1:F1},{2:F1},{1:F1}", lineWidth * 4, lineWidth * 2, lineWidth * 1.5);
                default:
                    return string.Empty;
            }
        }

        private static string EscapeXml(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }
    }
}

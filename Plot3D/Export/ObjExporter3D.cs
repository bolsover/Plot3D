using System;
using System.Globalization;
using System.IO;
using System.Text;
using Plot3D.Figure;
using Plot3D.Objects;

namespace Plot3D.Export
{
    /// <summary>
    /// Exports 3D curves as Wavefront OBJ polyline meshes for import into 3D software (Blender, Unity, CAD).
    /// </summary>
    public static class ObjExporter3D
    {
        public static string Export(Axes3D axes)
        {
            if (axes == null) throw new ArgumentNullException(nameof(axes));

            var sb = new StringBuilder();
            sb.AppendLine("# Wavefront OBJ exported by Plot3D C# library");
            sb.AppendLine();

            int vertexOffset = 1;

            for (int l = 0; l < axes.Lines.Count; l++)
            {
                var line = axes.Lines[l];
                if (!line.Visible) continue;

                string objectName = string.IsNullOrWhiteSpace(line.DisplayName) ? "Curve_" + (l + 1) : line.DisplayName!;
                sb.AppendLine("o " + objectName.Replace(" ", "_"));

                foreach (var segment in line.Segments)
                {
                    if (segment.Count < 2) continue;

                    int startIdx = vertexOffset;
                    foreach (var pt in segment)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "v {0:F6} {1:F6} {2:F6}\n", pt.X, pt.Y, pt.Z);
                        vertexOffset++;
                    }

                    sb.Append("l");
                    for (int i = 0; i < segment.Count; i++)
                    {
                        sb.Append(" " + (startIdx + i));
                    }
                    sb.AppendLine();
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static void SaveToFile(Axes3D axes, string filePath)
        {
            File.WriteAllText(filePath, Export(axes), Encoding.UTF8);
        }
    }
}

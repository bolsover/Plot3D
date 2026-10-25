using System;
using System.Globalization;
using System.IO;
using System.Text;
using Plot3D.Objects;

namespace Plot3D.Export
{
    /// <summary>
    /// Exports 3D sampled curve points to CSV, TSV, and tabular formats.
    /// </summary>
    public static class DataExporter3D
    {
        public static string ToCsv(ParameterizedFunctionLine3D line, char delimiter = ',')
        {
            if (line == null) throw new ArgumentNullException(nameof(line));

            var sb = new StringBuilder();
            sb.AppendFormat("t{0}x{0}y{0}z\n", delimiter);

            var t = line.TData;
            var x = line.XData;
            var y = line.YData;
            var z = line.ZData;

            for (int i = 0; i < t.Count; i++)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture,
                    "{0:G10}{4}{1:G10}{4}{2:G10}{4}{3:G10}\n",
                    t[i], x[i], y[i], z[i], delimiter);
            }

            return sb.ToString();
        }

        public static void SaveCsv(ParameterizedFunctionLine3D line, string filePath, char delimiter = ',')
        {
            File.WriteAllText(filePath, ToCsv(line, delimiter), Encoding.UTF8);
        }
    }
}

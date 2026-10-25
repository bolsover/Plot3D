using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Plot3D.Core;
using Plot3D.Export;
using Plot3D.Expressions;
using Plot3D.Figure;
using Plot3D.Objects;
using Plot3D.Projection;
using Plot3D.Rendering;

namespace Plot3D.UI
{
    public enum ColorMode
    {
        GradientByZ,
        GradientByT,
        GradientByCurvature,
        SolidColor
    }

    public enum MarkerType
    {
        None,
        Circle,
        Square,
        Cross,
        Plus,
        Asterisk
    }

    /// <summary>
    /// Interactive high-performance 3D canvas control for real-time visualization of parametric curves.
    /// Provides smooth GDI+ anti-aliased rendering, interactive mouse orbit/pan/zoom, turntable animation,
    /// scientific colormaps, customizable axis boxes, coordinate ticks, and hover inspection.
    /// </summary>
    public sealed class Plot3DControl : Control
    {
        private Axes3D _axes = new Axes3D();
        private ParameterizedFunctionLine3D? _currentLine;
        private ColorMap _colorMap = ColorMap.Turbo;
        private ColorMode _colorMode = ColorMode.GradientByZ;
        private Color _solidColor = Color.FromArgb(0, 114, 189);
        private float _lineWidth = 2.5f;
        private DashStyle _dashStyle = DashStyle.Solid;
        private MarkerType _markerType = MarkerType.None;
        private float _markerSize = 6.0f;

        private double _azimuth = -37.5;
        private double _elevation = 30.0;
        private double _zoom = 1.0;
        private PointF _panOffset = PointF.Empty;

        private bool _isDragging;
        private MouseButtons _dragButton = MouseButtons.None;
        private Point _lastMousePos;

        private readonly System.Windows.Forms.Timer _animTimer;
        private bool _isAnimating;
        private double _rotationSpeed = 0.75;

        private (Point3D Point, double T, PointF ScreenPos)? _hoveredPoint;
        private bool _showGizmo = true;
        private bool _showHoverInfo = true;

        public event EventHandler? CameraChanged;
        public event EventHandler<(Point3D Point, double T)?>? PointHovered;

        public Plot3DControl()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable,
                true);

            UpdateStyles();
            BackColor = Color.White;
            Cursor = Cursors.Hand;

            _animTimer = new System.Windows.Forms.Timer { Interval = 20 };
            _animTimer.Tick += (s, e) =>
            {
                Azimuth = (Azimuth + _rotationSpeed + 180.0) % 360.0 - 180.0;
                Invalidate();
            };
        }

        #region Properties

        public Axes3D Axes => _axes;
        public ParameterizedFunctionLine3D? CurrentLine => _currentLine;

        [Category("Plot3D Appearance")]
        [Description("Scientific colormap used for gradient rendering.")]
        public ColorMap ColorMap
        {
            get => _colorMap;
            set { _colorMap = value ?? ColorMap.Turbo; Invalidate(); }
        }

        [Category("Plot3D Appearance")]
        public ColorMode ColorMode
        {
            get => _colorMode;
            set { _colorMode = value; Invalidate(); }
        }

        [Category("Plot3D Appearance")]
        public Color SolidColor
        {
            get => _solidColor;
            set { _solidColor = value; Invalidate(); }
        }

        [Category("Plot3D Appearance")]
        public float LineWidth
        {
            get => _lineWidth;
            set { _lineWidth = Math.Max(0.5f, value); Invalidate(); }
        }

        [Category("Plot3D Appearance")]
        public DashStyle DashStyle
        {
            get => _dashStyle;
            set { _dashStyle = value; Invalidate(); }
        }

        [Category("Plot3D Appearance")]
        public MarkerType Marker
        {
            get => _markerType;
            set { _markerType = value; Invalidate(); }
        }

        [Category("Plot3D Appearance")]
        public float MarkerSize
        {
            get => _markerSize;
            set { _markerSize = Math.Max(2.0f, value); Invalidate(); }
        }

        [Category("Plot3D Camera")]
        public double Azimuth
        {
            get => _azimuth;
            set
            {
                double normalized = (value + 180.0) % 360.0 - 180.0;
                if (Math.Abs(_azimuth - normalized) > 1e-6)
                {
                    _azimuth = normalized;
                    _axes.Camera.Azimuth = _azimuth;
                    CameraChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        [Category("Plot3D Camera")]
        public double Elevation
        {
            get => _elevation;
            set
            {
                double clamped = Math.Max(-89.9, Math.Min(89.9, value));
                if (Math.Abs(_elevation - clamped) > 1e-6)
                {
                    _elevation = clamped;
                    _axes.Camera.Elevation = _elevation;
                    CameraChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        [Category("Plot3D Camera")]
        public double Zoom
        {
            get => _zoom;
            set
            {
                double clamped = Math.Max(0.1, Math.Min(10.0, value));
                if (Math.Abs(_zoom - clamped) > 1e-6)
                {
                    _zoom = clamped;
                    CameraChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        [Category("Plot3D Camera")]
        public ProjectionMode Projection
        {
            get => _axes.Camera.Projection;
            set
            {
                _axes.Camera.Projection = value;
                Invalidate();
            }
        }

        [Category("Plot3D Features")]
        public bool ShowGrid
        {
            get => _axes.GridOn;
            set { _axes.GridOn = value; Invalidate(); }
        }

        [Category("Plot3D Features")]
        public bool ShowBox
        {
            get => _axes.BoxOn;
            set { _axes.BoxOn = value; Invalidate(); }
        }

        [Category("Plot3D Features")]
        public bool ShowGizmo
        {
            get => _showGizmo;
            set { _showGizmo = value; Invalidate(); }
        }

        [Category("Plot3D Features")]
        public bool ShowHoverInfo
        {
            get => _showHoverInfo;
            set { _showHoverInfo = value; Invalidate(); }
        }

        [Category("Plot3D Animation")]
        public bool IsAnimating
        {
            get => _isAnimating;
            set
            {
                _isAnimating = value;
                if (_isAnimating) _animTimer.Start();
                else _animTimer.Stop();
            }
        }

        [Category("Plot3D Animation")]
        public double RotationSpeed
        {
            get => _rotationSpeed;
            set => _rotationSpeed = value;
        }

        #endregion

        #region Public Methods

        public void SetPlot(
            Func<double, double> funx,
            Func<double, double> funy,
            Func<double, double> funz,
            Interval interval,
            Plot3DOptions? options = null)
        {
            var opt = options ?? Plot3DOptions.CreateDefault();
            opt.TRange = interval;

            _currentLine = new ParameterizedFunctionLine3D(funx, funy, funz, opt);
            _axes.Clear();
            _axes.AddLine(_currentLine);
            _hoveredPoint = null;
            Invalidate();
        }

        public void SetPlot(
            string funxExpr,
            string funyExpr,
            string funzExpr,
            Interval interval,
            Plot3DOptions? options = null)
        {
            var funx = FormulaParser.Parse(funxExpr);
            var funy = FormulaParser.Parse(funyExpr);
            var funz = FormulaParser.Parse(funzExpr);
            SetPlot(funx, funy, funz, interval, options);
        }

        public void ResetView()
        {
            Azimuth = -37.5;
            Elevation = 30.0;
            Zoom = 1.0;
            _panOffset = PointF.Empty;
            Invalidate();
        }

        public void ExportToPng(string filePath, int width = 1920, int height = 1080)
        {
            using var bmp = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bmp))
            {
                RenderToGraphics(g, width, height);
            }
            bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
        }

        public void ExportToSvg(string filePath, int width = 1200, int height = 900)
        {
            var renderer = new SvgRenderer3D(width, height);
            string svg = renderer.Render(_axes);
            File.WriteAllText(filePath, svg);
        }

        public void ExportToObj(string filePath)
        {
            ObjExporter3D.SaveToFile(_axes, filePath);
        }

        public void ExportToCsv(string filePath)
        {
            if (_currentLine != null)
            {
                DataExporter3D.SaveCsv(_currentLine, filePath);
            }
        }

        #endregion

        #region Mouse Interaction

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            _isDragging = true;
            _dragButton = e.Button;
            _lastMousePos = e.Location;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isDragging)
            {
                int dx = e.X - _lastMousePos.X;
                int dy = e.Y - _lastMousePos.Y;
                _lastMousePos = e.Location;

                if (_dragButton == MouseButtons.Left)
                {
                    Azimuth += dx * 0.5;
                    Elevation -= dy * 0.5;
                }
                else if (_dragButton == MouseButtons.Right || _dragButton == MouseButtons.Middle)
                {
                    _panOffset.X += dx;
                    _panOffset.Y += dy;
                    Invalidate();
                }
            }
            else if (_showHoverInfo && _currentLine != null)
            {
                UpdateHoverInspection(e.Location);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isDragging = false;
            _dragButton = MouseButtons.None;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            double factor = e.Delta > 0 ? 1.15 : 0.87;
            Zoom *= factor;
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);
            ResetView();
        }

        private void UpdateHoverInspection(Point mouseLoc)
        {
            if (_currentLine == null || _currentLine.Points.Count == 0) return;

            var bounds = _axes.GetEffectiveBounds();
            var camera = _axes.Camera;
            camera.Azimuth = _azimuth;
            camera.Elevation = _elevation;
            camera.Setup(bounds, Width, Height, margin: 70.0 / _zoom);

            double minDist = double.MaxValue;
            (Point3D Point, double T, PointF ScreenPos)? nearest = null;

            var sampled = _currentLine.SampledData;
            int count = sampled.Points.Count;

            for (int i = 0; i < count; i++)
            {
                var pt = sampled.Points[i];
                if (!pt.IsValid) continue;

                var (sx, sy, _) = camera.Project(pt, Width, Height);
                if (double.IsNaN(sx) || double.IsNaN(sy)) continue;

                float finalX = (float)sx + _panOffset.X;
                float finalY = (float)sy + _panOffset.Y;

                double dist = Math.Sqrt(Math.Pow(mouseLoc.X - finalX, 2) + Math.Pow(mouseLoc.Y - finalY, 2));
                if (dist < 18.0 && dist < minDist)
                {
                    minDist = dist;
                    nearest = (pt, sampled.TValues[i], new PointF(finalX, finalY));
                }
            }

            if (nearest.HasValue != _hoveredPoint.HasValue ||
                (nearest.HasValue && _hoveredPoint.HasValue && nearest.Value.Point != _hoveredPoint.Value.Point))
            {
                _hoveredPoint = nearest;
                if (_hoveredPoint.HasValue)
                {
                    PointHovered?.Invoke(this, (_hoveredPoint.Value.Point, _hoveredPoint.Value.T));
                }
                else
                {
                    PointHovered?.Invoke(this, null);
                }
                Invalidate();
            }
        }

        #endregion

        #region Painting and GDI+ Rendering

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            RenderToGraphics(e.Graphics, Width, Height);
        }

        public void RenderToGraphics(Graphics g, int width, int height)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // Background
            using (var bgBrush = new SolidBrush(BackColor))
            {
                g.FillRectangle(bgBrush, 0, 0, width, height);
            }

            if (width < 20 || height < 20) return;

            var bounds = _axes.GetEffectiveBounds();
            var camera = _axes.Camera;
            camera.Azimuth = _azimuth;
            camera.Elevation = _elevation;
            camera.Setup(bounds, width, height, margin: 70.0 / _zoom);

            // Title
            if (!string.IsNullOrWhiteSpace(_axes.Title))
            {
                using var titleFont = new Font("Segoe UI", 12f, FontStyle.Bold);
                using var titleBrush = new SolidBrush(Color.FromArgb(40, 40, 40));
                using var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(_axes.Title, titleFont, titleBrush, width * 0.5f, 15f, sf);
            }

            // Render 3D Box, Planes, and Grid Lines
            RenderAxesBoxAndGrids(g, camera, bounds, width, height);

            // Render Curve Lines and Markers
            RenderCurve(g, camera, bounds, width, height);

            // Render Hover Highlight
            if (_showHoverInfo && _hoveredPoint.HasValue)
            {
                RenderHoverTooltip(g, _hoveredPoint.Value);
            }

            // Render 3D Orientation Gizmo
            if (_showGizmo)
            {
                RenderGizmo(g, camera, width, height);
            }
        }

        private void RenderAxesBoxAndGrids(Graphics g, Camera3D camera, Bounds3D bounds, int width, int height)
        {
            if (bounds.IsEmpty) return;

            Point3D[] corners = new Point3D[]
            {
                new Point3D(bounds.XMin, bounds.YMin, bounds.ZMin), // 0
                new Point3D(bounds.XMax, bounds.YMin, bounds.ZMin), // 1
                new Point3D(bounds.XMax, bounds.YMax, bounds.ZMin), // 2
                new Point3D(bounds.XMin, bounds.YMax, bounds.ZMin), // 3
                new Point3D(bounds.XMin, bounds.YMin, bounds.ZMax), // 4
                new Point3D(bounds.XMax, bounds.YMin, bounds.ZMax), // 5
                new Point3D(bounds.XMax, bounds.YMax, bounds.ZMax), // 6
                new Point3D(bounds.XMin, bounds.YMax, bounds.ZMax)  // 7
            };

            var (xTicks, yTicks, zTicks) = _axes.CalculateTicks(bounds);

            // Draw Grids
            if (_axes.GridOn)
            {
                using var gridPen = new Pen(Color.FromArgb(228, 230, 235), 1.0f) { DashStyle = DashStyle.Dot };

                // Bottom grid (Z = ZMin)
                foreach (var x in xTicks)
                {
                    DrawProjectedLine(g, gridPen, camera, new Point3D(x, bounds.YMin, bounds.ZMin), new Point3D(x, bounds.YMax, bounds.ZMin), width, height);
                }
                foreach (var y in yTicks)
                {
                    DrawProjectedLine(g, gridPen, camera, new Point3D(bounds.XMin, y, bounds.ZMin), new Point3D(bounds.XMax, y, bounds.ZMin), width, height);
                }

                // Back grid (Y = YMax)
                foreach (var x in xTicks)
                {
                    DrawProjectedLine(g, gridPen, camera, new Point3D(x, bounds.YMax, bounds.ZMin), new Point3D(x, bounds.YMax, bounds.ZMax), width, height);
                }
                foreach (var z in zTicks)
                {
                    DrawProjectedLine(g, gridPen, camera, new Point3D(bounds.XMin, bounds.YMax, z), new Point3D(bounds.XMax, bounds.YMax, z), width, height);
                }

                // Left grid (X = XMin)
                foreach (var y in yTicks)
                {
                    DrawProjectedLine(g, gridPen, camera, new Point3D(bounds.XMin, y, bounds.ZMin), new Point3D(bounds.XMin, y, bounds.ZMax), width, height);
                }
                foreach (var z in zTicks)
                {
                    DrawProjectedLine(g, gridPen, camera, new Point3D(bounds.XMin, bounds.YMin, z), new Point3D(bounds.XMin, bounds.YMax, z), width, height);
                }
            }

            // Draw Box Edges
            if (_axes.BoxOn)
            {
                using var boxPen = new Pen(Color.FromArgb(160, 165, 175), 1.2f);
                int[][] edges = new int[][]
                {
                    new[] {0, 1}, new[] {1, 2}, new[] {2, 3}, new[] {3, 0},
                    new[] {4, 5}, new[] {5, 6}, new[] {6, 7}, new[] {7, 4},
                    new[] {0, 4}, new[] {1, 5}, new[] {2, 6}, new[] {3, 7}
                };

                foreach (var edge in edges)
                {
                    DrawProjectedLine(g, boxPen, camera, corners[edge[0]], corners[edge[1]], width, height);
                }
            }

            // Draw Tick Labels and Axis Labels
            using var font = new Font("Segoe UI", 8.0f);
            using var textBrush = new SolidBrush(Color.FromArgb(90, 95, 105));
            using var labelFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            using var labelBrush = new SolidBrush(Color.FromArgb(30, 35, 45));
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            // X-Ticks along edge (YMin, ZMin)
            foreach (var x in xTicks)
            {
                var pt = camera.Project(new Point3D(x, bounds.YMin, bounds.ZMin), width, height);
                if (!double.IsNaN(pt.ScreenX))
                {
                    g.DrawString(x.ToString("G4", CultureInfo.InvariantCulture), font, textBrush, (float)pt.ScreenX + _panOffset.X, (float)pt.ScreenY + 12f + _panOffset.Y, sf);
                }
            }

            // Y-Ticks along edge (XMin, ZMin)
            foreach (var y in yTicks)
            {
                var pt = camera.Project(new Point3D(bounds.XMin, y, bounds.ZMin), width, height);
                if (!double.IsNaN(pt.ScreenX))
                {
                    g.DrawString(y.ToString("G4", CultureInfo.InvariantCulture), font, textBrush, (float)pt.ScreenX - 16f + _panOffset.X, (float)pt.ScreenY + 6f + _panOffset.Y, sf);
                }
            }

            // Z-Ticks along edge (XMin, YMin)
            foreach (var z in zTicks)
            {
                var pt = camera.Project(new Point3D(bounds.XMin, bounds.YMin, z), width, height);
                if (!double.IsNaN(pt.ScreenX))
                {
                    g.DrawString(z.ToString("G4", CultureInfo.InvariantCulture), font, textBrush, (float)pt.ScreenX - 18f + _panOffset.X, (float)pt.ScreenY + _panOffset.Y, sf);
                }
            }

            // Axis Names (X, Y, Z)
            var xMid = camera.Project(new Point3D((bounds.XMin + bounds.XMax) * 0.5, bounds.YMin, bounds.ZMin), width, height);
            if (!double.IsNaN(xMid.ScreenX))
            {
                g.DrawString(_axes.XLabel, labelFont, labelBrush, (float)xMid.ScreenX + _panOffset.X, (float)xMid.ScreenY + 26f + _panOffset.Y, sf);
            }

            var yMid = camera.Project(new Point3D(bounds.XMin, (bounds.YMin + bounds.YMax) * 0.5, bounds.ZMin), width, height);
            if (!double.IsNaN(yMid.ScreenX))
            {
                g.DrawString(_axes.YLabel, labelFont, labelBrush, (float)yMid.ScreenX - 32f + _panOffset.X, (float)yMid.ScreenY + 10f + _panOffset.Y, sf);
            }

            var zMid = camera.Project(new Point3D(bounds.XMin, bounds.YMin, (bounds.ZMin + bounds.ZMax) * 0.5), width, height);
            if (!double.IsNaN(zMid.ScreenX))
            {
                g.DrawString(_axes.ZLabel, labelFont, labelBrush, (float)zMid.ScreenX - 36f + _panOffset.X, (float)zMid.ScreenY + _panOffset.Y, sf);
            }
        }

        private void DrawProjectedLine(Graphics g, Pen pen, Camera3D camera, Point3D p1, Point3D p2, int width, int height)
        {
            var proj1 = camera.Project(p1, width, height);
            var proj2 = camera.Project(p2, width, height);

            if (!double.IsNaN(proj1.ScreenX) && !double.IsNaN(proj2.ScreenX))
            {
                g.DrawLine(pen,
                    (float)proj1.ScreenX + _panOffset.X, (float)proj1.ScreenY + _panOffset.Y,
                    (float)proj2.ScreenX + _panOffset.X, (float)proj2.ScreenY + _panOffset.Y);
            }
        }

        private void RenderCurve(Graphics g, Camera3D camera, Bounds3D bounds, int width, int height)
        {
            if (_currentLine == null || _currentLine.Points.Count < 2) return;

            var sampled = _currentLine.SampledData;
            int count = sampled.Points.Count;
            var tRange = _currentLine.Options.TRange;
            double tLength = tRange.Length > 1e-12 ? tRange.Length : 1.0;
            double zDelta = bounds.DeltaZ > 1e-12 ? bounds.DeltaZ : 1.0;

            // Project all points
            var screenPoints = new (PointF Pos, double Depth, double NormalizedVal, bool Valid)[count];
            for (int i = 0; i < count; i++)
            {
                var pt = sampled.Points[i];
                if (!pt.IsValid)
                {
                    screenPoints[i] = (PointF.Empty, double.NaN, 0.0, false);
                    continue;
                }

                var (sx, sy, depth) = camera.Project(pt, width, height);
                if (double.IsNaN(sx) || double.IsNaN(sy))
                {
                    screenPoints[i] = (PointF.Empty, double.NaN, 0.0, false);
                    continue;
                }

                double norm = 0.0;
                switch (_colorMode)
                {
                    case ColorMode.GradientByZ:
                        norm = (pt.Z - bounds.ZMin) / zDelta;
                        break;
                    case ColorMode.GradientByT:
                        norm = (sampled.TValues[i] - tRange.Min) / tLength;
                        break;
                    case ColorMode.GradientByCurvature:
                        norm = Math.Min(1.0, _currentLine.GetCurvature(sampled.TValues[i]) * 0.5);
                        break;
                }

                screenPoints[i] = (new PointF((float)sx + _panOffset.X, (float)sy + _panOffset.Y), depth, norm, true);
            }

            // Draw line segments
            for (int i = 0; i < count - 1; i++)
            {
                var p1 = screenPoints[i];
                var p2 = screenPoints[i + 1];

                if (!p1.Valid || !p2.Valid) continue;

                // Color calculation
                Color segColor = _solidColor;
                if (_colorMode != ColorMode.SolidColor)
                {
                    double avgNorm = 0.5 * (p1.NormalizedVal + p2.NormalizedVal);
                    segColor = _colorMap.GetColor(avgNorm);
                }

                using var pen = new Pen(segColor, _lineWidth)
                {
                    DashStyle = _dashStyle,
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round,
                    LineJoin = LineJoin.Round
                };

                g.DrawLine(pen, p1.Pos, p2.Pos);

                // Markers
                if (_markerType != MarkerType.None)
                {
                    DrawMarker(g, p1.Pos, segColor);
                }
            }

            // Last marker
            if (_markerType != MarkerType.None && count > 0 && screenPoints[count - 1].Valid)
            {
                Color lastColor = _colorMode != ColorMode.SolidColor ? _colorMap.GetColor(screenPoints[count - 1].NormalizedVal) : _solidColor;
                DrawMarker(g, screenPoints[count - 1].Pos, lastColor);
            }
        }

        private void DrawMarker(Graphics g, PointF pt, Color color)
        {
            float half = _markerSize * 0.5f;
            using var brush = new SolidBrush(color);
            using var pen = new Pen(Color.FromArgb(20, 20, 20), 1.0f);

            switch (_markerType)
            {
                case MarkerType.Circle:
                    g.FillEllipse(brush, pt.X - half, pt.Y - half, _markerSize, _markerSize);
                    g.DrawEllipse(pen, pt.X - half, pt.Y - half, _markerSize, _markerSize);
                    break;
                case MarkerType.Square:
                    g.FillRectangle(brush, pt.X - half, pt.Y - half, _markerSize, _markerSize);
                    g.DrawRectangle(pen, pt.X - half, pt.Y - half, _markerSize, _markerSize);
                    break;
                case MarkerType.Plus:
                    using (var p = new Pen(color, 1.8f))
                    {
                        g.DrawLine(p, pt.X - half, pt.Y, pt.X + half, pt.Y);
                        g.DrawLine(p, pt.X, pt.Y - half, pt.X, pt.Y + half);
                    }
                    break;
                case MarkerType.Cross:
                    using (var p = new Pen(color, 1.8f))
                    {
                        g.DrawLine(p, pt.X - half, pt.Y - half, pt.X + half, pt.Y + half);
                        g.DrawLine(p, pt.X - half, pt.Y + half, pt.X + half, pt.Y - half);
                    }
                    break;
                case MarkerType.Asterisk:
                    using (var p = new Pen(color, 1.5f))
                    {
                        g.DrawLine(p, pt.X - half, pt.Y, pt.X + half, pt.Y);
                        g.DrawLine(p, pt.X, pt.Y - half, pt.X, pt.Y + half);
                        g.DrawLine(p, pt.X - half * 0.7f, pt.Y - half * 0.7f, pt.X + half * 0.7f, pt.Y + half * 0.7f);
                        g.DrawLine(p, pt.X - half * 0.7f, pt.Y + half * 0.7f, pt.X + half * 0.7f, pt.Y - half * 0.7f);
                    }
                    break;
            }
        }

        private void RenderHoverTooltip(Graphics g, (Point3D Point, double T, PointF ScreenPos) item)
        {
            var (pt, tVal, pos) = item;

            // Highlight ring
            using (var ringPen = new Pen(Color.FromArgb(220, 20, 60), 2.5f))
            {
                g.DrawEllipse(ringPen, pos.X - 6f, pos.Y - 6f, 12f, 12f);
            }
            using (var centerBrush = new SolidBrush(Color.White))
            {
                g.FillEllipse(centerBrush, pos.X - 3f, pos.Y - 3f, 6f, 6f);
            }

            // Info Card Box
            string line1 = $"t = {tVal:F3}";
            string line2 = $"X = {pt.X:F4}";
            string line3 = $"Y = {pt.Y:F4}";
            string line4 = $"Z = {pt.Z:F4}";

            using var font = new Font("Consolas", 8.5f);
            var sz1 = g.MeasureString(line1, font);
            var sz2 = g.MeasureString(line2, font);
            var sz3 = g.MeasureString(line3, font);
            var sz4 = g.MeasureString(line4, font);

            float cardWidth = Math.Max(sz1.Width, Math.Max(sz2.Width, Math.Max(sz3.Width, sz4.Width))) + 18f;
            float cardHeight = sz1.Height * 4 + 14f;

            float boxX = pos.X + 14f;
            float boxY = pos.Y - cardHeight * 0.5f;

            if (boxX + cardWidth > Width - 10f) boxX = pos.X - cardWidth - 14f;
            if (boxY < 10f) boxY = 10f;
            if (boxY + cardHeight > Height - 10f) boxY = Height - cardHeight - 10f;

            var rect = new RectangleF(boxX, boxY, cardWidth, cardHeight);

            using (var bgBrush = new SolidBrush(Color.FromArgb(235, 30, 35, 45)))
            {
                g.FillPath(bgBrush, GetRoundedRect(rect, 6f));
            }
            using (var borderPen = new Pen(Color.FromArgb(100, 110, 130), 1.0f))
            {
                g.DrawPath(borderPen, GetRoundedRect(rect, 6f));
            }

            using (var textBrush = new SolidBrush(Color.FromArgb(240, 240, 245)))
            {
                float textY = boxY + 6f;
                g.DrawString(line1, font, textBrush, boxX + 8f, textY);
                textY += sz1.Height;
                g.DrawString(line2, font, textBrush, boxX + 8f, textY);
                textY += sz2.Height;
                g.DrawString(line3, font, textBrush, boxX + 8f, textY);
                textY += sz3.Height;
                g.DrawString(line4, font, textBrush, boxX + 8f, textY);
            }
        }

        private void RenderGizmo(Graphics g, Camera3D camera, int width, int height)
        {
            float cx = 45f;
            float cy = height - 45f;
            float length = 28f;

            // Camera vectors
            double azRad = _azimuth * Math.PI / 180.0;
            double elRad = _elevation * Math.PI / 180.0;

            double cosAz = Math.Cos(azRad), sinAz = Math.Sin(azRad);
            double cosEl = Math.Cos(elRad), sinEl = Math.Sin(elRad);

            // Right: (cosAz, sinAz, 0)
            // Up: (-sinAz*sinEl, cosAz*sinEl, cosEl)
            // Screen coords for UnitX (1,0,0), UnitY (0,1,0), UnitZ (0,0,1)
            PointF projX = new PointF(cx + (float)(cosAz * length), cy - (float)(-sinAz * sinEl * length));
            PointF projY = new PointF(cx + (float)(sinAz * length), cy - (float)(cosAz * sinEl * length));
            PointF projZ = new PointF(cx, cy - (float)(cosEl * length));

            using var redPen = new Pen(Color.FromArgb(230, 50, 50), 2.2f) { EndCap = LineCap.ArrowAnchor };
            using var greenPen = new Pen(Color.FromArgb(40, 180, 50), 2.2f) { EndCap = LineCap.ArrowAnchor };
            using var bluePen = new Pen(Color.FromArgb(40, 100, 240), 2.2f) { EndCap = LineCap.ArrowAnchor };
            using var font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            // Draw axis lines
            g.DrawLine(redPen, cx, cy, projX.X, projX.Y);
            g.DrawLine(greenPen, cx, cy, projY.X, projY.Y);
            g.DrawLine(bluePen, cx, cy, projZ.X, projZ.Y);

            // Draw labels
            using var redBrush = new SolidBrush(Color.FromArgb(230, 50, 50));
            using var greenBrush = new SolidBrush(Color.FromArgb(40, 180, 50));
            using var blueBrush = new SolidBrush(Color.FromArgb(40, 100, 240));

            g.DrawString("X", font, redBrush, projX.X + 6f, projX.Y, sf);
            g.DrawString("Y", font, greenBrush, projY.X + 6f, projY.Y, sf);
            g.DrawString("Z", font, blueBrush, projZ.X, projZ.Y - 8f, sf);
        }

        private static GraphicsPath GetRoundedRect(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();
            float d = radius * 2f;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

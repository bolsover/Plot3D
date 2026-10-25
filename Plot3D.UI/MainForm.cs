using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AlibreX;
using Plot3D.Core;
using Plot3D.Expressions;
using Plot3D.Figure;
using Plot3D.Projection;
using Plot3D.UI.Alibre;

namespace Plot3D.UI
{
    /// <summary>
    /// Main user interface window for entering 3D parametric formulas and visualizing interactive plots.
    /// Can be launched standalone or as an Alibre Design Add-On to generate 3D sketches in the Part environment.
    /// </summary>
    public sealed class MainForm : Form
    {
        // Alibre Design Integration Context
        private readonly IADRoot? _alibreRoot;
        private readonly IntPtr _alibreParentWindow;

        // UI Controls
        private readonly SplitContainer _splitContainer;
        private readonly Panel _sidePanel;
        private readonly Plot3DControl _plotControl;

        // Formula inputs
        private readonly ComboBox _cboPresets;
        private readonly TextBox _txtX;
        private readonly TextBox _txtY;
        private readonly TextBox _txtZ;
        private readonly TextBox _txtTMin;
        private readonly TextBox _txtTMax;
        private readonly Label _lblFormulaStatus;

        // Alibre 3D Sketch controls
        private readonly ComboBox _cboAlibreGeometryType;
        private readonly ComboBox _cboAlibreUnits;
        private readonly TextBox _txtAlibreScale;
        private readonly TextBox _txtAlibreSketchName;
        private readonly CheckBox _chkAlibreNewSketch;
        private readonly Button _btnCreateAlibre3DSketch;
        private readonly Label _lblAlibreStatus;

        // View inputs
        private readonly RadioButton _rbOrthographic;
        private readonly RadioButton _rbPerspective;
        private readonly CheckBox _chkShowGrid;
        private readonly CheckBox _chkShowBox;
        private readonly CheckBox _chkShowGizmo;
        private readonly CheckBox _chkShowHover;
        private readonly TrackBar _trkAzimuth;
        private readonly TrackBar _trkElevation;
        private readonly TrackBar _trkZoom;
        private readonly Label _lblAzimuthVal;
        private readonly Label _lblElevationVal;
        private readonly Label _lblZoomVal;

        // Action buttons
        private readonly Button _btnPlot;
        private readonly Button _btnResetView;
        private readonly Button _btnAnimate;

        // Export buttons
        private readonly Button _btnExportPng;
        private readonly Button _btnExportSvg;
        private readonly Button _btnExportObj;
        private readonly Button _btnExportCsv;
        private readonly Button _btnCopyClipboard;

        // Status bar
        private readonly StatusStrip _statusStrip;
        private readonly ToolStripStatusLabel _statusLabel;
        private readonly ToolStripStatusLabel _pointsLabel;
        private readonly ToolStripStatusLabel _arcLengthLabel;
        private readonly ToolStripStatusLabel _cameraLabel;
        private readonly ToolStripStatusLabel _hoverLabel;

        private bool _suppressPresetEvents;

        public MainForm() : this(null, IntPtr.Zero)
        {
        }

        public MainForm(IADRoot? alibreRoot, IntPtr alibreParentWindow = default)
        {
            _alibreRoot = alibreRoot;
            _alibreParentWindow = alibreParentWindow;

            Text = "Plot3D - 3D Parametric Formula Studio";
            Width = 1400;
            Height = 1060;
            MinimumSize = new Size(900, 650);
            StartPosition = alibreParentWindow != IntPtr.Zero ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9f);
            BackColor = Color.FromArgb(245, 246, 248);

            // Split Container
            _splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                FixedPanel = FixedPanel.Panel1,
                SplitterDistance = 400,
                SplitterWidth = 4,
                BackColor = Color.FromArgb(220, 224, 230)
            };

            // Sidebar Panel
            _sidePanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(250, 251, 252),
                Padding = new Padding(12, 10, 16, 12)
                
            };
           

            _splitContainer.Panel1.Controls.Add(_sidePanel);
            

            // 3D Canvas Control
            _plotControl = new Plot3DControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            _splitContainer.Panel2.Controls.Add(_plotControl);


            // Status Strip
            _statusStrip = new StatusStrip { BackColor = Color.FromArgb(240, 242, 245) };
            _statusLabel = new ToolStripStatusLabel("Ready") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            _pointsLabel = new ToolStripStatusLabel("Points: 0") { BorderSides = ToolStripStatusLabelBorderSides.Left, BorderStyle = Border3DStyle.Etched };
            _arcLengthLabel = new ToolStripStatusLabel("Arc Length: 0.0000") { BorderSides = ToolStripStatusLabelBorderSides.Left, BorderStyle = Border3DStyle.Etched };
            _cameraLabel = new ToolStripStatusLabel("Az: -37.5° | El: 30.0°") { BorderSides = ToolStripStatusLabelBorderSides.Left, BorderStyle = Border3DStyle.Etched };
            _hoverLabel = new ToolStripStatusLabel("Hover: -") { BorderSides = ToolStripStatusLabelBorderSides.Left, BorderStyle = Border3DStyle.Etched };

            _statusStrip.Items.AddRange(new ToolStripItem[]
            {
                _statusLabel,
                _pointsLabel,
                _arcLengthLabel,
                _cameraLabel,
                _hoverLabel
            });

            Controls.Add(_splitContainer);
            Controls.Add(_statusStrip);

            // Initialize Controls
            int curY = 8;

            // 1. Title Banner
            var lblTitle = new Label
            {
                Text = "3D Formula Studio",
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(12, curY),
                AutoSize = true
            };
            _sidePanel.Controls.Add(lblTitle);
            curY += 30;

            // var lblSubtitle = new Label
            // {
            //     Text = "Interactive visualizer",
            //     Font = new Font("Segoe UI", 8.2f),
            //     ForeColor = Color.FromArgb(100, 116, 139),
            //     Location = new Point(12, curY),
            //     AutoSize = true
            // };
            // _sidePanel.Controls.Add(lblSubtitle);
            // curY += 22;

            // 2. Presets Group
            var grpPresets = CreateSectionHeader("Predefined Presets", ref curY);
            _cboPresets = new ComboBox
            {
                Location = new Point(16, curY),
                Width = 320,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f)
            };
            foreach (var preset in PresetCurve.Presets)
            {
                _cboPresets.Items.Add(preset);
            }
            _cboPresets.SelectedIndex = 0;
            _cboPresets.SelectedIndexChanged += OnPresetChanged;
            _sidePanel.Controls.Add(_cboPresets);
            curY += 34;

            // 3. Formula Input Group
            var grpFormulas = CreateSectionHeader("Parametric Equations", ref curY);

            // X(t)
            var lblX = new Label { Text = "X(t) =", Location = new Point(16, curY + 2), AutoSize = true, Font = new Font("Segoe UI Semibold", 9f) };
            _txtX = new TextBox { Location = new Point(62, curY), Width = 274, Font = new Font("Consolas", 9.5f) };
            _sidePanel.Controls.Add(lblX);
            _sidePanel.Controls.Add(_txtX);
            curY += 30;

            // Y(t)
            var lblY = new Label { Text = "Y(t) =", Location = new Point(16, curY + 2), AutoSize = true, Font = new Font("Segoe UI Semibold", 9f) };
            _txtY = new TextBox { Location = new Point(62, curY), Width = 274, Font = new Font("Consolas", 9.5f) };
            _sidePanel.Controls.Add(lblY);
            _sidePanel.Controls.Add(_txtY);
            curY += 30;

            // Z(t)
            var lblZ = new Label { Text = "Z(t) =", Location = new Point(16, curY + 2), AutoSize = true, Font = new Font("Segoe UI Semibold", 9f) };
            _txtZ = new TextBox { Location = new Point(62, curY), Width = 274, Font = new Font("Consolas", 9.5f) };
            _sidePanel.Controls.Add(lblZ);
            _sidePanel.Controls.Add(_txtZ);
            curY += 30;

            // Status message
            _lblFormulaStatus = new Label
            {
                Location = new Point(16, curY),
                Width = 320,
                Height = 22,
                Font = new Font("Segoe UI", 8.2f),
                ForeColor = Color.FromArgb(220, 38, 38)
            };
            _sidePanel.Controls.Add(_lblFormulaStatus);
            curY += 24;

            // 4. Parameter Domain [tmin, tmax]
            var grpDomain = CreateSectionHeader("Parameter Range (t)", ref curY);
            var lblTMin = new Label { Text = "t min:", Location = new Point(16, curY + 2), AutoSize = true };
            _txtTMin = new TextBox { Location = new Point(58, curY), Width = 100, Text = "0", Font = new Font("Consolas", 9f) };

            var lblTMax = new Label { Text = "t max:", Location = new Point(176, curY + 2), AutoSize = true };
            _txtTMax = new TextBox { Location = new Point(220, curY), Width = 116, Text = "6*pi", Font = new Font("Consolas", 9f) };

            _sidePanel.Controls.Add(lblTMin);
            _sidePanel.Controls.Add(_txtTMin);
            _sidePanel.Controls.Add(lblTMax);
            _sidePanel.Controls.Add(_txtTMax);
            curY += 30;

            // Quick range buttons
            var pnlQuickRanges = new FlowLayoutPanel
            {
                Location = new Point(16, curY),
                Width = 320,
                Height = 60,
                WrapContents = true,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            AddQuickRangeButton(pnlQuickRanges, "[0, 2π]", "0", "2*pi");
            AddQuickRangeButton(pnlQuickRanges, "[0, 6π]", "0", "6*pi");
            AddQuickRangeButton(pnlQuickRanges, "[-5, 5]", "-5", "5");
            AddQuickRangeButton(pnlQuickRanges, "[-2π, 2π]", "-2*pi", "2*pi");
            AddQuickRangeButton(pnlQuickRanges, "[0, 20]", "0", "20");
            AddQuickRangeButton(pnlQuickRanges, "[0, 50]", "0", "50");
            AddQuickRangeButton(pnlQuickRanges, "[0, 100]", "0", "100");
            
            _sidePanel.Controls.Add(pnlQuickRanges);
            curY += 65;

            // 5. Action Buttons (Plot, Reset, Animate)
            _btnPlot = new Button
            {
                Text = "▶  Plot Formula",
                Location = new Point(16, curY),
                Width = 320,
                Height = 36,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnPlot.FlatAppearance.BorderSize = 0;
            _btnPlot.Click += (s, e) => PlotCurrentFormula();
            _sidePanel.Controls.Add(_btnPlot);
            curY += 42;

            var pnlActionRow = new FlowLayoutPanel
            {
                Location = new Point(16, curY),
                Width = 320,
                Height = 32,
                WrapContents = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            _btnResetView = new Button
            {
                Text = "🔄 Reset View",
                Width = 154,
                Height = 30,
                BackColor = Color.FromArgb(241, 245, 249),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnResetView.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            _btnResetView.Click += (s, e) => _plotControl.ResetView();

            _btnAnimate = new Button
            {
                Text = "⏵ Spin Turntable",
                Width = 160,
                Height = 30,
                BackColor = Color.FromArgb(241, 245, 249),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnAnimate.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            _btnAnimate.Click += (s, e) =>
            {
                _plotControl.IsAnimating = !_plotControl.IsAnimating;
                _btnAnimate.Text = _plotControl.IsAnimating ? "⏸ Stop Turntable" : "⏵ Spin Turntable";
                _btnAnimate.BackColor = _plotControl.IsAnimating ? Color.FromArgb(254, 226, 226) : Color.FromArgb(241, 245, 249);
            };

            pnlActionRow.Controls.Add(_btnResetView);
            pnlActionRow.Controls.Add(_btnAnimate);
            _sidePanel.Controls.Add(pnlActionRow);
            curY += 50;

            // 6. Alibre 3D Sketch Integration Group
            var grpAlibre = CreateSectionHeader("Alibre 3D Sketch (Part)", ref curY);

            // Curve Geometry Type
            var lblGeom = new Label { Text = "Curve:", Location = new Point(16, curY + 2), AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5f) };
            _cboAlibreGeometryType = new ComboBox
            {
                Location = new Point(72, curY),
                Width = 264,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8.5f)
            };
            _cboAlibreGeometryType.Items.AddRange(new object[]
            {
                "B-Spline (Interpolation)",
                "Polyline (Line Segments)",
                "Point Nodes"
            });
            _cboAlibreGeometryType.SelectedIndex = 0;
            _sidePanel.Controls.Add(lblGeom);
            _sidePanel.Controls.Add(_cboAlibreGeometryType);
            curY += 30;

            // Unit Scale Mode
            var lblUnits = new Label { Text = "Units:", Location = new Point(16, curY + 2), AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5f) };
            _cboAlibreUnits = new ComboBox
            {
                Location = new Point(72, curY),
                Width = 170,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8.5f)
            };
            _cboAlibreUnits.Items.AddRange(new object[]
            {
                "Part Display Units",
                "Millimeters (mm)",
                "Centimeters (cm)",
                "Inches (in)",
                "Meters (m)",
                "Custom Scale"
            });
            _cboAlibreUnits.SelectedIndex = 0;

            _txtAlibreScale = new TextBox
            {
                Location = new Point(248, curY),
                Width = 88,
                Text = "1.0",
                Enabled = false,
                Font = new Font("Consolas", 8.5f)
            };
            _cboAlibreUnits.SelectedIndexChanged += (s, e) =>
            {
                _txtAlibreScale.Enabled = _cboAlibreUnits.SelectedIndex == (int)AlibreUnitScale.Custom;
            };

            _sidePanel.Controls.Add(lblUnits);
            _sidePanel.Controls.Add(_cboAlibreUnits);
            _sidePanel.Controls.Add(_txtAlibreScale);
            curY += 30;

            // Sketch Name & Target
            var lblSketchName = new Label { Text = "Name:", Location = new Point(16, curY + 2), AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5f) };
            _txtAlibreSketchName = new TextBox
            {
                Location = new Point(72, curY),
                Width = 264,
                Font = new Font("Segoe UI", 8.5f),
                Text = "Plot3D Curve"
            };
            _sidePanel.Controls.Add(lblSketchName);
            _sidePanel.Controls.Add(_txtAlibreSketchName);
            curY += 28;

            _chkAlibreNewSketch = new CheckBox
            {
                Text = "Create in New 3D Sketch",
                Checked = true,
                Location = new Point(72, curY),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.2f)
            };
            _sidePanel.Controls.Add(_chkAlibreNewSketch);
            curY += 26;

            // Action Button
            _btnCreateAlibre3DSketch = new Button
            {
                Text = "📐 Create 3D Sketch in Alibre",
                Location = new Point(16, curY),
                Width = 320,
                Height = 34,
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnCreateAlibre3DSketch.FlatAppearance.BorderSize = 0;
            _btnCreateAlibre3DSketch.Click += (s, e) => CreateAlibre3DSketch();
            _sidePanel.Controls.Add(_btnCreateAlibre3DSketch);
            curY += 38;

            // Status label for Alibre
            _lblAlibreStatus = new Label
            {
                Location = new Point(16, curY),
                Width = 320,
                Height = 32,
                Font = new Font("Segoe UI", 8.2f),
                ForeColor = Color.FromArgb(100, 116, 139)
            };
            _sidePanel.Controls.Add(_lblAlibreStatus);
            curY += 36;

            // 7. View & Camera Angles
            var grpView = CreateSectionHeader("View & Camera Angles", ref curY);

            // Projection Radio Buttons
            var pnlProj = new FlowLayoutPanel
            {
                Location = new Point(16, curY),
                Width = 320,
                Height = 24,
                WrapContents = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            _rbOrthographic = new RadioButton { Text = "Orthographic", Checked = true, AutoSize = true, Margin = new Padding(0, 0, 14, 0) };
            _rbPerspective = new RadioButton { Text = "Perspective", AutoSize = true };
            _rbOrthographic.CheckedChanged += (s, e) => { if (_rbOrthographic.Checked) _plotControl.Projection = ProjectionMode.Orthographic; };
            _rbPerspective.CheckedChanged += (s, e) => { if (_rbPerspective.Checked) _plotControl.Projection = ProjectionMode.Perspective; };
            pnlProj.Controls.Add(_rbOrthographic);
            pnlProj.Controls.Add(_rbPerspective);
            _sidePanel.Controls.Add(pnlProj);
            curY += 28;

            // Checkboxes (Grid, Box, Gizmo, Hover)
            var pnlToggles = new FlowLayoutPanel
            {
                Location = new Point(16, curY),
                Width = 320,
                Height = 55,
                WrapContents = true,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            _chkShowGrid = new CheckBox { Text = "Grid", Checked = true, AutoSize = true, Width = 65 };
            _chkShowBox = new CheckBox { Text = "3D Box", Checked = true, AutoSize = true, Width = 75 };
            _chkShowGizmo = new CheckBox { Text = "Tripod Gizmo", Checked = true, AutoSize = true, Width = 110 };
            _chkShowHover = new CheckBox { Text = "Inspect Tooltip", Checked = true, AutoSize = true, Width = 110 };

            _chkShowGrid.CheckedChanged += (s, e) => _plotControl.ShowGrid = _chkShowGrid.Checked;
            _chkShowBox.CheckedChanged += (s, e) => _plotControl.ShowBox = _chkShowBox.Checked;
            _chkShowGizmo.CheckedChanged += (s, e) => _plotControl.ShowGizmo = _chkShowGizmo.Checked;
            _chkShowHover.CheckedChanged += (s, e) => _plotControl.ShowHoverInfo = _chkShowHover.Checked;

            pnlToggles.Controls.Add(_chkShowGrid);
            pnlToggles.Controls.Add(_chkShowBox);
            pnlToggles.Controls.Add(_chkShowGizmo);
            pnlToggles.Controls.Add(_chkShowHover);
            _sidePanel.Controls.Add(pnlToggles);
            curY += 50;

            // Azimuth Slider
            var lblAz = new Label { Text = "Azimuth:", Location = new Point(16, curY + 2), AutoSize = true };
            _trkAzimuth = new TrackBar { Location = new Point(78, curY), Width = 190, Minimum = -180, Maximum = 180, Value = -38, TickFrequency = 45 };
            _lblAzimuthVal = new Label { Text = "-37.5°", Location = new Point(274, curY + 2), AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5f) };
            _trkAzimuth.ValueChanged += (s, e) =>
            {
                _plotControl.Azimuth = _trkAzimuth.Value;
                _lblAzimuthVal.Text = $"{_trkAzimuth.Value:F1}°";
            };
            _sidePanel.Controls.Add(lblAz);
            _sidePanel.Controls.Add(_trkAzimuth);
            _sidePanel.Controls.Add(_lblAzimuthVal);
            curY += 50;

            // Elevation Slider
            var lblEl = new Label { Text = "Elevation:", Location = new Point(16, curY + 2), AutoSize = true };
            _trkElevation = new TrackBar { Location = new Point(78, curY), Width = 190, Minimum = -89, Maximum = 89, Value = 30, TickFrequency = 30 };
            _lblElevationVal = new Label { Text = "30.0°", Location = new Point(274, curY + 2), AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5f) };
            _trkElevation.ValueChanged += (s, e) =>
            {
                _plotControl.Elevation = _trkElevation.Value;
                _lblElevationVal.Text = $"{_trkElevation.Value:F1}°";
            };
            _sidePanel.Controls.Add(lblEl);
            _sidePanel.Controls.Add(_trkElevation);
            _sidePanel.Controls.Add(_lblElevationVal);
            curY += 50;

            // Zoom Slider
            var lblZm = new Label { Text = "Zoom:", Location = new Point(16, curY + 2), AutoSize = true };
            _trkZoom = new TrackBar { Location = new Point(78, curY), Width = 190, Minimum = 20, Maximum = 400, Value = 100, TickFrequency = 50 };
            _lblZoomVal = new Label { Text = "100%", Location = new Point(274, curY + 2), AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5f) };
            _trkZoom.ValueChanged += (s, e) =>
            {
                _plotControl.Zoom = _trkZoom.Value / 100.0;
                _lblZoomVal.Text = $"{_trkZoom.Value}%";
            };
            _sidePanel.Controls.Add(lblZm);
            _sidePanel.Controls.Add(_trkZoom);
            _sidePanel.Controls.Add(_lblZoomVal);
            curY += 50;

            // 8. Export Group
            var grpExport = CreateSectionHeader("Export & Sharing", ref curY);
            var pnlExport1 = new FlowLayoutPanel
            {
                Location = new Point(16, curY),
                Width = 320,
                Height = 30,
                WrapContents = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            _btnExportPng = CreateExportButton("PNG Image", (s, e) => ExportFile("PNG Image (*.png)|*.png", ".png", path => _plotControl.ExportToPng(path, 2400, 1600)));
            _btnExportSvg = CreateExportButton("SVG Vector", (s, e) => ExportFile("Scalable Vector Graphics (*.svg)|*.svg", ".svg", path => _plotControl.ExportToSvg(path, 1200, 900)));
            pnlExport1.Controls.Add(_btnExportPng);
            pnlExport1.Controls.Add(_btnExportSvg);
            _sidePanel.Controls.Add(pnlExport1);
            curY += 32;

            var pnlExport2 = new FlowLayoutPanel
            {
                Location = new Point(16, curY),
                Width = 320,
                Height = 30,
                WrapContents = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            _btnExportObj = CreateExportButton("3D OBJ", (s, e) => ExportFile("Wavefront 3D Object (*.obj)|*.obj", ".obj", path => _plotControl.ExportToObj(path)));
            _btnExportCsv = CreateExportButton("CSV Points", (s, e) => ExportFile("CSV Coordinates (*.csv)|*.csv", ".csv", path => _plotControl.ExportToCsv(path)));
            pnlExport2.Controls.Add(_btnExportObj);
            pnlExport2.Controls.Add(_btnExportCsv);
            _sidePanel.Controls.Add(pnlExport2);
            curY += 32;

            _btnCopyClipboard = new Button
            {
                Text = "📋 Copy Image to Clipboard",
                Location = new Point(16, curY),
                Width = 320,
                Height = 28,
                BackColor = Color.FromArgb(241, 245, 249),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnCopyClipboard.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            _btnCopyClipboard.Click += (s, e) =>
            {
                using var bmp = new Bitmap(_plotControl.Width, _plotControl.Height);
                using (var g = Graphics.FromImage(bmp))
                {
                    _plotControl.RenderToGraphics(g, bmp.Width, bmp.Height);
                }
                Clipboard.SetImage(bmp);
                _statusLabel.Text = "Plot image copied to clipboard.";
            };
            _sidePanel.Controls.Add(_btnCopyClipboard);
            curY += 40;

            // Wire Plot Events
            _plotControl.CameraChanged += (s, e) =>
            {
                _cameraLabel.Text = $"Az: {_plotControl.Azimuth:F1}° | El: {_plotControl.Elevation:F1}° | Zoom: {(_plotControl.Zoom * 100.0):F0}%";

                int azInt = (int)Math.Round(_plotControl.Azimuth);
                if (azInt >= _trkAzimuth.Minimum && azInt <= _trkAzimuth.Maximum && _trkAzimuth.Value != azInt)
                {
                    _trkAzimuth.Value = azInt;
                }

                int elInt = (int)Math.Round(_plotControl.Elevation);
                if (elInt >= _trkElevation.Minimum && elInt <= _trkElevation.Maximum && _trkElevation.Value != elInt)
                {
                    _trkElevation.Value = elInt;
                }
            };

            _plotControl.PointHovered += (s, item) =>
            {
                if (item.HasValue)
                {
                    var (pt, tVal) = item.Value;
                    _hoverLabel.Text = $"Hover: ({pt.X:F3}, {pt.Y:F3}, {pt.Z:F3}) at t={tVal:F2}";
                }
                else
                {
                    _hoverLabel.Text = "Hover: -";
                }
            };

            // Enter key on inputs triggers plot
            _txtX.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { PlotCurrentFormula(); e.SuppressKeyPress = true; } };
            _txtY.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { PlotCurrentFormula(); e.SuppressKeyPress = true; } };
            _txtZ.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { PlotCurrentFormula(); e.SuppressKeyPress = true; } };
            _txtTMin.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { PlotCurrentFormula(); e.SuppressKeyPress = true; } };
            _txtTMax.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { PlotCurrentFormula(); e.SuppressKeyPress = true; } };

            _splitContainer.SplitterDistance = 400;

            // Initial Plot
            LoadPreset((PresetCurve)_cboPresets.SelectedItem!);
        }

        private void AddQuickRangeButton(FlowLayoutPanel pnl, string label, string tmin, string tmax)
        {
            var btn = new Button
            {
                Text = label,
                AutoSize = true,
                Height = 24,
                BackColor = Color.FromArgb(241, 245, 249),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 4, 0),
                Font = new Font("Segoe UI", 7.8f),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btn.Click += (s, e) =>
            {
                _txtTMin.Text = tmin;
                _txtTMax.Text = tmax;
                PlotCurrentFormula();
            };
            pnl.Controls.Add(btn);
        }

        private Button CreateExportButton(string text, EventHandler handler)
        {
            var btn = new Button
            {
                Text = text,
                Width = 154,
                Height = 28,
                BackColor = Color.FromArgb(241, 245, 249),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 8, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btn.Click += handler;
            return btn;
        }

        private Label CreateSectionHeader(string title, ref int curY)
        {
            var lbl = new Label
            {
                Text = title,//.ToUpperInvariant(),
                Font = new Font("Segoe UI", 7.8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(14, curY),
                AutoSize = true
            };
            _sidePanel.Controls.Add(lbl);
            curY += 20;
            return lbl;
        }

        private void OnPresetChanged(object? sender, EventArgs e)
        {
            if (_suppressPresetEvents) return;
            if (_cboPresets.SelectedItem is PresetCurve preset)
            {
                LoadPreset(preset);
            }
        }

        private void LoadPreset(PresetCurve preset)
        {
            _suppressPresetEvents = true;
            _txtX.Text = preset.FormulaX;
            _txtY.Text = preset.FormulaY;
            _txtZ.Text = preset.FormulaZ;
            _txtTMin.Text = preset.TMin;
            _txtTMax.Text = preset.TMax;
            _txtAlibreSketchName.Text = $"Plot3D - {preset.Name}";
            // _cboColorMap.SelectedItem = preset.DefaultColorMap;
            _plotControl.Azimuth = preset.Azimuth;
            _plotControl.Elevation = preset.Elevation;
            _suppressPresetEvents = false;

            PlotCurrentFormula(preset.Name);
        }

        private void PlotCurrentFormula(string? title = null)
        {
            try
            {
                _lblFormulaStatus.Text = string.Empty;

                double tmin = FormulaParser.EvaluateConstant(_txtTMin.Text);
                double tmax = FormulaParser.EvaluateConstant(_txtTMax.Text);

                if (double.IsNaN(tmin) || double.IsNaN(tmax))
                {
                    _lblFormulaStatus.Text = "Error: Invalid t min or t max value.";
                    return;
                }

                var interval = new Interval(tmin, tmax);

                var options = Plot3DOptions.CreateDefault();
                options.TRange = interval;

                var watch = System.Diagnostics.Stopwatch.StartNew();
                _plotControl.SetPlot(_txtX.Text, _txtY.Text, _txtZ.Text, interval, options);
                watch.Stop();

                var line = _plotControl.CurrentLine;
                if (line != null)
                {
                    _plotControl.Axes.Title = title ?? $"{_txtX.Text}, {_txtY.Text}, {_txtZ.Text}";
                    _pointsLabel.Text = $"Points: {line.Points.Count}";
                    _arcLengthLabel.Text = $"Arc Length: {line.ArcLength:F4}";
                    _statusLabel.Text = $"Rendered {line.Points.Count} points in {watch.ElapsedMilliseconds} ms (Function Evaluations: {line.FunctionEvaluations}).";
                }
            }
            catch (Exception ex)
            {
                _lblFormulaStatus.Text = ex.Message;
                _statusLabel.Text = $"Plot Error: {ex.Message}";
            }
        }

        private void CreateAlibre3DSketch()
        {
            try
            {
                _lblAlibreStatus.ForeColor = Color.FromArgb(100, 116, 139);
                _lblAlibreStatus.Text = "Generating 3D sketch...";

                // Ensure curve is plotted
                if (_plotControl.CurrentLine == null || _plotControl.CurrentLine.Points.Count < 2)
                {
                    PlotCurrentFormula();
                }

                var line = _plotControl.CurrentLine;
                if (line == null || line.Points.Count < 2)
                {
                    _lblAlibreStatus.ForeColor = Color.FromArgb(220, 38, 38);
                    _lblAlibreStatus.Text = "Error: Please plot a valid 3D formula first.";
                    return;
                }

                var root = ResolveAlibreRoot();
                if (root == null)
                {
                    _lblAlibreStatus.ForeColor = Color.FromArgb(220, 38, 38);
                    _lblAlibreStatus.Text = "Alibre Design is not active. Open a Part in Alibre Design.";
                    MessageBox.Show(
                        "Unable to connect to Alibre Design.\n\nPlease ensure Alibre Design is running with an active Part workspace (.AD_PRT).",
                        "Alibre Design Not Connected",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var options = new Alibre3DSketchOptions
                {
                    GeometryType = (AlibreGeometryType)_cboAlibreGeometryType.SelectedIndex,
                    UnitScale = (AlibreUnitScale)_cboAlibreUnits.SelectedIndex,
                    SketchName = string.IsNullOrWhiteSpace(_txtAlibreSketchName.Text) ? null : _txtAlibreSketchName.Text.Trim(),
                    TargetMode = _chkAlibreNewSketch.Checked
                        ? AlibreSketchTargetMode.New3DSketch
                        : AlibreSketchTargetMode.ActiveOrNew3DSketch
                };

                if (options.UnitScale == AlibreUnitScale.Custom)
                {
                    if (double.TryParse(_txtAlibreScale.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double customScale) && customScale > 0)
                    {
                        options.CustomScaleFactor = customScale;
                    }
                    else
                    {
                        _lblAlibreStatus.ForeColor = Color.FromArgb(220, 38, 38);
                        _lblAlibreStatus.Text = "Error: Invalid custom scale factor.";
                        return;
                    }
                }

                string curveTitle = _cboPresets.SelectedItem is PresetCurve p ? p.Name : $"{_txtX.Text}, {_txtY.Text}, {_txtZ.Text}";
                var result = Alibre3DSketchService.Create3DSketch(root, line.Points, options, curveTitle);

                if (result.Success)
                {
                    _lblAlibreStatus.ForeColor = Color.FromArgb(16, 185, 129);
                    _lblAlibreStatus.Text = $"✓ {result.Message}";
                    _statusLabel.Text = $"Alibre: {result.Message}";
                }
                else
                {
                    _lblAlibreStatus.ForeColor = Color.FromArgb(220, 38, 38);
                    _lblAlibreStatus.Text = $"✗ {result.Message}";
                    _statusLabel.Text = $"Alibre Error: {result.Message}";
                    MessageBox.Show(result.Message, "Alibre 3D Sketch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                _lblAlibreStatus.ForeColor = Color.FromArgb(220, 38, 38);
                _lblAlibreStatus.Text = $"Error: {ex.Message}";
                _statusLabel.Text = $"Alibre Error: {ex.Message}";
            }
        }

        private void ExportFile(string filter, string defaultExt, Action<string> exportAction)
        {
            using var dlg = new SaveFileDialog
            {
                Filter = filter,
                DefaultExt = defaultExt,
                FileName = $"Plot3D_{DateTime.Now:yyyyMMdd_HHmmss}{defaultExt}"
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    exportAction(dlg.FileName);
                    _statusLabel.Text = $"Successfully exported to: {Path.GetFileName(dlg.FileName)}";
                    MessageBox.Show($"File successfully exported to:\n{dlg.FileName}", "Export Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export file:\n{ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private IADRoot? ResolveAlibreRoot()
        {
            if (_alibreRoot != null) return _alibreRoot;
            var root = AlibreAddOnAssembly.AlibreAddOn.GetRoot();
            if (root != null) return root;

            try
            {
                var hook = GetActiveComObject("AlibreX.AutomationHook") as IAutomationHook;
                if (hook != null && hook.Root is IADRoot adRoot)
                {
                    return adRoot;
                }
            }
            catch
            {
                // Alibre is not running or not registered as active COM object
            }

            return null;
        }

#if NETFRAMEWORK
        private static object? GetActiveComObject(string progId)
        {
            try
            {
                return Marshal.GetActiveObject(progId);
            }
            catch
            {
                return null;
            }
        }
#else
        [DllImport("oleaut32.dll", PreserveSig = false)]
        private static extern void GetActiveObject(ref Guid rclsid, IntPtr pvReserved, [MarshalAs(UnmanagedType.IUnknown)] out object ppunk);

        [DllImport("ole32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void CLSIDFromProgID(string lpszProgID, out Guid lpclsid);

        private static object? GetActiveComObject(string progId)
        {
            try
            {
                CLSIDFromProgID(progId, out var clsid);
                GetActiveObject(ref clsid, IntPtr.Zero, out var obj);
                return obj;
            }
            catch
            {
                return null;
            }
        }
#endif
    }
}

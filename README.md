# Plot3D

**Plot3D** is a high-performance, cross-platform .NET library and interactive desktop studio for evaluating, visualizing, and exporting 3D parametric mathematical curves $x(t)$, $y(t)$, and $z(t)$. It features an adaptive mesh refinement engine, a fast formula parser and compiler, multi-format 3D/vector export capabilities, and seamless CAD integration with Alibre Design.

---

## Table of Contents

- [Features](#features)
- [Project Architecture](#project-architecture)
- [Target Frameworks](#target-frameworks)
- [Quick Start](#quick-start)
  - [1. Programmatic Plotting with C# Delegates](#1-programmatic-plotting-with-c-delegates)
  - [2. Dynamic Mathematical Formula Parsing](#2-dynamic-mathematical-formula-parsing)
  - [3. Line Specifications & Styling](#3-line-specifications--styling)
  - [4. Multi-Format Exporting](#4-multi-format-exporting)
  - [5. Console ASCII / ANSI Rendering](#5-console-ascii--ansi-rendering)
- [Formula Syntax & Built-in Functions](#formula-syntax--built-in-functions)
- [Preset Curves](#preset-curves)
- [Color Maps & Palettes](#color-maps--palettes)
- [Interactive UI & Alibre Design Add-On](#interactive-ui--alibre-design-add-on)
  - [Standalone Interactive Studio](#standalone-interactive-studio)
  - [Alibre Design CAD Integration](#alibre-design-cad-integration)
- [Sample Console Application](#sample-console-application)
- [Building & Testing](#building--testing)
- [License](#license)

---

## Features

- **Adaptive Mesh Generation**: Recursive chord-error and curvature-based subdivision algorithm that allocates points where curvature is high while avoiding oversampling flat sections.
- **Dynamic Formula Compiler**: Compiles mathematical string expressions into fast, native .NET expression tree delegates (`Func<double, double>`) with support for implicit multiplication (`2pi*t`, `(2+cos(t))sin(t)`).
- **Comprehensive Math Library**: Built-in support for trigonometric, inverse trigonometric, hyperbolic, exponential, logarithmic, clamping, step/Heaviside, and special functions.
- **Multi-Format 3D & Vector Export**:
  - **SVG**: Clean vector graphic rendering with depth-sorted segments, coordinate axes, tick labels, and legends.
  - **Wavefront OBJ**: 3D polygonal polyline mesh export for Blender, MeshLab, CAD, or game engines.
  - **CSV / TSV / JSON**: Tabular point and parameter coordinate data export.
  - **PNG / Raster**: High-resolution image capture and clipboard copying via UI.
- **3D Camera & Projections**: Orthographic and perspective cameras with customizable azimuth, elevation, zoom, pan, and interactive orbit controls.
- **Color Palettes**: 13 built-in scientific color maps including `Turbo`, `Viridis`, `Plasma`, `Parula`, `ElectricBlue`, `Crimson`, `Emerald`, `Ocean`, `CoolWarm`, `Sunset`, `Rainbow`, `Magma`, and `Jet`.
- **Alibre Design 3D CAD Integration**: Direct generation of native 3D B-splines and sketches inside Alibre Design Part environments with unit scaling and target sketch selection.
- **Zero External Dependencies**: The core `Plot3D` engine is lightweight and self-contained.

---

## Project Architecture

```
Plot3D/
├── Plot3D/                 # Core plotting engine, formula parser, adaptive sampler & exporters
│   ├── Core/               # Point3D, Vector3D, Bounds3D, Interval, LineSpec, Plot3DOptions
│   ├── Export/             # ObjExporter3D, DataExporter3D (CSV, TSV, JSON)
│   ├── Expressions/        # FormulaParser, MathFunctionHelper, Expression Compiler
│   ├── Figure/             # Plot3DFigure, Axes3D
│   ├── Objects/            # ParameterizedFunctionLine3D
│   ├── Palettes/           # ColorMap3D & built-in palette definitions
│   ├── Presets/            # Built-in mathematical curve presets
│   ├── Projection/         # Camera3D, TickCalculator
│   ├── Rendering/          # SvgRenderer3D, ConsoleRenderer3D
│   └── Sampling/           # AdaptiveSampler3D, SampledCurve3D
├── Plot3D.UI/              # WinForms Interactive Studio & Alibre Design Add-On
│   ├── Alibre/             # Alibre 3D Sketch Service, Add-On entry point, Unit converters
│   ├── MainForm.cs         # Main GUI window with side-panel controls and real-time preview
│   └── Plot3DControl.cs    # Custom GDI+ interactive 3D rendering canvas
├── Plot3D.Sample/          # Console demonstration application
└── Plot3D.Tests/           # Unit test suite (100+ tests covering parser, sampler, UI, exports)
```

---

## Target Frameworks

| Project | Target Frameworks | Notes |
|---|---|---|
| **Plot3D** | `netstandard2.0`, `net481`, `net8.0`, `net9.0` | Cross-platform core engine |
| **Plot3D.UI** | `net481`, `net8.0-windows`, `net9.0-windows`, `net10.0-windows` | Windows Forms & Alibre Add-On |
| **Plot3D.Sample** | `net8.0` | Command-line demo |
| **Plot3D.Tests** | `net8.0` | xUnit / MSTest suite |

---

## Quick Start

### 1. Programmatic Plotting with C# Delegates

```csharp
using System;
using Plot3D;
using Plot3D.Core;

// Define parametric equations x(t), y(t), z(t) over an interval [0, 6*pi]
var (figure, curve) = ThreeDimensionalPlotter.PlotFigure(
    t => Math.Sin(t),
    t => Math.Cos(t),
    t => t,
    new Interval(0.0, 6.0 * Math.PI),
    LineSpec.Parse("r--")
);

figure.Title("3D Circular Helix")
      .XLabel("X = sin(t)")
      .YLabel("Y = cos(t)")
      .ZLabel("Z = t");

Console.WriteLine($"Sampled points: {curve.Points.Count}");
Console.WriteLine($"Total arc length: {curve.ArcLength:F4}");
```

### 2. Dynamic Mathematical Formula Parsing

You can pass string expressions directly to be parsed and compiled at runtime:

```csharp
using Plot3D;
using Plot3D.Core;

var (figure, curve) = ThreeDimensionalPlotter.PlotFigure(
    "sin(t) + 2*sin(2*t)",    // x(t)
    "cos(t) - 2*cos(2*t)",    // y(t)
    "-sin(3*t)",              // z(t)
    new Interval(0.0, 2.0 * Math.PI),
    "b-o"
);

figure.Title("Trefoil Knot");
```

### 3. Line Specifications & Styling

`LineSpec` strings follow familiar scientific plotting conventions combining color, style, and marker tokens:

- **Colors**: `r` (Red), `g` (Green), `b` (Blue), `c` (Cyan), `m` (Magenta), `y` (Yellow), `k` (Black), `w` (White)
- **Line Styles**: `-` (Solid), `--` (Dashed), `:` (Dotted), `-.` (Dash-Dot), `none` (No line)
- **Markers**: `o` (Circle), `s` (Square), `^` (Triangle Up), `v` (Triangle Down), `*` (Asterisk), `+` (Plus), `x` (Cross), `.` (Dot), `d` (Diamond)

```csharp
var spec = LineSpec.Parse("r--o"); // Red dashed line with circular markers
```

### 4. Multi-Format Exporting

```csharp
using Plot3D.Export;

// 1. Export vector SVG
figure.SaveSvg("output/Helix.svg", width: 900, height: 650);

// 2. Export 3D Wavefront OBJ mesh
ObjExporter3D.SaveToFile(figure.CurrentAxes, "output/TrefoilKnot.obj");

// 3. Export CSV point coordinate data
DataExporter3D.SaveCsv(curve, "output/CurvePoints.csv");
```

### 5. Console ASCII / ANSI Rendering

You can render 3D plots directly to the terminal using ASCII/Unicode art:

```csharp
string consolePlot = figure.ToConsoleString(columns: 74, rows: 20);
Console.WriteLine(consolePlot);
```

---

## Formula Syntax & Built-in Functions

The built-in formula parser supports standard mathematical notation, operator precedence, parentheses, and implicit multiplication:

- **Operators**: `+`, `-`, `*`, `/`, `^` (power), `%` (modulo)
- **Implicit Multiplication**: `2t`, `4cos(t)`, `(t+1)(t-1)`, `2pi*t`
- **Constants**: `pi`, `tau` ($2\pi$), `e`, `phi` (Golden Ratio)
- **Trigonometric**: `sin`, `cos`, `tan`, `sec`, `csc`, `cot`
- **Inverse Trigonometric**: `asin` (`arcsin`), `acos` (`arccos`), `atan` (`arctan`), `atan2(y, x)`
- **Hyperbolic**: `sinh`, `cosh`, `tanh`, `asinh`, `acosh`, `atanh`
- **Exponential & Logarithmic**: `exp`, `log` / `ln` (natural log), `log10`, `log2`, `sqrt`, `cbrt`, `pow(base, exp)`
- **Utility & Special**: `abs`, `sign`, `floor`, `ceil`, `round`, `clamp(val, min, max)`, `hypot(x, y)`, `sinc(t)`, `heaviside(t)` / `step(t)`, `deg2rad`, `rad2deg`

---

## Preset Curves

Plot3D comes with predefined mathematical curves ready for immediate visualization and study:

1. **Circular Helix**: Cylindrical helix winding around the Z-axis.
2. **Trefoil Knot**: Simplest non-trivial mathematical knot ($3_1$).
3. **Torus Knot $(p, q)$**: Parametric knot winding around a torus.
4. **Viviani's Curve**: Figure-eight intersection of a sphere with a cylinder.
5. **Conical Spiral**: Expanding spiral growing linearly with height.
6. **Lissajous 3D Knot**: Harmonic 3D Lissajous orbit with frequency ratios.
7. **Spherical Spiral**: Spiral curve constrained to a sphere's surface.
8. **Damped 3D Spiral Wave**: Exponentially decaying oscillating spiral trajectory.
9. **Butterfly 3D Curve**: Temple H. Fay's transcendental butterfly curve in 3D.
10. **Saddle Helix**: Curve traversing along a hyperbolic paraboloid saddle surface.
11. **Clelia Curve**: Longitude and latitude parameterized spherical trajectory.
12. **Rose Curve in 3D**: Multi-petaled rhodonea curve with harmonic vertical oscillation.
13. **Rational Discontinuous Curve**: Demonstrates adaptive singularity and asymptote handling.

---

## Color Maps & Palettes

Customize line gradient shading with 13 built-in palettes:

- `ColorMap3D.Turbo` (Default)
- `ColorMap3D.Viridis`
- `ColorMap3D.Plasma`
- `ColorMap3D.Parula`
- `ColorMap3D.ElectricBlue`
- `ColorMap3D.Crimson`
- `ColorMap3D.Emerald`
- `ColorMap3D.Ocean`
- `ColorMap3D.CoolWarm`
- `ColorMap3D.Sunset`
- `ColorMap3D.Rainbow`
- `ColorMap3D.Magma`
- `ColorMap3D.Jet`

---

## Interactive UI & Alibre Design Add-On

### Standalone Interactive Studio

Launch the desktop UI for interactive formula editing and real-time visualization:

```bash
dotnet run --project Plot3D.UI\Plot3D.UI.csproj
```

**UI Capabilities**:
- Real-time formula validation and instant curve rendering.
- Interactive 3D viewport:
  - **Left Mouse Drag**: Orbit rotation (Azimuth / Elevation).
  - **Right Mouse Drag / Shift + Left Drag**: Pan.
  - **Mouse Wheel**: Smooth zoom.
- Projection modes: Orthographic vs. Perspective.
- Display toggles for coordinate grid, bounding box, XYZ orientation gizmo, and coordinate tooltips.
- Animation mode for continuous azimuth rotation.
- Instant export to PNG, SVG, Wavefront OBJ, CSV, or Clipboard.

### Alibre Design CAD Integration

`Plot3D.UI` integrates directly into **Alibre Design** (Part / 3D Sketch modeling environment) as an Add-On:
- Seamlessly transfers 3D parametric curves as native **3D B-Splines** into active or newly created 3D sketches.
- Built-in CAD unit conversion support (`Millimeters`, `Centimeters`, `Inches`, `Meters`).
- Configurable scale multiplier and target sketch naming.

---

## Sample Console Application

Run the sample project to see live console demonstrations and generate sample output files (`.svg`, `.obj`, `.csv`):

```bash
dotnet run --project Plot3D.Sample\Plot3D.Sample.csproj
```

Output files will be generated in `Plot3D.Sample/bin/Debug/net8.0/Output/`.

---

## Building & Testing

### Prerequisites
- [.NET SDK 8.0 or later](https://dotnet.microsoft.com/download)

### Build Solution
```bash
dotnet build Plot3D.slnx
```

### Run Unit Tests
```bash
dotnet test
```

All 100+ tests cover formula parsing, adaptive subdivision tolerances, projection mathematics, export formatting, and preset definitions.

---

## License

This project is licensed under the [MIT License](LICENSE).

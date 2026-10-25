# Plot3D for Alibre Design: User Guide & Mathematical Reference Manual

**3D Parametric Formula Studio & Geometric Modeling Engine for Alibre Design**

---

## 1. Introduction & Overview

**Plot3D for Alibre Design** is a high-performance mathematical modeling and CAD integration tool designed for engineers, industrial designers, mathematicians, and educators. It enables direct synthesis, visualization, and native generation of complex 3D parametric curves, helical paths, mathematical knots, and organic wireframe geometries directly inside **Alibre Design Part workspaces (`.AD_PR`)**.

```
+-----------------------------------------------------------------------------+
|                                Plot3D Engine                                |
|                                                                             |
|   +-----------------------+     +--------------------+     +------------+   |
|   | Mathematical Formulas | --> | Adaptive Sampling  | --> | 3D Viewport|   |
|   | x(t), y(t), z(t)      |     | Chord & Curvature  |     | Projection |   |
|   +-----------------------+     +--------------------+     +------------+   |
|                                            |                                |
+--------------------------------------------|--------------------------------+
                                             v
               +---------------------------------------------+
               |     Alibre Design Integration (IAlibreAddOn) |
               |                                             |
               |  +---------------------------------------+  |
               |  | Unit Scaling & Coordinate Transform   |  |
               |  +---------------------------------------+  |
               |                     |                       |
               |     +---------------+---------------+       |
               |     |               |               |       |
               |     v               v               v       |
               |  +----------+  +----------+  +-----------+  |
               |  | B-Spline |  | Polyline |  | Point Node|  |
               |  +----------+  +----------+  +-----------+  |
               |                     |                       |
               |                     v                       |
               |       [ Active Alibre 3D Sketch ]           |
               +---------------------------------------------+
                                     |
                                     v
                       [ 3D CAD Sweep / Loft / Pipe ]
```

### Key Capabilities
- **Native Alibre Design Add-On & Standalone Studio**: Run directly inside the Alibre Design ribbon/menu environment or as a standalone desktop utility.
- **Fast Compiled Formula Engine**: Converts mathematical string expressions into compiled C# expression tree delegates with support for implicit multiplication, standard constants ($\pi, e, \tau, \phi$), and comprehensive transcendental functions.
- **Curvature-Adaptive Sampling**: Subdivides curve domains based on local chord error and segment angle bounds, capturing tight radii without over-sampling linear spans.
- **Native 3D Sketch Entity Creation**: Inserts smooth native cubic **B-Splines**, segmented **Polylines**, or discrete **Point Nodes** into active or newly created 3D sketches.
- **CAD Unit & Coordinate Awareness**: Converts between user units (mm, cm, m, inches, feet) and Alibre's internal centimeter kernel representation with scale factor controls.
- **Multi-Format Export Pipeline**: Export curves to Wavefront OBJ (`.obj`), CSV/TSV point clouds (`.csv`), Scalable Vector Graphics (`.svg`), PNG screenshots (`.png`), or Windows Clipboard.

---

## 2. Installation & System Requirements

### System Requirements
| Component | Minimum Requirement | Recommended |
|---|---|---|
| **Alibre Design** | Alibre Design v28 / v29+ (Build $\ge 29000$) | Alibre Design Expert / Professional v29+ |
| **Operating System** | Windows 10 (64-bit) | Windows 11 (64-bit) |
| **Runtime Environment** | .NET 8.0 Desktop Runtime / .NET Framework 4.8 | .NET 8.0 x64 |
| **Display & GPU** | $1280 \times 800$, DirectX / GDI+ hardware acceleration | $1920 \times 1080$ or 4K display |

### Alibre Add-On Registration
To register Plot3D as an integrated Alibre Design Add-On:
1. Locate the Alibre Add-Ons directory:
   - **Per-User**: `%APPDATA%\Alibre Design\Add-Ons\`
   - **System-Wide**: `%ALLUSERSPROFILE%\Alibre Design\Add-Ons\`
2. Place the `Plot3D.UI.adaddin` manifest file into the directory:
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <AlibreAddOnManager>
     <AddOn Assembly="Plot3D.UI.dll"
            Class="Plot3D.UI.Alibre.AlibreAddOn"
            Icon="Plot3D.ico"
            MenuText="Plot3D"
            ToolTip="3D Parametric Formula Studio for Alibre Design"
            AutoLoad="1" />
   </AlibreAddOnManager>
   ```
3. Ensure `Plot3D.UI.dll`, `Plot3D.dll`, and `AlibreX.dll` reside in the same folder.
4. Launch Alibre Design. Open **Tools > Add-On Manager** to verify that **Plot3D** is active.
5. In any **Part Modeling workspace (`.AD_PR`)**, access Plot3D from the top menu: **Plot3D > 3D Parametric Formula Studio...**

---

## 3. User Interface & Interactive Controls

The Plot3D interface consists of a dedicated **Control & Parameter Side Panel** on the left and an **Interactive 3D Hardware-Accelerated Viewport** on the right.

```
+------------------------------------+----------------------------------------+
|  PLOT3D CONTROL PANEL              |  INTERACTIVE 3D VIEWPORT               |
|                                    |                                        |
|  [ Preset Curves Dropdown      v ] |                    Z                   |
|                                    |                    ^                   |
|  Parametric Equations:             |                    |   ,-'""`-.        |
|  X(t) = [ sin(t) + 2*sin(2*t)    ] |                    |  /  (3,1) \       |
|  Y(t) = [ cos(t) - 2*cos(2*t)    ] |                    | | Trefoil  |      |
|  Z(t) = [ -sin(3*t)              ] |                    |  \  Knot  /       |
|                                    |                    +----`.__.-'---> Y  |
|  Domain Range:                     |                   /                    |
|  tMin = [ 0          ]             |                  /                     |
|  tMax = [ 2*pi       ]             |                 L X                    |
|                                    |                                        |
|  [ Plot / Evaluate Formula       ] |  [Azimuth: -37.5° | Elevation: 30.0°]  |
|                                    |                                        |
|  Alibre 3D Sketch Generation:      |  Projection: (*) Ortho  ( ) Persp      |
|  Geometry: [ B-Spline Curve    v ] |  View Options: [X] Grid  [X] Box       |
|  Units:    [ Millimeters (mm)  v ] |                [X] Gizmo [X] Hover     |
|  Scale:    [ 1.0     ]             |                                        |
|  Name:     [ 3D-Curve-1          ] |  [ Reset View ]   [ Toggle Animation ] |
|  [X] Create in New 3D Sketch       |                                        |
|  [ >>> Create in Alibre 3D Sketch] |  Export:                               |
|                                    |  [ PNG ] [ SVG ] [ OBJ ] [ CSV ] [Clip]|
+------------------------------------+----------------------------------------+
| Status: 248 points | Arc Length: 42.18 mm | Az: -37.5°, El: 30.0°, Zoom: 1.0x |
+-----------------------------------------------------------------------------+
```

### 3D Viewport Navigation
| Input Action | Interaction Mode | Functionality |
|---|---|---|
| **Left Mouse Button Drag** | **Orbit / Turntable** | Rotates azimuth ($-180^\circ \dots +180^\circ$) and elevation ($-90^\circ \dots +90^\circ$) |
| **Right Mouse Button Drag** | **Pan / Translate** | Translates the camera focus point along the view plane |
| **Middle Mouse Button Drag** | **Pan / Translate** | Alternate camera pan control |
| **Mouse Wheel Scroll** | **Zoom In / Out** | Adjusts the camera zoom scale ($0.1\times \dots 20.0\times$) smoothly |
| **Hover over Curve** | **Coordinate Inspection** | Displays parametric $(t, x, y, z)$ data under the mouse pointer |

### Hotkeys & Keyboard Shortcuts
- **`R`**: Reset camera rotation, pan offsets, and zoom to default framing.
- **`A`**: Toggle continuous turntable rotation animation.
- **`P`**: Re-parse mathematical expressions and re-sample the curve.
- **`G`**: Toggle 3D grid plane visibility.
- **`B`**: Toggle 3D bounding box cage visibility.
- **`Z`**: Toggle orientation XYZ coordinate gizmo.
- **`H`**: Toggle interactive point coordinate hover HUD.

---

## 4. Mathematical Formula Engine & Syntax Specification

Plot3D features a mathematical compiler that converts parametric equations into compiled delegates.

### 4.1 Parameter Variable & Basic Arithmetic
The independent parameter is **`t`**. Expressions are case-insensitive.

| Operator | Syntax | Description | Example |
|---|---|---|---|
| **Addition** | `+` | Adds two values | `t + 2` |
| **Subtraction** | `-` | Subtracts right from left | `t - 5` |
| **Multiplication** | `*` | Multiplies two terms | `3 * t` |
| **Implicit Mult.** | *(adjacent)* | Multiplies adjacent variables/parentheses | `2t`, `3sin(t)`, `(t+1)(t-1)` |
| **Division** | `/` | Left-associative division | `t / 2`, `12 / 3 / 2` $(= 2)$ |
| **Modulo** | `%` | Remainder of division | `t % (2*pi)` |
| **Exponentiation** | `^` | Right-associative power | `t^2`, `2^3^2` $(= 2^9 = 512)$ |
| **Unary Minus** | `-` | Negates term value | `-t`, `-cos(t)` |

### 4.2 Standard Mathematical Constants
| Constant Name | Aliases | Exact Value | Numerical Approximation |
|---|---|---|---|
| **Pi ($\pi$)** | `pi`, `PI` | $\pi$ | `3.141592653589793` |
| **Tau ($\tau$)** | `tau`, `TAU` | $2\pi$ | `6.283185307179586` |
| **Euler's Number ($e$)** | `e`, `E` | $e$ | `2.718281828459045` |
| **Golden Ratio ($\phi$)** | `phi`, `PHI` | $(1 + \sqrt{5}) / 2$ | `1.618033988749895` |

*Constants can be combined with implicit multipliers:* `2pi`, `0.5tau`, `3e`.

### 4.3 Comprehensive Built-In Function Reference

#### Trigonometric Functions (Radians)
- `sin(t)`: Sine
- `cos(t)`: Cosine
- `tan(t)`: Tangent
- `sec(t)`: Secant ($1 / \cos(t)$)
- `csc(t)`: Cosecant ($1 / \sin(t)$)
- `cot(t)`: Cotangent ($1 / \tan(t)$)
- `sinc(t)`: Normalized sinc ($\sin(t)/t$, returns $1.0$ at $t=0$)

#### Inverse Trigonometric Functions
- `asin(t)` / `arcsin(t)`: Arc sine (domain $[-1, 1]$, range $[-\pi/2, \pi/2]$)
- `acos(t)` / `arccos(t)`: Arc cosine (domain $[-1, 1]$, range $[0, \pi]$)
- `atan(t)` / `arctan(t)`: Arc tangent (range $(-\pi/2, \pi/2)$)
- `atan2(y, x)` / `arctan2(y, x)`: Four-quadrant arc tangent of $y/x$ (range $(-\pi, \pi]$)

#### Hyperbolic & Inverse Hyperbolic Functions
- `sinh(t)`: Hyperbolic sine
- `cosh(t)`: Hyperbolic cosine (catenary curve generator)
- `tanh(t)`: Hyperbolic tangent
- `asinh(t)`: Inverse hyperbolic sine ($\ln(t + \sqrt{t^2+1})$)
- `acosh(t)`: Inverse hyperbolic cosine (domain $[1, \infty)$)
- `atanh(t)`: Inverse hyperbolic tangent (domain $(-1, 1)$)

#### Exponential & Logarithmic Functions
- `exp(t)`: Natural exponential ($e^t$)
- `ln(t)` / `log(t)`: Natural logarithm (base $e$)
- `log10(t)`: Common logarithm (base 10)
- `log2(t)`: Binary logarithm (base 2)

#### Roots, Powers & Modifiers
- `sqrt(t)`: Square root ($\sqrt{t}$, domain $[0, \infty)$)
- `cbrt(t)`: Cube root ($\sqrt[3]{t}$, handles negative inputs: $\text{cbrt}(-8) = -2$)
- `pow(base, exp)`: Power function ($\text{base}^{\text{exp}}$)
- `hypot(x, y)`: Euclidean hypotenuse ($\sqrt{x^2 + y^2}$)
- `abs(t)`: Absolute value ($|t|$)
- `sign(t)`: Signum function ($-1, 0, +1$)

#### Rounding & Discontinuous Steps
- `floor(t)`: Greatest integer $\le t$
- `ceil(t)` / `ceiling(t)`: Least integer $\ge t$
- `round(t)`: Nearest integer
- `heaviside(t)` / `step(t)`: Unit step function ($0.0$ for $t < 0$, $1.0$ for $t \ge 0$)
- `clamp(val, min, max)`: Clamps value within $[\text{min}, \text{max}]$

#### Angle Conversion Helpers
- `rad2deg(rad)`: Converts radians to degrees ($rad \times 180 / \pi$)
- `deg2rad(deg)`: Converts degrees to radians ($deg \times \pi / 180$)

---

## 5. Adaptive Sampling & Precision Controls

Uniform parameter discretization can lead to two main CAD problems:
1. **Under-sampling high-curvature regions**: Causes faceted corners, distorted B-Spline interpolation, or self-intersecting sweeps.
2. **Over-sampling low-curvature spans**: Generates thousands of redundant control points, bloating `.AD_PR` file sizes and slowing sketch rebuilds.

```
UNIFORM SAMPLING (Fixed step delta_t):
  Point 1 ---- Point 2 ---- Point 3 ---- Point 4 ---- Point 5
  [ Coarse on sharp bends, wasteful on straight runs ]

ADAPTIVE SAMPLING (Curvature & Chord-Error Driven):
  Point 1 ------------------- Point 2 --- Pt3 - Pt4 - Pt5 --- Point 6
  [ Sparse on straight runs | Dense clustering on tight corners ]
```

### The Adaptive Sampling Algorithm
Plot3D uses a recursive subdivision algorithm:
1. **Initial Mesh Grid**: Generates an initial uniform base grid of $N = \text{MeshDensity}$ points across $[t_{\text{min}}, t_{\text{max}}]$ (default: 23 points).
2. **Midpoint Evaluation**: For every interval $[t_0, t_1]$, calculates midpoint $t_m = \frac{t_0 + t_1}{2}$ and evaluates exact coordinate $\mathbf{P}_m = (x(t_m), y(t_m), z(t_m))$.
3. **Chord Error Metric**: Computes perpendicular deviation $e_{\text{chord}} = \|\mathbf{P}_m - \frac{\mathbf{P}_0 + \mathbf{P}_1}{2}\|$.
4. **Tolerance Boundary**: Subdivision occurs if:
   $$e_{\text{chord}} > \text{Tol}_{\text{abs}} + \text{Tol}_{\text{rel}} \cdot \max(\|\mathbf{P}_0\|, \|\mathbf{P}_1\|, \|\mathbf{P}_m\|, 1.0)$$
5. **Angular Curvature Check**: Evaluates tangent angle change $\theta = \angle(\mathbf{P}_m - \mathbf{P}_0, \mathbf{P}_1 - \mathbf{P}_m)$. If $\theta > \theta_{\text{max}}$ (default $0.15\text{ rad} \approx 8.5^\circ$), subdivision triggers automatically.
6. **Singularity & Asymptote Handling**: Automatically isolates arithmetic poles (e.g. division by zero) and splits the curve into separate continuous segments without throwing exceptions.

---

## 6. Alibre Design 3D Sketch Generation

The **Alibre 3D Sketch** panel connects formula calculations directly to the Alibre Design Part modeling kernel (`AlibreX`).

```
+-----------------------------------------------------------------------------+
|                     ALIBRE 3D SKETCH GENERATION WORKFLOW                    |
|                                                                             |
| 1. Open / Activate Alibre Part (.AD_PR)                                     |
|    └─ Verified by IADPartSession and ADDocumentType.AD_PART                 |
|                                                                             |
| 2. Configure Geometry & Units                                               |
|    ├─ Geometry Type : [ B-Spline Curve / Polyline / Point Nodes ]           |
|    ├─ Input Units   : [ Millimeters / Centimeters / Inches / Feet / Meters ]|
|    └─ Scale Multiplier : [ 1.000 ]                                          |
|                                                                             |
| 3. Select Sketch Target                                                     |
|    ├─ [X] Create in New 3D Sketch -> Auto-creates "3D-Curve-1"              |
|    └─ [ ] Insert into Active Open 3D Sketch                                 |
|                                                                             |
| 4. Execute Transaction                                                      |
|    └─ session.StartTransaction() -> AddGeometry -> session.CompleteTransaction()
+-----------------------------------------------------------------------------+
```

### 6.1 Geometry Generation Types

#### 1. B-Spline Curve (`IAD3DSketch.AddBSplineByPoints`) — *Recommended*
- **Description**: Fits a smooth, continuous cubic NURBS/B-Spline curve passing through all adaptively sampled points.
- **Best For**: Aerodynamic lofts, sweeping organic tubes, smooth cam tracks, optical light pipes, and fluid channels.
- **Fail-Safe Mechanism**: If Alibre's geometric kernel rejects knot vectors (e.g. at cusps), Plot3D falls back to a high-density segmented polyline.

#### 2. Polyline (`IAD3DSketch.AddLine`)
- **Description**: Chains consecutive sample coordinates with discrete 3D linear wireframe segments.
- **Best For**: Faceted paths, polygonal wireframes, rapid prototyping tests, or linear toolpath approximations.

#### 3. Point Nodes (`IAD3DSketch.AddPointNode`)
- **Description**: Generates standalone 3D reference point entities at each coordinate.
- **Best For**: CMM inspection points, guide rail stations, discrete hole patterns, and loft cross-section placement guides.

### 6.2 Unit Conversion & Coordinate Scaling
Alibre Design stores all internal 3D coordinates in **Centimeters (`AD_CENTIMETERS`)**. Plot3D handles this coordinate translation transparently.

$$\mathbf{P}_{\text{Alibre}}(\text{cm}) = \mathbf{P}_{\text{Formula}} \cdot S \cdot \frac{\text{ToMillimeters}(\text{Units}_{\text{Input}})}{10.0}$$

| Input Unit Selection | Conversion to Internal Kernel (cm) | Standard Scale $S=1.0$ Example |
|---|---|---|
| **Millimeters (mm)** | $\mathbf{P} \times 0.1$ | $100\text{ mm} \rightarrow 10.0\text{ cm}$ in CAD |
| **Centimeters (cm)** | $\mathbf{P} \times 1.0$ | $10.0\text{ cm} \rightarrow 10.0\text{ cm}$ in CAD |
| **Meters (m)** | $\mathbf{P} \times 100.0$ | $1.0\text{ m} \rightarrow 100.0\text{ cm}$ in CAD |
| **Inches (in)** | $\mathbf{P} \times 2.54$ | $1.0\text{ in} \rightarrow 2.54\text{ cm}$ in CAD |
| **Feet (ft)** | $\mathbf{P} \times 30.48$ | $1.0\text{ ft} \rightarrow 30.48\text{ cm}$ in CAD |

---

## 7. Downstream CAD Modeling Best Practices in Alibre Design

Once a 3D parametric curve is generated in an Alibre 3D sketch, it can be used for downstream feature modeling.

```
                          [ 3D Parametric B-Spline ]
                                      |
         +----------------------------+----------------------------+
         |                                                         |
         v                                                         v
   [ 3D Sweep Feature ]                                  [ 3D Loft Guide Rail ]
   1. Insert Normal Plane at t0                          1. Create Cross-Section Sketches
   2. Sketch Profile (Circle/Rect)                       2. Select 3D Spline as Rail
   3. Sweep along 3D B-Spline Path                       3. Loft Solid / Surface
         |                                                         |
         v                                                         v
  ( Helical Springs, Pipes, Ducts )                       ( Aerodynamic Blends )
```

### 7.1 Creating 3D Sweeps (Pipes, Springs, Coils)
1. **Insert a Normal Sketch Plane**:
   - Go to **Feature > Plane**.
   - Select **Normal to Curve** mode.
   - Pick the generated 3D B-Spline and select its start vertex ($t = t_{\text{min}}$).
2. **Sketch the Cross-Section Profile**:
   - Create a 2D sketch on the newly created plane.
   - Sketch your desired wire/pipe profile (e.g. a circle of radius $r$). Ensure the center is coincident with the curve origin.
3. **Execute 3D Sweep**:
   - Select **Sweep Boss / Cut**.
   - Set **Profile** = 2D Sketch.
   - Set **Path** = 3D Sketch (the Plot3D B-Spline).

> **CAD Best Practice (Sweep Curvature Rule)**:
> To avoid sweep self-intersection errors in Alibre Design, ensure that profile radius $r_{\text{profile}}$ is strictly less than the minimum radius of curvature $\rho_{\text{min}}$ along the entire 3D curve:
> $$r_{\text{profile}} < \rho_{\text{min}} = \min_{t} \frac{\|\mathbf{r}'(t)\|^3}{\|\mathbf{r}'(t) \times \mathbf{r}''(t)\|}$$

### 7.2 Creating Guided Lofts (Aerodynamic & Ergonomic Surfacing)
1. Generate two or more matching parametric guide curves across adjacent 3D sketches.
2. Construct planar 2D cross-section stations along the path.
3. Launch **Loft Boss**. Select cross-sections in order, then assign the 3D curves under **Guide Curves**.

---

## 8. Mathematical Curve Catalog & Engineering Presets

Plot3D comes pre-configured with a catalog of engineering, mathematical, and architectural curves.

### 8.1 Circular Helix
- **Category**: Spring & Thread Mechanics
- **Parametric Equations**:
  $$x(t) = R \cdot \sin(t)$$
  $$y(t) = R \cdot \cos(t)$$
  $$z(t) = \frac{P}{2\pi} \cdot t$$
- **Domain**: $t \in [0, 2\pi \cdot N_{\text{turns}}]$ (e.g., `0` to `6*pi` for 3 turns)
- **Presets & Usage**: Standard helical compression springs, custom thread forms, auger flights, spiral staircases.

```
       Z
       ^
       |       ,-'""`-.
       |      /   __   \
       |     |   /  \   |    Helix Parameters:
       |      \  \__/  /     - Radius R = 1.0
       |     ,-'""`-. /      - Pitch P = 2*pi * PitchFactor
       |    /   __   \       - Turns N = tMax / (2*pi)
       |   |   /  \   |
       +---|---|--|---|---> Y
          / \  \__/  /
         /   `-.__.-'
        L X
```

### 8.2 Variable-Pitch Conical Spiral
- **Category**: Tapered Springs & Fluid Nozzles
- **Parametric Equations**:
  $$x(t) = t \cdot \cos(10t)$$
  $$y(t) = t \cdot \sin(10t)$$
  $$z(t) = t$$
- **Domain**: $t \in [0, 2\pi]$
- **Usage**: Vortex flow conditioners, antenna spirals, conical shock absorbers.

### 8.3 Trefoil Knot ($3_1$ Knot)
- **Category**: Topological Geometry & Decorative CAD
- **Parametric Equations**:
  $$x(t) = \sin(t) + 2\sin(2t)$$
  $$y(t) = \cos(t) - 2\cos(2t)$$
  $$z(t) = -\sin(3t)$$
- **Domain**: $t \in [0, 2\pi]$
- **Usage**: Interlocking structural rings, jewelry modeling, magnetic field trajectory simulations.

### 8.4 Torus Knot $(p, q) = (2, 3)$
- **Category**: Toroidal Manifolds
- **Parametric Equations**:
  $$x(t) = (R + r\cos(q \cdot t)) \cos(p \cdot t) = (2 + \cos(3t)) \cos(2t)$$
  $$y(t) = (R + r\cos(q \cdot t)) \sin(p \cdot t) = (2 + \cos(3t)) \sin(2t)$$
  $$z(t) = r\sin(q \cdot t) = \sin(3t)$$
- **Domain**: $t \in [0, 2\pi]$
- **Usage**: Toroidal transformer windings, filament winding mandrels, decorative hardware.

### 8.5 Viviani's Curve
- **Category**: Intersecting Quadric Geometry
- **Parametric Equations**:
  $$x(t) = R(1 + \cos(t))$$
  $$y(t) = R\sin(t)$$
  $$z(t) = 2R\sin(t/2)$$
- **Domain**: $t \in [-2\pi, 2\pi]$
- **Usage**: Figure-eight intersection of a sphere with an intersecting tangent cylinder. Used in cylindrical cam grooves and spherical porting.

### 8.6 3D Lissajous Harmonic Knot
- **Category**: Modal Dynamics & Harmonic Motion
- **Parametric Equations**:
  $$x(t) = A\sin(n_x \cdot t) = \sin(3t)$$
  $$y(t) = B\cos(n_y \cdot t) = \cos(4t)$$
  $$z(t) = C\sin(n_z \cdot t) = \sin(5t)$$
- **Domain**: $t \in [0, 2\pi]$
- **Usage**: Multi-axis vibration paths, laser galvanometer scanning trajectories, harmonic resonators.

### 8.7 Clelia & Spherical Spirals
- **Category**: Geodesics & Spherical Kinematics
- **Parametric Equations**:
  $$x(t) = R\cos(m \cdot t)\cos(t)$$
  $$y(t) = R\sin(m \cdot t)\cos(t)$$
  $$z(t) = R\sin(t)$$
- **Domain**: $t \in [-\pi/2, \pi/2]$ or $[0, 2\pi]$
- **Usage**: Satellite ground tracks, spherical ball-cage paths, geodesic dome structural ribs.

### 8.8 Damped Oscillatory Spiral
- **Category**: Transient Dynamic Decay
- **Parametric Equations**:
  $$x(t) = \exp(-\alpha t)\cos(\omega t) = \exp(-0.12t)\cos(5t)$$
  $$y(t) = \exp(-\alpha t)\sin(\omega t) = \exp(-0.12t)\sin(5t)$$
  $$z(t) = t$$
- **Domain**: $t \in [0, 20]$
- **Usage**: Helical acoustic dampers, spring recoil decay paths, vortex dissipation tracks.

---

## 9. Multi-Format Data Export & Interoperability

Plot3D includes an export pipeline for exchanging curve data with other CAD/CAM, rendering, and simulation systems:

| Format / Button | File Extension | Output Description | Downstream Compatibility |
|---|---|---|---|
| **Wavefront OBJ** | `.obj` | 3D polyline vertex (`v`) and line (`l`) mesh | Blender, Rhino, Maya, 3ds Max, Unity |
| **Point Cloud CSV** | `.csv` | Tabular $(t, x, y, z)$ numerical coordinates | Excel, MATLAB, Python/NumPy, CMM machines |
| **Vector Graphics** | `.svg` | High-DPI scalable 2D projection rendering | Technical manuals, patent drawings, Illustrator |
| **Raster Image** | `.png` | Viewport frame capture with transparent background | Reports, design presentations, web documentation |
| **Copy Clipboard** | *Memory* | Direct tab-delimited coordinate text | Paste directly into Alibre tables or Excel |

---

## 10. Best Practices, Troubleshooting & FAQs

### 10.1 Mathematical Modeling Best Practices
1. **Maintain Units Consistency**:
   - If equations are normalized (e.g. radius $= 1.0$), set **Units = Millimeters** and **Scale = 50.0** to generate a $50\text{ mm}$ radius component in Alibre without rewriting the formula.
2. **Explicit Multiplications for Ambiguous Terms**:
   - While implicit multiplication handles `2t` and `3sin(t)` cleanly, use explicit parentheses or `*` when chaining complex quotients: `(sin(t)) / (2 * pi * t)`.
3. **Avoid Division by Zero**:
   - Use `sinc(t)` instead of `sin(t)/t` to ensure zero division does not introduce numerical artifacts at $t = 0$.
4. **Parametric Interval Matching**:
   - Ensure periodic curves (trigonometric harmonics) match their fundamental periods ($2\pi, 4\pi, 12\pi$) to ensure closed loops without overlap.

### 10.2 Troubleshooting Guide

#### "Failed to access Alibre Design active part"
- **Cause**: No active part file (`.AD_PR`) is currently open, or an assembly (`.AD_ASM`) or drawing (`.AD_DRW`) is active.
- **Solution**: Open or create a new Part document (`.AD_PR`), then re-trigger 3D Sketch creation.

#### "B-Spline creation failed; falling back to polyline"
- **Cause**: The curve contains a sharp geometric cusp, self-intersection, or duplicate co-linear points that exceed Alibre's knot tolerance.
- **Solution**: Plot3D automatically switches to a high-density polyline. Alternatively, split the curve into two continuous sub-intervals.

#### Formula Parsing Error: "Unexpected token at position N"
- **Cause**: Missing parenthesis, unrecognized function name, or incomplete operator.
- **Solution**: Check the syntax indicator box below the formula inputs. Plot3D highlights the character index where parsing failed.

---

## 11. Appendix: Complete API & Class Architecture

For developers extending Plot3D:

| Assembly / Namespace | Core Class | Responsibility |
|---|---|---|
| `Plot3D.Expressions` | `FormulaParser` | Tokenizer, compiler, AST expression generator, and delegate builder |
| `Plot3D.Expressions` | `MathFunctionHelper` | Transcendental math functions (`sinc`, `cbrt`, `hypot`, `clamp`, `heaviside`) |
| `Plot3D.Sampling` | `AdaptiveSampler3D` | Recursive curvature and chord-error adaptive discretization engine |
| `Plot3D.Sampling` | `SampledCurve3D` | Bounding box, arc length, tangent, and point collection data structures |
| `Plot3D.UI.Alibre` | `AlibreAddOn` | Alibre Design `IAlibreAddOn` entry point, menu hooks, and lifecycle management |
| `Plot3D.UI.Alibre` | `AlibreSketchService` | Alibre COM API bridge, transaction manager, and 3D sketch entity generator |
| `Plot3D.UI.Alibre` | `AlibreUnits` | Physical unit scale factors and conversion matrices |
| `Plot3D.Export` | `ObjExporter3D` | Wavefront OBJ polyline generator |
| `Plot3D.Export` | `DataExporter3D` | CSV/TSV point cloud serialization |
| `Plot3D.Rendering` | `SvgRenderer3D` | Scalable vector graphic projection and depth-shaded path renderer |

---

*Plot3D for Alibre Design — Engineered for Precision Parametric CAD Modeling.*

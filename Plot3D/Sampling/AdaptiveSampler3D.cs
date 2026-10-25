using System;
using System.Collections.Generic;
using Plot3D.Core;

namespace Plot3D.Sampling
{
    /// <summary>
    /// Adaptive mesh generation and curve sampling engine for 3D parametric curves.
    /// Implements recursive chord-error and curvature-based subdivision.
    /// </summary>
    public sealed class AdaptiveSampler3D
    {
        private readonly Func<double, Point3D> _evaluator;
        private readonly Plot3DOptions _options;
        private int _evalCount;

        public AdaptiveSampler3D(Func<double, Point3D> evaluator, Plot3DOptions? options = null)
        {
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            _options = options ?? Plot3DOptions.CreateDefault();
        }

        public AdaptiveSampler3D(
            Func<double, double> funx,
            Func<double, double> funy,
            Func<double, double> funz,
            Plot3DOptions? options = null)
        {
            if (funx == null) throw new ArgumentNullException(nameof(funx));
            if (funy == null) throw new ArgumentNullException(nameof(funy));
            if (funz == null) throw new ArgumentNullException(nameof(funz));

            _evaluator = t =>
            {
                try
                {
                    double x = funx(t);
                    double y = funy(t);
                    double z = funz(t);
                    return new Point3D(x, y, z);
                }
                catch
                {
                    return Point3D.NaN;
                }
            };
            _options = options ?? Plot3DOptions.CreateDefault();
        }

        private Point3D Evaluate(double t)
        {
            _evalCount++;
            try
            {
                var pt = _evaluator(t);
                if (double.IsNaN(pt.X) || double.IsInfinity(pt.X) ||
                    double.IsNaN(pt.Y) || double.IsInfinity(pt.Y) ||
                    double.IsNaN(pt.Z) || double.IsInfinity(pt.Z))
                {
                    return Point3D.NaN;
                }
                return pt;
            }
            catch
            {
                return Point3D.NaN;
            }
        }

        public SampledCurve3D Sample()
        {
            _evalCount = 0;
            var interval = _options.TRange;
            int meshDensity = Math.Max(2, _options.MeshDensity);

            List<double> tList = new List<double>();
            List<Point3D> ptList = new List<Point3D>();

            // Generate initial uniform mesh points
            double[] baseT = new double[meshDensity];
            Point3D[] basePts = new Point3D[meshDensity];

            for (int i = 0; i < meshDensity; i++)
            {
                double fraction = (double)i / (meshDensity - 1);
                baseT[i] = interval.Lerp(fraction);
                basePts[i] = Evaluate(baseT[i]);
            }

            // Subdivide each initial interval adaptively
            for (int i = 0; i < meshDensity - 1; i++)
            {
                double t0 = baseT[i];
                double t1 = baseT[i + 1];
                Point3D p0 = basePts[i];
                Point3D p1 = basePts[i + 1];

                tList.Add(t0);
                ptList.Add(p0);

                SubdivideInterval(t0, p0, t1, p1, 1, tList, ptList);
            }

            // Add the final point
            tList.Add(baseT[meshDensity - 1]);
            ptList.Add(basePts[meshDensity - 1]);

            return new SampledCurve3D(tList, ptList, _evalCount);
        }

        private void SubdivideInterval(
            double t0, Point3D p0,
            double t1, Point3D p1,
            int depth,
            List<double> tList,
            List<Point3D> ptList)
        {
            if (depth > _options.MaxRecursionDepth || _evalCount >= _options.MaxFunctionEvaluations)
                return;

            double tm = 0.5 * (t0 + t1);
            Point3D pm = Evaluate(tm);

            bool p0Valid = p0.IsValid;
            bool p1Valid = p1.IsValid;
            bool pmValid = pm.IsValid;

            // Handle invalid/discontinuous intervals
            if (!p0Valid || !p1Valid || !pmValid)
            {
                if (_options.ShowDiscontinuities)
                {
                    // If one side is valid and the other is invalid, we can sample deeper to locate the boundary
                    if (depth < 6 && (p0Valid || p1Valid))
                    {
                        SubdivideInterval(t0, p0, tm, pm, depth + 1, tList, ptList);
                        tList.Add(tm);
                        ptList.Add(pm);
                        SubdivideInterval(tm, pm, t1, p1, depth + 1, tList, ptList);
                    }
                }
                return;
            }

            // Linear chord midpoint
            Point3D pChord = Point3D.Midpoint(p0, p1);

            // Chord error (deviation from straight line)
            double chordError = pm.DistanceTo(pChord);

            // Adaptive tolerance scale
            double maxMag = Math.Max(p0.Magnitude, Math.Max(p1.Magnitude, pm.Magnitude));
            double tol = _options.AbsoluteTolerance + _options.RelativeTolerance * Math.Max(maxMag, 1.0);

            // Curvature / angle check between consecutive segments (p0 -> pm) and (pm -> p1)
            Vector3D v1 = pm - p0;
            Vector3D v2 = p1 - pm;
            double angle = v1.AngleTo(v2);

            bool needsSubdivision = (chordError > tol) || (angle > _options.MaxSegmentAngle && chordError > 0.1 * tol);

            // Also check for potential asymptote where distance is disproportionately huge
            double span = p0.DistanceTo(p1);
            if (span > 100.0 * (1.0 + maxMag) && depth < 8)
            {
                needsSubdivision = true;
            }

            if (needsSubdivision)
            {
                // Recurse left
                SubdivideInterval(t0, p0, tm, pm, depth + 1, tList, ptList);

                // Add midpoint
                tList.Add(tm);
                ptList.Add(pm);

                // Recurse right
                SubdivideInterval(tm, pm, t1, p1, depth + 1, tList, ptList);
            }
        }
    }
}

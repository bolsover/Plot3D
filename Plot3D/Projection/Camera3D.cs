using System;
using Plot3D.Core;

namespace Plot3D.Projection
{
    /// <summary>
    /// Projection mode for 3D camera.
    /// </summary>
    public enum ProjectionMode
    {
        Orthographic,
        Perspective
    }

    /// <summary>
    /// Represents a 3D camera and projection transformer mapping world 3D coordinates to 2D viewport coordinates.
    /// </summary>
    public sealed class Camera3D
    {
        public double Azimuth { get; set; } = -37.5;
        public double Elevation { get; set; } = 30.0;
        public ProjectionMode Projection { get; set; } = ProjectionMode.Orthographic;
        public double CameraDistance { get; set; } = 5.0;

        // View transformation basis
        private Vector3D _right;
        private Vector3D _up;
        private Vector3D _forward;
        private Point3D _target;
        private double _scale;

        public Camera3D(double azimuth = -37.5, double elevation = 30.0, ProjectionMode projection = ProjectionMode.Orthographic)
        {
            Azimuth = azimuth;
            Elevation = elevation;
            Projection = projection;
        }

        /// <summary>
        /// Updates camera matrices for the given data bounds and viewport dimensions.
        /// </summary>
        public void Setup(Bounds3D bounds, double viewportWidth, double viewportHeight, double margin = 60.0)
        {
            _target = bounds.IsEmpty ? Point3D.Zero : bounds.Center;

            double azRad = Azimuth * Math.PI / 180.0;
            double elRad = Elevation * Math.PI / 180.0;

            double cosAz = Math.Cos(azRad);
            double sinAz = Math.Sin(azRad);
            double cosEl = Math.Cos(elRad);
            double sinEl = Math.Sin(elRad);

            // Forward vector (from camera towards target)
            _forward = new Vector3D(
                -sinAz * cosEl,
                cosAz * cosEl,
                -sinEl
            ).Normalized();

            // Right vector
            _right = new Vector3D(
                cosAz,
                sinAz,
                0.0
            ).Normalized();

            // Up vector
            _up = _right.Cross(_forward).Normalized();

            // Determine scale to fit bounds into viewport
            double maxDim = bounds.IsEmpty ? 2.0 : bounds.MaxDimension;
            if (maxDim <= 1e-9) maxDim = 2.0;

            double availableWidth = Math.Max(viewportWidth - 2 * margin, 50.0);
            double availableHeight = Math.Max(viewportHeight - 2 * margin, 50.0);

            // Diagonal bounding box radius
            double radius = bounds.IsEmpty ? 1.0 : Math.Sqrt(
                Math.Pow(bounds.DeltaX, 2) + Math.Pow(bounds.DeltaY, 2) + Math.Pow(bounds.DeltaZ, 2)
            ) * 0.5;
            if (radius <= 1e-9) radius = 1.0;

            double minViewportDim = Math.Min(availableWidth, availableHeight);
            _scale = (minViewportDim * 0.45) / radius;
        }

        /// <summary>
        /// Transforms a 3D point in world space to 2D viewport coordinates and depth.
        /// </summary>
        public (double ScreenX, double ScreenY, double Depth) Project(
            in Point3D worldPoint,
            double viewportWidth,
            double viewportHeight)
        {
            if (!worldPoint.IsValid)
            {
                return (double.NaN, double.NaN, double.NaN);
            }

            Vector3D offset = worldPoint - _target;

            double xCam = offset.Dot(_right);
            double yCam = offset.Dot(_up);
            double zCam = -offset.Dot(_forward); // Distance along view direction

            double projX;
            double projY;

            if (Projection == ProjectionMode.Perspective)
            {
                double dist = CameraDistance + zCam;
                if (dist < 0.1) dist = 0.1;
                double factor = CameraDistance / dist;
                projX = xCam * factor * _scale;
                projY = yCam * factor * _scale;
            }
            else
            {
                projX = xCam * _scale;
                projY = yCam * _scale;
            }

            // Invert Y for screen coordinates (screen Y grows downwards)
            double screenX = viewportWidth * 0.5 + projX;
            double screenY = viewportHeight * 0.5 - projY;

            return (screenX, screenY, zCam);
        }
    }
}

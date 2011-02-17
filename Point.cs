using System;
using System.Collections.Generic;
using System.Drawing;

using OpenTK;

namespace OpenTKGUI
{
    /// <summary>
    /// A two-dimensional floating point position or offset (vector).
    /// </summary>
    public struct Point
    {
        public Point(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        /// <summary>
        /// Gets the square of the length of this point offset (vector). This function is quicker to compute than the actual length
        /// because it avoids a square root, which may be costly.
        /// </summary>
        public double SquareLength
        {
            get
            {
                return this.X * this.X + this.Y * this.Y;
            }
        }

        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        public double Length
        {
            get
            {
                return Math.Sqrt(this.SquareLength);
            }
        }

        /// <summary>
        /// The width divided by the height of the size represented by this point.
        /// </summary>
        public double AspectRatio
        {
            get
            {
                return this.X / this.Y;
            }
        }

        /// <summary>
        /// Creates a unit vector (point offset) for the specified angle.
        /// </summary>
        public static Point Unit(double Angle)
        {
            return new Point(Math.Sin(Angle), Math.Cos(Angle));
        }

        /// <summary>
        /// Scales the point by the given point.
        /// </summary>
        public Point Scale(Point Scale)
        {
            return new Point(this.X * Scale.X, this.Y * Scale.Y);
        }

        /// <summary>
        /// Gets a point that has its components swapped from this point.
        /// </summary>
        public Point Swap
        {
            get
            {
                return new Point(this.Y, this.X);
            }
        }

        /// <summary>
        /// Swaps the two components of the point if condition is true.
        /// </summary>
        public Point SwapIf(bool Condition)
        {
            if (Condition)
            {
                return this.Swap;
            }
            else
            {
                return this;
            }
        }

        /// <summary>
        /// Rounds the point to the nearest integer components.
        /// </summary>
        public Point Round
        {
            get
            {
                return new Point(Math.Round(this.X), Math.Round(this.Y));
            }
        }

        /// <summary>
        /// Rounds the point to the next highest integer components. Useful for sizes.
        /// </summary>
        public Point Ceiling 
        {
            get
            {
                return new Point(Math.Ceiling(this.X), Math.Ceiling(this.Y));
            }
        }

        /// <summary>
        /// Gets the angle of this point (representing an offset).
        /// </summary>
        public double Angle
        {
            get
            {
                return Math.Atan2(this.Y, this.X);
            }
        }

        /// <summary>
        /// Gets the dot product of two points (representing offsets).
        /// </summary>
        public static double Dot(Point A, Point B)
        {
            return A.X * B.X + A.Y * B.Y;
        }

        /// <summary>
        /// Rotates the point by an increment of 90 degrees about the origin.
        /// </summary>
        public Point Rotate(Rotation Rotation)
        {
            switch (Rotation)
            {
                case Rotation.None: return this;
                case Rotation.CounterClockwise: return new Point(this.Y, -this.X);
                case Rotation.Half: return new Point(-this.X, -this.Y);
                default: return new Point(-this.Y, this.X);
            }
        }

        /// <summary>
        /// Rotates the point by an increment of 90 degrees about the pivot.
        /// </summary>
        public Point Rotate(Point Pivot, Rotation Rotation)
        {
            return (this - Pivot).Rotate(Rotation) + Pivot;
        }

        /// <summary>
        /// Gets the inverse rotation of the specified rotation.
        /// </summary>
        public static Rotation Inverse(Rotation Rotation)
        {
            return (Rotation)((2 - ((int)Rotation - 2)) % 4);
        }

        public static implicit operator Vector2(Point Vector)
        {
            return new Vector2((float)Vector.X, (float)Vector.Y);
        }

        public static implicit operator PointF(Point Vector)
        {
            return new PointF((float)Vector.X, (float)Vector.Y);
        }

        public static Point operator -(Point A, Point B)
        {
            return new Point(A.X - B.X, A.Y - B.Y);
        }

        public static Point operator -(Point A)
        {
            return new Point(-A.X, -A.Y);
        }

        public static Point operator +(Point A, Point B)
        {
            return new Point(A.X + B.X, A.Y + B.Y);
        }

        public static Point operator *(Point A, double B)
        {
            return new Point(A.X * B, A.Y * B);
        }

        public double X;
        public double Y;
    }

    /// <summary>
    /// An axis.
    /// </summary>
    public enum Axis
    {
        Horizontal,
        Vertical
    }

    /// <summary>
    /// Represents a rotation in increments of 90 degrees.
    /// </summary>
    public enum Rotation
    {
        /// <summary>
        /// No rotation.
        /// </summary>
        None = 0,

        /// <summary>
        /// Rotation of 90 degrees counterclockwise.
        /// </summary>
        CounterClockwise = 1,

        /// <summary>
        /// Rotation of 180 degrees.
        /// </summary>
        Half = 2,

        /// <summary>
        /// Rotation of 90 degrees clockwise.
        /// </summary>
        Clockwise = 3,
    }
}
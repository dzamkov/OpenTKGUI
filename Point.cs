﻿using System;
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
        /// Creates a unit vector (point offset) for the specified angle.
        /// </summary>
        public static Point Unit(double Angle)
        {
            return new Point(Math.Sin(Angle), Math.Cos(Angle));
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
}
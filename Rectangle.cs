using System;
using System.Collections.Generic;
using System.Drawing;

using OpenTK;

namespace OpenTKGUI
{
    /// <summary>
    /// An orthagonal rectangle defined by a point and a size (described as a point offset).
    /// </summary>
    public struct Rectangle
    {
        public Rectangle(double X, double Y, double Width, double Height)
        {
            this.Location = new Point(X, Y);
            this.Size = new Point(Width, Height);
        }

        public Rectangle(Point Location, Point Size)
        {
            this.Location = Location;
            this.Size = Size;
        }

        public Rectangle(Point Size)
        {
            this.Location = new Point();
            this.Size = Size;
        }

        /// <summary>
        /// Creates a copy of this insuring that width and height are positive while still covering the same area.
        /// </summary>
        public Rectangle Fix
        {
            get
            {
                Rectangle f = this;
                if (f.Size.X < 0.0)
                {
                    f.Size.X = -f.Size.X;
                    f.Location.X -= f.Size.X;
                }
                if (f.Size.Y < 0.0)
                {
                    f.Size.Y = -f.Size.Y;
                    f.Location.Y -= f.Size.Y;
                }
                return f;
            }
        }

        /// <summary>
        /// Creates a rectangle from this rectangle with a margin applied.
        /// </summary>
        public Rectangle Margin(double Amount)
        {
            return new Rectangle(
                this.Location + new Point(Amount, Amount),
                this.Size - new Point(Amount, Amount) * 2.0);
        }

        /// <summary>
        /// Gets if the specified point is inside the rectangle.
        /// </summary>
        public bool In(Point Point)
        {
            return Point.X >= this.Location.X && Point.Y >= this.Location.Y &&
                Point.X < this.Location.X + this.Size.X && Point.Y < this.Location.Y + this.Size.Y;
        }

        /// <summary>
        /// Gets if this rectangle intersects another.
        /// </summary>
        public bool Intersects(Rectangle Rect)
        {
            return this.Location.X < Rect.Location.X + Rect.Size.X
                && this.Location.Y < Rect.Location.Y + Rect.Size.Y
                && Rect.Location.X < this.Location.X + this.Size.X
                && Rect.Location.Y < this.Location.Y + this.Size.Y;
        }

        /// <summary>
        /// Gets the intersecting area between this rectangle and another.
        /// </summary>
        public Rectangle Intersection(Rectangle Rect)
        {
            Point atl = this.Location; Point btl = Rect.Location;
            Point abr = this.BottomRight; Point bbr = Rect.BottomRight;
            double x = Math.Max(atl.X, btl.X);
            double y = Math.Max(atl.Y, btl.Y);
            double xx = Math.Min(abr.X, bbr.X);
            double yy = Math.Min(abr.Y, bbr.Y);
            return new Rectangle(x, y, xx - x, yy - y);
        }

        public static implicit operator Rectangle(RectangleF A)
        {
            return new Rectangle(A.X, A.Y, A.Width, A.Height);
        }

        public static Rectangle operator +(Rectangle A, Point B)
        {
            return new Rectangle(A.Location + B, A.Size);
        }

        /// <summary>
        /// Gets the relative offset of the given absolute point in this rectangle.
        /// </summary>
        public Point ToRelative(Point Absolute)
        {
            return new Point(
                (Absolute.X - this.Location.X) / this.Size.X,
                (Absolute.Y - this.Location.Y) / this.Size.Y);
        }

        /// <summary>
        /// Gets the relative offset of the given rectangle in this rectangle.
        /// </summary>
        public Rectangle ToRelative(Rectangle Absolute)
        {
            return new Rectangle(
                this.ToRelative(Absolute.Location),
                new Point(Absolute.Size.X / this.Size.X, Absolute.Size.Y / this.Size.Y));
        }

        /// <summary>
        /// Scales this rectangle by a point multiplier.
        /// </summary>
        public Rectangle Scale(Point Scale)
        {
            return new Rectangle(this.Location.Scale(Scale), this.Size.Scale(Scale));
        }

        /// <summary>
        /// Gets or sets the position of the top-left corner of the rectangle.
        /// </summary>
        public Point TopLeft
        {
            get
            {
                return this.Location;
            }
            set
            {
                this.Location = value;
            }
        }

        /// <summary>
        /// Gets or sets the position of the bottom-right corner of the rectangle.
        /// </summary>
        public Point BottomRight
        {
            get
            {
                return this.Location + this.Size;
            }
            set
            {
                this.Size = value - this.Location;
            }
        }

        /// <summary>
        /// The location of the top-left corner of the rectangle.
        /// </summary>
        public Point Location;

        /// <summary>
        /// The size of the rectangle.
        /// </summary>
        public Point Size;
    }
}
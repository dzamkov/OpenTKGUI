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
        /// Creates a rectangle from this rectangle with padding applied.
        /// </summary>
        public Rectangle Pad(double Amount)
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
        /// Gets the intersection of two rectangles.
        /// </summary>
        public static Rectangle Intersection(Rectangle A, Rectangle B)
        {
            Point loc = new Point(
               Math.Max(A.Location.X, B.Location.X),
               Math.Max(A.Location.Y, B.Location.Y));
            Point size = new Point(
                Math.Min(A.Location.X + A.Size.X, B.Location.X + B.Size.X) - loc.X,
                Math.Min(A.Location.Y + A.Size.Y, B.Location.Y + B.Size.Y) - loc.Y);
            return new Rectangle(loc, size);
        }

        public static implicit operator Rectangle(RectangleF A)
        {
            return new Rectangle(A.X, A.Y, A.Width, A.Height);
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
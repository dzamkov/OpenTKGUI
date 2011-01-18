using System;
using System.Collections.Generic;
namespace OpenTKGUI
{
    /// <summary>
    /// The basic unit of TKGUI. Describes a gui component that can be drawn and receive events.
    /// </summary>
    public abstract class Control
    {
        /// <summary>
        /// Gets the size (in pixels) of this panel when rendered.
        /// </summary>
        public Point Size
        {
            get
            {
                return this._Size;
            }
        }

        /// <summary>
        /// Renders the control with the given context.
        /// </summary>
        /// <remarks>Rendering, when the given context is current, should be done from (0.0, 0.0) to (Size.X, Size.Y).</remarks>
        public virtual void Render(GUIRenderContext Context)
        {

        }

        /// <summary>
        /// Updates the state of the control after the specified amount of time elapses.
        /// </summary>
        public virtual void Update(GUIControlContext Context, double Time)
        {

        }

        /// <summary>
        /// Updates the state of the control as a root control.
        /// </summary>
        public void Update(GUIContext Context, double Time)
        {
            this.Update(Context.CreateRootControlContext(this, new Point(0.0, 0.0)), Time);
        }

        /// <summary>
        /// Resizes the control. Size must be specified in pixels.
        /// </summary>
        public void Resize(Point Size)
        {
            Point oldsize = this._Size;
            this._Size = Size;
            this.OnResize(oldsize, Size);
        }

        /// <summary>
        /// Called when the size of the control is forcibly changed.
        /// </summary>
        protected virtual void OnResize(Point OldSize, Point NewSize)
        {

        }

        private Point _Size;
    }
}
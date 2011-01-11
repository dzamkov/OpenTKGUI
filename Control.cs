using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace TKGUI
{
    /// <summary>
    /// Context given to a GLPanel when an event occurs.
    /// </summary>
    public interface IGLGUIContext
    {

    }

    /// <summary>
    /// The basic unit of TKGUI. Describes a gui component that can be drawn and receive events.
    /// </summary>
    public abstract class Control
    {
        /// <summary>
        /// Gets the size (in pixels) of this panel when rendered.
        /// </summary>
        public Vector2d Size
        {
            get
            {
                return this._Size;
            }
        }

        /// <summary>
        /// Renders the panel to the current GL context. The coordinate space for the panel
        /// should already be set up (with (0, 0) at the upperleft corner and (Size.X, Size.Y) at the
        /// bottomright corner).
        /// </summary>
        public virtual void Render()
        {

        }

        /// <summary>
        /// Updates the state of the panel after the specified amount of time elapses.
        /// </summary>
        public virtual void Update(double Time, IGLGUIContext Context)
        {

        }

        /// <summary>
        /// Resizes the control. Size must be specified in pixels.
        /// </summary>
        public void Resize(Vector2d Size, IGLGUIContext Context)
        {
            Vector2d oldsize = this._Size;
            this._Size = Size;
            this.OnResize(oldsize, Size, Context);
        }

        /// <summary>
        /// Called when the size of the control is forcibly changed.
        /// </summary>
        protected virtual void OnResize(Vector2d OldSize, Vector2d NewSize, IGLGUIContext Context)
        {

        }

        private Vector2d _Size;
    }
}
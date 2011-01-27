using System;
using System.Collections.Generic;
namespace OpenTKGUI
{
    /// <summary>
    /// The basic unit of TKGUI. Describes a gui component that can be drawn and receive events.
    /// </summary>
    public abstract class Control : IDisposable
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
        /// Resizes this control in a not very nice way that may cause problems. Do not use the function unless
        /// you know what you are doing.
        /// </summary>
        public void ForceResize(Point Size)
        {
            this._Size = Size;
            this.OnResize(Size);
        }

        /// <summary>
        /// Resizes a child control.
        /// </summary>
        protected void ResizeChild(Control Child, Point Size)
        {
            Child._Size = Size;
            Child.OnResize(Size);
        }

        /// <summary>
        /// Called when the size of the control is forcibly changed.
        /// </summary>
        protected virtual void OnResize(Point Size)
        {

        }

        /// <summary>
        /// Called when the control is cleaning up it's used resources. The control will not be used
        /// after this. 
        /// </summary>
        protected virtual void OnDispose()
        {

        }

        /// <summary>
        /// Disposes of all resources this control is using along with all its current child controls.
        /// </summary>
        public void Dispose()
        {
            this.OnDispose();
        }

        internal Point _Size;
    }

    /// <summary>
    /// A container of a single control that produces some effect or modification on it.
    /// </summary>
    public class SingleContainer : Control
    {
        public SingleContainer(Control Client)
        {
            this._Client = Client;
        }

        /// <summary>
        /// Gets the control that is affected or modified by this container.
        /// </summary>
        public Control Client
        {
            get
            {
                return this._Client;
            }
        }

        public override void Render(GUIRenderContext Context)
        {
            this._Client.Render(Context);
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            this._Client.Update(Context.CreateChildContext(this._Client, new Point(0.0, 0.0)), Time);
        }

        protected override void OnResize(Point Size)
        {
            this.ResizeChild(this._Client, Size);
        }

        private Control _Client;
    }
}
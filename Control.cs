using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenTKGUI
{
    /// <summary>
    /// The basic unit of TKGUI. Describes a gui component that can be drawn and receive events.
    /// </summary>
    public abstract class Control : IDisposable
    {
        /// <summary>
        /// A convenience function for wrapping the control in a BorderContainer with a uniformly-sized black border.
        /// </summary>
        public BorderContainer WithBorder(double Size)
        {
            return this.WithBorder(Size, Size, Size, Size);
        }

        /// <summary>
        /// A convenience function for wrapping the control in a BorderContainer and applying a black border.
        /// </summary>
        public BorderContainer WithBorder(double Left, double Top, double Right, double Bottom)
        {
            return this.WithBorder(Color.RGB(0.0, 0.0, 0.0), Left, Top, Right, Bottom);
        }

        /// <summary>
        /// A convenience function for wrapping the control in a BorderContainer and applying a border.
        /// </summary>
        public BorderContainer WithBorder(Color Color, double Left, double Top, double Right, double Bottom)
        {
            BorderContainer bc = new BorderContainer(this);
            bc.Color = Color;
            bc.Set(Left, Top, Right, Bottom);
            return bc;
        }

        /// <summary>
        /// A convenience function for wrapping the control in a MarginContainer, applying a uniform margin to the control.
        /// </summary>
        public MarginContainer WithMargin(double Margin)
        {
            return new MarginContainer(this, Margin);
        }

        /// <summary>
        /// A convenience function for wrapping the control in an AlignContainer, giving it a target size and aligning it in
        /// its container.
        /// </summary>
        public AlignContainer WithAlign(Point TargetSize, Align HorizontalAlign, Align VerticalAlign)
        {
            return new AlignContainer(this, TargetSize, HorizontalAlign, VerticalAlign);
        }

        /// <summary>
        /// A convenience function for wrapping the control in an AlignContainer, giving it a target size and aligning it in the center of
        /// its container.
        /// </summary>
        public AlignContainer WithCenterAlign(Point TargetSize)
        {
            return new AlignContainer(this, TargetSize, Align.Center, Align.Center);
        }

        /// <summary>
        /// A convenience function for wrapping the control in an RotateContainer, rotating it by an increment of 90 degrees.
        /// </summary>
        public RotateContainer WithRotate(Rotation Rotation)
        {
            return new RotateContainer(this, Rotation);
        }

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
    /// A control that displays a 3D scene.
    /// </summary>
    public abstract class Render3DControl : Control
    {
        /// <summary>
        /// Sets up the projection matrix for the 3D scene.
        /// </summary>
        public abstract void SetupProjection(Point Viewsize);

        /// <summary>
        /// Renders the scene for the control. This may use calls outside of a GUI render context, as long as it
        /// resets the GL state to how it was before the render.
        /// </summary>
        public abstract void RenderScene();

        /// <summary>
        /// Performs 2D rendering over the scene, for HUDs or overlays.
        /// </summary>
        public virtual void OverRender(GUIRenderContext Context)
        {

        }

        public sealed override void Render(GUIRenderContext Context)
        {
            Context.Draw3D(this.SetupProjection, this.RenderScene, this.Size);
            this.OverRender(Context);
        }
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

        protected override void OnDispose()
        {
            this._Client.Dispose();
        }

        private Control _Client;
    }
}
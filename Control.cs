using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenTKGUI
{
    /// <summary>
    /// Context given to a control when an event occurs. This allows the control to interact with the gui system as a whole.
    /// </summary>
    public class GUIContext
    {

    }
    /// <summary>
    /// Context given to a control for render that prevents controls from interfering with each other by setting GL states directly.
    /// </summary>
    public class GUIRenderContext
    {
        public GUIRenderContext(Point ViewSize)
        {
            this._Effects = new Stack<_Effect>();
            this._ViewSize = ViewSize;
        }

        /// <summary>
        /// Pushes an effect on the stack that will cause all rendering not the specified region defined by a rectangle to be
        /// ignored. The rectangle is in the current coordinate space of the context.
        /// </summary>
        public void PushClip(Rectangle Clip)
        {
            if (this._TopTranslate != null)
            {
                Clip.Location += this._TopTranslate.Offset;
            }
            if (this._TopClip == null)
            {
                GL.Enable(EnableCap.ScissorTest);
            }
            else
            {
                Clip = Rectangle.Intersection(Clip, this._TopClip.Rectangle);
            }
            _ClipEffect ce = new _ClipEffect()
            {
                Previous = this._TopClip,
                Rectangle = Clip
            };
            ce.Apply(this._ViewSize.Y);
            this._Effects.Push(this._TopClip = ce);
        }

        /// <summary>
        /// Pushes an effect that translates the coordinate space by the specified amount.
        /// </summary>
        public void PushTranslate(Point Offset)
        {
            GL.Translate(Offset.X, Offset.Y, 0.0);
            if (this._TopTranslate != null)
            {
                Offset += this._TopTranslate.Offset;
            }
            _TranslateEffect te = new _TranslateEffect()
            {
                Offset = Offset,
                Previous = this._TopTranslate
            };
            this._Effects.Push(this._TopTranslate = te);
        }

        /// <summary>
        /// Undoes the most recent command/effect given to the context.
        /// </summary>
        public void Pop()
        {
            _Effect e = this._Effects.Pop();

            // Remove clip effect
            _ClipEffect ce = e as _ClipEffect;
            if (ce != null)
            {
                this._TopClip = ce.Previous;
                if (this._TopClip == null)
                {
                    GL.Disable(EnableCap.ScissorTest);
                }
                else
                {
                    this._TopClip.Apply(this._ViewSize.Y);
                }
            }

            // Remove translate effect
            _TranslateEffect te = e as _TranslateEffect;
            if (te != null)
            {
                this._TopTranslate = te.Previous;
                if (this._TopTranslate != null)
                {
                    Point noffset = this._TopTranslate.Offset - te.Offset;
                    GL.Translate(noffset.X, noffset.Y, 0.0);
                }
                else
                {
                    GL.Translate(-te.Offset.X, -te.Offset.Y, 0.0);
                }
            }
        }

        private class _Effect
        {

        }

        private class _ClipEffect : _Effect
        {
            public Rectangle Rectangle;
            public _ClipEffect Previous;

            public void Apply(double ViewHeight)
            {
                GL.Scissor(
                    (int)this.Rectangle.Location.X,
                    (int)(ViewHeight - this.Rectangle.Location.Y - this.Rectangle.Size.Y),
                    (int)this.Rectangle.Size.X,
                    (int)this.Rectangle.Size.Y);
            }
        }

        private class _TranslateEffect : _Effect
        {
            public Point Offset;
            public _TranslateEffect Previous;
        }

        private Point _ViewSize;
        private _TranslateEffect _TopTranslate;
        private _ClipEffect _TopClip;
        private Stack<_Effect> _Effects;
    }

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
        /// Updates the state of the panel after the specified amount of time elapses.
        /// </summary>
        public virtual void Update(GUIContext Context, double Time)
        {

        }

        /// <summary>
        /// Resizes the control. Size must be specified in pixels.
        /// </summary>
        public void Resize(GUIContext Context, Point Size)
        {
            Point oldsize = this._Size;
            this._Size = Size;
            this.OnResize(Context, oldsize, Size);
        }

        /// <summary>
        /// Called when the size of the control is forcibly changed.
        /// </summary>
        protected virtual void OnResize(GUIContext Context, Point OldSize, Point NewSize)
        {

        }

        private Point _Size;
    }
}
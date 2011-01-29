using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenTKGUI
{
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
        /// Sets up the render context on the current GL context.
        /// </summary>
        public void Setup()
        {
            GL.Scale(2.0, -2.0, 1.0);
            GL.Translate(-0.5, -0.5, 0.0);
            GL.Scale(1.0 / (double)this._ViewSize.X, 1.0 / (double)this._ViewSize.Y, 1.0);

            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        /// <summary>
        /// Draws a solid-colored rectangle at the specified location, with the specified color.
        /// </summary>
        public void DrawSolid(Color Color, Rectangle Rectangle)
        {
            GL.Disable(EnableCap.Texture2D);
            GL.Color4(Color);
            double x = Rectangle.Location.X;
            double y = Rectangle.Location.Y;
            double xx = x + Rectangle.Size.X;
            double yy = y + Rectangle.Size.Y;
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(x, y);
            GL.Vertex2(xx, y);
            GL.Vertex2(xx, yy);
            GL.Vertex2(x, yy);
            GL.End();
        }

        /// <summary>
        /// Draws a textured rectangle at the specified location.
        /// </summary>
        /// <param name="Modulate">A color to multiply the texture by.</param>
        public void DrawTexture(int Texture, Color Modulate, Rectangle Rectangle)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, Texture);
            GL.Color4(Modulate);
            double x = Rectangle.Location.X;
            double y = Rectangle.Location.Y;
            double xx = x + Rectangle.Size.X;
            double yy = y + Rectangle.Size.Y;
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0f, 0f); GL.Vertex2(x, y);
            GL.TexCoord2(1f, 0f); GL.Vertex2(xx, y);
            GL.TexCoord2(1f, 1f); GL.Vertex2(xx, yy);
            GL.TexCoord2(0f, 1f); GL.Vertex2(x, yy); 
            GL.End();
        }

        public void DrawTexture(int Texture, Rectangle Rectangle)
        {
            this.DrawTexture(Texture, Color.RGB(1.0, 1.0, 1.0), Rectangle);
        }

        /// <summary>
        /// Draws a surface with the specified offset.
        /// </summary>
        public void DrawSurface(Surface Surface, Point Offset)
        {
            Surface.Render(Offset, this);
        }

        public void DrawSurface(Surface Surface)
        {
            Surface.Render(this);
        }

        /// <summary>
        /// Draws text to the specified location.
        /// </summary>
        public void DrawText(Color Color, TextSample Sample, Point TopLeft)
        {
            Sample.Render(Color, TopLeft);
        }

        /// <summary>
        /// Draws text to the specified region, overriding its layout parameters.
        /// </summary>
        public void DrawText(Color Color, TextSample Sample, Rectangle Region, TextAlign HorizontalAlign, TextAlign VerticalAlign)
        {
            double x = Region.Location.X;
            double y = Region.Location.Y;
            switch (HorizontalAlign)
            {
                case TextAlign.Center: x += Region.Size.X / 2.0 - Sample.Size.X / 2.0; break;
                case TextAlign.Right: x += Region.Size.X - Sample.Size.X; break;
            }
            switch (VerticalAlign)
            {
                case TextAlign.Center: y += Region.Size.Y / 2.0 - Sample.Size.Y / 2.0; break;
                case TextAlign.Bottom: y += Region.Size.Y - Sample.Size.Y; break;
            }
            this.DrawText(Color, Sample, new Point(x, y));
        }

        /// <summary>
        /// Draws text to the specified region using the layout parameters it was supplied with.
        /// </summary>
        public void DrawText(Color Color, TextSample Sample, Rectangle Region)
        {
            this.DrawText(Color, Sample, Region, Sample.HorizontalAlign, Sample.VerticalAlign);
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
                if (this.Rectangle.Size.X < 0.0 || this.Rectangle.Size.Y < 0.0)
                {
                    this.Rectangle.Size = new Point(0.0, 0.0);
                }

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
}
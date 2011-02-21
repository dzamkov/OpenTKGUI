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
        public GUIRenderContext(Point ViewSize, IEffectStack<RenderEffect, GUIRenderContext> EffectStack)
        {
            this._EffectStack = EffectStack;
            this._ViewSize = ViewSize;
        }

        public GUIRenderContext(Point ViewSize)
        {
            this._EffectStack = new EffectStack<RenderEffect, GUIRenderContext>(this);
            this._ViewSize = ViewSize;
        }

        /// <summary>
        /// Gets the size of the area rendering is done to.
        /// </summary>
        public Point ViewSize
        {
            get
            {
                return this._ViewSize;
            }
        }

        /// <summary>
        /// Sets up the render context on the current GL context.
        /// </summary>
        public void Setup()
        {
            GL.Viewport(0, 0, (int)this._ViewSize.X, (int)this._ViewSize.Y);
            GL.MatrixMode(MatrixMode.Projection);
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
        /// Draws a surface at the specified area.
        /// </summary>
        public void DrawSurface(Surface Surface, Rectangle Area)
        {
            Surface.Render(Area, this);
        }

        /// <summary>
        /// Draws a fixed surface to the specified point.
        /// </summary>
        public void DrawSurface(FixedSurface Surface, Point Offset)
        {
            Surface.Render(Offset, this);
        }

        /// <summary>
        /// Draws a fixed surface to an area with the given alignment.
        /// </summary>
        public void DrawSurface(FixedSurface Surface, Align Horizontal, Align Vertical, Rectangle Area)
        {
            Point surfsize = Surface.Size;
            double x = 0.0;
            double y = 0.0;
            switch (Horizontal)
            {
                case Align.Center: x = Area.Size.X * 0.5 - surfsize.X * 0.5; break;
                case Align.Right: x = Area.Size.X - surfsize.X; break;
            }
            switch (Vertical)
            {
                case Align.Center: y = Area.Size.Y * 0.5 - surfsize.Y * 0.5; break;
                case Align.Bottom: y = Area.Size.Y - surfsize.Y; break;
            }
            Surface.Render(new Point(x, y) + Area.Location, this);
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
        /// Draws a 3D scene.
        /// </summary>
        public void Draw3D(SetupProjectionHandler SetupProjection, SceneRenderHandler RenderScene, Point Size)
        {
            GL.Viewport(0, 0, (int)this._ViewSize.X, (int)this._ViewSize.Y);
            using (this.Clip(new Rectangle(Size)))
            {
                GL.Clear(ClearBufferMask.DepthBufferBit);
                GL.PushMatrix();
                GL.Scale(Size.X * 0.5, -Size.Y * 0.5, 1.0);
                GL.Translate(1.0, -1.0, 0.0);
                SetupProjection(Size);
                GL.MatrixMode(MatrixMode.Modelview);
                RenderScene();
                GL.MatrixMode(MatrixMode.Projection);
                GL.PopMatrix();
            }
        }

        /// <summary>
        /// Gets the effect stack for this render context.
        /// </summary>
        public IEffectStack<RenderEffect, GUIRenderContext> EffectStack
        {
            get
            {
                return this._EffectStack;
            }
        }

        /// <summary>
        /// Creates an effect on the stack that will cause all rendering not the specified region defined by a rectangle to be
        /// ignored.
        /// </summary>
        public IDisposable Clip(Rectangle Area)
        {
            this._EffectStack.Push(new ClipRenderEffect(Area));
            return EffectStack<RenderEffect, GUIRenderContext>.PopOnDispose(this._EffectStack);
        }

        /// <summary>
        /// Creates an effect on the effect stack that rotates render operations.
        /// </summary>
        public IDisposable Rotate(Point Pivot, Rotation Rotation)
        {
            this._EffectStack.Push(new RotateRenderEffect(Pivot, Rotation));
            return EffectStack<RenderEffect, GUIRenderContext>.PopOnDispose(this._EffectStack);
        }

        /// <summary>
        /// Creates an effect that translates the coordinate space by the specified amount.
        /// </summary>
        public IDisposable Translate(Point Offset)
        {
            this._EffectStack.Push(new TranslateRenderEffect(Offset));
            return EffectStack<RenderEffect, GUIRenderContext>.PopOnDispose(this._EffectStack);
        }

        /// <summary>
        /// Converts a rectangle in the GUI coordinate system to a rectangle in view coordinates, if possible.
        /// </summary>
        public Rectangle ToView(Rectangle Rectangle)
        {
            foreach (RenderEffect e in this._EffectStack.Effects)
            {
                TranslateRenderEffect te = e as TranslateRenderEffect;
                if (te != null)
                {
                    Rectangle.Location += te.Offset;
                    continue;
                }


                RotateRenderEffect re = e as RotateRenderEffect;
                if (re != null)
                {
                    Rectangle.Location = Rectangle.Location.Rotate(re.Pivot, re.Rotation);
                    Rectangle.Size = Rectangle.Size.Rotate(re.Rotation);
                    Rectangle = Rectangle.Fix;
                    continue;
                }
            }
            return Rectangle;
        }

        private Point _ViewSize;
        private IEffectStack<RenderEffect, GUIRenderContext> _EffectStack;
    }

    /// <summary>
    /// When called, multiplies the current matrix on the current GL matrix to reflect a projection.
    /// </summary>
    public delegate void SetupProjectionHandler(Point Size);

    /// <summary>
    /// When called, renders a 3D scene on the current GL context.
    /// </summary>
    public delegate void SceneRenderHandler();

    /// <summary>
    /// A effect that can be applied to a render context.
    /// </summary>
    public abstract class RenderEffect : Effect<GUIRenderContext>
    {

    }

    /// <summary>
    /// An effect that translates rendering operations.
    /// </summary>
    public class TranslateRenderEffect : RenderEffect
    {
        public TranslateRenderEffect(Point Offset)
        {
            this._Offset = Offset;
        }

        public override void Apply(GUIRenderContext Environment)
        {
            GL.Translate(this._Offset.X, this._Offset.Y, 0.0);
        }

        public override void Remove(GUIRenderContext Environment)
        {
            GL.Translate(-this._Offset.X, -this._Offset.Y, 0.0);
        }

        /// <summary>
        /// Gets the translation offset for this effect.
        /// </summary>
        public Point Offset
        {
            get
            {
                return this._Offset;
            }
        }

        private Point _Offset;
    }

    /// <summary>
    /// An effect that rotates rendering about a pivot.
    /// </summary>
    public class RotateRenderEffect : RenderEffect
    {
        public RotateRenderEffect(Point Pivot, Rotation Rotation)
        {
            this._Pivot = Pivot;
            this._Rotation = Rotation;
        }

        public override void Apply(GUIRenderContext Environment)
        {
            GL.PushMatrix();
            GL.Translate(Pivot.X, Pivot.Y, 0.0);
            GL.Rotate(-(int)Rotation * 90.0, 0.0, 0.0, 1.0);
            GL.Translate(-Pivot.X, -Pivot.Y, 0.0);
        }

        public override void Remove(GUIRenderContext Environment)
        {
            GL.PopMatrix();
        }

        /// <summary>
        /// Gets the pivot of the rotation.
        /// </summary>
        public Point Pivot
        {
            get
            {
                return this._Pivot;
            }
        }

        /// <summary>
        /// Gets the rotation.
        /// </summary>
        public Rotation Rotation
        {
            get
            {
                return this._Rotation;
            }
        }

        private Point _Pivot;
        private Rotation _Rotation;
    }

    /// <summary>
    /// An effect that limits rendering to a rectangular area.
    /// </summary>
    public class ClipRenderEffect : RenderEffect
    {
        public ClipRenderEffect(Rectangle Area)
        {
            this._Area = Area;
        }

        public override void Apply(GUIRenderContext Environment)
        {
            this._Area = Environment.ToView(this._Area);

            bool first = true;
            foreach (RenderEffect re in Environment.EffectStack.Effects)
            {
                var cre = re as ClipRenderEffect;
                if (cre != null)
                {
                    first = false;
                    this._Area = this._Area.Intersection(cre._Area);
                    break;
                }
            }
            if (first)
            {
                GL.Enable(EnableCap.ScissorTest);
            }

            if (this._Area.Size.X < 0.0 || this._Area.Size.Y < 0.0)
            {
                this._Area.Size = new Point(0.0, 0.0);
            }
            _Scissor(Environment.ViewSize.Y, this._Area);
        }

        public override void Remove(GUIRenderContext Environment)
        {
            ClipRenderEffect prev = null;
            foreach (RenderEffect re in Environment.EffectStack.Effects)
            {
                var cre = re as ClipRenderEffect;
                if (cre != null)
                {
                    prev = cre;
                    break;
                }
            }
            if (prev == null)
            {
                GL.Disable(EnableCap.ScissorTest);
            }
            else
            {
                _Scissor(Environment.ViewSize.Y, prev._Area);
            }
        }

        private static void _Scissor(double ViewHeight, Rectangle Area)
        {
            GL.Scissor(
                    (int)Area.Location.X,
                    (int)(ViewHeight - Area.Location.Y - Area.Size.Y),
                    (int)Area.Size.X,
                    (int)Area.Size.Y);
        }

        private Rectangle _Area;
    }
}
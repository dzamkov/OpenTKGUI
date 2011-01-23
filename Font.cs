using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using SFont = System.Drawing.Font;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenTKGUI
{
    /// <summary>
    /// Represents a font, an interface that can be used for converting text into textures.
    /// </summary>
    public abstract class Font : IDisposable
    {
        /// <summary>
        /// Gets or sets the default font used for controls.
        /// </summary>
        public static Font Default
        {
            get
            {
                if (_Default == null)
                {
                    _Default = new SystemFont("Verdana", 14.0, true);
                }
                return _Default;
            }
            set
            {
                _Default = value;
            }
        }

        private static Font _Default;

        /// <summary>
        /// Gets a text sample for the specified text when drawn with this font. Note that multiple lines can be created with '\n'.
        /// </summary>
        public TextSample CreateSample(string Text)
        {
            return this.CreateSample(Text, null, TextAlign.Left, TextAlign.Top, TextWrap.Clip);
        }

        /// <summary>
        /// Gets a text sample for the specified text when drawn with this font. Note that multiple lines can be created with '\n'.
        /// <returns></returns>
        public abstract TextSample CreateSample(string Text, Point? MaxSize, TextAlign HorizontalAlign, TextAlign VerticalAlign, TextWrap Wrap);

        /// <summary>
        /// Gets an estimate of the size of a character drawn with this font.
        /// </summary>
        public virtual Point EstimateCharacterSize
        {
            get
            {
                return new Point(20.0, 20.0);
            }
        }

        public void Dispose()
        {
            this.OnDispose();
        }

        /// <summary>
        /// Called when the font is no longer needed.
        /// </summary>
        protected virtual void OnDispose()
        {

        }
    }

    /// <summary>
    /// Information and methods for a text when drawn with font. TextSample's may be rendered directly, or preferably, from
    /// a GUIRenderContext. TextSample's that need to be rendered repeatedly should be saved and used until they are no longer needed, as opposed to
    /// being discarded and recreated on each render.
    /// </summary>
    public abstract class TextSample : IDisposable
    {
        /// <summary>
        /// Gets the text used in the sample.
        /// </summary>
        public abstract string Text { get; }

        /// <summary>
        /// Gets an array of rectangles where each rectangle corresponds to a the bounding region the character at the same index occupies.
        /// </summary>
        public abstract Rectangle[] CharacterBounds { get; }

        /// <summary>
        /// Gets the font the text sample is drawn with.
        /// </summary>
        public abstract Font Font { get; }

        /// <summary>
        /// Gets the size, in pixels, of the text sample.
        /// </summary>
        public abstract Point Size { get; }

        /// <summary>
        /// Gets the horizontal alignment of the text in the text sample.
        /// </summary>
        public abstract TextAlign HorizontalAlign { get; }

        /// <summary>
        /// Gets the vertical alignment of the text in the text sample.
        /// </summary>
        public abstract TextAlign VerticalAlign { get; }

        /// <summary>
        /// Renders the text sample to the current graphics context with the specified color and offset.
        /// </summary>
        public abstract void Render(Color Color, Point Offset);

        public void Dispose()
        {
            this.OnDispose();
        }

        /// <summary>
        /// Called when the sample is no longer needed.
        /// </summary>
        protected virtual void OnDispose()
        {

        }
        
    }

    /// <summary>
    /// A font identified by a name installed on the current system.
    /// </summary>
    public class SystemFont : Font
    {
        public SystemFont(string Name, double PixelSize, bool Antialias)
        {
            this._Font = new SFont(Name, (float)PixelSize, GraphicsUnit.Pixel);
            this._Antialias = Antialias;
        }

        public override TextSample CreateSample(string Text, Point? MaxSize, TextAlign HorizontalAlign, TextAlign VerticalAlign, TextWrap Wrap)
        {
            return new _TextSample(this, Text, MaxSize, HorizontalAlign, VerticalAlign, Wrap);
        }

        protected override void OnDispose()
        {
            this._Font.Dispose();
        }

        /// <summary>
        /// Gets a GDI-usable version of this font.
        /// </summary>
        public SFont GDIFont
        {
            get
            {
                return this._Font;
            }
        }

        /// <summary>
        /// Gets if this font is antialias'd.
        /// </summary>
        public bool Antialias
        {
            get
            {
                return this._Antialias;
            }
        }

        public override Point EstimateCharacterSize
        {
            get
            {
                return new Point(this._Font.Size, this._Font.Size * 0.7);
            }
        }

        /// <summary>
        /// A text sample for this kind of font.
        /// </summary>
        private class _TextSample : TextSample
        {
            public _TextSample(SystemFont Font, string Text, Point? MaxSize, TextAlign HorizontalAlign, TextAlign VerticalAlign, TextWrap Wrap)
            {
                this._Font = Font;
                this._Text = Text;
                this._TextureID = -1;

                this._MaxSize = MaxSize;
                this._HorizontalAlign = HorizontalAlign;
                this._VerticalAlign = VerticalAlign;
                this._Wrap = Wrap;
            }

            public override Font Font
            {
                get
                {
                    return this._Font;
                }
            }

            public override string Text
            {
                get
                {
                    return this._Text;
                }
            }

            public override Rectangle[] CharacterBounds
            {
                get
                {
                    if (this._CharacterBounds == null)
                    {
                        this._MakeTexture();
                    }
                    return this._CharacterBounds;
                }
            }

            public override Point Size
            {
                get
                {
                    if (this._Size == null)
                    {
                        // Oh why Microsoft, Why couldn't you make this static?
                        using (Bitmap bm = new Bitmap(1, 1))
                        {
                            using (Graphics g = Graphics.FromImage(bm))
                            {
                                SizeF sf;
                                if(this._MaxSize == null)
                                {
                                    sf = g.MeasureString(this._Text, this._Font.GDIFont, new PointF(0.0f, 0.0f), this._Format);
                                }
                                else
                                {
                                    Point maxsize = this._MaxSize.Value;
                                    sf = g.MeasureString(this._Text, this._Font.GDIFont, new SizeF((float)maxsize.X, (float)maxsize.Y), this._Format);
                                }
                                Point size = new Point(sf.Width, sf.Height);
                                this._Size = size;
                                return size;
                            }
                        }
                    }
                    else
                    {
                        return this._Size.Value;
                    }
                }
            }

            public override void Render(Color Color, Point Offset)
            {
                if (this._TextureID < 0)
                {
                    this._MakeTexture();
                }

                if (this._TextureWidth > 0 && this._TextureHeight > 0)
                {
                    GL.Enable(EnableCap.Texture2D);
                    GL.BindTexture(TextureTarget.Texture2D, this._TextureID);
                    GL.Color4((Color4)Color);
                    double x = Offset.X;
                    double y = Offset.Y;
                    double xx = x + this._TextureWidth;
                    double yy = y + this._TextureHeight;
                    GL.Begin(BeginMode.Quads);
                    GL.TexCoord2(0f, 0f); GL.Vertex2(x, y);
                    GL.TexCoord2(1f, 0f); GL.Vertex2(xx, y);
                    GL.TexCoord2(1f, 1f); GL.Vertex2(xx, yy);
                    GL.TexCoord2(0f, 1f); GL.Vertex2(x, yy);
                    GL.End();
                }
            }

            private void _MakeTexture()
            {
                Point size = this.Size;
                int texwidth = this._TextureWidth = (int)Math.Ceiling(size.X);
                int texheight = this._TextureHeight = (int)Math.Ceiling(size.Y);
                if (texwidth > 0 && texheight > 0)
                {
                    // Draw text to texture
                    using (Bitmap bm = new Bitmap(texwidth, texheight))
                    {
                        using (Graphics g = Graphics.FromImage(bm))
                        {
                            if (this._Font.Antialias)
                            {
                                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                            }
                            g.Clear(Color.Transparent);

                            string text = this._Text;


                            StringFormat sf = this._Format;
                            g.DrawString(text, this._Font.GDIFont, Brushes.White, new RectangleF(0f, 0f, (float)size.X, (float)size.Y), sf);

                            // Get character bounds (32 at a time) (and kind of a hacky way to do it).
                            this._CharacterBounds = new Rectangle[text.Length];
                            int advance = 32;
                            for (int i = 0; i < text.Length; i += advance)
                            {
                                int dif = text.Length - i;
                                CharacterRange[] crmeasure = new CharacterRange[dif > advance ? advance : dif];
                                for (int t = 0; t < crmeasure.Length; t++)
                                {
                                    crmeasure[t] = new CharacterRange(i + t, 1);
                                }

                                sf.SetMeasurableCharacterRanges(crmeasure);
                                Region[] rs = g.MeasureCharacterRanges(text, this._Font.GDIFont, new RectangleF(0.0f, 0.0f, texwidth, texheight), sf);
                                for (int t = 0; t < rs.Length; t++)
                                {
                                    if (i + t < this._CharacterBounds.Length)
                                    {
                                        this._CharacterBounds[i + t] = rs[t].GetBounds(g);
                                    }
                                }
                            }
                        }

                        this._TextureID = GL.GenTexture();
                        GL.BindTexture(TextureTarget.Texture2D, this._TextureID);
                        GL.TexEnv(TextureEnvTarget.TextureEnv,
                            TextureEnvParameter.TextureEnvMode,
                            (float)TextureEnvMode.Modulate);

                        BitmapData bd = bm.LockBits(
                            new System.Drawing.Rectangle(0, 0, bm.Width, bm.Height),
                            ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                        GL.TexImage2D(TextureTarget.Texture2D,
                            0, PixelInternalFormat.Rgba,
                            bm.Width, bm.Height, 0,
                            OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bd.Scan0);

                        bm.UnlockBits(bd);

                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    }
                }
                else
                {
                    this._CharacterBounds = new Rectangle[this._Text.Length];
                }
            }

            /// <summary>
            /// Gets the string format used for drawing or measuring text.
            /// </summary>
            private StringFormat _Format
            {
                get
                {
                    StringFormat sf = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);
                    switch (this._VerticalAlign)
                    {
                        case TextAlign.Left:
                            sf.LineAlignment = StringAlignment.Near;
                            break;
                        case TextAlign.Center:
                            sf.LineAlignment = StringAlignment.Center;
                            break;
                        case TextAlign.Right:
                            sf.LineAlignment = StringAlignment.Far;
                            break;
                    };
                    switch (this._HorizontalAlign)
                    {
                        case TextAlign.Top:
                            sf.Alignment = StringAlignment.Near;
                            break;
                        case TextAlign.Center:
                            sf.Alignment = StringAlignment.Center;
                            break;
                        case TextAlign.Bottom:
                            sf.Alignment = StringAlignment.Far;
                            break;
                    };
                    switch (this._Wrap)
                    {
                        case TextWrap.Clip:
                            sf.Trimming = StringTrimming.None;
                            sf.FormatFlags |= StringFormatFlags.NoWrap;
                            break;
                        case TextWrap.Ellipsis:
                            sf.Trimming = StringTrimming.EllipsisCharacter;
                            sf.FormatFlags |= StringFormatFlags.NoWrap;
                            break;
                        case TextWrap.Wrap:
                            sf.Trimming = StringTrimming.None;
                            break;
                    };
                    return sf;
                }
            }

            protected override void OnDispose()
            {
                if (this._TextureID >= 0)
                {
                    GL.DeleteTexture(this._TextureID);
                }
            }

            public override TextAlign HorizontalAlign
            {
                get
                {
                    return this._HorizontalAlign;
                }
            }

            public override TextAlign VerticalAlign
            {
                get
                {
                    return this._VerticalAlign;
                }
            }

            private int _TextureID;
            private int _TextureWidth;
            private int _TextureHeight;

            private TextAlign _VerticalAlign;
            private TextAlign _HorizontalAlign;
            private TextWrap _Wrap;
            private Point? _MaxSize;
            private Rectangle[] _CharacterBounds;
            private Point? _Size;
            private SystemFont _Font;
            private string _Text;
        }

        private bool _Antialias;
        private SFont _Font;
    }
}
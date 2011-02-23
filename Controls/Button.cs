using System;
using System.Collections.Generic;

using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// A clickable button with text.
    /// </summary>
    public class Button : Control
    {
        public Button()
            : this(ButtonStyle.Default)
        {
        }

        public Button(string Text)
            : this(ButtonStyle.Default, Text)
        {
        }

        public Button(ButtonStyle Style, string Text)
            : this(Style, new LabelStyle()
            {
                HorizontalAlign = TextAlign.Center,
                VerticalAlign = TextAlign.Center,
                Wrap = TextWrap.Ellipsis,
            }, Color.RGB(0.0, 0.0, 0.0), Text)
        {

        }

        public Button(ButtonStyle Style, LabelStyle LabelStyle, Color TextColor, string Text)
            : this(Style, new Label(Text, TextColor, LabelStyle))
        {
            this.Text = Text;
        }

        public Button(Control Client)
            : this(ButtonStyle.Default, Client)
        {

        }

        public Button(ButtonStyle Style, Control Client)
            : this(Style)
        {
            this._Client = Client;
        }

        public Button(ButtonStyle Style)
        {
            this._Style = Style;
            this._Scope = new _ButtonScope();
        }

        /// <summary>
        /// Gets or sets the text in the middle of the button. This may only be done if the client of the button is a label.
        /// </summary>
        public string Text
        {
            get
            {
                Label l = this._Client as Label;
                if (l != null)
                {
                    return l.Text;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                Label l = this._Client as Label;
                if (l != null)
                {
                    l.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets the client control that appears in the middle of the button.
        /// </summary>
        public Control Client
        {
            get
            {
                return this._Client;
            }
        }

        /// <summary>
        /// Gets the size of the client in the middle of the button.
        /// </summary>
        public Point ClientSize
        {
            get
            {
                return this.Size - new Point(this._Style.ClientMargin, this._Style.ClientMargin) * 2.0;
            }
        }

        /// <summary>
        /// Gets the full size of the button needed to have the specified client size.
        /// </summary>
        public Point GetFullSize(Point ClientSize)
        {
            return ClientSize + new Point(this._Style.ClientMargin, this._Style.ClientMargin) * 2.0;
        }

        public override void Render(RenderContext Context)
        {
            ButtonStyle style = this._Style;
            Surface sf;
            if (this._MouseOver)
            {
                if (this._MouseDown)
                {
                    sf = style.Pushed;
                }
                else
                {
                    sf = style.Active;
                }
            }
            else
            {
                sf = style.Normal;
            }
            Context.DrawSurface(sf, new Rectangle(this.Size));

            // Render client, if any
            if (this._Client != null)
            {
                using (Context.Translate(new Point(this._Style.ClientMargin, this._Style.ClientMargin)))
                {
                    this._Client.Render(Context);
                }
            }
        }

        public override void Update(InputContext Context)
        {
            
            using (Context.Stencil)
            {
                Context.StencilClip(new Rectangle(this.Size));
                using (Context.Scope(this._Scope))
                {
                    // Update client, if any
                    if (this._Client != null)
                    {
                        using (Context.Translate(new Point(this._Style.ClientMargin, this._Style.ClientMargin)))
                        {
                            this._Client.Update(Context);
                        }
                    }

                    // Check for click
                    MouseState ms = Context.MouseState;
                    if (ms != null)
                    {
                        this._MouseOver = true;
                        if (ms.IsButtonDown(MouseButton.Left))
                        {
                            this._MouseDown = true;
                        }
                        else
                        {
                            if (this._MouseDown)
                            {
                                // That would be a click!
                                this._Click();
                                this._MouseDown = false;
                            }
                        }
                    }
                    else
                    {
                        this._MouseDown = false;
                        this._MouseOver = false;
                    }
                }
            }
        }

        private void _Click()
        {
            if (this.Click != null)
            {
                this.Click.Invoke();
            }
        }

        protected override void OnResize(Point Size)
        {
            this._ResizeClient();
        }

        private void _ResizeClient()
        {
            if (this._Client != null)
            {
                this.ResizeChild(this._Client, this.ClientSize);
            }
        }

        /// <summary>
        /// A scope for a button.
        /// </summary>
        private class _ButtonScope : Scope
        {
            public override void AlterInternalControl(InputContext Context, ref MouseState MouseState, ref KeyboardState KeyboardState)
            {
                // Do not allow the button to access an occluded mouse.
                if (!Context.MouseVisible)
                {
                    MouseState = null;
                }
            }
        }

        /// <summary>
        /// An event fired whenever this button is clicked.
        /// </summary>
        public event ClickHandler Click;

        private bool _MouseDown;
        private bool _MouseOver;
        private _ButtonScope _Scope;
        private Control _Client;
        private ButtonStyle _Style;
    }

    /// <summary>
    /// Gives styling options for a button.
    /// </summary>
    public sealed class ButtonStyle
    {
        public ButtonStyle()
        {

        }

        public ButtonStyle(Skin Skin)
        {
            this.Normal = Skin.GetStretchableSurface(new SkinArea(0, 0, 16, 16));
            this.Active = Skin.GetStretchableSurface(new SkinArea(16, 0, 16, 16));
            this.Pushed = Skin.GetStretchableSurface(new SkinArea(32, 0, 16, 16));
        }

        public static readonly ButtonStyle Default = new ButtonStyle(Skin.Default);

        /// <summary>
        /// Creates a button style for a solid button.
        /// </summary>
        public static ButtonStyle CreateSolid(Skin Skin)
        {
            ButtonStyle bs = new ButtonStyle();
            bs.Normal = Skin.GetStretchableSurface(new SkinArea(0, 32, 16, 16));
            bs.Active = Skin.GetStretchableSurface(new SkinArea(16, 32, 16, 16));
            bs.Pushed = Skin.GetStretchableSurface(new SkinArea(32, 32, 16, 16));
            return bs;
        }

        public Surface Normal;
        public Surface Active;
        public Surface Pushed;
        public double ClientMargin = 6.0;
    }

}
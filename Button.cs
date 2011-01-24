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
            : this(new ButtonStyle())
        {
        }

        public Button(string Text)
            : this(Text, new ButtonStyle())
        {
        }

        public Button(string Text, ButtonStyle Style)
            : this(Style)
        {
            this.Text = Text;
        }

        public Button(ButtonStyle Style)
        {
            this._Style = Style;
        }

        /// <summary>
        /// Sets the text in the middle of the button. This will replace any client the button currently has.
        /// </summary>
        public string Text
        {
            set
            {
                Label l = this._Client as Label;
                if (l == null)
                {
                    this._Client = new Label(value, this._Style.TextColor, this._Style.TextStyle);
                }
                else
                {
                    l.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the client control that appears in the middle of the button.
        /// </summary>
        public Control Client
        {
            get
            {
                return this._Client;
            }
            set
            {
                this._Client = value;
                this._ResizeClient();
            }
        }

        public override void Render(GUIRenderContext Context)
        {
            Skin s = this._Style.Skin;
            Surface sf;
            if (this._MouseOver)
            {
                if (this._MouseDown)
                {
                    sf = s.GetSurface(this._Style.Pushed, this.Size);
                }
                else
                {
                    sf = s.GetSurface(this._Style.Active, this.Size);
                }
            }
            else
            {
                sf = s.GetSurface(this._Style.Normal, this.Size);
            }
            Context.DrawSurface(sf);

            // Render client, if any
            if (this._Client != null)
            {
                Context.PushTranslate(new Point(this._Style.ClientMargin, this._Style.ClientMargin));
                this._Client.Render(Context);
                Context.Pop();
            }
        }

        public override void Update(GUIControlContext Context, double Time)
        {
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

            // Update client, if any
            if (this._Client != null)
            {
                this._Client.Update(Context.CreateChildContext(this._Client, new Point(this._Style.ClientMargin, this._Style.ClientMargin)), Time);
            }
        }

        private void _Click()
        {
            if (this.Click != null)
            {
                this.Click.Invoke(this);
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
                this._Client.Resize(this.Size - new Point(this._Style.ClientMargin, this._Style.ClientMargin) * 2.0);
            }
        }

        /// <summary>
        /// An event fired whenever this button is clicked.
        /// </summary>
        public event ClickHandler Click;

        private bool _MouseDown;
        private bool _MouseOver;
        private Control _Client;
        private ButtonStyle _Style;
    }

    /// <summary>
    /// Gives styling options for a button.
    /// </summary>
    public sealed class ButtonStyle
    {
        public Skin Skin = Skin.Default;
        public SkinArea Normal = new SkinArea(0, 0, 32, 32);
        public SkinArea Active = new SkinArea(32, 0, 32, 32);
        public SkinArea Pushed = new SkinArea(64, 0, 32, 32);
        public Color TextColor = Color.RGB(0.0, 0.0, 0.0);
        public LabelStyle TextStyle = new LabelStyle()
        {
            HorizontalAlign = TextAlign.Center,
            VerticalAlign = TextAlign.Center,
            Wrap = TextWrap.Ellipsis
        };
        public double ClientMargin = 6.0;
    }

}
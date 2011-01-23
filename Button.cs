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
        public Button(string Text)
            : this(new ButtonStyle(), Text)
        {
        }

        public Button(ButtonStyle Style, string Text)
        {
            this._Style = Style;
            this._Text = Text;
        }

        /// <summary>
        /// Gets or sets the text for the button.
        /// </summary>
        public string Text
        {
            get
            {
                return this._Text;
            }
            set
            {
                this._Text = value;
                if (this._TextSample != null)
                {
                    this._TextSample.Dispose();
                    this._TextSample = null;
                }
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

            if (this._Text != null && this._Text.Length != 0)
            {
                if (this._TextSample == null)
                {
                    this._TextSample = this._Style.Font.CreateSample(this._Text, this.Size, TextAlign.Center, TextAlign.Center, TextWrap.Clip);
                }

                Context.DrawText(this._Style.TextColor, this._TextSample, new Rectangle(this.Size));
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
        }

        private void _Click()
        {
            if (this.Click != null)
            {
                this.Click.Invoke(this);
            }
        }

        /// <summary>
        /// An event fired whenever this button is clicked.
        /// </summary>
        public event ClickHandler Click;

        private bool _MouseDown;
        private bool _MouseOver;
        private TextSample _TextSample;
        private string _Text;
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
        public Font Font = Font.Default;
    }

    /// <summary>
    /// Handles a click event from a button.
    /// </summary>
    public delegate void ClickHandler(Button Button);
}
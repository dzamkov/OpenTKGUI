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
        {
            this._Text = Text;
        }

        public override void Render(GUIRenderContext Context)
        {
            Skin s = Skin.Default;
            if (this._MouseOver)
            {
                if (this._MouseDown)
                {
                    Context.DrawSkinPart(s.GetPart(64, 0, 32, 32), new Rectangle(this.Size));
                }
                else
                {
                    Context.DrawSkinPart(s.GetPart(32, 0, 32, 32), new Rectangle(this.Size));
                }
            }
            else
            {
                Context.DrawSkinPart(s.GetPart(0, 0, 32, 32), new Rectangle(this.Size));
            }
        }

        public override void Update(GUIContext Context, double Time)
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
        private string _Text;
    }

    /// <summary>
    /// Handles a click event from a button.
    /// </summary>
    public delegate void ClickHandler(Button Button);
}
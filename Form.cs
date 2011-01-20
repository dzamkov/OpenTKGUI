using System;
using System.Collections.Generic;

using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// A floating, draggable control that can be used in a layer container.
    /// </summary>
    public class Form : LayerControl
    {
        public Form(Control Client)
        {
            this._Client = Client;
        }

        /// <summary>
        /// Gets or sets the control that is inside the form.
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
                this._Client.Resize(this.ClientRectangle.Size);
            }
        }

        /// <summary>
        /// Gets the rectangle in relation to the form for the client of the form.
        /// </summary>
        public Rectangle ClientRectangle
        {
            get
            {
                return new Rectangle(_BorderSize, _TitleBarSize, this.Size.X - _BorderSize * 2.0, this.Size.Y - _BorderSize - _TitleBarSize);
            }
        }

        public override void Render(GUIRenderContext Context)
        {
            Context.PushTranslate(this.ClientRectangle.Location);
            this._Client.Render(Context);
            Context.Pop();

            Skin s = Skin.Default;
            Context.DrawSurface(s.GetSurface(0, 32, 64, 64, 32, 44, this.Size));
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            this._Client.Update(Context.CreateChildContext(this._Client, this.ClientRectangle.Location), Time);
        }

        protected override void OnResize(Point OldSize, Point NewSize)
        {
            this._Client.Resize(this.ClientRectangle.Size);
        }

        private const double _TitleBarSize = 32.0;
        private const double _BorderSize = 7.0;

        /// <summary>
        /// Resizes the form so that the client has the specified size.
        /// </summary>
        public void ResizeClient(Point Size)
        {
            this.Resize(new Point(Size.X + _BorderSize * 2.0, Size.Y + _BorderSize + _TitleBarSize));
        }

        private Control _Client;
    }
}
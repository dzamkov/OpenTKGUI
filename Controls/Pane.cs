using System;
using System.Collections.Generic;

using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// A simple immobile container control that can be used in a layer container.
    /// </summary>
    public class Pane : LayerControl
    {
        public Pane(Control Client) : this(PaneStyle.Default, Client)
        {

        }

        public Pane(PaneStyle Style, Control Client)
        {
            this._Style = Style;
            this._Client = Client;
        }

        /// <summary>
        /// Makes the pane go away and be disposed.
        /// </summary>
        public void Dismiss()
        {
            this.Container.RemoveControl(this);
            this.Dispose();
        }

        /// <summary>
        /// Gets the control that is inside the pane.
        /// </summary>
        public Control Client
        {
            get
            {
                return this._Client;
            }
        }

        /// <summary>
        /// Gets the rectangle in relation to the pane for the client of the pane.
        /// </summary>
        public Rectangle ClientRectangle
        {
            get
            {
                double bs = this._Style.BorderSize;
                return new Rectangle(bs, bs, this.Size.X - bs * 2.0, this.Size.Y - bs * 2.0);
            }
        }

        /// <summary>
        /// Gets or sets the client size of the pane, which is the size allocated to the client.
        /// </summary>
        public Point ClientSize
        {
            get
            {
                return this.ClientRectangle.Size;
            }
            set
            {
                this.Size = new Point(value.X + this._Style.BorderSize * 2.0, value.Y + this._Style.BorderSize * 2.0);
            }
        }

        public override void Render(GUIRenderContext Context)
        {
            Context.DrawSurface(this._Style.Pane, new Rectangle(this.Size));
            Context.PushTranslate(this.ClientRectangle.Location);
            this._Client.Render(Context);
            Context.Pop();
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            this._Client.Update(Context.CreateChildContext(this._Client, this.ClientRectangle.Location), Time);
        }

        protected override void OnDispose()
        {
            this._Client.Dispose();
        }

        protected override void OnResize(Point Size)
        {
            this.ResizeChild(this._Client, this.ClientSize);
        }

        private PaneStyle _Style;
        private Control _Client;
    }

    /// <summary>
    /// Gives styling options for a form.
    /// </summary>
    public sealed class PaneStyle
    {
        public PaneStyle()
        {

        }

        public PaneStyle(Skin Skin)
        {
            this.Pane = Skin.GetStretchableSurface(new SkinArea(16, 16, 16, 16));
        }

        public static readonly PaneStyle Default = new PaneStyle(Skin.Default);

        public Surface Pane;
        public double BorderSize = 6.0;
    }
}
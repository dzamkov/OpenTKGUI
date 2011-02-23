using System;
using System.Collections.Generic;

namespace OpenTKGUI
{
    /// <summary>
    /// A container that shows only a small portion of a larger client control at a time.
    /// </summary>
    public class WindowContainer : Control
    {
        public WindowContainer(Control Client)
        {
            this._Client = Client;
        }

        /// <summary>
        /// Gets the client control.
        /// </summary>
        public Control Client
        {
            get
            {
                return this._Client;
            }
        }

        /// <summary>
        /// Gets or sets the point on the client that the top-left corner of the window container is currently showing.
        /// </summary>
        public Point Offset
        {
            get
            {
                return this._Offset;
            }
            set
            {
                this._Offset = value;
            }
        }

        /// <summary>
        /// Gets or sets the full size of the client.
        /// </summary>
        public Point FullSize
        {
            get
            {
                return this._Client.Size;
            }
            set
            {
                this.ResizeChild(this._Client, value);
            }
        }

        public override void Render(RenderContext Context)
        {
            using (Context.Clip(new Rectangle(this._Size)))
            using (Context.Translate(-new Point(Math.Round(this._Offset.X), Math.Round(this._Offset.Y))))
            {
                this._Client.Render(Context);
            }
        }

        public override void Update(InputContext Context)
        {
            using (Context.Stencil)
            using (Context.Translate(-this._Offset))
            {
                Context.StencilClip(new Rectangle(this.Size));
                this._Client.Update(Context);
            }
        }

        protected override void OnDispose()
        {
            this._Client.Dispose();
        }

        private Control _Client;
        private Point _Offset;
    }
}
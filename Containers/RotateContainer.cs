using System;
using System.Collections.Generic;

namespace OpenTKGUI
{
    /// <summary>
    /// A control that rotates its contents by increments of 90 degrees.
    /// </summary>
    public class RotateContainer : Control
    {
        public RotateContainer(Control Client, Rotation Rotation)
        {
            this._Client = Client;
            this._Rotation = Rotation;
        }

        /// <summary>
        /// Gets the client in this rotation control.
        /// </summary>
        public Control Client
        {
            get
            {
                return this._Client;
            }
        }

        public override void Render(RenderContext Context)
        {
            using (Context.Translate(this._Size * 0.5 - this._Client.Size * 0.5))
            using (Context.Rotate(this._Client.Size * 0.5, this._Rotation))
            {
                this._Client.Render(Context);
            }
        }

        public override void Update(InputContext Context)
        {
            this._Client.Update(Context);
        }

        protected override void OnResize(Point Size)
        {
            this.ResizeChild(this._Client, Size.SwapIf((int)this._Rotation % 2 == 1));
        }

        protected override void OnDispose()
        {
            this._Client.Dispose();
        }

        private Control _Client;
        private Rotation _Rotation;
    }
}
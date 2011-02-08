using System;
using System.Collections.Generic;

namespace OpenTKGUI
{
    /// <summary>
    /// A container of a control that can be replaced in place.
    /// </summary>
    public class VariableContainer : Control
    {
        public VariableContainer()
        {

        }

        public VariableContainer(Control Client)
        {
            this._Client = Client;
        }

        /// <summary>
        /// Gets or sets the client control. Note that when setting, the previous control will be disposed.
        /// </summary>
        public Control Client
        {
            get
            {
                return this._Client;
            }
            set
            {
                if (this._Client != value)
                {
                    if (this._Client != null)
                    {
                        this._Client.Dispose();
                    }
                    this._Client = value;
                    this.ResizeChild(this._Client, this.Size);
                }
            }
        }

        public override void Render(GUIRenderContext Context)
        {
            if (this._Client != null)
            {
                this._Client.Render(Context);
            }
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            if (this._Client != null)
            {
                this._Client.Update(Context.CreateChildContext(this._Client, new Point(0.0, 0.0)), Time);
            }
        }

        protected override void OnResize(Point Size)
        {
            this.ResizeChild(this._Client, this.Size);
        }

        protected override void OnDispose()
        {
            if (this._Client != null)
            {
                this._Client.Dispose();
            }
        }

        private Control _Client;
    }
}
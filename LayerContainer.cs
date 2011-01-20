using System;
using System.Collections.Generic;

namespace OpenTKGUI
{
    /// <summary>
    /// A container that allows hovering controls (such as context menus or forms) to be placed over a
    /// background control.
    /// </summary>
    public class LayerContainer : Control
    {
        public LayerContainer()
        {
            this._LayerControls = new LinkedList<LayerControl>();
        }

        public LayerContainer(Control Background)
            : this()
        {
            this._Background = Background;
        }

        /// <summary>
        /// Adds a hovering layer control to the top of the layer container (above all other controls). The added control must not
        /// be used by any other layer containers.
        /// </summary>
        public void AddControl(LayerControl Control, Point Position)
        {
            if (Control._Container == null)
            {
                Control._Container = this;
                Control._Position = Position;
                this._LayerControls.AddLast(Control);
            }
        }

        public override void Render(GUIRenderContext Context)
        {
            if (this._Background != null)
            {
                this._Background.Render(Context);
            }
            foreach (LayerControl lc in this._LayerControls)
            {
                Context.PushTranslate(lc._Position);
                lc.Render(Context);
                Context.Pop();
            }
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            MouseState ms = Context.MouseState;
            Point? mousepos = ms != null ? (Point?)ms.Position : null;
            foreach (LayerControl lc in this._LayerControls)
            {
                lc.Update(Context.CreateChildContext(lc, lc._Position, mousepos == null), Time);
                if (mousepos != null)
                {
                    if (new Rectangle(lc._Position, lc.Size).In(mousepos.Value))
                    {
                        mousepos = null;
                    }
                }
            }
            if (this._Background != null)
            {
                this._Background.Update(Context.CreateChildContext(this._Background, new Point(), mousepos == null), Time);
            }
        }

        protected override void OnResize(Point OldSize, Point NewSize)
        {
            if (this._Background != null)
            {
                this._Background.Resize(NewSize);
            }
        }

        /// <summary>
        /// Gets or sets the background control. The background control will always be behind all visible hovering controls.
        /// </summary>
        public Control Background
        {
            get
            {
                return this._Background;
            }
            set
            {
                this._Background = value;
                this._Background.Resize(this.Size);
            }
        }

        private Control _Background;
        private LinkedList<LayerControl> _LayerControls;
    }

    /// <summary>
    /// A hovering control showing in the front layer of a layer container.
    /// </summary>
    public class LayerControl : Control
    {
        /// <summary>
        /// Gets the layer container this hovering control is in.
        /// </summary>
        public LayerContainer Container
        {
            get
            {
                return this._Container;
            }
        }

        /// <summary>
        /// Gets or sets the position of the layer control within its container.
        /// </summary>
        public Point Position
        {
            get
            {
                return this._Position;
            }
            set
            {
                this._Position = value;
            }
        }

        internal LayerContainer _Container;
        internal Point _Position;
    }
}
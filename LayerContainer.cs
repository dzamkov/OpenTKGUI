using System;
using System.Collections.Generic;

using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// A container that allows hovering controls (such as context menus or forms) to be placed over a
    /// background control.
    /// </summary>
    public class LayerContainer : Control
    {
        public LayerContainer()
            : this(null)
        {
            
        }

        public LayerContainer(Control Background)
            : this(new LayerContainerStyle(), Background)
        {

        }

        public LayerContainer(LayerContainerStyle Style, Control Background)
        {
            this._Style = Style;
            this._LayerControls = new LinkedList<LayerControl>();
            this._Background = Background;
        }

        /// <summary>
        /// Gets or sets the current modal mode. If set to null, there is no modal control.
        /// </summary>
        public ModalOptions Modal
        {
            get
            {
                return this._ModalOptions;
            }
            set
            {
                this._ModalOptions = value;
            }
        }

        /// <summary>
        /// Gets the style for the layer container.
        /// </summary>
        public LayerContainerStyle Style
        {
            get
            {
                return this._Style;
            }
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

        /// <summary>
        /// Removes a hovering layer control from this container.
        /// </summary>
        public void RemoveControl(LayerControl Control)
        {
            if (Control._Container == this)
            {
                Control._Container = null;
                this._LayerControls.Remove(Control);
            }
        }

        /// <summary>
        /// Brings the specified control to the top level of the layer container (above all other controls).
        /// </summary>
        public void BringToTop(LayerControl Control)
        {
            this._LayerControls.Remove(Control);
            this._LayerControls.AddLast(Control);
        }

        public override void Render(GUIRenderContext Context)
        {
            if (this._Background != null)
            {
                this._Background.Render(Context);
            }
            bool lightbox = this._ModalOptions != null && this._ModalOptions.Lightbox;
            foreach (LayerControl lc in this._LayerControls)
            {
                if (lightbox && this._ModalOptions.LowestModal == lc)
                {
                    this._DrawLightbox(Context);
                    lightbox = false;
                }
                lc.RenderShadow(lc._Position, Context);
                Context.PushTranslate(lc._Position);
                lc.Render(Context);
                Context.Pop();
            }
            if (this._ModalOptions == null && this._LightboxTime > 0.0)
            {
                this._DrawLightbox(Context);
            }
        }

        private void _DrawLightbox(GUIRenderContext Context)
        {
            Color c = this._Style.LightBoxColor;
            c.A *= this._LightboxTime / this._Style.LightBoxFadeTime;
            Context.DrawSolid(c, new Rectangle(this.Size));
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            MouseState ms = Context.MouseState;
            Point? mousepos = ms != null ? (Point?)ms.Position : null;
            LinkedList<LayerControl> oldlayercontrols = this._LayerControls;
            this._LayerControls = new LinkedList<LayerControl>(this._LayerControls);
            ModalOptions mo = this._ModalOptions;

            // Go through control in reverse order (since the last control is the top-most).
            LinkedListNode<LayerControl> cur = oldlayercontrols.Last;
            while (cur != null) 
            {
                LayerControl lc = cur.Value;

                // Standard updating procedure
                lc.Update(Context.CreateChildContext(lc, lc._Position, mousepos == null), Time);
                if (mousepos != null)
                {
                    // If the mouse is over a hover control, do not let it fall through to lower controls.
                    if (new Rectangle(lc._Position, lc.Size).In(mousepos.Value))
                    {
                        mousepos = null;
                    }
                }

                // Handle modal options
                if (mo != null)
                {
                    if (mo.LowestModal == lc)
                    {
                        // Background click?
                        if (mousepos != null && ms.IsButtonDown(MouseButton.Left))
                        {
                            mo._BackgroundClick();
                        }

                        // Mouse blocked?
                        if (!mo.MouseFallthrough)
                        {
                            mousepos = null;
                        }
                    }
                }

                cur = cur.Previous;
            }


            if (this._Background != null)
            {
                this._Background.Update(Context.CreateChildContext(this._Background, new Point(), mousepos == null), Time);
            }

            // Update lightbox (really low priority).
            if (this._ModalOptions != null && this._ModalOptions.Lightbox)
            {
                this._LightboxTime = Math.Min(this._Style.LightBoxFadeTime, this._LightboxTime + Time);
            }
            else
            {
                this._LightboxTime = Math.Max(0.0, this._LightboxTime - Time);
            }
        }

        protected override void OnResize(Point Size)
        {
            if (this._Background != null)
            {
                this.ResizeChild(this._Background, Size);
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
                this.ResizeChild(this._Background, this.Size);
            }
        }

        protected override void OnDispose()
        {
            if (this._Background != null)
            {
                this._Background.Dispose();
            }
            foreach (LayerControl lc in this._LayerControls)
            {
                lc.Dispose();
            }
        }

        private LayerContainerStyle _Style;
        private double _LightboxTime;
        private Control _Background;
        private LinkedList<LayerControl> _LayerControls;
        private ModalOptions _ModalOptions;
    }

    /// <summary>
    /// Options given to a layer container when creating a modal mode (where higher level hovering controls are brought to the attention of the user).
    /// </summary>
    public class ModalOptions
    {
        /// <summary>
        /// The lowest (or furthest to the back) control that is modal.
        /// </summary>
        public LayerControl LowestModal;

        /// <summary>
        /// Determines wether controls behind the modal hovering controls can still receive mouse input.
        /// </summary>
        public bool MouseFallthrough;

        /// <summary>
        /// Determines wether the space behind the modal hovering controls is darkened.
        /// </summary>
        public bool Lightbox;

        /// <summary>
        /// An event fired when the background area (behind the modal controls) is clicked.
        /// </summary>
        public event ClickHandler BackgroundClick;

        internal void _BackgroundClick()
        {
            if (this.BackgroundClick != null)
            {
                this.BackgroundClick.Invoke();
            }
        }
    }

    /// <summary>
    /// A hovering control showing in the front layer of a layer container.
    /// </summary>
    public class LayerControl : Control
    {
        /// <summary>
        /// Gets or sets the size of the layer control. This size is independant of any other control.
        /// </summary>
        public new Point Size
        {
            get
            {
                return base.Size;
            }
            protected set
            {
                base.ForceResize(value);
            }
        }

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

        /// <summary>
        /// Renders a shadow for the hovering control. The context given will not be translated or clipped from
        /// the layer container.
        /// </summary>
        public virtual void RenderShadow(Point Position, GUIRenderContext Context)
        {
            ShadowStyle ss = this._Container.Style.DefaultShadowStyle;
            double width = ss.Width;
            Context.DrawSurface(ss.Skin.GetSurface(ss.Image, this.Size + new Point(width * 2.0, width * 2.0)), Position - new Point(width, width));
        }

        internal LayerContainer _Container;
        internal Point _Position;
    }

    /// <summary>
    /// Gives styling options for a layer container.
    /// </summary>
    public class LayerContainerStyle
    {
        public ShadowStyle DefaultShadowStyle = new ShadowStyle();
        public Color LightBoxColor = Color.RGBA(0.9, 0.9, 0.9, 0.6);
        public double LightBoxFadeTime = 0.3;
    }

    /// <summary>
    /// Gives styling options for the default shadow on hovering controls.
    /// </summary>
    public class ShadowStyle
    {
        public Skin Skin = Skin.Default;
        public SkinArea Image = new SkinArea(112, 32, 16, 16);
        public double Width = 6.0;
    }
}
using System;
using System.Collections.Generic;

using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// A hovering control that shows possible options in vertically descending order. (A context menu). Note that the popups created
    /// directly by the constructor will not be immediately active. Use the static call method instead to create and display a popup properly.
    /// </summary>
    public class Popup : LayerControl
    {
        public Popup(IEnumerable<MenuItem> Items)
            : this(new PopupStyle(), Items)
        {
        }

        public Popup(PopupStyle Style, IEnumerable<MenuItem> Items)
        {
            this._Style = Style;
            double y = 0.0;
            double width = 0.0;
            this._Items = new List<_Item>();
            foreach (MenuItem mi in Items)
            {
                _Item i = new _Item(Style, mi, y);
                Point size = i.Size;
                y += size.Y;
                width = Math.Max(width, size.X);
                this._Items.Add(i);
            }
            this.Size = new Point(width, y) + new Point(this._Style.Margin, this._Style.Margin) * 2.0;
        }

        /// <summary>
        /// Calls up a precreated popup in the given container at the offset (which becomes the topleft point).
        /// </summary>
        public static void Call(LayerContainer Container, Point Offset, Popup Popup)
        {
            Point size = Popup.Size;
            Offset.X = Math.Min(Offset.X, Container.Size.X - size.X);
            Offset.Y = Math.Min(Offset.Y, Container.Size.Y - size.Y);
            Container.AddControl(Popup, Offset);
            ModalOptions mo = new ModalOptions()
            {
                MouseFallthrough = true,
                Lightbox = true,
                LowestModal = Popup
            };
            mo.BackgroundClick += delegate
            {
                Popup.Dismiss();
            };
            Container.Modal = mo;
        }

        /// <summary>
        /// Calls up a popup in the container at the offset (which becomes the topleft point). 
        /// </summary>
        public static Popup Call(LayerContainer Container, Point Offset, IEnumerable<MenuItem> Items, PopupStyle Style)
        {
            Popup p = new Popup(Style, Items);
            Call(Container, Offset, p);
            return p;
        }

        /// <summary>
        /// Calls up a popup in the container at the offset, using the default popup style.
        /// </summary>
        public static Popup Call(LayerContainer Container, Point Offset, IEnumerable<MenuItem> Items)
        {
            return Call(Container, Offset, Items, new PopupStyle());
        }

        public override void Render(GUIRenderContext Context)
        {
            PopupStyle style = this._Style;
            Skin s = style.Skin;

            Context.DrawSurface(s.GetSurface(style.Back, this.Size));
            Rectangle inner = new Rectangle(this.Size).Margin(this._Style.Margin);
            
            foreach (_Item i in this._Items)
            {
                if (this._Active == i)
                {
                    Context.DrawSurface(s.GetSurface(style.ActiveItem, new Point(this.Size.X, i.Size.Y)), new Point(0.0, i.Y + inner.Location.Y));
                }
                i.Render(Context, style, new Rectangle(0.0, i.Y, inner.Size.X, i.Size.Y) + inner.Location);
            }
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            MouseState ms = Context.MouseState;
            if (ms != null)
            {
                Rectangle inner = new Rectangle(this.Size).Margin(this._Style.Margin);
                foreach (_Item i in this._Items)
                {
                    Rectangle irect = new Rectangle(0.0, i.Y + inner.Location.Y, this.Size.X, i.Size.Y);
                    if (irect.In(ms.Position))
                    {
                        this._Active = i;
                    }
                }
            }
            else
            {
                this._Active = null;
            }
        }

        /// <summary>
        /// Dismisses the popup, deallocating all resources it uses and removing it form its parent container.
        /// </summary>
        public void Dismiss()
        {
            LayerContainer container = this.Container;
            if (container != null)
            {
                container.Modal = null;
                container.RemoveControl(this);
            }
        }

        /// <summary>
        /// Information about a item.
        /// </summary>
        private class _Item
        {
            public _Item(PopupStyle Style, MenuItem Source, double Y)
            {
                this.Source = Source;
                this.Y = Y;

                TextMenuItem tmi = Source as TextMenuItem;
                if (tmi != null)
                {
                    this.Sample = Style.Font.CreateSample(tmi.Text, null, TextAlign.Left, TextAlign.Center, TextWrap.Ellipsis);
                }

                CommandMenuItem cmi = Source as CommandMenuItem;
                if (cmi != null)
                {
                    this.Size = new Point(this.Sample.Size.X, Style.StandardItemHeight);
                }
            }

            /// <summary>
            /// The menuitem associated with this item.
            /// </summary>
            public MenuItem Source;

            /// <summary>
            /// The location of the top of the item not accounting for padding.
            /// </summary>
            public double Y;

            /// <summary>
            /// The size of the item.
            /// </summary>
            public Point Size;

            /// <summary>
            /// The text sample, if any, associated with this item.
            /// </summary>
            public TextSample Sample;

            /// <summary>
            /// Renders the item to the specified location.
            /// </summary>
            public void Render(GUIRenderContext Context, PopupStyle Style, Rectangle Area)
            {
                CommandMenuItem cmi = Source as CommandMenuItem;
                if (cmi != null)
                {
                    Context.DrawText(Style.TextColor, this.Sample, Area);
                }
            }
        }

        private _Item _Active;
        private PopupStyle _Style;
        private List<_Item> _Items;
    }

    /// <summary>
    /// Gives styling options for a popup.
    /// </summary>
    public class PopupStyle
    {
        public Skin Skin = Skin.Default;
        public SkinArea Back = new SkinArea(96, 80, 16, 16);
        public SkinArea ActiveItem = new SkinArea(112, 80, 16, 16);
        public Font Font = Font.Default;
        public Color TextColor = Color.RGB(0.0, 0.0, 0.0);
        public double Margin = 3.0;
        public double StandardItemHeight = 30.0;
    }

    /// <summary>
    /// A container that allows a child control to display popups, either actively by a command, or passively by right click.
    /// </summary>
    public class PopupContainer : SingleContainer
    {
        public PopupContainer(Control Client)
            : base(Client)
        {
            this._Style = new PopupStyle();
        }

        /// <summary>
        /// Gets or sets the style future popups will be created with.
        /// </summary>
        public PopupStyle Style
        {
            get
            {
                return this._Style;
            }
            set
            {
                this._Style = value;
            }
        }

        /// <summary>
        /// Gets or sets the items future popups will contain.
        /// </summary>
        public IEnumerable<MenuItem> Items
        {
            get
            {
                return this._Items;
            }
            set
            {
                this._Items = value;
            }
        }

        /// <summary>
        /// Calls the popup described in the container at the given offset from the container.
        /// </summary>
        public void Call(Point Offset)
        {
            this._Callpoint = Offset;
        }

        /// <summary>
        /// Calls the popup described in the container at the current mouse position.
        /// </summary>
        public void CallAtMouse()
        {
            this._CallAtMouse = true;
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            base.Update(Context, Time);

            // Set up call on mouse
            if (this._CallAtMouse)
            {
                MouseState ms = Context.MouseState;
                if (ms != null)
                {
                    this._Callpoint = ms.Position;
                }
                this._CallAtMouse = false;
            }
            
            // Call up popup, if needed
            if (this._Callpoint != null && this._Items != null)
            {
                Point cp = this._Callpoint.Value;

                LayerContainer container;
                Point layeroffset;
                if (Context.FindAncestor<LayerContainer>(out container, out layeroffset))
                {
                    Popup.Call(container, cp - layeroffset, this._Items, this._Style);
                }

                this._Callpoint = null;
            }
        }

        private bool _CallAtMouse;
        private Point? _Callpoint;
        private IEnumerable<MenuItem> _Items;
        private PopupStyle _Style;
    }
}
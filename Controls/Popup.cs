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
            : this(PopupStyle.Default, Items)
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
                Lightbox = false,
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

        /// <summary>
        /// Sets the minimum width this popup can have.
        /// </summary>
        public double MinWidth
        {
            set
            {
                Point size = this.Size;
                if (size.X < value)
                {
                    this.Size = new Point(value, size.Y);
                }
            }
        }

        public override void Render(GUIRenderContext Context)
        {
            PopupStyle style = this._Style;

            Context.DrawSurface(style.Back, new Rectangle(this.Size));
            Rectangle inner = new Rectangle(this.Size).Margin(this._Style.Margin);
            
            foreach (_Item i in this._Items)
            {
                if (this._Active == i)
                {
                    Surface sa = this._MouseDown ? style.PushedItem : style.ActiveItem;
                    Context.DrawSurface(sa, new Rectangle(0.0, i.Y + inner.Location.Y, this.Size.X, i.Size.Y));
                }
                i.Render(Context, style, new Rectangle(0.0, i.Y, inner.Size.X, i.Size.Y) + inner.Location);
            }
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            // Mouse navigation.
            Rectangle inner = new Rectangle(this.Size).Margin(this._Style.Margin);
            MouseState ms = Context.MouseState;
            this._MouseDown = false;
            if (ms != null)
            {
                Point mousepos = ms.Position;
                foreach (_Item i in this._Items)
                {
                    Rectangle irect = this._ItemRect(inner, i);
                    if (irect.In(mousepos))
                    {
                        if (ms.IsButtonDown(MouseButton.Left))
                        {
                            this._MouseDown = true;
                        }
                        if ((mousepos - this._LastMouse).SquareLength > 1.0 || this._MouseDown)
                        {
                            if (this._Select(i))
                            {
                                this._UpdateSubmenu(irect);
                            }
                        }
                        if (ms.HasReleasedButton(MouseButton.Left))
                        {
                            this._Click();
                        }
                    }
                }
                this._LastMouse = mousepos;
            }
            

            // Keyboard navigation
            KeyboardState ks = Context.KeyboardState;
            if (ks != null && !this._MouseDown)
            {
                foreach (KeyEvent ke in ks.Events)
                {
                    _Item nitem = this._Active;
                    if (ke.Type == ButtonEventType.Down)
                    {
                        if (ke.Key == Key.Down)
                        {
                            // Navigate down
                            if (this._Active == null)
                            {
                                nitem = this._Items[0];
                            }
                            else
                            {
                                nitem = this._ItemAtOffsetIndex(this._Active, 1);
                            }
                        }
                        if (ke.Key == Key.Up)
                        {
                            // Navigate up
                            if (this._Active == null)
                            {
                                nitem = this._Items[this._Items.Count - 1];
                            }
                            else
                            {
                                nitem = this._ItemAtOffsetIndex(this._Active, -1);
                            }
                        }
                        if (ke.Key == Key.Left)
                        {
                            if (this._Parent != null)
                            {
                                this._Parent._Submenu = null;
                            }
                            this.Dismiss();
                            return;
                        }
                        if (ke.Key == Key.Right)
                        {
                            if (this._Active != null)
                            {
                                this._UpdateSubmenu(_ItemRect(inner, this._Active));
                                return;
                            }
                        }
                        if (ke.Key == Key.Enter)
                        {
                            this._Click();
                            return;
                        }
                    }
                    if (nitem != this._Active)
                    {
                        this._Select(nitem);
                    }
                }

                // Super quick number navigation
                foreach (char c in ks.Presses)
                {
                    int n = (int)c - 49;
                    if (n >= 0 && n < 9)
                    {
                        if (n < this._Items.Count)
                        {
                            _Item item = this._Items[n];
                            if (this._Select(item))
                            {
                                this._UpdateSubmenu(this._ItemRect(inner, item));
                                this._Click();
                            }
                        }
                    }
                }
            }
            if (this._Submenu == null && !Context.HasKeyboard)
            {
                Context.CaptureKeyboard();
            }

        }

        private _Item _ItemAtOffsetIndex(_Item Current, int Offset)
        {
            int curindex = 0;
            _Item cur = Current;
            for (int t = 0; t < this._Items.Count; t++)
            {
                if (this._Items[t] == Current)
                {
                    curindex = t;
                    break;
                }
            }

            while (Offset > 0)
            {
                curindex += 1;
                if (curindex >= this._Items.Count)
                {
                    curindex -= this._Items.Count;
                }
                cur = this._Items[curindex];
                if (cur.Selectable)
                {
                    Offset--;
                }
            }

            while (Offset < 0)
            {
                curindex -= 1;
                if (curindex < 0)
                {
                    curindex += this._Items.Count;
                }
                cur = this._Items[curindex];
                if (cur.Selectable)
                {
                    Offset++;
                }
            }

            return cur;
        }

        /// <summary>
        /// Gets the item rectangle for an item, given the inner rectangle.
        /// </summary>
        private Rectangle _ItemRect(Rectangle Inner, _Item Item)
        {
            return new Rectangle(0.0, Item.Y + Inner.Location.Y, this.Size.X, Item.Size.Y);
        }

        /// <summary>
        /// Sets the specified item as active. Returns if the selection is new.
        /// </summary>
        private bool _Select(_Item Item)
        {
            if (Item.Selectable && this._Active != Item)
            {
                this._Active = Item;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates a submenu, if needed
        /// </summary>
        private void _UpdateSubmenu(Rectangle ActiveItemRect)
        {
            // Remove current submenu if any
            if (this._Submenu != null)
            {
                this._Submenu.Dismiss();
                this._Submenu = null;
            }

            // Perhaps a submenu is needed?
            MenuItem source = this._Active.Source;
            CompoundMenuItem cpmi = source as CompoundMenuItem;
            if (cpmi != null)
            {
                this._CreateSubmenu(cpmi.Items, ActiveItemRect + this.Position);
            }
        }

        /// <summary>
        /// Creates a submenu with the specified items.
        /// </summary>
        private void _CreateSubmenu(IEnumerable<MenuItem> Items, Rectangle SourceRect)
        {
            PopupStyle style = this._Style;
            LayerContainer container = this.Container;
            Popup subpopup = new Popup(style, Items);
            Point size = subpopup.Size;
            Point offset = new Point(SourceRect.Location.X + SourceRect.Size.X - 1, SourceRect.Location.Y - style.Margin);
            if (offset.Y + size.Y > container.Size.Y)
            {
                offset.Y = SourceRect.Location.Y + SourceRect.Size.Y - size.Y + style.Margin;
            }
            if (offset.X + size.X > container.Size.X)
            {
                offset.X = SourceRect.Location.X - size.X + 1;
            }
            this._Submenu = subpopup;
            subpopup._Parent = this;
            container.AddControl(subpopup, offset);
        }

        /// <summary>
        /// Clicks on the active item.
        /// </summary>
        private void _Click()
        {
            MenuItem item = this._Active.Source;
            CommandMenuItem cmi = item as CommandMenuItem;
            if (cmi != null)
            {
                this._DismissFull();
                cmi._Click();
                return;
            }
        }

        /// <summary>
        /// Dismisses this popup and all ancestors.
        /// </summary>
        private void _DismissFull()
        {
            if (this._Parent == null)
            {
                this.Dismiss();
            }
            else
            {
                this._Parent._DismissFull();
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
                if (this._Parent == null)
                {
                    container.Modal = null;
                }
                container.RemoveControl(this);
            }
            if (this._Submenu != null)
            {
                this._Submenu.Dismiss();
            }
            this.Dispose();
            if (this.Dismissed != null)
            {
                this.Dismissed.Invoke();
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

                CompoundMenuItem cpmi = Source as CompoundMenuItem;
                if (cpmi != null)
                {
                    this.Size = new Point(this.Sample.Size.X + Style.CompoundArrow.Size.X, Style.StandardItemHeight);
                }

                SeperatorMenuItem smi = Source as SeperatorMenuItem;
                if (smi != null)
                {
                    this.Size = new Point(0.0, Style.SeperatorHeight + Style.SeperatorPadding * 2.0);
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
            /// Gets if this item can be selected.
            /// </summary>
            public bool Selectable
            {
                get
                {
                    return this.Source.Selectable;
                }
            }

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

                CompoundMenuItem cpmi = Source as CompoundMenuItem;
                if (cpmi != null)
                {
                    Context.DrawText(Style.TextColor, this.Sample, Area);
                    FixedSurface arrow = Style.CompoundArrow;
                    Point arrowsize = arrow.Size;
                    Context.DrawSurface(arrow, new Point(Area.Location.X + Area.Size.X - arrowsize.X, Area.Location.Y + Area.Size.Y * 0.5 - arrowsize.Y * 0.5));
                }

                SeperatorMenuItem smi = Source as SeperatorMenuItem;
                if (smi != null)
                {
                    Context.DrawSurface(Style.Seperator, new Rectangle(0.0, Style.SeperatorPadding, Area.Size.X, Style.SeperatorHeight) + Area.Location);
                }
            }
        }

        protected override void OnDispose()
        {
            foreach (_Item i in this._Items)
            {
                if (i.Sample != null)
                {
                    i.Sample.Dispose();
                }
            }
        }

        public event DismissedHandler Dismissed;

        private bool _MouseDown;
        private Point _LastMouse;
        private _Item _Active;
        private Popup _Parent;
        private Popup _Submenu;
        private PopupStyle _Style;
        private List<_Item> _Items;
    }

    /// <summary>
    /// Gives styling options for a popup.
    /// </summary>
    public class PopupStyle
    {
        public PopupStyle()
        {

        }

        public PopupStyle(Skin Skin)
        {
            this.Back = Skin.GetStretchableSurface(new SkinArea(96, 80, 16, 16));
            this.ActiveItem = Skin.GetStretchableSurface(new SkinArea(112, 80, 16, 16));
            this.PushedItem = Skin.GetStretchableSurface(new SkinArea(80, 96, 16, 16));
            this.Seperator = Skin.GetStretchableSurface(new SkinArea(96, 96, 16, 2));
            this.CompoundArrow = Skin.GetSurface(new SkinArea(64, 96, 16, 16));
        }

        public static readonly PopupStyle Default = new PopupStyle(Skin.Default);

        public Surface Back;
        public Surface ActiveItem;
        public Surface PushedItem;
        public Surface Seperator;
        public FixedSurface CompoundArrow;
        public Font Font = Font.Default;
        public Color TextColor = Color.RGB(0.0, 0.0, 0.0);
        public double Margin = 3.0;
        public double SeperatorHeight = 2.0;
        public double SeperatorPadding = 2.0;
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
            this._Style = PopupStyle.Default;
            this._ShowOnRightClick = true;
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
        /// Gets or sets wether the popup menu is shown on right click (as a context menu).
        /// </summary>
        public bool ShowOnRightClick
        {
            get
            {
                return this._ShowOnRightClick;
            }
            set
            {
                this._ShowOnRightClick = value;
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
            MouseState ms = Context.MouseState;

            // Shown on right click.
            if (this._ShowOnRightClick && ms != null && ms.HasReleasedButton(MouseButton.Right))
            {
                this._CallAtMouse = true;
            }

            // Set up call on mouse
            if (this._CallAtMouse)
            {
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
                    Popup popup = Popup.Call(container, cp - layeroffset, this._Items, this._Style);
                    if (this.PopupCreated != null)
                    {
                        this.PopupCreated.Invoke(popup);
                    }
                }

                this._Callpoint = null;
            }
        }

        public event PopupCreatedHandler PopupCreated;

        private bool _ShowOnRightClick;
        private bool _CallAtMouse;
        private Popup _Current;
        private Point? _Callpoint;
        private IEnumerable<MenuItem> _Items;
        private PopupStyle _Style;
    }
}
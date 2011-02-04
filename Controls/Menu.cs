using System;
using System.Collections.Generic;

using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// A control that arranges menu items horizontally and optionally allows popups for compound menu items (requires the menu to
    /// be embedded directly or indirectly in a layer container).
    /// </summary>
    public class Menu : Control
    {
        public Menu(IEnumerable<MenuItem> Items)
            : this(MenuStyle.Default, Items)
        {

        }

        public Menu(MenuStyle Style, IEnumerable<MenuItem> Items)
        {
            this._Style = Style;
            this._Items = new List<_Item>();
            double x = Style.ItemMargin;
            foreach (MenuItem item in Items)
            {
                _Item i = new _Item(Style, item, x);
                x += i.Width;
                x += Style.ItemSeperation;
                this._Items.Add(i);
            }
        }

        public override void Render(GUIRenderContext Context)
        {
            MenuStyle style = this._Style;
            Context.DrawSurface(style.Backing, new Rectangle(this.Size));
            double height = this.Size.Y;
            foreach (_Item i in this._Items)
            {
                Rectangle irect = new Rectangle(i.X, 0.0, i.Width, height);
                if (this._Selected == i)
                {
                    if (this._MouseDown)
                    {
                        Context.DrawSurface(style.Pushed, irect);
                    }
                    else
                    {
                        Context.DrawSurface(style.Active, irect);
                    }
                }
                i.Render(Context, style, irect);
            }
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            MouseState ms = Context.MouseState;
            if (ms != null)
            {
                this._MouseDown = ms.IsButtonDown(MouseButton.Left);

                double height = this.Size.Y;
                bool mouseclick = ms.HasReleasedButton(MouseButton.Left);
                foreach (_Item i in this._Items)
                {
                    Rectangle irect = new Rectangle(i.X, 0.0, i.Width, height);
                    if (i.Item.Selectable && irect.In(ms.Position))
                    {
                        this._Select(irect, i, Context);
                        if (mouseclick)
                        {
                            this._Click(irect, i, Context);
                        }
                    }
                }
            }
            else
            {
                if (this._CurPopup == null)
                {
                    this._Selected = null;
                }
            }
        }

        protected override void OnDispose()
        {

        }

        /// <summary>
        /// Called when an item is clicked.
        /// </summary>
        private void _Click(Rectangle ItemRect, _Item Item, GUIControlContext Context)
        {
            if (this._Selected != null)
            {
                MenuItem mi = Item.Item;

                CommandMenuItem cmi = mi as CommandMenuItem;
                if (cmi != null)
                {
                    cmi._Click();
                }

                CompoundMenuItem cpmi = mi as CompoundMenuItem;
                if (cpmi != null)
                {
                    // Oh no, now we have to make a popup.
                    this._MakePopup(ItemRect, cpmi.Items, Context);
                }
            }
        }

        /// <summary>
        /// Selects an item.
        /// </summary>
        private void _Select(Rectangle ItemRect, _Item Item, GUIControlContext Context)
        {
            if (Item != this._Selected)
            {
                this._Selected = Item;
                if (this._CurPopup != null)
                {
                    this._CurPopup.Dismiss();

                    // We're in popup mode, apparently. So we can popup other compound items when they are moused over
                    CompoundMenuItem cpmi = Item.Item as CompoundMenuItem;
                    if (cpmi != null)
                    {
                        this._MakePopup(ItemRect, cpmi.Items, Context);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a popup for an item.
        /// </summary>
        private void _MakePopup(Rectangle ItemRect, IEnumerable<MenuItem> Items, GUIControlContext Context)
        {
            LayerContainer container;
            Point offset;
            if (Context.FindAncestor<LayerContainer>(out container, out offset))
            {
                offset = new Point(ItemRect.Location.X, ItemRect.Location.Y + ItemRect.Size.Y) - offset;
                Popup popup = Popup.Call(container, offset, Items, this._Style.PopupStyle);
                popup.MinWidth = ItemRect.Size.X;
                this._CurPopup = popup;
                popup.Dismissed += delegate
                {
                    if (this._CurPopup == popup)
                    {
                        this._CurPopup = null;
                    }
                };
            }
        }

        /// <summary>
        /// An item in the menu.
        /// </summary>
        private class _Item
        {
            public _Item(MenuStyle Style, MenuItem Item, double X)
            {
                this.X = X;
                this.Item = Item;

                TextMenuItem tmi = Item as TextMenuItem;
                if (tmi != null)
                {
                    this.Sample = Style.TextFont.CreateSample(tmi.Text, null, TextAlign.Center, TextAlign.Center, TextWrap.Ellipsis);
                    this.Width = this.Sample.Size.X + Style.TextMargin * 2.0;
                }
            }

            /// <summary>
            /// The x location of the item in relation to the menu.
            /// </summary>
            public double X;

            /// <summary>
            /// The width of the item.
            /// </summary>
            public double Width;

            /// <summary>
            /// The text sample, if any, for the item.
            /// </summary>
            public TextSample Sample;

            /// <summary>
            /// The item.
            /// </summary>
            public MenuItem Item;

            /// <summary>
            /// Renders the contents of the item to the specified rectangle.
            /// </summary>
            public void Render(GUIRenderContext Context, MenuStyle Style, Rectangle Area)
            {
                if (this.Sample != null)
                {
                    Context.DrawText(Style.TextColor, this.Sample, 
                        new Rectangle(
                            Area.Location.X + Style.TextMargin, Area.Location.Y, 
                            Area.Size.X - Style.TextMargin * 2.0, Area.Size.Y));
                }
            }
        }

        private bool _MouseDown;
        private Popup _CurPopup;
        private _Item _Selected;
        private List<_Item> _Items;
        private MenuStyle _Style;
    }

    /// <summary>
    /// Gives styling options for a menu.
    /// </summary>
    public class MenuStyle
    {
        public MenuStyle()
        {

        }

        public MenuStyle(Skin Skin)
        {
            this.Backing = Skin.GetStretchableSurface(new SkinArea(64, 0, 16, 16));
            this.Active = Skin.GetStretchableSurface(new SkinArea(80, 0, 16, 16));
            this.Pushed = Skin.GetStretchableSurface(new SkinArea(96, 0, 16, 16));
            this.PopupStyle = new PopupStyle(Skin);
        }

        public static readonly MenuStyle Default = new MenuStyle(Skin.Default);

        public Surface Backing;
        public Surface Active;
        public Surface Pushed;
        public double ItemMargin = 20.0;
        public double ItemSeperation = 5.0;
        public double TextMargin = 3.0;
        public Font TextFont = Font.Default;
        public Color TextColor = Color.RGB(0.0, 0.0, 0.0);
        public PopupStyle PopupStyle;
    }


    /// <summary>
    /// An item on a menu or popup.
    /// </summary>
    public abstract class MenuItem
    {
        public MenuItem()
        {
            
        }

        /// <summary>
        /// Gets if the menu item can be selected.
        /// </summary>
        public bool Selectable
        {
            get
            {
                return !(this is SeperatorMenuItem);
            }
        }

        /// <summary>
        /// Creates a command menu item.
        /// </summary>
        public static CommandMenuItem Create(string Text, ClickHandler OnClick)
        {
            return new CommandMenuItem(Text, OnClick);
        }

        /// <summary>
        /// Creates a compound menu item.
        /// </summary>
        public static CompoundMenuItem Create(string Text, IEnumerable<MenuItem> Items)
        {
            return new CompoundMenuItem(Text, Items);
        }

        /// <summary>
        /// Creates a command menu item that doesn't do anything.
        /// </summary>
        public static CommandMenuItem Create(string Text)
        {
            return new CommandMenuItem(Text, null);
        }

        /// <summary>
        /// Gets a seperator menu item.
        /// </summary>
        public static SeperatorMenuItem Seperator
        {
            get
            {
                return SeperatorMenuItem.Singleton;
            }
        }
    }

    /// <summary>
    /// A menu item that displays text.
    /// </summary>
    public abstract class TextMenuItem : MenuItem
    {
        public TextMenuItem(string Text)
        {
            this._Text = Text;
        }

        /// <summary>
        /// Gets the text for this menu item.
        /// </summary>
        public string Text
        {
            get
            {
                return this._Text;
            }
        }

        private string _Text;
    }

    /// <summary>
    /// A menu item that calls an event on click.
    /// </summary>
    public class CommandMenuItem : TextMenuItem
    {
        public CommandMenuItem(string Text) : base(Text)
        {

        }

        public CommandMenuItem(string Text, ClickHandler OnClick)
            : this(Text)
        {
            this.Click += OnClick;
        }

        internal void _Click()
        {
            if (this.Click != null)
            {
                this.Click.Invoke();
            }
        }

        public event ClickHandler Click;
    }

    /// <summary>
    /// A menu item that when selected, opens a submenu with a new set of items.
    /// </summary>
    public class CompoundMenuItem : TextMenuItem
    {
        public CompoundMenuItem(string Text)
            : this(Text, null)
        {

        }

        public CompoundMenuItem(string Text, IEnumerable<MenuItem> Items)
            : base(Text)
        {
            this._Items = Items;
        }

        /// <summary>
        /// Gets or sets the subordinate items in the compound menu item.
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

        private IEnumerable<MenuItem> _Items;
    }

    /// <summary>
    /// A menu item that seperates items before and after it. Can be used to group together logically related items.
    /// </summary>
    public class SeperatorMenuItem : MenuItem
    {
        private SeperatorMenuItem()
        {

        }

        /// <summary>
        /// The only instance of this menu item, since it contains no data anyway.
        /// </summary>
        public static readonly SeperatorMenuItem Singleton = new SeperatorMenuItem();
    }
}
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
            : this(new MenuStyle(), Items)
        {

        }

        public Menu(MenuStyle Style, IEnumerable<MenuItem> Items)
        {
            this._Style = Style;
            this._Flow = new FlowContainer(this._Style.ButtonSeperation, Axis.Horizontal);

            // Add buttons
            foreach (MenuItem mi in Items)
            {
                CommandMenuItem cmi = mi as CommandMenuItem;
                if (cmi != null)
                {
                    Button button; double size;
                    this._MakeButton(cmi.Text, out button, out size);
                    button.Click += delegate { cmi._Click(); };
                    this._Flow.AddChild(button, size);
                }

                CompoundMenuItem cpmi = mi as CompoundMenuItem;
                if (cpmi != null)
                {
                    Button button; double size;
                    IEnumerable<MenuItem> subitems = cpmi.Items;
                    this._MakeButton(cpmi.Text, out button, out size);
                    PopupContainer pc = new PopupContainer(button);
                    pc.ShowOnRightClick = true;
                    pc.Style = this._Style.PopupStyle;
                    pc.Items = subitems;
                    button.Click += delegate 
                    {
                        pc.Call(new Point(0.0, pc.Size.Y - 1.0));
                    };
                    this._Flow.AddChild(pc, size);
                }
            }
        }

        /// <summary>
        /// Creates a button in the style of the menu.
        /// </summary>
        private void _MakeButton(string Text, out Button Button, out double Size)
        {
            ButtonStyle bs = this._Style.ButtonStyle;
            Label lbl = new Label(Text, bs.TextColor, bs.TextStyle);
            Button = new Button(bs, lbl);
            Size = Button.GetFullSize(lbl.SuggestSize).X;
        }

        public override void Render(GUIRenderContext Context)
        {
            Context.DrawSurface(this._Style.Skin.GetSurface(this._Style.Backing, this.Size), new Point(0.0, 0.0));
            Context.PushTranslate(new Point(this._Style.ButtonLeftRightMargin, this._Style.ButtonUpDownMargin));
            this._Flow.Render(Context);
            Context.Pop();
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            this._Flow.Update(Context.CreateChildContext(this._Flow, new Point(this._Style.ButtonLeftRightMargin, this._Style.ButtonUpDownMargin)), Time);
        }

        protected override void OnResize(Point Size)
        {
            this.ResizeChild(this._Flow, Size - new Point(this._Style.ButtonLeftRightMargin, this._Style.ButtonUpDownMargin) * 2.0);
        }

        protected override void OnDispose()
        {
            this._Flow.Dispose();
        }

        private MenuStyle _Style;
        private FlowContainer _Flow;
    }

    /// <summary>
    /// Gives styling options for a menu.
    /// </summary>
    public class MenuStyle
    {
        public Skin Skin = Skin.Default;
        public SkinArea Backing = new SkinArea(64, 0, 16, 16);
        public double ButtonSeperation = 5.0;
        public double ButtonLeftRightMargin = 10.0;
        public double ButtonUpDownMargin = 4.0;
        public PopupStyle PopupStyle = new PopupStyle();
        public ButtonStyle ButtonStyle = new ButtonStyle()
        {
            Normal = new SkinArea(112, 96, 16, 16),
            Active = new SkinArea(112, 80, 16, 16),
            Pushed = new SkinArea(80, 96, 16, 16)
        };
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
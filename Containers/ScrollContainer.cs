using System;
using System.Collections.Generic;

namespace OpenTKGUI
{
    /// <summary>
    /// A container that uses scrollbars to allow a limited area of a larger control to be shown by controlling a window container. The window container can
    /// be created automatically or specified manually. Note that the window (the window container that shows the targer content) and the view (the control in the
    /// center of the scroll container) need not be the same size, or even in the same area.
    /// </summary>
    public class ScrollContainer : Control
    {
        public ScrollContainer(Control Client)
            : this(ScrollContainerStyle.Default, Client)
        {

        }

        public ScrollContainer(WindowContainer Window, Control View)
            : this(ScrollContainerStyle.Default, Window, View)
        {

        }

        public ScrollContainer(ScrollContainerStyle Style, Control Client)
        {
            this._Style = Style;
            this._View = this._Window = new WindowContainer(Client);
        }

        public ScrollContainer(ScrollContainerStyle Style, WindowContainer Window, Control View)
        {
            this._Style = Style;
            this._View = View;
            this._Window = Window;
        }

        /// <summary>
        /// Gets or sets the client width, or null to indicate that it is fixed to the width of the scroll container and there is
        /// no horizontal scrollbar.
        /// </summary>
        public double? ClientWidth
        {
            get
            {
                return this._ClientWidth;
            }
            set
            {
                this._ClientWidth = value;
                this.OnResize(this.Size);
            }
        }

        /// <summary>
        /// Gets or sets the client height, or null to indicate that it is fixed to the height of the scroll container and there is
        /// no vertical scrollbar.
        /// </summary>
        public double? ClientHeight
        {
            get
            {
                return this._ClientHeight;
            }
            set
            {
                this._ClientHeight = value;
                this.OnResize(this.Size);
            }
        }

        /// <summary>
        /// Sets the ClientWidth and ClientHeight properties at the same time to avoid excessive size updating.
        /// </summary>
        public void SetClientSize(double? Width, double? Height)
        {
            this._ClientWidth = Width;
            this._ClientHeight = Height;
            this.OnResize(this.Size);
        }

        public override void Render(GUIRenderContext Context)
        {
            this._View.Render(Context);
            if (this._VScroll != null)
            {
                Context.PushTranslate(new Point(this.Size.X - this._VScroll.Size.X, 0.0));
                this._VScroll.Render(Context);
                Context.Pop();
            }
            if (this._HScroll != null)
            {
                Context.PushTranslate(new Point(0.0, this.Size.Y - this._HScroll.Size.Y));
                this._HScroll.Render(Context);
                Context.Pop();
            }
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            // Try scrollwheel
            if (this._VScroll != null)
            {
                MouseState ms = Context.MouseState;
                if (ms != null)
                {
                    double scrollamount = ms.Scroll;
                    if (Math.Abs(scrollamount) > 0.0)
                    {
                        this._VScroll.Value -= scrollamount / (this._ClientHeight.Value - this.Size.Y);
                    }
                }
            }


            this._View.Update(Context.CreateChildContext(this._View, new Point(0.0, 0.0)), Time);
            if (this._VScroll != null)
            {
                this._VScroll.Update(Context.CreateChildContext(this._VScroll, new Point(this.Size.X - this._VScroll.Size.X, 0.0)), Time);
            }
            if (this._HScroll != null)
            {
                this._HScroll.Update(Context.CreateChildContext(this._HScroll, new Point(0.0, this.Size.Y - this._HScroll.Size.Y)), Time);
            }
        }

        protected override void OnResize(Point Size)
        {
            double cliwidth = this.Size.X;
            double cliheight = this.Size.Y;
            double viewwidth = cliwidth;
            double viewheight = cliheight;
            bool hscroll = false;
            bool vscroll = false;
            if (this._ClientWidth != null)
            {
                cliwidth = this._ClientWidth.Value;
                viewheight -= this._Style.ScrollbarSize;
                hscroll = true;
            }
            if (this._ClientHeight != null)
            {
                cliheight = this._ClientHeight.Value;
                viewwidth -= this._Style.ScrollbarSize;
                vscroll = true;
            }
            
            this.ResizeChild(this._View, new Point(viewwidth, viewheight));
            Point winactualsize = this._Window.Size;
            if (!hscroll) cliwidth = winactualsize.X;
            if (!vscroll) cliheight = winactualsize.Y;

            this._Window.FullSize = new Point(cliwidth, cliheight);
            Point winoffset = this._Window.Offset;
            
            this._UpdateScrollbar(ref this._HScroll, Axis.Horizontal, hscroll, vscroll, viewwidth, winactualsize.X, winoffset.X, cliwidth);
            this._UpdateScrollbar(ref this._VScroll, Axis.Vertical, vscroll, hscroll, viewheight, winactualsize.Y, winoffset.Y, cliheight);
        }

        private void _UpdateScrollbar(ref Scrollbar Scrollbar, Axis Direction, bool Exists, bool OppositeExists, double ViewSize, double WinSize, double WinOffset, double CliSize)
        {
            if (Exists)
            {
                if (Scrollbar == null)
                {
                    Scrollbar = new Scrollbar(this._Style.ScrollbarStyle, Direction);
                    Scrollbar.ValueChanged += delegate(double Value)
                    {
                        this._ScrollbarSet(Direction, Value);
                    };
                }
                Point size = new Point(ViewSize, this._Style.ScrollbarSize);
                if (OppositeExists)
                {
                    size.X += 1;
                }
                size = size.SwapIf(Direction == Axis.Vertical);
                this.ResizeChild(Scrollbar, size);

                double slidersize = WinSize / CliSize;
                if (slidersize > 1.0)
                {
                    Scrollbar.Enabled = false;
                }
                else
                {
                    double d = (CliSize - WinSize);
                    Scrollbar.Enabled = true;
                    Scrollbar.SliderSize = slidersize;
                    Scrollbar.Value = WinOffset / d;
                    Scrollbar.MajorIncrement = WinSize / d;
                    Scrollbar.MinorIncrement = this._Style.MinorIncrement / d;
                }
            }
            else
            {
                if (Scrollbar != null)
                {
                    Scrollbar.Dispose();
                    Scrollbar = null;
                }
            }
        }

        private void _ScrollbarSet(Axis Direction, double Value)
        {
            double awinsize = this._Window.Size.SwapIf(Direction == Axis.Vertical).X;
            double aclisize = this._Window.FullSize.SwapIf(Direction == Axis.Vertical).X;
            Point winoffset = this._Window.Offset.SwapIf(Direction == Axis.Vertical);
            winoffset.X = (aclisize - awinsize) * Value;
            this._Window.Offset = winoffset.SwapIf(Direction == Axis.Vertical);
        }

        protected override void OnDispose()
        {
            this._View.Dispose();
        }

        private double? _ClientWidth;
        private double? _ClientHeight;
        private Scrollbar _HScroll;
        private Scrollbar _VScroll;
        private Control _View;
        private WindowContainer _Window;
        private ScrollContainerStyle _Style;
    }

    /// <summary>
    /// Gives styling options for a scroll container.
    /// </summary>
    public class ScrollContainerStyle
    {
        public ScrollContainerStyle()
        {

        }

        public ScrollContainerStyle(Skin Skin)
        {
            this.ScrollbarStyle = new ScrollbarStyle(Skin);
        }

        public static readonly ScrollContainerStyle Default = new ScrollContainerStyle(Skin.Default);

        public ScrollbarStyle ScrollbarStyle;
        public double ScrollbarSize = 26.0;
        public double MinorIncrement = 20.0;
    }
}
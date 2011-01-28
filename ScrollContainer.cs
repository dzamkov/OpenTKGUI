using System;
using System.Collections.Generic;

namespace OpenTKGUI
{
    /// <summary>
    /// A container that uses scrollbars to allow a limited area of a larger control to be shown by controlling a window container. The window container can
    /// be created automatically or specified manually.
    /// </summary>
    public class ScrollContainer : Control
    {
        public ScrollContainer(Control Client)
            : this(new ScrollContainerStyle(), Client)
        {

        }

        public ScrollContainer(ScrollContainerStyle Style, Control Client)
            : this(Style, new WindowContainer(Client))
        {

        }

        public ScrollContainer(ScrollContainerStyle Style, WindowContainer Window)
        {
            this._Style = Style;
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
                this._NeedUpdate = true;
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
                this._NeedUpdate = true;
            }
        }

        public override void Render(GUIRenderContext Context)
        {
            if (this._NeedUpdate)
            {
                this._UpdateControls();
            }

            this._Window.Render(Context);
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
            if (this._NeedUpdate)
            {
                this._UpdateControls();
            }

            this._Window.Update(Context.CreateChildContext(this._Window, new Point(0.0, 0.0)), Time);
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
            this._NeedUpdate = true;
        }

        private void _UpdateControls()
        {
            double cliwidth = this.Size.X;
            double cliheight = this.Size.Y;
            double winwidth = cliwidth;
            double winheight = cliheight;
            bool hscroll = false;
            bool vscroll = false;
            if (this._ClientWidth != null)
            {
                cliwidth = this._ClientWidth.Value;
                winheight -= this._Style.ScrollbarSize;
                hscroll = true;
            }
            if (this._ClientHeight != null)
            {
                cliheight = this._ClientHeight.Value;
                winwidth -= this._Style.ScrollbarSize;
                vscroll = true;
            }
            if (!hscroll) cliwidth = winwidth;
            if (!vscroll) cliheight = winheight;
            Point winoffset = this._Window.Offset;
            this._UpdateScrollbar(ref this._HScroll, Axis.Horizontal, hscroll, vscroll, winwidth, winoffset.X, cliwidth);
            this._UpdateScrollbar(ref this._VScroll, Axis.Vertical, vscroll, hscroll, winheight, winoffset.Y, cliheight);
            this.ResizeChild(this._Window, new Point(winwidth, winheight));
            this._Window.FullSize = new Point(cliwidth, cliheight);
            this._NeedUpdate = false;
        }

        private void _UpdateScrollbar(ref Scrollbar Scrollbar, Axis Direction, bool Exists, bool OppositeExists, double WinSize, double WinOffset, double CliSize)
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
                Point size = new Point(WinSize, this._Style.ScrollbarSize);
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
            this._Window.Dispose();
        }

        private bool _NeedUpdate;
        private double? _ClientWidth;
        private double? _ClientHeight;
        private Scrollbar _HScroll;
        private Scrollbar _VScroll;
        private WindowContainer _Window;
        private ScrollContainerStyle _Style;
    }

    /// <summary>
    /// Gives styling options for a scroll container.
    /// </summary>
    public class ScrollContainerStyle
    {
        public ScrollbarStyle ScrollbarStyle = new ScrollbarStyle();
        public double ScrollbarSize = 30.0;
        public double MinorIncrement = 10.0;
    }
}
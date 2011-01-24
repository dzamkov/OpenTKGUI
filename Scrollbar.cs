using System;
using System.Collections.Generic;

using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// An elongated control that allows a user to pick a value within a range by moving a slider.
    /// </summary>
    public class Scrollbar : Control
    {
        public Scrollbar(Axis Direction) : this(new ScrollbarStyle(), Direction)
        {
            
        }

        public Scrollbar(ScrollbarStyle Style, Axis Direction)
        {
            this._Style = Style;
            this._Direction = Direction;
            ButtonStyle tlstyle =
                new ButtonStyle() 
                {
                    Normal = Style.LeftButtonNormal,
                    Active = Style.LeftButtonActive,
                    Pushed = Style.LeftButtonPushed
                };
            ButtonStyle brstyle =
                new ButtonStyle()
                {
                    Normal = Style.RightButtonNormal,
                    Active = Style.RightButtonActive,
                    Pushed = Style.RightButtonPushed
                };
            this._TopLeftButton = new Button(tlstyle);
            this._BottomRightButton = new Button(brstyle);


            this._TopLeftButton.Click += delegate
            {
                this.Value = this._Value - this._MinorIncrement;
            };
            this._BottomRightButton.Click += delegate
            {
                this.Value = this._Value + this._MinorIncrement;
            };

            this._Value = 0.0;
            this._SliderSize = 0.1;
            this._MinorIncrement = 0.1;
            this._MajorIncrement = 0.3;
        }

        /// <summary>
        /// Gets or sets the value of the scrollbar, between 0.0 and 1.0.
        /// </summary>
        public double Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                this._Value = value;
                this._Value = Math.Max(0.0, Math.Min(1.0, this._Value));
                if (this.ValueChanged != null)
                {
                    this.ValueChanged(this._Value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the size of the slider on the scrollbar, in relation to the size of the scrollable area.
        /// </summary>
        public double SliderSize
        {
            get
            {
                return this._SliderSize;
            }
            set
            {
                this._SliderSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount added or subtracted to value when one of the small adjustment buttons is pressed.
        /// </summary>
        public double MinorIncrement
        {
            get
            {
                return this._MinorIncrement;
            }
            set
            {
                this._MinorIncrement = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount added or subtracted to value when the empty area on one side of the slider is pressed.
        /// </summary>
        public double MajorIncrement
        {
            get
            {
                return this._MajorIncrement;
            }
            set
            {
                this._MajorIncrement = value;
            }
        }


        public override void Render(GUIRenderContext Context)
        {
            Skin s = this._Style.Skin;

            double areastart;
            double areaend;
            double areasize;
            double sliderstart;
            double slidersize;
            this._GetScrollMeasurements(out areastart, out areaend, out areasize, out sliderstart, out slidersize);
            if (this._Direction == Axis.Horizontal)
            {
                Context.DrawSurface(s.GetSurface(this._Style.HorizontalBackground, new Point(areasize, this.Size.Y)), new Point(Math.Round(areastart), 0.0));
                Context.DrawSurface(s.GetSurface(this._Style.HorizontalSlider, new Point(slidersize, this.Size.Y)), new Point(Math.Round(sliderstart), 0.0));
            }


            this._TopLeftButton.Render(Context);
            Context.PushTranslate(this._BottomRightButtonOffset);
            this._BottomRightButton.Render(Context);
            Context.Pop();
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            this._TopLeftButton.Update(Context.CreateChildContext(this._TopLeftButton, new Point(0.0, 0.0)), Time);
            this._BottomRightButton.Update(Context.CreateChildContext(this._BottomRightButton, this._BottomRightButtonOffset), Time);

            // Handle mouse
            MouseState ms = Context.MouseState;
            if (ms != null)
            {
                bool oldmousedown = this._MouseDown;
                this._MouseDown = ms.IsButtonDown(MouseButton.Left);

                double areastart;
                double areaend;
                double areasize;
                double sliderstart;
                double slidersize;
                this._GetScrollMeasurements(out areastart, out areaend, out areasize, out sliderstart, out slidersize);
                double mouse = ms.Position.SwapIf(this._Direction == Axis.Vertical).X;

                // Dragging
                if (this._DragOffset != null)
                {
                    this.Value = (mouse - this._DragOffset.Value - areastart) / (areasize - slidersize);
                    if (!this._MouseDown)
                    {
                        this._DragOffset = null;
                        Context.ReleaseMouse();
                    }
                }

                // Check for press
                if (!oldmousedown && this._MouseDown)
                {
                    // See if its in one of the empty regions to either end of the scrollbar.
                    if (mouse > areastart && mouse < sliderstart)
                    {
                        this.Value = this._Value - this._MajorIncrement;
                    }
                    if (mouse < areaend && mouse > sliderstart + slidersize)
                    {
                        this.Value = this._Value + this._MajorIncrement;
                    }

                    // Perhaps the user intends to drag?
                    if (mouse > sliderstart && mouse < sliderstart + slidersize)
                    {
                        Context.CaptureMouse();
                        this._DragOffset = mouse - sliderstart;
                    }
                }
            }
        }

        private void _GetScrollMeasurements(out double AreaStart, out double AreaEnd, out double AreaSize, out double SliderStart, out double SliderSize)
        {
            AreaStart = this._Style.ButtonSize - 1;
            AreaEnd = (this.Size.SwapIf(this._Direction == Axis.Vertical).X) - this._Style.ButtonSize + 1;
            AreaSize = AreaEnd - AreaStart;
            SliderSize = AreaSize * this._SliderSize;
            SliderStart = AreaStart * (1.0 - this._Value) + (AreaEnd - SliderSize) * this._Value;
        }

        private Point _BottomRightButtonOffset
        {
            get
            {
                if (this._Direction == Axis.Horizontal)
                    return new Point(this.Size.X - this._Style.ButtonSize, 0.0);
                else
                    return new Point(0.0, this.Size.Y - this._Style.ButtonSize);
            }
        }

        private Point _ButtonSize
        {
            get
            {
                if (this._Direction == Axis.Horizontal)
                    return new Point(this._Style.ButtonSize, this.Size.Y);
                else
                    return new Point(this.Size.X, this._Style.ButtonSize);
            }
        }

        protected override void OnResize(Point Size)
        {
            Point buttonsize = this._ButtonSize;
            this._TopLeftButton.Resize(buttonsize);
            this._BottomRightButton.Resize(buttonsize);
        }

        private bool _MouseDown;
        private double? _DragOffset;
        private double _MajorIncrement;
        private double _MinorIncrement;
        private double _Value;
        private double _SliderSize;
        private Axis _Direction;
        private Button _TopLeftButton;
        private Button _BottomRightButton;
        private ScrollbarStyle _Style;
		
		public event ValueChangedHandeler ValueChanged;
    }

    /// <summary>
    /// Gives styling options for a button.
    /// </summary>
    public sealed class ScrollbarStyle
    {
        public Skin Skin = Skin.Default;
        public SkinArea HorizontalBackground = new SkinArea(0, 96, 16, 16);
        public SkinArea HorizontalSlider = new SkinArea(16, 96, 16, 16);
        public SkinArea LeftButtonNormal = new SkinArea(32, 96, 16, 16);
        public SkinArea RightButtonNormal = new SkinArea(48, 96, 16, 16);
        public SkinArea LeftButtonActive = new SkinArea(32, 112, 16, 16);
        public SkinArea RightButtonActive = new SkinArea(48, 112, 16, 16);
        public SkinArea LeftButtonPushed = new SkinArea(0, 112, 16, 16);
        public SkinArea RightButtonPushed = new SkinArea(16, 112, 16, 16);
        public double ButtonSize = 16.0;
    }
}
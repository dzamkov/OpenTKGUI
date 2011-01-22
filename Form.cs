using System;
using System.Collections.Generic;

using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// A floating, draggable control that can be used in a layer container.
    /// </summary>
    public class Form : LayerControl
    { 
        public Form(Control Client, string Text)
            : this(new FormStyle(), Client, Text)
        {

        }

        public Form(FormStyle Style, Control Client, string Text)
        {
            this._Buttons = new List<Button>();
            this._Client = Client;
            this._Style = Style;
            this._Text = Text;
        }

        /// <summary>
        /// Gets or sets the control that is inside the form.
        /// </summary>
        public Control Client
        {
            get
            {
                return this._Client;
            }
            set
            {
                this._Client = value;
                this._Client.Resize(this.ClientRectangle.Size);
            }
        }

        /// <summary>
        /// Gets the rectangle in relation to the form for the client of the form.
        /// </summary>
        public Rectangle ClientRectangle
        {
            get
            {
                double bs = this._Style.BorderSize;
                double tbs = this._Style.TitleBarSize;
                return new Rectangle(bs, tbs, this.Size.X - bs * 2.0, this.Size.Y - bs - tbs);
            }
        }

        /// <summary>
        /// Gets or sets the text in the titlebar of the form.
        /// </summary>
        public string Text
        {
            get
            {
                return this._Text;
            }
            set
            {
                this._Text = value;
                if (this._TextSample != null)
                {
                    this._TextSample.Dispose();
                    this._TextSample = null;
                }
            }
        }

        /// <summary>
        /// Adds a button the the right of the titlebar of the form. Buttons will have no text on them and will be distinguishable only by style.
        /// </summary>
        public Button AddTitlebarButton(ButtonStyle Style)
        {
            Button b = new Button(Style, "");
            b.Resize(this._Style.ButtonSize);
            this._Buttons.Add(b);
            return b;
        }

        /// <summary>
        /// Adds a close button on the titlebar of the form.
        /// </summary>
        public void AddCloseButton()
        {
            Button b = this.AddTitlebarButton(this._Style.CloseButtonStyle);
            b.Click += delegate
            {
                this.Container.RemoveControl(this);
            };
        }

        public override void Render(GUIRenderContext Context)
        {
            // Client
            Context.PushTranslate(this.ClientRectangle.Location);
            this._Client.Render(Context);
            Context.Pop();

            // Form
            Skin s = this._Style.Skin;
            Context.DrawSurface(s.GetSurface(this._Style.Form, this._Style.Form.Width / 2, this._Style.FormVerticalStretchLine, this.Size));


            // Buttons
            int i = 0;
            foreach (Point loc in this._ButtonLocations)
            {
                Context.PushTranslate(loc);
                this._Buttons[i].Render(Context);
                Context.Pop();
                i++;
            }

            // Text
            if (this._Text != null && this._Text != "")
            {
                if (this._TextSample == null)
                {
                    this._TextSample = this._Style.TitleBarFont.GetSample(this._Text);
                }
                Context.DrawText(
                    this._Style.TitleBarTextColor, 
                    this._TextSample, 
                    new Point(this._Style.TitleBarMargin, this._Style.TitleBarMidline - this._TextSample.Size.Y / 2.0));
            }
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            int i = 0;
            double lasttitlex = this.Size.X - this._Style.TitleBarMargin;
            foreach (Point loc in this._ButtonLocations)
            {
                Button button = this._Buttons[i];
                button.Update(Context.CreateChildContext(button, loc), Time);
                lasttitlex = loc.X;
                i++;
            }

            this._Client.Update(Context.CreateChildContext(this._Client, this.ClientRectangle.Location), Time);

            // Form needs to be dragged?
            MouseState ms = Context.MouseState;
            if (ms != null)
            {
                Point mousepos = ms.Position;
                if (this._FormDragOffset == null)
                {
                    if (new Rectangle(this._Style.TitleBarMargin, 0.0, lasttitlex - this._Style.TitleBarMargin, this._Style.TitleBarSize).In(mousepos))
                    {
                        if (ms.IsButtonDown(MouseButton.Left))
                        {
                            Context.CaptureMouse();
                            this._FormDragOffset = mousepos;
                        }
                    }
                }
                else
                {
                    this.Position = this.Position + mousepos - this._FormDragOffset.Value;
                    if (!ms.IsButtonDown(MouseButton.Left))
                    {
                        Context.ReleaseMouse();
                        this._FormDragOffset = null;
                    }
                }
            }
        }

        protected override void OnResize(Point OldSize, Point NewSize)
        {
            this._Client.Resize(this.ClientRectangle.Size);
        }

        private const double _TitleBarSize = 32.0;
        private const double _BorderSize = 7.0;

        /// <summary>
        /// Resizes the form so that the client has the specified size.
        /// </summary>
        public void ResizeClient(Point Size)
        {
            this.Resize(new Point(Size.X + this._Style.BorderSize * 2.0, Size.Y + this._Style.BorderSize + this._Style.TitleBarSize));
        }

        /// <summary>
        /// Gets the locations for the buttons in the form.
        /// </summary>
        private IEnumerable<Point> _ButtonLocations
        {
            get
            {
                double x = this.Size.X - this._Style.TitleBarMargin;
                double y = this._Style.TitleBarMidline - this._Style.ButtonSize.Y / 2.0;
                for (int t = 0; t < this._Buttons.Count; t++)
                {
                    x -= this._Style.ButtonSize.X;
                    yield return new Point(x, y);
                    x -= this._Style.ButtonSeperation;
                }
            }
        }

        private TextSample _TextSample;
        private string _Text;
        private Point? _FormDragOffset;
        private List<Button> _Buttons;
        private FormStyle _Style;
        private Control _Client;
    }

    /// <summary>
    /// Gives styling options for a form.
    /// </summary>
    public sealed class FormStyle
    {
        public Skin Skin = Skin.Default;
        public SkinRectangle Form = new SkinRectangle(0, 32, 64, 64);
        public int FormVerticalStretchLine = 44;
        public double TitleBarSize = 32.0;
        public double BorderSize = 7.0;
        public double TitleBarMidline = 15.0;
        public double TitleBarMargin = 7.0;
        public Font TitleBarFont = Font.Default;
        public Color TitleBarTextColor = Color.RGB(0.3, 0.3, 0.3);
        public Point ButtonSize = new Point(16.0, 16.0);
        public double ButtonSeperation = 4.0;
        public ButtonStyle CloseButtonStyle = new ButtonStyle()
        {
            Skin = Skin.Default,
            Normal = new SkinRectangle(64, 32, 16, 16),
            Active = new SkinRectangle(80, 32, 16, 16),
            Pushed = new SkinRectangle(96, 32, 16, 16)
        };
    }
}
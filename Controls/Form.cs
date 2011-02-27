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
            : this(FormStyle.Default, Client, Text)
        {

        }

        public Form(FormStyle Style, Control Client, string Text)
        {
            this._RightTitleBar = new FlowContainer(Style.TitleBarItemSeperation, Axis.Horizontal);
            this._Client = Client;
            this._Style = Style;
            this._Text = Text;

            this._TitleBarScope = new DragScope(MouseButton.Left);
        }

        /// <summary>
        /// Gets the control that is inside the form.
        /// </summary>
        public Control Client
        {
            get
            {
                return this._Client;
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
        /// Gets or sets the client size of the form, which is the size allocated to the client.
        /// </summary>
        public Point ClientSize
        {
            get
            {
                return this.ClientRectangle.Size;
            }
            set
            {
                this.Size = new Point(value.X + this._Style.BorderSize * 2.0, value.Y + this._Style.BorderSize + this._Style.TitleBarSize);
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
        /// Adds an item to the right of the titlebar of the form.
        /// </summary>
        public void AddTitlebarItem(Control Item, double Width)
        {
            this._RightTitleBar.AddChild(Item, Width);
            this.OnResize(this.Size);
        }

        /// <summary>
        /// Adds a button the the right of the titlebar of the form. Buttons will have no text on them and will be distinguishable only by style.
        /// </summary>
        public Button AddTitlebarButton(ButtonStyle Style, Control Client)
        {
            Button b = new Button(Style, Client);
            this.AddTitlebarItem(b, this._Style.TitleBarButtonWidth);
            return b;
        }

        /// <summary>
        /// Makes the form go away and be disposed.
        /// </summary>
        public void Dismiss()
        {
            this.Container.RemoveControl(this);
            this.Dispose();
        }

        /// <summary>
        /// Adds a close button on the titlebar of the form.
        /// </summary>
        public void AddCloseButton()
        {
            Button b = this.AddTitlebarButton(this._Style.CloseButtonStyle, null);
            b.Click += delegate
            {
                this.Container.RemoveControl(this);
            };
        }

        public override void Render(RenderContext Context)
        {
            Rectangle clirect = this.ClientRectangle;

            // Back
            Context.DrawSolid(this._Style.BackColor, clirect);

            // Client
            using (Context.Translate(clirect.Location))
            {
                this._Client.Render(Context);
            }

            // Form
            FormStyle style = this._Style;
            Context.DrawSurface(style.Pane, new Rectangle(new Point(0.0, style.TitleBarSize - style.BorderSize), this.Size - new Point(0.0, style.TitleBarSize - style.BorderSize)));
            Context.DrawSurface(style.TitleBar, new Rectangle(0.0, 0.0, this.Size.X, style.TitleBarSize));

            // Right of the title bar.
            using (Context.Translate(this._RightTitleBarOffset))
            {
                this._RightTitleBar.Render(Context);
            }

            // Text
            if (this._Text != null && this._Text != "")
            {
                Rectangle textrect = new Rectangle(
                        this._Style.TitleBarLeftRightMargin,
                        this._Style.TitleBarTopMargin,
                        this.Size.X - this._Style.TitleBarLeftRightMargin - this._RightTitleBar.Size.X - this._Style.TitleBarItemSeperation,
                        this._Style.TitleBarSize - this._Style.TitleBarTopMargin - this._Style.TitleBarBottomMargin);
                if (this._TextSample == null)
                {
                    this._TextSample = this._Style.TitleBarFont.CreateSample(this._Text, textrect.Size, TextAlign.Left, TextAlign.Center, TextWrap.Ellipsis);
                }
                Context.DrawText(this._Style.TitleBarTextColor, this._TextSample, textrect);
            }
        }

        public override void Update(InputContext Context)
        {
            using (Context.Translate(this.ClientRectangle.Location))
            {
                this._Client.Update(Context);
            }
            using (Context.Translate(this._RightTitleBarOffset))
            {
                this._RightTitleBar.Update(Context);
            }

            // Form needs to be dragged?
            using (Context.Stencil)
            {
                Context.StencilClip(new Rectangle(
                    this._Style.TitleBarLeftRightMargin,
                    0.0,
                    this.Size.X - this._RightTitleBar.Size.X - this._Style.TitleBarLeftRightMargin,
                    this._Style.TitleBarSize));
                using (Context.Scope(this._TitleBarScope))
                {
                    this._TitleBarScope.Update(Context);
                    if (this._TitleBarScope.Dragging)
                    {
                        this.Text = "Dragging";
                    }
                    else
                    {
                        this.Text = "Not Dragging";
                    }
                }
            }
        }

        private Point _RightTitleBarOffset
        {
            get
            {
                return new Point(this.Size.X - this._Style.TitleBarLeftRightMargin - this._RightTitleBar.Size.X, this._Style.TitleBarTopMargin);
            }
        }

        protected override void OnResize(Point Size)
        {
            this.ResizeChild(this._Client, this.ClientSize);
            this.ResizeChild(this._RightTitleBar, new Point(
                this._RightTitleBar.SuggestLength,
                this._Style.TitleBarSize - this._Style.TitleBarTopMargin - this._Style.TitleBarBottomMargin));
            if (this._TextSample != null)
            {
                this._TextSample.Dispose();
            }
        }

        private const double _TitleBarSize = 32.0;
        private const double _BorderSize = 7.0;

        protected override void OnDispose()
        {
            if (this._TextSample != null)
            {
                this._TextSample.Dispose();
            }
            this._Client.Dispose();
            this._RightTitleBar.Dispose();
        }

        private DragScope _TitleBarScope;
        private TextSample _TextSample;
        private string _Text;
        private Point? _FormDragOffset;
        private FlowContainer _RightTitleBar;
        private FormStyle _Style;
        private Control _Client;
    }

    /// <summary>
    /// Gives styling options for a form.
    /// </summary>
    public sealed class FormStyle
    {
        public FormStyle()
        {

        }

        public FormStyle(Skin Skin)
        {
            this.Pane = Skin.GetStretchableSurface(new SkinArea(16, 16, 16, 16));
            this.TitleBar = Skin.GetStretchableSurface(new SkinArea(0, 16, 16, 16));
            this.CloseButtonStyle = new ButtonStyle()
            {
                Normal = Skin.GetStretchableSurface(new SkinArea(64, 32, 16, 16)),
                Active = Skin.GetStretchableSurface(new SkinArea(80, 32, 16, 16)),
                Pushed = Skin.GetStretchableSurface(new SkinArea(96, 32, 16, 16)),
            };
        }

        public static readonly FormStyle Default = new FormStyle(Skin.Default);

        public Surface Pane;
        public Surface TitleBar;
        public Color BackColor = Color.RGB(0.8, 0.8, 0.8);
        public double BorderSize = 6.0;
        public double TitleBarSize = 31.0;
        public double TitleBarLeftRightMargin = 7.0;
        public double TitleBarTopMargin = 6.0;
        public double TitleBarBottomMargin = 4.0;
        public Font TitleBarFont = Font.Default;
        public Color TitleBarTextColor = Color.RGB(0.3, 0.3, 0.3);

        public double TitleBarItemSeperation = 4.0;
        public double TitleBarButtonWidth = 20.0;

        public ButtonStyle CloseButtonStyle;
    }
}
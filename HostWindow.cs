using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// A window hosting a TKGUI Panel.
    /// </summary>
    public class HostWindow : GameWindow
    {
        public HostWindow(BuildControlHandler BuildControl, string Title)
            : base(640, 480, GraphicsMode.Default, Title, GameWindowFlags.Default)
        {
            this.WindowState = WindowState.Maximized;
            this._KeyboardState = new WindowKeyboardState(this);
            this._MouseState = new WindowMouseState(this);
            this._Control = BuildControl();
        }

        /// <summary>
        /// Main entry point (for testing).
        /// </summary>
        public static void Main(string[] Args)
        {
            new HostWindow(delegate
            {
                FlowContainer fc = new FlowContainer(10.0, Axis.Vertical);
                Button a;
                Button b;
                Textbox c;
                Textbox d;
                Scrollbar e;
                Label l = new Label(@"The quick brown fox jumped over the lazy tortise.");
				Progressbar fp;
				Checkbox g;
                PopupContainer pc;
                fc.AddChild(a = new Button("Jello?!?"), 30.0);
                fc.AddChild(pc = new PopupContainer(b = new Button("Make a popup!")), 30.0);
                fc.AddChild(c = new Textbox(), 30.0);
                fc.AddChild(d = new Textbox(), 30.0);
                fc.AddChild(e = new Scrollbar(Axis.Horizontal), 30.0);
                fc.AddChild(fp = new Progressbar(), 30.0);
                fc.AddChild(l, 110.0);
				fc.AddChild(g = new Checkbox(true, "I am checkbox"), 30.0);

                MarginContainer mc = new MarginContainer(fc, 20.0);
                Point mcsize = mc.GetSize(new Point(300.0, fc.SuggestLength));

                WindowContainer wc = new WindowContainer(mc);
                ScrollContainer sc = new ScrollContainer(wc, new SunkenContainer(wc));
                sc.ClientHeight = mcsize.Y;
                sc.ClientWidth = null;

                SplitContainer spc = new SplitContainer(Axis.Vertical, new Blank(Color.RGB(1.0, 0.0, 0.0)), sc);
                spc.NearSize = 40.0;

                LayerContainer lc = new LayerContainer();
                Form f = new Form(spc, "Test");
                f.ClientSize = new Point(mcsize.X, 300.0);
                f.AddCloseButton();
                lc.AddControl(f, new Point(200.0, 200.0));


                CompoundMenuItem cmi = new CompoundMenuItem("Recurse");
                pc.Items = cmi.Items = new MenuItem[]
                {
                    new CommandMenuItem("Do nothing"),
                    cmi,
                    MenuItem.Seperator,
                    new CommandMenuItem("Stay the course!"),
                    new CommandMenuItem("Don't do anything"),
                    new CommandMenuItem("Avoid action"),
                    new CommandMenuItem("Remain inert"),
                };

                b.Click += delegate
                {
                    pc.CallAtMouse();
                };


				e.ValueChanged += delegate(double Value)
				{
					fp.Value = Value;
				};
                c.TextEntered += delegate(string Text)
                {
                    a.Text = Text;
                };





                return lc;
            }, "Test").Run();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(Color.RGB(1.0, 1.0, 1.0));
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
			
            GUIRenderContext rc = new GUIRenderContext(this.ViewSize);
            rc.Setup();
            this._Control.Render(rc);

            this.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            this._MouseState.Update();
            this._Control.Update(new _GUIContext(this), e.Time);
            this._KeyboardState.PostUpdate();
        }

        /// <summary>
        /// The top-level gui context when using a host window.
        /// </summary>
        private sealed class _GUIContext : GUIContext
        {
            public _GUIContext(HostWindow Window)
            {
                this.Window = Window;
            }

            public override Control KeyboardFocus
            {
                get
                {
                    return this.Window._KeyboardFocus;
                }
                set
                {
                    this.Window._KeyboardFocus = value;
                }
            }

            public override Control MouseFocus
            {
                get
                {
                    return this.Window._MouseFocus;
                }
                set
                {
                    this.Window._MouseFocus = value;
                }
            }

            public override MouseState MouseState
            {
                get
                {
                    return Window._MouseState;
                }
            }

            public override KeyboardState KeyboardState
            {
                get
                {
                    return Window._KeyboardState;
                }
            }

            public HostWindow Window;
        }

        /// <summary>
        /// Gets the size of the area rendered on this window.
        /// </summary>
        public Point ViewSize
        {
            get
            {
                return new Point((double)this.Width, (double)this.Height);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            if (this._Control != null)
            {
                GL.Viewport(0, 0, this.Width, this.Height);
                this._Control.ForceResize(this.ViewSize);
            }
        }

        private WindowKeyboardState _KeyboardState;
        private WindowMouseState _MouseState;
        private Control _MouseFocus;
        private Control _KeyboardFocus;
        private Control _Control;
    }

    /// <summary>
    /// A function that creates a control hierarchy when controls are needed. This allows the construction of controls to be delayed until
    /// the graphics context is set up.
    /// </summary>
    public delegate Control BuildControlHandler();
}
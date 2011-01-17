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
        public HostWindow(Control Control, string Title)
            : base(640, 480, GraphicsMode.Default, Title, GameWindowFlags.Default)
        {
            this.WindowState = WindowState.Maximized;
            this._Control = Control;

            this._KeyEvents = new List<KeyEvent>();
            this._KeyPresses = new List<char>();
            this.Keyboard.KeyUp += delegate(object sender, KeyboardKeyEventArgs e)
            {
                this._KeyEvents.Add(new KeyEvent(e.Key, ButtonEventType.Up));
            };
            this.Keyboard.KeyDown += delegate(object sender, KeyboardKeyEventArgs e)
            {
                this._KeyEvents.Add(new KeyEvent(e.Key, ButtonEventType.Down));
            };
            this.KeyPress += delegate(object sender, KeyPressEventArgs e)
            {
                this._KeyPresses.Add(e.KeyChar);
            };
        }

#if DEBUG
        /// <summary>
        /// Main entry point (for testing).
        /// </summary>
        public static void Main(string[] Args)
        {
            ManualContainer mc = new ManualContainer();
            Button a;
            Button b;
            Textbox c;
            Textbox d;
            mc.AddChild(a = new Button("Jello?!?"), new Rectangle(200.0, 200.0, 300.0, 30.0));
            mc.AddChild(b = new Button("Test"), new Rectangle(200.0, 250.0, 300.0, 30.0));
            mc.AddChild(c = new Textbox(), new Rectangle(200.0, 300.0, 300.0, 30.0));
            mc.AddChild(d = new Textbox(), new Rectangle(200.0, 350.0, 300.0, 30.0));
            c.TextEntered += delegate(string Text)
            {
                a.Text = Text;
            };
            d.TextChanged += delegate(string Text)
            {
                b.Text = Text;
            };
            new HostWindow(mc, "Test").Run();
        }
#endif

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(Color.RGB(0.8, 0.8, 0.8));
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
            this._Control.Update(new _GUIContext(this), e.Time);
            this._KeyEvents.Clear();
            this._KeyPresses.Clear();
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

            public override Control Control
            {
                get
                {
                    return this.Window._Control;
                }
            }

            public override MouseState ForceMouseState
            {
                get
                {
                    MouseDevice md = this.Window.Mouse;
                    if (md != null)
                    {
                        return new _MouseState(md);
                    }
                    return null;
                }
            }

            public override MouseState MouseState
            {
                get
                {
                    if (CanHaveMouseState(this.MouseFocus, this.Control))
                    {
                        return this.ForceMouseState;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            public override KeyboardState ForceKeyboardState
            {
                get
                {
                    return new _KeyboardState(this.Window, this.Window.Keyboard);
                }
            }

            public HostWindow Window;
        }

        private class _MouseState : MouseState
        {
            public _MouseState(MouseDevice Device)
            {
                this.Device = Device;
            }

            public override Point Position
            {
                get
                {
                    return new Point(this.Device.X, this.Device.Y);
                }
            }

            public override bool IsButtonDown(MouseButton Button)
            {
                return this.Device[Button];
            }

            public MouseDevice Device;
        }

        private class _KeyboardState : KeyboardState
        {
            public _KeyboardState(HostWindow Window, KeyboardDevice Device)
            {
                this.Window = Window;
                this.Device = Device;
            }

            public override bool IsKeyDown(Key Key)
            {
                return this.Device[Key];
            }

            public override IEnumerable<KeyEvent> Events
            {
                get
                {
                    return this.Window._KeyEvents;
                }
            }

            public override IEnumerable<char> Presses
            {
                get
                {
                    return this.Window._KeyPresses;
                }
            }

            public HostWindow Window;
            public KeyboardDevice Device;
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
                this._Control.Resize(this.ViewSize);
            }
        }

        private List<KeyEvent> _KeyEvents;
        private List<char> _KeyPresses;
        private Control _MouseFocus;
        private Control _KeyboardFocus;
        private Control _Control;
    }
}
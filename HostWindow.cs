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
        }

#if DEBUG
        /// <summary>
        /// Main entry point (for testing).
        /// </summary>
        public static void Main(string[] Args)
        {
            ManualContainer mc = new ManualContainer();
            mc.AddChild(new Button("Hello?!?"), new Rectangle(200.0, 200.0, 300.0, 40.0));
            mc.AddChild(new Button("Test?!?"), new Rectangle(200.0, 250.0, 300.0, 40.0));
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
        }

        /// <summary>
        /// The top-level gui context when using a host window.
        /// </summary>
        private class _GUIContext : GUIContext
        {
            public _GUIContext(HostWindow Window)
            {
                this.Window = Window;
            }

            public override Control Control
            {
                get
                {
                    return this.Window._Control;
                }
            }

            public override MouseState MouseState
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

        private Control _Control;
    }
}
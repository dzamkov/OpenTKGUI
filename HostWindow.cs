using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace TKGUI
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
            new HostWindow(new Blank(Color.RGB(1.0, 0.0, 0.0)), "Test").Run();
        }
#endif

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Scale(2.0, -2.0, 1.0);
            GL.Translate(-0.5, -0.5, 0.0);
            GL.Scale(1.0 / (double)this.Width, 1.0 / (double)this.Height, 1.0);

            this._Control.Render();

            this.SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            if (this._Control != null)
            {
                GL.Viewport(0, 0, this.Width, this.Height);
                this._Control.Resize(new Vector2d((double)this.Width, (double)this.Height), null);
            }
        }

        private Control _Control;
    }
}
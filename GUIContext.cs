using System;
using System.Collections.Generic;

using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// Context used to update controls. This context is not intended for any specific control and gives absolute positions and values
    /// for all parameters.
    /// </summary>
    public abstract class GUIContext
    {
        /// <summary>
        /// Gets or sets the control that currently has keyboard focus, or null if there is no such control.
        /// </summary>
        public abstract Control KeyboardFocus { get; set; }

        /// <summary>
        /// Gets or sets the control that currently has mouse focus, or null if there is no such control.
        /// </summary>
        public abstract Control MouseFocus { get; set; }

        /// <summary>
        /// Gets the state of the mouse, if possible.
        /// </summary>
        public abstract MouseState MouseState { get; }

        /// <summary>
        /// Gets the state of the keyboard, if possible.
        /// </summary>
        public abstract KeyboardState KeyboardState { get; }

        /// <summary>
        /// Creates a control context based on this context for a top-level control.
        /// </summary>
        public GUIControlContext CreateRootControlContext(Control Control, Point Offset)
        {
            return new GUIControlContext(this, Control, Offset);
        }
    }

    /// <summary>
    /// Context used to update a control. This control provides information specific to a single control.
    /// </summary>
    public class GUIControlContext
    {
        internal GUIControlContext(GUIContext Source, Control Control, Point Offset)
        {
            this._Source = Source;
            this._Control = Control;
            this._Offset = Offset;
        }

        /// <summary>
        /// Gets the control this context is intended for.
        /// </summary>
        public Control Control
        {
            get
            {
                return this._Control;
            }
        }

        /// <summary>
        /// Gets the offset of this control from the root update context.
        /// </summary>
        public Point Offset
        {
            get
            {
                return this._Offset;
            }
        }

        /// <summary>
        /// Captures mouse focus.
        /// </summary>
        public void CaptureMouse()
        {
            this._Source.MouseFocus = this._Control;
        }

        /// <summary>
        /// Releases mouse focus, if this control currently has it.
        /// </summary>
        public void ReleaseMouse()
        {
            this._Source.MouseFocus = null;
        }

        /// <summary>
        /// Gets if this control has mouse focus.
        /// </summary>
        public bool HasMouse
        {
            get
            {
                return this._Source.MouseFocus == this._Control;
            }
        }

        /// <summary>
        /// Captures keyboard focus.
        /// </summary>
        public void CaptureKeyboard()
        {
            this._Source.KeyboardFocus = this._Control;
        }

        /// <summary>
        /// Releases keyboard focus, if this control currently has it.
        /// </summary>
        public void ReleaseKeyboard()
        {
            this._Source.KeyboardFocus = null;
        }

        /// <summary>
        /// Gets if this control has keyboard focus.
        /// </summary>
        public bool HasKeyboard
        {
            get
            {
                return this._Source.KeyboardFocus == this._Control;
            }
        }

        /// <summary>
        /// Gets the mouse state for the control
        /// </summary>
        public MouseState MouseState
        {
            get
            {
                MouseState ms = this._Source.MouseState;
                if (ms != null)
                {
                    Point pos = ms.Position - this._Offset;
                    Control focus = this._Source.MouseFocus;
                    if (focus == this._Control)
                    {
                        return new _OffsetMouseState(this._Source.MouseState, pos);
                    }
                    if (!this._DisableMouse && focus == null)
                    {
                        Point size = this._Control.Size;
                        if (pos.X >= 0.0 && pos.Y >= 0.0 && pos.X < size.X && pos.Y < size.Y)
                        {
                            return new _OffsetMouseState(this._Source.MouseState, pos);
                        }
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the keyboard state for the control
        /// </summary>
        public KeyboardState KeyboardState
        {
            get
            {
                KeyboardState ks = this._Source.KeyboardState;
                if (ks != null)
                {
                    if (this._Source.KeyboardFocus == this._Control)
                    {
                        return ks;
                    }
                }
                return null;
            }
        }

        private class _OffsetMouseState : MouseState
        {
            public _OffsetMouseState(MouseState Source, Point Position)
            {
                this._Source = Source;
                this._Position = Position;
            }

            public override Point Position
            {
                get
                {
                    return this._Position;
                }
            }

            public override bool IsButtonDown(MouseButton Button)
            {
                return this._Source.IsButtonDown(Button);
            }

            private Point _Position;
            private MouseState _Source;
        }

        /// <summary>
        /// Creates a control context for a child control of this control.
        /// </summary>
        /// <param name="Control">The child control.</param>
        /// <param name="Offset">The offset of the child control.</param>
        public GUIControlContext CreateChildContext(Control Control, Point Offset)
        {
            GUIControlContext cc = new GUIControlContext(this._Source, Control, this._Offset + Offset);
            cc._DisableMouse = this._DisableMouse;
            return cc;
        }

        /// <summary>
        /// Creates a control context for a child control of this control. If DisableMouse is true, the child context will not 
        /// be able to access the mouse unless it has mouse focus.
        /// </summary>
        public GUIControlContext CreateChildContext(Control Control, Point Offset, bool DisableMouse)
        {
            GUIControlContext cc = new GUIControlContext(this._Source, Control, this._Offset + Offset);
            cc._DisableMouse |= DisableMouse;
            return cc;
        }

        private GUIContext _Source;
        private bool _DisableMouse;
        private Point _Offset;
        private Control _Control;
    }

    /// <summary>
    /// The state of the mouse at one time.
    /// </summary>
    public abstract class MouseState
    {
        /// <summary>
        /// Gets the current position of the mouse.
        /// </summary>
        public abstract Point Position { get; }

        /// <summary>
        /// Gets if the specified mouse button is down.
        /// </summary>
        public abstract bool IsButtonDown(MouseButton Button);
    }

    /// <summary>
    /// The state of the keyboard during an update, keeping track of changes since the last update.
    /// </summary>
    public abstract class KeyboardState
    {
        /// <summary>
        /// Gets if the specified keyboard key is down.
        /// </summary>
        public abstract bool IsKeyDown(Key Key);

        /// <summary>
        /// Gets the keys events that were generated since the last update.
        /// </summary>
        public abstract IEnumerable<KeyEvent> Events { get; }

        /// <summary>
        /// Gets the characters that where typed since the last update.
        /// </summary>
        public abstract IEnumerable<char> Presses { get; }
    }

    /// <summary>
    /// A type of event for a bottom.
    /// </summary>
    public enum ButtonEventType
    {
        /// <summary>
        /// The button is pushed down.
        /// </summary>
        Down,

        /// <summary>
        /// The button is released.
        /// </summary>
        Up,
    }

    /// <summary>
    /// An event for a key.
    /// </summary>
    public struct KeyEvent
    {
        public KeyEvent(Key Key, ButtonEventType Type)
        {
            this.Key = Key;
            this.Type = Type;
        }

        /// <summary>
        /// The key that is the subject of the event.
        /// </summary>
        public Key Key;
        
        /// <summary>
        /// The type of event this is.
        /// </summary>
        public ButtonEventType Type;
    }
}
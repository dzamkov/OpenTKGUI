using System;
using System.Collections.Generic;

using OpenTK;
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
        /// Searches for the closest ancestor of the specified type. Returns true on sucsess and false on failure.
        /// </summary>
        /// <param name="Offset">The ancestor's offset from the current control. This will be negative.</param>
        public bool FindAncestor<T>(out T Ancestor, out Point Offset)
            where T : Control
        {
            GUIControlContext context;
            if (this._FindAncestor<T>(out Ancestor, out context))
            {
                Offset = context._Offset - this._Offset;
                return true;
            }
            Offset = new Point();
            return false;
        }

        private bool _FindAncestor<T>(out T Ancestor, out GUIControlContext AncestorContext)
            where T : Control
        {
            GUIControlContext parent = this._Parent;
            if (parent == null)
            {
                Ancestor = null;
                AncestorContext = null;
                return false;
            }
            Ancestor = parent._Control as T;
            if (Ancestor != null)
            {
                AncestorContext = parent;
                return true;
            }
            return parent._FindAncestor<T>(out Ancestor, out AncestorContext);
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

            public override bool HasPushedButton(MouseButton Button)
            {
                return this._Source.HasPushedButton(Button);
            }

            public override bool HasReleasedButton(MouseButton Button)
            {
                return this._Source.HasReleasedButton(Button);
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
            cc._Parent = this;
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
            cc._Parent = this;
            return cc;
        }

        private GUIContext _Source;
        private GUIControlContext _Parent;
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

        /// <summary>
        /// Gets if the mouse button has been pushed since the last update.
        /// </summary>
        public abstract bool HasPushedButton(MouseButton Button);

        /// <summary>
        /// Gets if the mouse button has been released since the last update.
        /// </summary>
        public abstract bool HasReleasedButton(MouseButton Button);
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

    /// <summary>
    /// A mouse state for a game window.
    /// </summary>
    public class WindowMouseState : MouseState
    {
        public WindowMouseState(GameWindow Window)
        {
            this._Device = Window.Mouse;
            this._ButtonState = new bool[_ButtonAmount];
            this._OldButtonState = new bool[_ButtonAmount];
        }

        /// <summary>
        /// Updates the mouse state with new information produced by its associated device.
        /// </summary>
        public void Update()
        {
            bool[] nbuttonstate = this._OldButtonState;
            this._OldButtonState = this._ButtonState;
            for (int t = 0; t < _ButtonAmount; t++)
            {
                nbuttonstate[t] = this._Device[(MouseButton)t];
            }
            this._ButtonState = nbuttonstate;
        }

        public override Point Position
        {
            get
            {
                return new Point(this._Device.X, this._Device.Y);
            }
        }

        public override bool IsButtonDown(MouseButton Button)
        {
            return this._ButtonState[(int)Button];
        }

        public override bool HasPushedButton(MouseButton Button)
        {
            return !this._OldButtonState[(int)Button] && this._ButtonState[(int)Button];
        }

        public override bool HasReleasedButton(MouseButton Button)
        {
            return this._OldButtonState[(int)Button] && !this._ButtonState[(int)Button];    
        }

        private const int _ButtonAmount = 12;

        private MouseDevice _Device;
        private bool[] _ButtonState;
        private bool[] _OldButtonState;
    }

    /// <summary>
    /// A keyboard state for a game window.
    /// </summary>
    public class WindowKeyboardState : KeyboardState
    {
        public WindowKeyboardState(GameWindow Window)
        {
            this._Device = Window.Keyboard;
            this._KeyEvents = new List<KeyEvent>();
            this._KeyPresses = new List<char>();

            this._Device.KeyUp += delegate(object sender, KeyboardKeyEventArgs e)
            {
                this._KeyEvents.Add(new KeyEvent(e.Key, ButtonEventType.Up));
            };
            this._Device.KeyDown += delegate(object sender, KeyboardKeyEventArgs e)
            {
                this._KeyEvents.Add(new KeyEvent(e.Key, ButtonEventType.Down));
            };
            Window.KeyPress += delegate(object sender, KeyPressEventArgs e)
            {
                this._KeyPresses.Add(e.KeyChar);
            };
        }

        /// <summary>
        /// Updates the keyboard state by replacing old information used in an update.
        /// </summary>
        public void PostUpdate()
        {
            this._KeyEvents.Clear();
            this._KeyPresses.Clear();
        }

        public override bool IsKeyDown(Key Key)
        {
            return this._Device[Key];
        }

        public override IEnumerable<KeyEvent> Events
        {
            get
            {
                return this._KeyEvents;
            }
        }

        public override IEnumerable<char> Presses
        {
            get
            {
                return this._KeyPresses;
            }
        }

        private List<KeyEvent> _KeyEvents;
        private List<char> _KeyPresses;
        private KeyboardDevice _Device;
    }
}
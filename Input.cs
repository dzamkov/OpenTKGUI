using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;

namespace OpenTKGUI
{
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

        /// <summary>
        /// Gets the amount (in "pixels") the mouse has scrolled since the last update. Positive numbers indicate
        /// upward scrolling and negative numbers indicate downard scrolling.
        /// </summary>
        public virtual double Scroll
        {
            get
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Creates a new mouse state with a different position but all other properties the same.
        /// </summary>
        public MouseState SetPosition(Point Position)
        {
            return new _MouseState(Position, this);
        }

        private class _MouseState : MouseState
        {
            public _MouseState(Point MousePos, MouseState Source)
            {
                this._MousePos = MousePos;
                this._Source = Source;
            }

            public override Point Position
            {
                get
                {
                    return this._MousePos;
                }
            }

            public override double Scroll
            {
                get
                {
                    return this._Source.Scroll;
                }
            }

            public override bool HasPushedButton(OpenTK.Input.MouseButton Button)
            {
                return this._Source.HasPushedButton(Button);
            }

            public override bool HasReleasedButton(OpenTK.Input.MouseButton Button)
            {
                return this._Source.HasReleasedButton(Button);
            }

            public override bool IsButtonDown(OpenTK.Input.MouseButton Button)
            {
                return this._Source.IsButtonDown(Button);
            }

            private Point _MousePos;
            private MouseState _Source;
        }
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
            this._OldWheel = this._NewWheel;
            this._NewWheel = this._Device.WheelPrecise;
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


        public override double Scroll
        {
            get
            {
                return (double)(this._NewWheel - this._OldWheel) * 10.0;
            }
        }

        private const int _ButtonAmount = 12;

        private MouseDevice _Device;
        private float _OldWheel;
        private float _NewWheel;
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
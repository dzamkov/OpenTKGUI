using System;
using System.Collections.Generic;

using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// An item on a menu or popup.
    /// </summary>
    public abstract class MenuItem
    {
        public MenuItem()
        {
            
        }
    }

    /// <summary>
    /// A menu item that displays text.
    /// </summary>
    public abstract class TextMenuItem : MenuItem
    {
        public TextMenuItem(string Text)
        {
            this._Text = Text;
        }

        /// <summary>
        /// Gets the text for this menu item.
        /// </summary>
        public string Text
        {
            get
            {
                return this._Text;
            }
        }

        private string _Text;
    }

    /// <summary>
    /// A menu item that calls an event on click.
    /// </summary>
    public class CommandMenuItem : TextMenuItem
    {
        public CommandMenuItem(string Text) : base(Text)
        {

        }

        public CommandMenuItem(string Text, ClickHandler OnClick)
            : this(Text)
        {
            this.Click += OnClick;
        }

        public event ClickHandler Click;
    }
}
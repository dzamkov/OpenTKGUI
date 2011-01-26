using System;
using System.Collections.Generic;

using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// An item on a menu or popup.
    /// </summary>
    public class MenuItem
    {
        public MenuItem(string Text)
        {
            this._Text = Text;
        }

        /// <summary>
        /// Gets the text for this item.
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
    public class CommandMenuItem : MenuItem
    {
        public CommandMenuItem(string Text)
            : base(Text)
        {

        }

        public CommandMenuItem(string Text, ClickHandler OnClick)
            : base(Text)
        {
            this.Click += OnClick;
        }

        public event ClickHandler Click;
    }
}
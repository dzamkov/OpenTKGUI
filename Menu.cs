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

        internal void _Click()
        {
            if (this.Click != null)
            {
                this.Click.Invoke();
            }
        }

        public event ClickHandler Click;
    }

    /// <summary>
    /// A menu item that when selected, opens a submenu with a new set of items.
    /// </summary>
    public class CompoundMenuItem : TextMenuItem
    {
        public CompoundMenuItem(string Text)
            : this(Text, null)
        {

        }

        public CompoundMenuItem(string Text, IEnumerable<MenuItem> Items)
            : base(Text)
        {
            this._Items = Items;
        }

        /// <summary>
        /// Gets or sets the subordinate items in the compound menu item.
        /// </summary>
        public IEnumerable<MenuItem> Items
        {
            get
            {
                return this._Items;
            }
            set
            {
                this._Items = value;
            }
        }

        private IEnumerable<MenuItem> _Items;
    }
}
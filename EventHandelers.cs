using System;
using System.Collections.Generic;

using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// Handler for a text changed event from a textbox.
    /// </summary>
    public delegate void TextChangedHandler(string Text);

    /// <summary>
    /// Handler for a text entered event from a textbox.
    /// </summary>
    public delegate void TextEnteredHandler(string Text);
	
	/// <summary>
    /// Value has changed
    /// </summary>
    public delegate void ValueChangedHandeler(double Value);
	
    /// <summary>
    /// Handles a click event from a button.
    /// </summary>
    public delegate void ClickHandler(Button Button);
}

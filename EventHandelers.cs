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
    public delegate void ClickHandler();
	
    /// <summary>
    /// Handles a click event from a checkbox.
    /// </summary>
    public delegate void CheckboxClickHandler(bool Value);

    /// <summary>
    /// Handles a popup created event from a popup container.
    /// </summary>
    public delegate void PopupCreatedHandler(Popup Popup);

    /// <summary>
    /// Handles a dismissed event from a form or other layer control.
    /// </summary>
    public delegate void DismissedHandler();
}

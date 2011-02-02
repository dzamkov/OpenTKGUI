using System;
using System.Collections.Generic;

namespace OpenTKGUI
{
    /// <summary>
    /// Console for your debug messages or debug input
    /// </summary>
    public class Terminal
    {
		public Terminal()
		{
			this.Buffer = new Queue<SelectableLabel>();
			this.Write("Terminal...");
		}
		
		/// <summary>
		/// Push a message to the terminal.
		/// </summary>
		public void Write(string Message)
		{
			SelectableLabel lbl = new SelectableLabel(Message);
			this.Buffer.Enqueue(lbl);
			this.Update(lbl);
		}
		
		private void Update(SelectableLabel lbl)
		{
			if(this._Terminal == null)
				return;
			this._LableItems.AddChild(lbl, this._Style.LableHeight);
			this._ScrollContainer.ClientHeight = this.Buffer.Count * this._Style.LableHeight;
		}
		
        /// <summary>
        /// Shows the terminal.
        /// </summary>
        public void Show(LayerContainer LayerContainer)
        {
			if(this._Terminal != null)
				return;
			
            this._LableItems = new FlowContainer(Axis.Vertical);
			
			foreach(SelectableLabel lbl in this.Buffer.ToArray())
				this._LableItems.AddChild(lbl, this._Style.LableHeight);
				
			this._ScrollContainer = new ScrollContainer(this._LableItems);
			this._ScrollContainer.ClientHeight = this.Buffer.Count * this._Style.LableHeight;
			
			FlowContainer flow2 = new FlowContainer(Axis.Vertical);
			
			Textbox command = new Textbox();
			
			command.TextChanged += delegate(string Text)
			{
				if(this.GetAutoComplete != null)
				{
					string[] result = this.GetAutoComplete(Text);
					PopupContainer pc = new PopupContainer(command);
					//pc.Call
					
					MenuItem[] items = new MenuItem[result.Length];
					
					for(int i = 0; i < result.Length; i++)
						items[i] = MenuItem.Create(result[i], delegate{});
					pc.Items = items;
					pc.Call(new Point(0,0));
				}
			};
			
			command.TextEntered += delegate(string Text)
			{
				this.Write("> " + Text);
				command.Text = "";
				
				LinkedList<string> parsed;
				string cmd;
				try
				{
					parsed = Terminal.ParseArguments(Text);
					cmd = parsed.First.Value;
					parsed.RemoveFirst();
				}
				catch(ParseArgsException ex)
				{
					this.Write("Parse error: " + ex.Message);
					return;
				}
				
				if(this.DoCommand != null)
				{
					if(!this.DoCommand(cmd, parsed))
					{
						this.Write("Command \"" + cmd + "\" not found!");
					}
				}
				
			};
			
			flow2.AddChild(this._ScrollContainer, this._Style.TerminalHeight - this._Style.TextboxHeight);
			flow2.AddChild(command, this._Style.TextboxHeight);
			
			this._Terminal = new Form(flow2, "Terminal");
			this._Terminal.AddCloseButton();
			this._Terminal.ClientSize = new Point(this._Style.TerminalWidth, this._Style.TerminalHeight);
			LayerContainer.AddControl(this._Terminal, new Point(200,200));
        }
		
		private static Dictionary<char, char> _Literal = null;
		private static void LoadDict()
		{
			_Literal = new Dictionary<char, char>();
			_Literal.Add('n', '\n');
			_Literal.Add('r', '\r');
			_Literal.Add('t', '\t');
		}
		/// <summary>
		/// Convert a string into arguments
		/// </summary>
		public static LinkedList<string> ParseArguments(string Text)
		{
			if(_Literal == null)
				LoadDict();
			
			LinkedList<string> retval = new LinkedList<string>();
			
			bool literal = false;
			bool inQuotes = false;
			
			string currentiteration = "";
			foreach(char ch in Text)
			{
				if(literal)
				{
					literal = false;
					char newchar;
					
					if(!_Literal.TryGetValue(ch, out newchar))
						newchar = ch;
					
					currentiteration += newchar;
				}
				else if(ch == '\\')
						literal = true;
				else if(ch == '\"')
						inQuotes = !inQuotes;
				else if(ch == ' ')
				{
						if(!inQuotes)
						{
							retval.AddLast(currentiteration);
							currentiteration = "";
						}else
							currentiteration += ch;
				}
				else
				{
					currentiteration += ch;
				}
				
			}
			if(currentiteration != "")
				retval.AddLast(currentiteration);
			return retval;
		}
		
		public event GetAutoCompleteHandeler GetAutoComplete; 
		public event DoCommandHandeler DoCommand;
		
		private TerminalStyle _Style = new TerminalStyle();
		private Form _Terminal;
		private ScrollContainer _ScrollContainer;
		private FlowContainer _LableItems;
		private Queue<SelectableLabel> Buffer;
    }
	/// <summary>
    /// Gives styling options for a message box.
    /// </summary>
    public class TerminalStyle
    {
		public double TerminalWidth = 500;
		public double TerminalHeight = 300;
        public double LableHeight = 20.0;
        public double TextboxHeight = 30.0;
    }
	
	public class ParseArgsException : Exception
	{
		public ParseArgsException(string Message) : base(Message)
		{
		}
	}
}
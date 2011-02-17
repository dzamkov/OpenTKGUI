using System;
using System.Collections.Generic;
using OpenTKGUI;

// This file must be removed with the project mode changed to class library before release.

public static class Program
{
    public static void Main()
    {
        HostWindow window = new HostWindow("Test", 640, 480);
        // Create a background, with a menu
        CompoundMenuItem cpmi = new CompoundMenuItem("Popup");
        IEnumerable<MenuItem> menuitems = new MenuItem[]
        {
            MenuItem.Create("Simple"),
            MenuItem.Create("Compound", new MenuItem[]
            {
                MenuItem.Create("Normal"),
                MenuItem.Create("Popup")
            }),
            MenuItem.Create("Test", new MenuItem[]
            {
                MenuItem.Create("Recursive"),
                cpmi
            })
        };
        cpmi.Items = menuitems;

        Menu menu = new Menu(menuitems);
        SplitContainer sc = new SplitContainer(Axis.Vertical, menu.WithBorder(0.0, 0.0, 0.0, 1.0), new Blank(Color.Transparent));
        sc.NearSize = 30.0;

        // Create a layer container
        LayerContainer lc = new LayerContainer(sc);

        // Create a form with many buttons
        {
            FlowContainer flow = new FlowContainer(10.0, Axis.Vertical);
            for (int t = 0; t < 40; t++)
            {
                int i = t + 1;
                Button b = new Button("Button #" + i.ToString());
                flow.AddChild(b, 30.0);
                b.Click += delegate
                {
                    MessageBox.ShowOKCancel(lc, "Button Clicked!", "You have clicked button #" + i.ToString() + ".", null);
                };
            }
            Point targetflowsize = new Point(120.0, flow.SuggestLength);
            MarginContainer margin = flow.WithMargin(10.0);
            Point targetmarginsize = margin.GetSize(targetflowsize);
            WindowContainer win = new WindowContainer(margin);
            SunkenContainer sunken = new SunkenContainer(win);
            ScrollContainer scroll = new ScrollContainer(win, sunken.WithBorder(1.0, 1.0, 0.0, 1.0));
            scroll.ClientHeight = targetmarginsize.Y;

            Form form = new Form(scroll.WithRotate(Rotation.CounterClockwise), "Lots of buttons");
            form.ClientSize = new Point(200.0, targetmarginsize.X + 20.0);
            lc.AddControl(form, new Point(30.0, 45.0));
        }

        // Popup test
        {
            Label label = new Label("Right click for popup", Color.RGB(0.0, 0.3, 0.0), new LabelStyle()
            {
                HorizontalAlign = TextAlign.Center,
                VerticalAlign = TextAlign.Center,
                Wrap = TextWrap.Ellipsis
            });
            PopupContainer pc = new PopupContainer(label);
            pc.ShowOnRightClick = true;
            pc.Items = new MenuItem[]
            {
                MenuItem.Create("Do nothing", delegate { }),
                MenuItem.Create("Remain inert", delegate { }),
                MenuItem.Create("Make a message box", delegate
                {
                    MessageBox.ShowOKCancel(lc, "Message Box", "Done", null);
                }),
                MenuItem.Seperator,
                MenuItem.Create("Mouseover me!", new MenuItem[]
                {
                    MenuItem.Create("Some", delegate { }),
                    MenuItem.Create("Items", delegate { }),
                    MenuItem.Create("Spawn", delegate { }),
                    MenuItem.Create("Another", delegate { }),
                    MenuItem.Create("Popup", delegate { }),
                }),
                MenuItem.Seperator,
                MenuItem.Create("Try", delegate { }),
                MenuItem.Create("The", delegate { }),
                MenuItem.Create("Keyboard", delegate { }),
            };

            Pane pane = new Pane(pc.WithAlign(label.SuggestSize, Align.Center, Align.Center).WithBorder(1.0));
            pane.ClientSize = new Point(200.0, 50.0);
            lc.AddControl(pane, new Point(230.0, 45.0));
        }

        // Timers and progress bars
        {
            FlowContainer flow = new FlowContainer(10.0, Axis.Vertical);
            Button addbutton = new Button("Add Some");
            Button resetbutton = new Button(ButtonStyle.CreateSolid(Skin.Default), "Reset");
            Progressbar bar = new Progressbar();
            flow.AddChild(addbutton, 30.0);
            flow.AddChild(resetbutton, 30.0);
            flow.AddChild(bar, 30.0);

            addbutton.Click += delegate
            {
                bar.Value = bar.Value + 0.05;
            };
            resetbutton.Click += delegate
            {
                bar.Value = 0.0;
            };

            MarginContainer margin = flow.WithMargin(20.0);

            Form form = new Form(margin.WithBorder(1.0), "Progress bars!");
            form.ClientSize = margin.GetSize(new Point(200.0, flow.SuggestLength)) + new Point(4, 4);
            lc.AddControl(form, new Point(230.0, 150.0));
            form.AddCloseButton();
        }

        // Textbox
        {
            Textbox tb = new Textbox();
            MarginContainer margin = tb.WithMargin(20.0);

            Form form = new Form(margin.WithBorder(1.0), "Change the title of this form!");
            form.ClientSize = margin.GetSize(new Point(400.0, 32.0));
            lc.AddControl(form, new Point(30.0, 360.0));

            tb.TextChanged += delegate(string Text)
            {
                form.Text = Text;
            };
        }

        // Button test
        {
            Button b1 = new Button("Clear this form");
            Button b2 = new Button(Skin.Default.GetSurface(new SkinArea(64, 96, 16, 16)).WithAlign(Align.Center, Align.Center).CreateControl());
            FlowContainer flow = new FlowContainer(10.0, Axis.Vertical);
            flow.AddChild(b1, 30.0);
            flow.AddChild(b2, 30.0);
            VariableContainer formcon = new VariableContainer(flow.WithMargin(10.0));
            Form form = new Form(new SunkenContainer(formcon).WithBorder(1.0), "Button Test");
            form.ClientSize = new Point(180.0, flow.SuggestLength + 22.0);
            lc.AddControl(form, new Point(300.0, 300.0));

            b1.Click += delegate
            {
                formcon.Client = new Blank(Color.RGB(1.0, 1.0, 1.0));
            };
        }

        window.Control = lc;
        window.Run();
    }
}
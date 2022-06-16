using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using static PGui.InputHelper;

namespace PGui
{
    public class AutoGuiLayout
    {
        public Window window;

        public List<IGuiElement> elements = new List<IGuiElement>();
    }

    public class UIButtonGroup : IGuiElement
    {
        //Data
        public UIRect Rect { get; set; }
        public Window ParentWindow { get; set; }

        public int currentWidth = 0;
        public Dictionary<string, UIButton> buttons;

        private int buttonTextSize = 10, buttonTextPadding = 7, buttonSeparationDistance = 7;

        public void Draw()
        {
            foreach (UIButton button in buttons.Values)
            {
                button.Draw();
            }
        }
        public void Update()
        {
            foreach (UIButton button in buttons.Values)
            {
                button.Update();
            }
        }

        public void AddButton(string text)
        {
            buttons.Add(text, new UIButton(text, buttonTextSize, buttonTextPadding, new Vector2(Rect.RawRect.x + currentWidth, Rect.RawRect.y), ParentWindow, UIHandle.topLeft));
            currentWidth += (int)buttons[text].Rect.RawRect.width + buttonSeparationDistance;
        }

        public UIButtonGroup()
        {
            buttons = new Dictionary<string, UIButton>();
        }

        public UIButtonGroup(string[] buttonKeys, Window parent, Rectangle rect) : this()
        {
            ParentWindow = parent;
            Rect = new UIRect(rect, parent.Size, UIHandle.topLeft);
            for (int i = 0; i < buttonKeys.Length; i++)
            {
                AddButton(buttonKeys[i]);
            }
        }
    }
}
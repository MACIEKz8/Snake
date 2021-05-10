using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Snake
{
    class snakePart
    {
        public UIElement UiElement { get; set; }
        public Point Position { get; set; }
        public bool IsHead { get; set; }

    }
}

using Godot;

namespace Ancient
{
    [Tool]
    public class HandDrawnMass : Node2D
    {
        private static Texture numbers = GD.Load<Texture>("res://textures/UI/numbers.png");
        private static readonly Vector2 charSize = new Vector2(24, 36);
        private System.Collections.Generic.Dictionary<char, int> charLUT = new System.Collections.Generic.Dictionary<char, int>() {
            { '0', 0 },
            { '1', 24 },
            { '2', 48 },
            { '3', 72 },
            { '4', 96 },
            { '5', 120 },
            { '6', 144 },
            { '7', 168 },
            { '8', 192 },
            { '9', 216 },
            { '.', 240 },
        };

        [Export]
        public string Text
        {
            get { return text; }
            set
            {
                if (text != value)
                { text = value; Update(); }
            }
        }
        private string text = "";
        [Export]
        public Color TextColour { get { return textColour; } set { textColour = value; Update(); } }
        private Color textColour = Colors.White;

        public override void _Draw()
        {
            base._Draw();

            // Numbers
            for (int i = 0; i < text.Length; i++)
            {
                DrawTextureRectRegion(numbers, new Rect2(i * charSize.x, 0, charSize), new Rect2(charLUT[text[i]], 0, charSize), textColour);
            }

            // kg
            DrawTextureRectRegion(numbers, new Rect2(text.Length * charSize.x, 0, charSize), new Rect2(264, 0, charSize), textColour);
        }
    }
}
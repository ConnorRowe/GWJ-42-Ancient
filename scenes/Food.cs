using Godot;

namespace Ancient
{
    public class Food : Sprite
    {
        public Diets FoodType { get; set; } = Diets.Herb;
        public float FoodValue { get; set; } = 0f;

        public override void _Ready()
        {
            base._Ready();
        }
    }
}
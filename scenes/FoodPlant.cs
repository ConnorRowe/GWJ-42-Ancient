using Godot;

namespace Ancient
{
    public class FoodPlant : Sprite
    {
        public Diets FoodType { get; } = Diets.Herb;
        public float FoodValue { get; set; } = 0f;

        public override void _Ready()
        {
            base._Ready();
        }
    }
}
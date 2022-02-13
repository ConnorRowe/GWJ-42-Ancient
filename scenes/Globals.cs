using Godot;

namespace Ancient
{
    public enum Diets
    {
        Herb,
        Omni,
        Carn
    }

    public static class Globals
    {
        public static RandomNumberGenerator RNG = new RandomNumberGenerator();
        public static Texture[] HerbTextures = new Texture[4];

        static Globals()
        {
            RNG.Randomize();

            HerbTextures[0] = GD.Load<Texture>("res://textures/leaf.png");
        }
    }
}
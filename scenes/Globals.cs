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

        static Globals()
        {
            RNG.Randomize();
        }
    }
}
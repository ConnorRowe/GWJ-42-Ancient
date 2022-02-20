using Godot;

namespace Ancient
{
    public class GlobalSound : AudioStreamPlayer
    {
        public static GlobalSound instance;

        private AudioStreamPlayer worldGrowPlayer;
        private AudioStreamPlayer uiClickPlayer;

        public override void _Ready()
        {
            base._Ready();

            instance = this;
            worldGrowPlayer = GetNode<AudioStreamPlayer>("WorldGrowPlayer");
            uiClickPlayer = GetNode<AudioStreamPlayer>("UIClickPlayer");
        }

        public static void PlayWorldGrowSound()
        {
            instance.worldGrowPlayer.Play();
        }

        public static void UIClick()
        {
            instance.uiClickPlayer.Play();
        }
    }
}
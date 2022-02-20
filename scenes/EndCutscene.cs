using Godot;

namespace Ancient
{
    public class EndCutscene : Node2D
    {
        private AudioStreamPlayer chompPlayer;
        private AudioStreamPlayer noisePlayer;
        public override void _Ready()
        {
            base._Ready();

            chompPlayer = GetNode<AudioStreamPlayer>("ChompPlayer");
            noisePlayer = GetNode<AudioStreamPlayer>("NoisePlayer");
            GetNode<AnimationPlayer>("AnimationPlayer").Play("Cutscene");
        }

        private void PlayChomp()
        {
            chompPlayer.Play();
        }

        private void StartNoise()
        {
            noisePlayer.Play();
        }

        private void StopNoise()
        {
            noisePlayer.Stop();
        }

        private void FinishCutscene()
        {
            GetTree().ChangeSceneTo(GD.Load<PackedScene>("res://scenes/menus/WinMenu.tscn"));
            QueueFree();
        }
    }
}
using Godot;

namespace Ancient
{
    public class Meteoroid : Node2D
    {
        [Signal]
        public delegate void Exploded(Vector2 globalPos);
        private AnimationPlayer animPlayer;

        public override void _Ready()
        {
            base._Ready();

            animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            animPlayer.Play("Fall");
        }

        private void Explode()
        {
            EmitSignal(nameof(Exploded), GlobalPosition);
        }
    }
}
using Godot;

namespace Ancient
{
    public class Raptor : Enemy
    {
        private AnimationPlayer animPlayer;
        [Export]
        private string walkAnimName = "Walk";

        public override void _Ready()
        {
            base._Ready();

            animPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
            if (animPlayer != null)
                animPlayer.Play(walkAnimName);
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            if (animPlayer != null)
                animPlayer.PlaybackSpeed = (velocity.LengthSquared() + externalVelocity.LengthSquared()) * delta * .00015f;
        }
    }
}
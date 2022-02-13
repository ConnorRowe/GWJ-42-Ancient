using Godot;

namespace Ancient
{
    public class Raptor : Enemy
    {
        private AnimationPlayer animPlayer;

        public override void _Ready()
        {
            base._Ready();

            animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
			animPlayer.Play("Walk");
			mass = 10f;
        }

        public override void _PhysicsProcess(float delta)
        {
			base._PhysicsProcess(delta);

            animPlayer.PlaybackSpeed = (velocity.LengthSquared() + externalVelocity.LengthSquared()) * delta * .00015f;
        }
    }
}
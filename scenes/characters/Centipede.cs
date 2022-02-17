using Godot;

namespace Ancient
{
    public class Centipede : Enemy
    {
        private Sprite[] segments = new Sprite[3];
        private Vector2 baseSegOffset = new Vector2(1, 6);
        private float physTimeFrac = 0f;
        private float animVal = 0f;

        public override void _Ready()
        {
            base._Ready();

            segments[0] = GetNode<Sprite>("Body1");
            segments[1] = GetNode<Sprite>("Body2");
            segments[2] = GetNode<Sprite>("Body3");
        }

        public override void _PhysicsProcess(float delta)
        {
            physTimeFrac += delta * .5f;
            if (physTimeFrac > 1f)
                physTimeFrac--;

            float speedFactor = velocity.Length() / maxSpeed;

            animVal += speedFactor * Mathf.Pi * delta * 20f;
            if (animVal > Mathf.Tau)
                animVal -= Mathf.Tau;

            // Animate segments
            for (int i = 0; i < 3; i++)
            {
                segments[i].Offset = baseSegOffset + new Vector2(0, Mathf.Cos(animVal + ((((float)i) / 3f) * Mathf.Tau)) * 6f);
            }

            // TODO Mirror movement to collision shapes

            base._PhysicsProcess(delta);
        }
    }
}
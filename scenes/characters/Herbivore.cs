using Godot;

namespace Ancient
{
    public class Herbivore : Raptor
    {
        private bool canBite = true;
        private Area2D eatArea;

        public override void _Ready()
        {
            base._Ready();

            eatArea = GetNode<Area2D>("EatArea");
        }

        protected override void AITick()
        {
            base.AITick();

            if (currentState == BehaviourState.HUNT && eatArea.GetOverlappingBodies().Contains(player))
            {
                BitePlayer(player);
            }
        }

        protected override void BitePlayer(Player player)
        {
            if (!canBite)
                return;

            base.BitePlayer(player);

            player.AddMass(mass * -.1f);
            player.AddHunger(-.15f);

            player.ApplyExternalImpulse(Position.DirectionTo(player.Position) * 800f);

            player.GameWorld.MakeBloodBurst(player.GlobalPosition);

            canBite = false;
            GetTree().CreateTimer(.5f).Connect("timeout", this, nameof(ResetCanBite));
        }

        private void ResetCanBite()
        {
            canBite = true;
        }
    }
}
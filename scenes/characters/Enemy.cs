using Godot;

namespace Ancient
{
    public class Enemy : Character
    {
        protected enum BehaviourState
        {
            ROAM,
            HUNT,
            FLEE
        }

        protected BehaviourState currentState = BehaviourState.ROAM;
        private Vector2 targetPos = Vector2.Zero;
        private Timer roamTimer;
        protected Player player;
        private Label debugLabel;
        protected float maxRange = 1032256f;
        [Export]
        private float roamSpeedScale = 1f;
        private bool canBite = true;
        private Area2D eatArea;
        [Export]
        private float biteMassDmg = 1f;

        public override void _Ready()
        {
            base._Ready();

            roamTimer = GetNode<Timer>("RoamTimer");
            roamTimer.Connect("timeout", this, nameof(RoamTimeout));
            GetNode<Timer>("AITick").Connect("timeout", this, nameof(AITick));
            debugLabel = GetNode<Label>("debug");
            eatArea = GetNode<Area2D>("EatArea");
            eatArea.Connect("body_entered", this, nameof(EatAreaBodyEntered));

            RoamTimeout();
            roamTimer.Start();
        }

        private void RoamTimeout()
        {
            targetPos = new Vector2(Globals.RNG.RandfRange(0f, 3760f), Globals.RNG.RandfRange(0f, 2080f));
        }

        protected virtual void AITick()
        {
            switch (currentState)
            {
                case BehaviourState.ROAM:
                    if (player != null && Position.DistanceSquaredTo(player.Position) < 1000000f)
                    {
                        if (player.Mass < Mass)
                            currentState = BehaviourState.HUNT;
                        else
                            currentState = BehaviourState.FLEE;

                        roamTimer.Stop();
                    }
                    break;
                case BehaviourState.HUNT:
                case BehaviourState.FLEE:
                    if (Position.DistanceSquaredTo(player.Position) > maxRange)
                    {
                        currentState = BehaviourState.ROAM;
                        RoamTimeout();
                        roamTimer.Start();
                    }
                    else if (currentState == BehaviourState.HUNT && player.Mass > Mass)
                        currentState = BehaviourState.FLEE;

                    break;
            }

            switch (currentState)
            {
                case BehaviourState.ROAM:
                    debugLabel.Text = "roam";
                    break;
                case BehaviourState.HUNT:
                    debugLabel.Text = "hunt";
                    break;
                case BehaviourState.FLEE:
                    debugLabel.Text = "flee";
                    break;
            }

            if (currentState == BehaviourState.HUNT && eatArea.GetOverlappingBodies().Contains(player))
            {
                BitePlayer(player);
            }
        }

        public virtual void DetectPlayer(Player player)
        {
            this.player = player;

            switch (currentState)
            {
                case BehaviourState.ROAM:
                    if (player.Mass < Mass)
                        currentState = BehaviourState.HUNT;
                    else
                        currentState = BehaviourState.FLEE;

                    roamTimer.Stop();
                    break;
                case BehaviourState.HUNT:
                    break;
                case BehaviourState.FLEE:
                    break;
            }
        }

        protected override Vector2 GetInputDir()
        {
            switch (currentState)
            {
                case BehaviourState.ROAM:
                    return GlobalPosition.DirectionTo(targetPos);
                case BehaviourState.HUNT:
                    return Position.DirectionTo(player.Position);
                case BehaviourState.FLEE:
                    return player.Position.DirectionTo(Position);
            }

            return Vector2.Zero;
        }

        protected override float GetMaxSpeed()
        {
            if (currentState == BehaviourState.ROAM)
                return base.GetMaxSpeed() * roamSpeedScale;
            else
                return base.GetMaxSpeed();
        }

        private void EatAreaBodyEntered(Node body)
        {
            if (body is Player p)
            {
                if (p.Mass < Mass)
                {
                    BitePlayer(p);
                }
            }
        }

        private void BitePlayer(Player player)
        {
            if (!canBite)
                return;

            player.AddMass(-biteMassDmg);
            player.AddHunger(-.15f);

            player.ApplyExternalImpulse(Position.DirectionTo(player.Position) * 800f);

            canBite = false;
            GetTree().CreateTimer(.5f).Connect("timeout", this, nameof(ResetCanBite));

        }

        private void ResetCanBite()
        {
            canBite = true;
        }
    }
}
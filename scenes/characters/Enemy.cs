using Godot;

namespace Ancient
{
    public class Enemy : Character
    {
        private enum BehaviourState
        {
            ROAM,
            HUNT,
            FLEE
        }

        private BehaviourState currentState = BehaviourState.ROAM;
        private Vector2 targetPos = Vector2.Zero;
        private Timer roamTimer;
        private Player player;
        private Area2D detectionArea;
        private Label debugLabel;

        public override void _Ready()
        {
            base._Ready();

            roamTimer = GetNode<Timer>("RoamTimer");
            roamTimer.Connect("timeout", this, nameof(RoamTimeout));
            detectionArea = GetNode<Area2D>("DetectionArea");
            detectionArea.Connect("body_entered", this, nameof(BodyDetected));
            GetNode<Timer>("AITick").Connect("timeout", this, nameof(AITick));
            debugLabel = GetNode<Label>("debug");
            GetNode("EatArea").Connect("body_entered", this, nameof(EatAreaBodyEntered));
        }

        private void RoamTimeout()
        {
            targetPos = new Vector2(Globals.RNG.RandfRange(0f, 3760f), Globals.RNG.RandfRange(0f, 2080f));
        }

        private void AITick()
        {
            switch (currentState)
            {
                case BehaviourState.ROAM:
                    if (player != null && Position.DistanceSquaredTo(player.Position) < 115600)
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
                    if (Position.DistanceSquaredTo(player.Position) > 129600f)
                    {
                        currentState = BehaviourState.ROAM;
                        RoamTimeout();
                        roamTimer.Start();
                    }
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
        }

        private void BodyDetected(Node body)
        {
            if (body is Player p)
            {
                player = p;

                switch (currentState)
                {
                    case BehaviourState.ROAM:
                        if (p.Mass < Mass)
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

        private void EatAreaBodyEntered(Node body)
        {
            if (body is Player p)
            {
                if (p.Mass < mass)
                {
                    // KILL PLAYER
                    GD.Print("YA GOTT ATE BOI");
                }
            }
        }
    }
}
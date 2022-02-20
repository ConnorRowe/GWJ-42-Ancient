using Godot;

namespace Ancient
{
    public class Meteoroid : Node2D
    {
        [Signal]
        public delegate void Exploded(Vector2 globalPos);
        private static readonly PackedScene meteoriteScene = GD.Load<PackedScene>("res://scenes/MeteoriteBig.tscn");
        private static readonly PackedScene tinyMeteoriteScene = GD.Load<PackedScene>("res://scenes/MeteoriteTiny.tscn");
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

            Node parent = GetParent();

            if (Globals.RNG.Randf() > .5f)
            {
                GetNode<Sprite>("Meteor").Visible = false;
                GetNode<Sprite>("Shadow").Visible = false;
                FoodPlant bigMet = meteoriteScene.Instance<FoodPlant>();
                parent.AddChild(bigMet);
                bigMet.Position = Position;
                bigMet.ZIndex = (int)Position.y;
                bigMet.FoodValue = 25000f;
            }

            int tinyMetCount = Globals.RNG.RandiRange(0, 4);
            var tinyMetPositions = GetNode("TinyMetoriteSpawnPositions").GetChildren();

            for (int i = 0; i < tinyMetCount; i++)
            {
                int posNum = Globals.RNG.RandiRange(0, tinyMetPositions.Count - 1);
                Vector2 pos = ((Position2D)tinyMetPositions[posNum]).GlobalPosition;
                tinyMetPositions.RemoveAt(posNum);

                FoodPlant tinyMet = tinyMeteoriteScene.Instance<FoodPlant>();
                parent.AddChild(tinyMet);
                tinyMet.GlobalPosition = pos;
                tinyMet.ZIndex = (int)pos.y;
                tinyMet.FoodValue = 2500f;
            }

            Area2D detectionArea = GetNode<Area2D>("DetectionArea");
            foreach (Area2D area in detectionArea.GetOverlappingAreas())
            {
                if (area.GetParent() is Player player)
                {
                    player.AddMass(-50000f);
                }
            }

            QueueFree();
        }

        public static Vector2[] GetSpacedCircumferancePoints(int numPoints, Vector2 gOrigin, float radius)
        {
            const float ellipseScalar = .5f;

            float spacing = Mathf.Tau / numPoints;

            Vector2[] outPoints = new Vector2[numPoints];
            for (int i = 0; i < numPoints; i++)
            {
                float angle = spacing * i;
                outPoints[i] = gOrigin + new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle) * ellipseScalar);
            }

            return outPoints;
        }

        public static Vector2[] GetSpacedPointsAlongLine(int numPoints, Vector2 gPosA, Vector2 gPosB)
        {
            float spacing = gPosA.DistanceTo(gPosB) / numPoints;
            Vector2 dir = gPosA.DirectionTo(gPosB);

            Vector2[] outPoints = new Vector2[numPoints];
            for (int i = 0; i < numPoints; i++)
            {
                outPoints[i] = gPosA + (dir * spacing * i);
            }

            return outPoints;
        }
    }
}
using Godot;
using System.Collections.Generic;

namespace Ancient
{
    public class Character : KinematicBody2D
    {
        public float Mass { get { return mass; } }
        public bool IsFacingRight { get { return isFacingRight; } }
        public float SpeedScale { get; set; } = 1f;

        protected Vector2 inputDir = Vector2.Zero;
        [Export]
        protected float acceleration = 3000f;
        [Export]
        protected float maxSpeed = 4000f;
        [Export]
        protected float movementDampening = 5f;
        protected Vector2 velocity = Vector2.Zero;
        protected Vector2 externalVelocity = Vector2.Zero;
        [Export]
        protected float mass = 1f;
        protected HashSet<Node> bodyPartsToFlip = new HashSet<Node>();
        [Export]
        private Godot.Collections.Array<NodePath> bodyPartPathsToFlip = new Godot.Collections.Array<NodePath>();
        private bool isFacingRight = true;

        public override void _Ready()
        {
            base._Ready();

            foreach (NodePath path in bodyPartPathsToFlip)
            {
                Node n = GetNodeOrNull(path);
                if (n != null)
                {
                    bodyPartsToFlip.Add(n);
                }
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            inputDir = GetInputDir().Normalized();

            velocity -= (velocity * movementDampening * delta);

            velocity += inputDir * acceleration * delta * SpeedScale;

            externalVelocity -= (externalVelocity * 2f * delta);

            if (velocity.Length() > maxSpeed * SpeedScale)
            {
                velocity = velocity.Normalized() * maxSpeed * SpeedScale;
            }

            MoveAndSlide(velocity + externalVelocity);

            // Check if facing dir has changed
            if (!Mathf.IsEqualApprox(velocity.x, 0f))
            {
                if (velocity.x > 0f && !isFacingRight)
                {
                    FlipDirection();
                }
                else if (velocity.x < 0f && isFacingRight)
                {
                    FlipDirection();
                }
            }

            ZAsRelative = false;
            ZIndex = (int)GlobalPosition.y;
        }

        protected virtual Vector2 GetInputDir()
        {
            return Vector2.Zero;
        }

        private void FlipDirection()
        {
            foreach (Node bodyPart in bodyPartsToFlip)
            {
                FlipBodyPart(bodyPart);
            }

            isFacingRight = !isFacingRight;
        }

        private void FlipBodyPart(Node bodyPart)
        {
            if (bodyPart is Node2D node2D)
            {
                node2D.Position = new Vector2(-node2D.Position.x, node2D.Position.y);
                node2D.Scale = new Vector2(-node2D.Scale.x, node2D.Scale.y);
            }
            else if (bodyPart is Control control)
            {
                control.RectPosition = new Vector2(-control.RectPosition.x, control.RectPosition.y);
                control.RectScale = new Vector2(-control.RectScale.x, control.RectScale.y);
            }
        }

        public void ApplyExternalImpulse(Vector2 impulse)
        {
            externalVelocity += impulse;
        }
    }
}
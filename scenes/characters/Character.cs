using Godot;
using System.Collections.Generic;

namespace Ancient
{
    public class Character : KinematicBody2D
    {
        public float Mass { get { return mass; } }
        public bool IsFacingRight { get { return isFacingRight; } }

        protected Vector2 inputDir = Vector2.Zero;
        [Export]
        protected float acceleration = 3000f;
        [Export]
        protected float maxSpeed = 4000f;
        [Export]
        protected float movementDampening = 5f;
        protected Vector2 velocity = Vector2.Zero;
        protected Vector2 externalVelocity = Vector2.Zero;
        protected float mass = 1f;
        protected HashSet<Node2D> bodyPartsToFlip = new HashSet<Node2D>();
        [Export]
        private Godot.Collections.Array<NodePath> bodyPartPathsToFlip = new Godot.Collections.Array<NodePath>();
        private bool isFacingRight = true;

        public override void _Ready()
        {
            base._Ready();

            foreach (NodePath path in bodyPartPathsToFlip)
            {
                Node2D n = GetNodeOrNull<Node2D>(path);
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

            velocity += inputDir * acceleration * delta;

            externalVelocity -= (externalVelocity * 2f * delta);

            if (velocity.Length() > maxSpeed)
            {
                velocity = velocity.Normalized() * maxSpeed;
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
        }

        protected virtual Vector2 GetInputDir()
        {
            return Vector2.Zero;
        }

        private void FlipDirection()
        {
            foreach (Node2D bodyPart in bodyPartsToFlip)
            {
                FlipBodyPart(bodyPart);
            }

            isFacingRight = !isFacingRight;
        }

        private void FlipBodyPart(Node2D bodyPart)
        {
            bodyPart.Position = new Vector2(-bodyPart.Position.x, bodyPart.Position.y);
            bodyPart.Scale = new Vector2(-bodyPart.Scale.x, bodyPart.Scale.y);
        }
    }
}
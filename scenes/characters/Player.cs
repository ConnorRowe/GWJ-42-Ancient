using Godot;

namespace Ancient
{
    public class Player : KinematicBody2D
    {
        public bool IsInputLocked { get; set; } = false;
        public bool IsFacingRight { get; set; } = true;
        public float Hunger { get { return hunger; } }
        public float Mass { get { return mass; } }
        public float DashCooldown { get; set; } = 0.0f;
        public Diets Diet { get; set; } = Diets.Herb;

        private AnimationPlayer animationPlayer;
        private Sprite head;
        private Sprite body;
        private Sprite rFoot;
        private Sprite lFoot;
        private Sprite tail;
        private Sprite arm;
        private CollisionShape2D headCollider;
        private CollisionShape2D bodyCollider;
        private Area2D eatArea;

        private Vector2 inputDir = Vector2.Zero;
        private float acceleration = 3000f;
        private float maxSpeed = 4000f;
        private float movementDampening = 5f;
        private Vector2 velocity = Vector2.Zero;
        private Vector2 externalVelocity = Vector2.Zero;
        private bool canTakeDamage = true;
        private bool isDead = false;
        private float hunger = 1f;
        private float mass = 1f;

        //Stats
        private float hungerDepletion = .0625f;


        public override void _Ready()
        {
            base._Ready();

            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            animationPlayer.Play("Walk");

            head = GetNode<Sprite>("Head");
            body = GetNode<Sprite>("Body");
            rFoot = GetNode<Sprite>("RFoot");
            lFoot = GetNode<Sprite>("LFoot");
            tail = GetNode<Sprite>("Tail");
            arm = GetNode<Sprite>("Arm");
            headCollider = GetNode<CollisionShape2D>("HeadShape");
            bodyCollider = GetNode<CollisionShape2D>("BodyShape");
            eatArea = GetNode<Area2D>("EatArea");
            eatArea.Connect("area_entered", this, nameof(EatAreaEntered));
        }

        public override void _Input(InputEvent evt)
        {
            if (evt.IsActionPressed("g_move_left") || evt.IsActionPressed("g_move_right"))
            {
                if ((evt.IsAction("g_move_left") && IsFacingRight) || (evt.IsAction("g_move_right") && !IsFacingRight))
                {
                    IsFacingRight = !IsFacingRight;

                    FlipBodyPart(head);
                    FlipBodyPart(body);
                    FlipBodyPart(lFoot);
                    FlipBodyPart(rFoot);
                    FlipBodyPart(tail);
                    FlipBodyPart(arm);
                    FlipBodyPart(headCollider);
                    FlipBodyPart(bodyCollider);
                }
            }
            else if (evt.IsActionPressed("g_dash") && DashCooldown <= 0f && inputDir != Vector2.Zero)
            {
                externalVelocity += inputDir * 800f;
                DashCooldown = 1.0f;
                hunger -= .1f;
            }
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            hunger -= hungerDepletion * delta;

            if (hunger <= 0.0f)
            {
                // TODO LOST CONDITION
                hunger = 0f;
            }

            DashCooldown -= delta;
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            inputDir = Vector2.Zero;

            if (!IsInputLocked && !isDead)
            {
                if (Input.IsActionPressed("g_move_up"))
                {
                    inputDir.y -= 1;
                }
                if (Input.IsActionPressed("g_move_down"))
                {
                    inputDir.y += 1;
                }
                if (Input.IsActionPressed("g_move_left"))
                {
                    inputDir.x -= 1;
                }
                if (Input.IsActionPressed("g_move_right"))
                {
                    inputDir.x += 1;
                }
            }

            inputDir = inputDir.Normalized();

            velocity -= (velocity * movementDampening * delta);

            velocity += inputDir * acceleration * delta;

            externalVelocity -= (externalVelocity * 2f * delta);

            if (velocity.Length() > maxSpeed)
            {
                velocity = velocity.Normalized() * maxSpeed;
            }

            animationPlayer.PlaybackSpeed = isDead ? 0f : ((velocity.LengthSquared() + externalVelocity.LengthSquared()) * delta * .0002f);

            MoveAndSlide(velocity + externalVelocity);
        }

        private void FlipBodyPart(Node2D bodyPart)
        {
            bodyPart.Position = new Vector2(-bodyPart.Position.x, bodyPart.Position.y);
            bodyPart.Scale = new Vector2(-bodyPart.Scale.x, bodyPart.Scale.y);
        }

        private void EatAreaEntered(Area2D area)
        {
            if (area.Owner is Food food && food.FoodType == Diet)
            {
                ConsumeFood(food);
                area.Owner.QueueFree();
            }
        }

        private void ConsumeFood(Food food)
        {
            hunger += food.FoodValue;
            if (hunger > 1f)
                hunger = 1f;

            mass += food.FoodValue * 2f;
        }
    }

}
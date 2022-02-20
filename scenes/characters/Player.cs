using Godot;

namespace Ancient
{
    public class Player : Character
    {
        [Signal]
        public delegate void Dashed();
        [Signal]
        public delegate void AteFoodPlant();
        [Signal]
        public delegate void AteEnemy();
        [Signal]
        public delegate void TookDamage();

        public bool IsInputLocked { get; set; } = false;
        public float Hunger { get { return hunger; } }
        public float DashCooldown { get; set; } = 0.0f;

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
        private AudioStreamPlayer hitPlayer;
        private AudioStreamPlayer chompPlayer;
        private AudioStreamPlayer whooshPlayer;

        private bool canTakeDamage = true;
        private bool isDead = false;
        private float hunger = 1f;

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
            eatArea.Connect("body_entered", this, nameof(EatAreaBodyEntered));
            GetNode("EnemyDetectionArea").Connect("body_entered", this, nameof(EnemyDetectionAreaBodyEntered));
            hitPlayer = GetNode<AudioStreamPlayer>("HitPlayer");
            chompPlayer = GetNode<AudioStreamPlayer>("ChompPlayer");
            whooshPlayer = GetNode<AudioStreamPlayer>("WhooshPlayer");

            Scale = new Vector2(.5f, .5f);
        }

        public override void _Input(InputEvent evt)
        {
            if (evt.IsActionPressed("g_dash") && DashCooldown <= 0f && inputDir != Vector2.Zero)
            {
                ApplyExternalImpulse(inputDir * 800f);
                DashCooldown = 1.0f;
                hunger -= .1f;
                whooshPlayer.Play();

                EmitSignal(nameof(Dashed));
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

        protected override Vector2 GetInputDir()
        {
            Vector2 dir = Vector2.Zero;

            if (!IsInputLocked && !isDead)
            {
                if (Input.IsActionPressed("g_move_up"))
                {
                    dir.y -= 1;
                }
                if (Input.IsActionPressed("g_move_down"))
                {
                    dir.y += 1;
                }
                if (Input.IsActionPressed("g_move_left"))
                {
                    dir.x -= 1;
                }
                if (Input.IsActionPressed("g_move_right"))
                {
                    dir.x += 1;
                }
            }

            return dir;
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            animationPlayer.PlaybackSpeed = isDead ? 0f : ((velocity.LengthSquared() + externalVelocity.LengthSquared()) * delta * .0002f);
        }

        private void EatAreaEntered(Area2D area)
        {
            if (area.Owner is FoodPlant food)
            {
                ConsumeFood(food);
                area.Owner.QueueFree();
            }
        }

        private void ConsumeFood(FoodPlant food)
        {
            AddHunger(.25f);

            AddMass(food.FoodValue);

            EmitSignal(nameof(AteFoodPlant));
            chompPlayer.Play(0);
        }

        private void EatAreaBodyEntered(Node body)
        {
            if (body is Enemy enemy)
            {
                if (enemy.Mass < Mass)
                {
                    AddMass(enemy.Mass * .5f);
                    AddHunger(enemy.Mass * .5f);
                    enemy.QueueFree();

                    EmitSignal(nameof(AteEnemy), enemy);
                    chompPlayer.Play(0);
                }
            }
        }

        public void AddHunger(float hungerAmount)
        {
            hunger += hungerAmount;
            if (hunger > 1f)
                hunger = 1f;
        }

        private void EnemyDetectionAreaBodyEntered(Node body)
        {
            if (body is Enemy enemy)
            {
                enemy.DetectPlayer(this);
            }
        }

        public void AddMass(float mass)
        {
            Mass += mass;

            if (mass < 0f)
            {
                EmitSignal(nameof(TookDamage));
                hitPlayer.Play(0);
            }

            if (Mass <= 0f)
            {
                //Die
                Mass = 0f;
                GD.Print("U DED");
            }
        }
    }
}
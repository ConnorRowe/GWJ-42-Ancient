using Godot;

namespace Ancient
{
    public class GameWorld : Node2D
    {
        private static RandomNumberGenerator rng = Globals.RNG;
        private static PackedScene foodScene = GD.Load<PackedScene>("res://scenes/FoodPlant.tscn");
        private static PackedScene bloodBurstScene = GD.Load<PackedScene>("res://scenes/BloodBurst.tscn");
        private static Curve shakeCurve = GD.Load<Curve>("res://ShakeCurve.tres");
        private static Color hungerFull = new Color("397cbe");
        private static Color hungerHungry = new Color("e64e4b");
        private static Color hungerStarving = new Color("31222c");
        private static readonly Vector2 levelBounds = new Vector2(3840, 2160);
        private const int MAX_PLANTS = 30;
        private const int MAX_DINOS = 5;
        private static readonly float[] levelMassGoals = new float[3] { 25f, 500f, 2000f };
        private static readonly Texture[] levelBackgrounds = new Texture[3]
        {
            GD.Load<Texture>("res://textures/floors/floor_grass.png"),
            GD.Load<Texture>("res://textures/floors/grass2.png"),
            GD.Load<Texture>("res://textures/floors/islands.png")
        };
        public static readonly Texture[] levelHerbTextures = new Texture[3]
        {
            GD.Load<Texture>("res://textures/leaf.png"),
            GD.Load<Texture>("res://textures/bush.png"),
            GD.Load<Texture>("res://textures/tinytree.png")
        };
        public static readonly PackedScene[] levelEnemies = new PackedScene[6]
        {
            GD.Load<PackedScene>("res://scenes/characters/Centipede.tscn"),
            GD.Load<PackedScene>("res://scenes/characters/Raptor.tscn"),
            GD.Load<PackedScene>("res://scenes/characters/Herbivore.tscn"),
            GD.Load<PackedScene>("res://scenes/characters/Majungasaurus.tscn"),
            GD.Load<PackedScene>("res://scenes/characters/Majungasaurus.tscn"),
            GD.Load<PackedScene>("res://scenes/characters/Majungasaurus.tscn")
        };

        public int Level { get; set; } = 0;
        public int CurrentPlantCount { get; set; } = 0;
        public int CurrentDinoCount { get; set; } = 0;
        [Export]
        public float TimeScale { get { return Engine.TimeScale; } set { Engine.TimeScale = value; animPlayer.PlaybackSpeed = 1f / value; } }
        [Export]
        public bool IsWorldGrowing { get; set; } = false;
        private float timeFrac = 0.0f;
        private float nextMassGoal = 0f;
        private float dashShakeTime = 0f;

        private Timer spawnFoodTimer;
        private Node2D currentStuff;
        private Node2D tinyStuff;
        private TextureRect hungerBar;
        private Player player;
        private Vector2 hungerBarOrigin;
        private Vector2 massFillOrigin;
        private Vector2 dashOrigin;
        private TextureProgress dash;
        private ShaderMaterial hungerBarMat;
        private TextureRect massFill;
        private ShaderMaterial massFillMat;
        private HandDrawnMass massText;
        private Camera2D camera;
        private TextureRect floorBG;
        private Node trees;
        private AnimationPlayer animPlayer;
        private Node bloodBG;
        private Camera2D bloodCamera;

        public override void _Ready()
        {
            base._Ready();

            spawnFoodTimer = GetNode<Timer>("SpawnFoodTimer");
            spawnFoodTimer.Connect("timeout", this, nameof(SpawnFoodTimerTick));
            currentStuff = GetNode<Node2D>("CurrentStuff");
            tinyStuff = GetNode<Node2D>("TinyStuff");
            hungerBar = GetNode<TextureRect>("UI/HungerBar");
            player = GetNode<Player>("CurrentStuff/Player");
            hungerBarOrigin = hungerBar.RectPosition;
            dash = GetNode<TextureProgress>("UI/Dash");
            dashOrigin = dash.RectPosition;
            hungerBarMat = (ShaderMaterial)hungerBar.Material;
            massFill = GetNode<TextureRect>("UI/Mass/Fill");
            massFillMat = (ShaderMaterial)massFill.Material;
            massFillOrigin = massFill.RectPosition;
            massText = GetNode<HandDrawnMass>("UI/Mass/Text");
            camera = GetNode<Camera2D>("CurrentStuff/Player/Camera2D");
            floorBG = GetNode<TextureRect>("FloorBG");
            trees = GetNode("Trees");
            animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            bloodBG = GetNode("UI/BloodViewport/BloodBG");
            bloodCamera = GetNode<Camera2D>("UI/BloodViewport/Camera2D");

            foreach (Object obj in trees.GetNode("Giant").GetChildren() + trees.GetNode("Normal").GetChildren() + GetNode("Volcanos").GetChildren())
            {
                if (obj is Sprite sprite)
                {
                    sprite.ZAsRelative = false;
                    sprite.ZIndex = (int)sprite.GlobalPosition.y;
                }
            }

            nextMassGoal = levelMassGoals[Level];
        }

        public override void _Process(float delta)
        {
            timeFrac += delta;
            if (timeFrac > 1f)
                timeFrac -= 1f;

            float shake = shakeCurve.InterpolateBaked(timeFrac);

            // Hunger bar

            float hunger = player.Hunger;
            hungerBarMat.SetShaderParam("fill_percent", hunger);

            Color currentFillColour = (Color)hungerBarMat.GetShaderParam("fill_colour");
            if (hunger <= .85 && hunger > .33f)
            {
                hungerBarMat.SetShaderParam("fill_colour", currentFillColour.LinearInterpolate(hungerHungry, delta * 5f));

            }
            else if (hunger <= .33f)
            {
                hungerBarMat.SetShaderParam("fill_colour", currentFillColour.LinearInterpolate(hungerStarving, delta * 5f));
            }
            else
            {
                hungerBarMat.SetShaderParam("fill_colour", currentFillColour.LinearInterpolate(hungerFull, delta * 5f));
            }

            if (hunger <= .33f)
            {
                hungerBar.RectPosition = hungerBarOrigin + new Vector2(shake * 4f, shake * 2f);
                hungerBar.RectRotation = shake * 1f;
            }

            // Dash
            dash.Value = player.DashCooldown;

            if (dashShakeTime > 0f)
            {
                dash.RectPosition = dashOrigin + new Vector2(shake * -8f * dashShakeTime, shake * -4f * dashShakeTime);
                dash.RectRotation = shake * 2f * dashShakeTime;

                dashShakeTime -= delta;
            }

            if (dashShakeTime < 0f)
                dashShakeTime = 0f;

            //Mass
            float p = Mathf.Clamp(player.Mass / nextMassGoal, 0f, 1f);
            massFillMat.SetShaderParam("uniform_scale", Mathf.Lerp(5f, 1f, p));
            if (player.Mass > 0f)
                massText.Text = player.Mass.ToString("0.00");

            player.Scale = player.Scale.LinearInterpolate(new Vector2(.5f + p, .5f + p), delta * 2f);

            camera.GlobalScale = Vector2.One;

            if (p >= .9f)
            {
                // shake mass indicator

                massFill.RectPosition = massFillOrigin + new Vector2(shake * -4f, shake * 2f);
                massFill.RectRotation = shake * 1f;
            }

            if (player.Mass >= nextMassGoal && !IsWorldGrowing)
            {
                //play world grow anim
                animPlayer.Play("WorldGrow");
                IsWorldGrowing = true;
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            bloodCamera.GlobalPosition = player.GlobalPosition;
        }

        private void SpawnFoodTimerTick()
        {
            SpawnFood(rng.Randf() >= .75f ? Diets.Carn : Diets.Herb);
        }

        private void SpawnFood(Diets foodType)
        {
            Node2D food = null;

            if (foodType == Diets.Herb)
            {
                if (CurrentPlantCount > MAX_PLANTS)
                    return;

                FoodPlant foodPlant = foodScene.Instance<FoodPlant>();
                foodPlant.FoodValue = ((Level + 1)) + rng.RandfRange(0f, Level * Level);
                foodPlant.Texture = levelHerbTextures[Level];
                food = foodPlant;
                CurrentPlantCount++;
            }
            else if (foodType == Diets.Carn)
            {
                if (CurrentDinoCount > MAX_DINOS)
                    return;

                int enemyID = Level * 2;
                if (rng.Randf() < .25f)
                    enemyID++;

                food = levelEnemies[enemyID].Instance<Node2D>();
                CurrentDinoCount++;
            }

            if (food != null)
            {
                currentStuff.AddChild(food);

                food.Position = new Vector2(rng.RandfRange(200f, levelBounds.x - 200f), rng.RandfRange(200f, levelBounds.y - 200f));
            }
        }

        public void GrowWorld()
        {
            Level++;

            nextMassGoal = levelMassGoals[Level];

            foreach (Node child in tinyStuff.GetChildren())
            {
                child.QueueFree();
            }

            foreach (Node child in currentStuff.GetChildren())
            {
                if (child is Enemy || child is FoodPlant)
                {
                    currentStuff.RemoveChild(child);
                    tinyStuff.AddChild(child);

                    ((Node2D)child).Scale = new Vector2(.3f, .3f);

                    if (child is Enemy enemy)
                    {
                        enemy.CollisionLayer = enemy.CollisionMask = 0;
                        enemy.SpeedScale = .3f;
                        CurrentDinoCount--;
                    }
                    else if (child is FoodPlant)
                    {
                        CurrentPlantCount--;
                    }

                    Area2D eatArea = child.GetNode<Area2D>("EatArea");
                    eatArea.CollisionLayer = eatArea.CollisionMask = 0;
                }
            }

            floorBG.Texture = levelBackgrounds[Level];

            if (Level == 1)
            {
                trees.GetNode("Giant").QueueFree();
                StaticBody2D normal = trees.GetNode<StaticBody2D>("Normal");
                normal.Visible = true;
                normal.Layers = 1;
            }
            else if (Level == 2)
            {
                trees.QueueFree();

                Node2D volcanos = GetNode<Node2D>("Volcanos");
                volcanos.Visible = true;
                volcanos.GetNode<StaticBody2D>("StaticBody2D").Layers = 1;
            }
        }

        public void ShakeDash()
        {
            dashShakeTime = 1f;
        }

        public void MakeBloodBurst(Vector2 globalPos)
        {
            Particles2D newBurst = bloodBurstScene.Instance<Particles2D>();
            bloodBG.AddChild(newBurst);
            newBurst.GlobalPosition = globalPos;
            GetTree().CreateTimer(1.5f).Connect("timeout", newBurst, "queue_free");
            newBurst.Emitting = true;
        }
    }
}

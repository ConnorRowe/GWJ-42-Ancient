using Godot;

namespace Ancient
{
    public class GameWorld : Node2D
    {
        private static RandomNumberGenerator rng = Globals.RNG;
        private static PackedScene FoodScene = GD.Load<PackedScene>("res://scenes/Food.tscn");
        private static Curve shakeCurve = GD.Load<Curve>("res://ShakeCurve.tres");
        private static Color hungerFull = new Color("397cbe");
        private static Color hungerHungry = new Color("f7e5b2");
        private static Color hungerStarving = new Color("e64e4b");
        private const int MAX_FOOD = 30;

        public int Level { get; set; } = 1;
        public int CurrentFoodCount { get; set; } = 0;

        private float timeFrac = 0.0f;

        private Timer spawnFoodTimer;
        private YSort ySort;
        private TextureRect hungerBar;
        private Player player;
        private Vector2 hungerBarOrigin;
        private TextureProgress dash;
        private ShaderMaterial hungerBarMat;
        private TextureRect massFill;
        private ShaderMaterial massFillMat;
		private HandDrawnMass massText;
        private float nextMassGoal = 50f;

        public override void _Ready()
        {
            base._Ready();

            spawnFoodTimer = GetNode<Timer>("SpawnFoodTimer");
            spawnFoodTimer.Connect("timeout", this, nameof(SpawnFoodTimerTick));
            ySort = GetNode<YSort>("YSort");
            hungerBar = GetNode<TextureRect>("UI/HungerBar");
            player = GetNode<Player>("YSort/Player");
            hungerBarOrigin = hungerBar.RectPosition;
            dash = GetNode<TextureProgress>("UI/Dash");
            hungerBarMat = (ShaderMaterial)hungerBar.Material;
            massFill = GetNode<TextureRect>("UI/Mass/Fill");
            massFillMat = (ShaderMaterial)massFill.Material;
			massText = GetNode<HandDrawnMass>("UI/Mass/Text");
        }

        public override void _Process(float delta)
        {
            timeFrac += delta;
            if (timeFrac > 1f)
                timeFrac -= 1f;

            // Hunger bar

            float hunger = player.Hunger;
            hungerBarMat.SetShaderParam("fill_percent", hunger);

            if (hunger <= .85 && hunger > .25f)
            {
                hungerBarMat.SetShaderParam("fill_colour", hungerHungry);

            }
            else if (hunger <= .25f)
            {
                hungerBarMat.SetShaderParam("fill_colour", hungerStarving);
            }
            else
            {
                hungerBarMat.SetShaderParam("fill_colour", hungerFull);
            }

            if (hunger <= .25f)
            {
                float shake = shakeCurve.InterpolateBaked(timeFrac);

                hungerBar.RectPosition = hungerBarOrigin + new Vector2(shake * 4f, shake * 2f);
                hungerBar.RectRotation = shake * 1f;
            }

            // Dash

            dash.Value = player.DashCooldown;

            //Mass
            float p = player.Mass / nextMassGoal;
            massFill.RectScale = new Vector2(.25f, .25f).LinearInterpolate(Vector2.One, p);
            massText.Text = player.Mass.ToString("0.00");
        }

        private void SpawnFoodTimerTick()
        {
            SpawnFood(Diets.Herb);
        }

        private void SpawnFood(Diets foodType)
        {
            if (CurrentFoodCount > MAX_FOOD)
                return;

            Food food = FoodScene.Instance<Food>();

            food.FoodType = foodType;
            food.FoodValue = (Level * .1f) + rng.RandfRange(0f, .1f);

            switch (foodType)
            {
                case Diets.Herb:
                    food.Texture = Globals.HerbTextures[Level - 1];
                    break;
                case Diets.Omni:
                    break;
                case Diets.Carn:
                    break;
            }

            ySort.AddChild(food);

            food.Position = new Vector2(rng.RandfRange(0f, 3760f), rng.RandfRange(0f, 2080f));

            CurrentFoodCount++;
        }
    }
}
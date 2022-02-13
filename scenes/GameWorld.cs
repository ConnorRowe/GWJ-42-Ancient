using Godot;

namespace Ancient
{
    public class GameWorld : Node2D
    {
        private static RandomNumberGenerator rng = Globals.RNG;
        private static PackedScene foodScene = GD.Load<PackedScene>("res://scenes/FoodPlant.tscn");
        private static PackedScene centiScene = GD.Load<PackedScene>("res://scenes/characters/Centipede.tscn");
        private static PackedScene raptorScene = GD.Load<PackedScene>("res://scenes/characters/Raptor.tscn");
        private static Curve shakeCurve = GD.Load<Curve>("res://ShakeCurve.tres");
        private static Color hungerFull = new Color("397cbe");
        private static Color hungerHungry = new Color("f7e5b2");
        private static Color hungerStarving = new Color("e64e4b");
        private static readonly Vector2 levelBounds = new Vector2(3840, 2160);
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
        private float nextMassGoal = 15f;

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
            float p = Mathf.Clamp(player.Mass / nextMassGoal, 0f, 1f);
            massFillMat.SetShaderParam("uniform_scale", Mathf.Lerp(5f, 1f, p));
            massText.Text = player.Mass.ToString("0.00");

            Scale = Scale.LinearInterpolate(Vector2.One + new Vector2(p * .5f, p * .5f), delta * 2f);
        }

        private void SpawnFoodTimerTick()
        {
            SpawnFood(rng.Randf() >= .75f ? Diets.Carn : Diets.Herb);
        }

        private void SpawnFood(Diets foodType)
        {
            if (CurrentFoodCount > MAX_FOOD)
                return;

            Node2D food = null;

            if (foodType == Diets.Herb)
            {
                FoodPlant foodPlant = foodScene.Instance<FoodPlant>();
                foodPlant.FoodValue = (Level * .1f) + rng.RandfRange(0f, .1f);
                foodPlant.Texture = Globals.HerbTextures[Level - 1];
                food = foodPlant;
            }
            else if (foodType == Diets.Carn)
            {
                food = rng.Randf() > .25f ? raptorScene.Instance<Node2D>() : centiScene.Instance<Node2D>();
            }

            if (food != null)
            {
                ySort.AddChild(food);

                food.Position = new Vector2(rng.RandfRange(200f, levelBounds.x - 200f), rng.RandfRange(200f, levelBounds.y - 200f));

                CurrentFoodCount++;
            }
        }
    }
}
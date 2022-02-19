using Godot;

namespace Ancient
{
    public class EarthWorld : Node2D
    {
        private const float massGoal = 1000000f;
        private static PackedScene meteorExplosionScene = GD.Load<PackedScene>("res://scenes/MeteorExplosion.tscn");
        private static PackedScene meteoroidScene = GD.Load<PackedScene>("res://scenes/Meteoroid.tscn");
        public static float PlayerStartMass { get; set; }

        private AnimationPlayer animPlayer;
        private Player player;
        private TextureRect massFill;
        private ShaderMaterial massFillMat;
        private HandDrawnMass massText;
        private Camera2D camera;
        private Vector2 massFillOrigin;
        private Vector2 dashOrigin;
        private TextureProgress dash;
        private HandDrawnMass fpsText;
        private Viewport metaballViewport;

        private float timeFrac = 0;
        private float dashShakeTime = 0f;

        public override void _Ready()
        {
            base._Ready();

            animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            animPlayer.Play("FadeIn");
            player = GetNode<Player>("Player");
            player.Mass = PlayerStartMass;
            player.Connect(nameof(Player.Dashed), this, nameof(ShakeDash));
            dash = GetNode<TextureProgress>("UI/Dash");
            dashOrigin = dash.RectPosition;
            massFill = GetNode<TextureRect>("UI/Mass/Fill");
            massFillMat = (ShaderMaterial)massFill.Material;
            massFillOrigin = massFill.RectPosition;
            massText = GetNode<HandDrawnMass>("UI/Mass/Text");
            camera = GetNode<Camera2D>("Player/Camera2D");
            fpsText = GetNode<HandDrawnMass>("UI/FpsText"); fpsText.DrawKG = false;
            metaballViewport = GetNode<Viewport>("UI/MetaballViewport");
            GetNode<Timer>("Player/MeteoroidTimer").Connect("timeout", this, nameof(SpawnMeteoroid));
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            timeFrac += delta;
            if (timeFrac > 1f)
                timeFrac -= 1f;

            float shake = GameWorld.ShakeCurve.InterpolateBaked(timeFrac);

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
            float p = Mathf.Clamp(player.Mass / massGoal, 0f, 1f);
            massFillMat.SetShaderParam("uniform_scale", Mathf.Lerp(5f, 1f, p));
            if (player.Mass >= 0f)
                massText.Text = player.Mass.ToString("0.00");

            player.Scale = player.Scale.LinearInterpolate(new Vector2(.5f + p, .5f + p), delta * 2f);

            camera.GlobalScale = Vector2.One;

            if (p >= .9f)
            {
                // shake mass indicator

                massFill.RectPosition = massFillOrigin + new Vector2(shake * -4f, shake * 2f);
                massFill.RectRotation = shake * 1f;
            }

            fpsText.Text = $"{(int)Engine.GetFramesPerSecond()}";
        }

        private void ShakeDash()
        {
            dashShakeTime = 1f;
        }

        private void MakeBloodBurst(Vector2 globalPos)
        {
            GameWorld.AddMetaballParticles(metaballViewport, GameWorld.bloodBurstScene, globalPos);
        }

        private void MakeMeteorExplosion(Vector2 globalPos)
        {
            GameWorld.AddMetaballParticles(metaballViewport, meteorExplosionScene, globalPos);
        }

        private void SpawnMeteoroid()
        {
            Meteoroid meteoroid = meteoroidScene.Instance<Meteoroid>();
            AddChild(meteoroid);
            meteoroid.Position = new Vector2(Globals.RNG.RandfRange(236, 1684), Globals.RNG.RandfRange(600, 1000));
            meteoroid.Connect(nameof(Meteoroid.Exploded), this, nameof(MakeMeteorExplosion));
            meteoroid.ZIndex = (int)meteoroid.Position.y;
        }
    }
}
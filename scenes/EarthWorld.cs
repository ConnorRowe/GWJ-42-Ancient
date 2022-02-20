using Godot;
using System;

namespace Ancient
{
    public class EarthWorld : Node2D
    {
        private const float massGoal = 1000000f;
        private static readonly PackedScene meteorExplosionScene = GD.Load<PackedScene>("res://scenes/MeteorExplosion.tscn");
        private static readonly PackedScene meteoroidScene = GD.Load<PackedScene>("res://scenes/Meteoroid.tscn");
        private static readonly PackedScene endCutscene = GD.Load<PackedScene>("res://scenes/EndCutscene.tscn");
        private static readonly Texture deadHeartTex = GD.Load<Texture>("res://textures/UI/deadheart.png");
        public static float PlayerStartMass { get; set; } = 100000f;
        public static float SpeedRunStartTime { get; set; } = 0f;

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
        private AudioStreamPlayer explosionPlayer;
        private TextureRect[] hearts = new TextureRect[3];
        private HandDrawnMass speedRunTimeText;

        private float timeFrac = 0;
        private float dashShakeTime = 0f;
        private readonly Vector2 earthMidPoint = new Vector2(960, 712);
        private readonly Rect2 earthBounds = new Rect2(212, 588, 1800, 540);
        private bool spawnCircle = false;
        private int playerHealth = 3;
        private bool canDamagePlayer = true;
        private float speedRunTime = 0;

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
            GetNode<Timer>("Player/MeteoroidTimer").Connect("timeout", this, nameof(SpawnRandomMeteoroid));
            explosionPlayer = GetNode<AudioStreamPlayer>("ExplosionPlayer");
            player.Connect(nameof(Player.TookDamage), this, nameof(PlayerTookDamage));
            player.Mass = PlayerStartMass;
            player.GetNode<CollisionShape2D>("HeadShape").Disabled = true;
            GetNode("Player/SpecialMeteoroidTimer").Connect("timeout", this, nameof(SpecialMeteoroidTimeout));
            hearts[0] = GetNode<TextureRect>("UI/Heart1");
            hearts[1] = GetNode<TextureRect>("UI/Heart2");
            hearts[2] = GetNode<TextureRect>("UI/Heart3");
            speedRunTimeText = GetNode<HandDrawnMass>("UI/SpeedRunTimer");
            speedRunTimeText.DrawKG = false;
            speedRunTime = SpeedRunStartTime;

            SpawnRandomMeteoroid();
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            speedRunTime += delta;

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

            if (playerHealth < 0)
            {
                camera.Zoom = camera.Zoom.LinearInterpolate(new Vector2(.66f, .66f), delta * 10f);
            }

            if (player.Mass >= massGoal)
            {
                //YOU WON!
                SpeedRunStartTime = speedRunTime;
                GetTree().ChangeSceneTo(endCutscene);
                QueueFree();
            }

            TimeSpan time = TimeSpan.FromSeconds(speedRunTime);
            speedRunTimeText.Text = time.ToString(@"mm\:ss\:fff");
        }

        private void ShakeDash()
        {
            dashShakeTime = 1f;
        }

        private void MakeBloodBurst(Vector2 globalPos)
        {
            GameWorld.AddMetaballParticles(metaballViewport, GameWorld.bloodBurstScene, globalPos);
        }

        private void PlayerTookDamage()
        {
            MakeBloodBurst(player.GlobalPosition);

            if (canDamagePlayer && !player.IsInputLocked)
            {
                playerHealth--;

                canDamagePlayer = false;
                GetTree().CreateTimer(1f).Connect("timeout", this, nameof(ResetCanDamagePlayer));

                if (playerHealth == 2)
                    hearts[2].Texture = deadHeartTex;
                else if (playerHealth == 1)
                    hearts[1].Texture = deadHeartTex;
                else if (playerHealth == 0)
                    hearts[0].Texture = deadHeartTex;
                else if (playerHealth < 0)
                {
                    player.IsInputLocked = true;
                    player.GetNode<ColorRect>("Shadow").Visible = false;
                    animPlayer.Play("PlayerDead");

                    GetTree().CreateTimer(4f).Connect("timeout", this, nameof(GameLost));
                }
            }
        }

        private void MakeMeteorExplosion(Vector2 globalPos)
        {
            GameWorld.AddMetaballParticles(metaballViewport, meteorExplosionScene, globalPos);
            explosionPlayer.Play();
        }

        private void SpawnRandomMeteoroid()
        {
            SpawnMeteoroid(new Vector2(Globals.RNG.RandfRange(236, 1684), Globals.RNG.RandfRange(600, 1000)));
        }

        private void SpawnMeteoroid(Vector2 position)
        {
            Meteoroid meteoroid = meteoroidScene.Instance<Meteoroid>();
            AddChild(meteoroid);
            meteoroid.Position = position;
            meteoroid.Connect(nameof(Meteoroid.Exploded), this, nameof(MakeMeteorExplosion));
            meteoroid.ZIndex = (int)meteoroid.Position.y;
        }

        private void SpawnMeteorRing(float radius)
        {
            foreach (Vector2 pos in Meteoroid.GetSpacedCircumferancePoints((int)radius / 64, earthMidPoint, radius))
            {
                SpawnMeteoroid(pos);
            }
        }

        private void SpawnMeteorLine(Vector2 pointA, Vector2 pointB)
        {
            int num = (int)(pointA.DistanceTo(pointB) / 200);

            foreach (Vector2 pos in Meteoroid.GetSpacedPointsAlongLine(num, pointA, pointB))
            {
                SpawnMeteoroid(pos);
            }
        }

        private void SpecialMeteoroidTimeout()
        {
            if (spawnCircle)
            {
                if (player.Mass / massGoal > .5f && Globals.RNG.Randf() > .5f)
                {
                    SpawnMeteorRing(600);
                    SpawnMeteorRing(200);
                }
                SpawnMeteorRing(Globals.RNG.RandfRange(200, 600));
            }
            else
            {
                int n = Globals.RNG.RandiRange(0, 3);
                Vector2 a = Vector2.Zero;
                Vector2 b = Vector2.Zero;

                switch (n)
                {
                    case 0:
                        a = earthBounds.Position;
                        b = earthBounds.Position + earthBounds.Size;
                        break;
                    case 1:
                        a = earthBounds.Position + new Vector2(earthBounds.Size.x, 0);
                        b = earthBounds.Position + new Vector2(0, earthBounds.Size.y);
                        break;
                    case 2:
                        a = earthMidPoint - new Vector2(0, earthBounds.Size.y / 2);
                        b = earthMidPoint + new Vector2(0, earthBounds.Size.y / 2);
                        break;
                    case 3:
                        a = earthMidPoint - new Vector2(0, earthBounds.Size.y / 2);
                        b = earthMidPoint + new Vector2(0, earthBounds.Size.y / 2);
                        break;
                }

                SpawnMeteorLine(a, b);
            }

            spawnCircle = !spawnCircle;
        }

        private void ResetCanDamagePlayer()
        {
            canDamagePlayer = true;
        }

        private void GameLost()
        {
            GetTree().ChangeSceneTo(GameWorld.deadMenuScene);
            QueueFree();
        }
    }
}
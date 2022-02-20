using Godot;

namespace Ancient
{
    public class MainMenuChar : Character
    {
        public FoodPlant TargetFood { get; set; } = null;

        private AnimationPlayer animPlayer;
        private AudioStreamPlayer chompPlayer;
        private MainMenu mainMenu;
        private Vector2 targetScale = new Vector2(.5f, .5f);

        public override void _Ready()
        {
            base._Ready();

            animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            chompPlayer = GetNode<AudioStreamPlayer>("ChompPlayer");
            mainMenu = GetParent().GetParent<MainMenu>();

            animPlayer.Play("Walk");

            Scale = new Vector2(.5f, .5f);
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            if (TargetFood != null && IsInstanceValid(TargetFood) && Position.DistanceSquaredTo(TargetFood.Position) <= 1600f)
            {
                mainMenu.RemoveFood(TargetFood);
                TargetFood.QueueFree();
                TargetFood = null;
                chompPlayer.Play();

                targetScale += new Vector2(.03f, .03f);

                if (mainMenu.FoodCount() > 0)
                    TargetFood = mainMenu.NextFood();
            }

            animPlayer.PlaybackSpeed = ((velocity.LengthSquared() + externalVelocity.LengthSquared()) * delta * .0002f);
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            Scale = Scale.LinearInterpolate(targetScale, delta * 10f);
        }

        protected override Vector2 GetInputDir()
        {
            if (TargetFood != null && IsInstanceValid(TargetFood))
            {
                return Position.DirectionTo(TargetFood.Position);
            }

            return base.GetInputDir();
        }
    }
}
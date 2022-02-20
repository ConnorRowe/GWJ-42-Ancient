using Godot;

namespace Ancient
{
    public class Instructions : Node2D
    {
        private static readonly PackedScene gameScene = GD.Load<PackedScene>("res://scenes/GameWorld.tscn");

        public override void _Ready()
        {
            base._Ready();

            GetNode<AnimationPlayer>("AnimationPlayer").Play("Intro");
        }

        private void StartGame()
        {
            GetTree().ChangeSceneTo(gameScene);
            QueueFree();
        }
    }
}
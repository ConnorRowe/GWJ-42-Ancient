using Godot;

namespace Ancient
{
	public class SplashScreen : Node2D
	{
		public override void _Ready()
		{
			base._Ready();

			GetNode<AnimationPlayer>("AnimationPlayer").Play("Splash");
		}

		public override void _Input(InputEvent evt)
		{
			base._Input(evt);

			if ((evt is InputEventMouseButton emb && emb.Pressed) || (evt is InputEventKey ek && ek.Pressed && ek.Scancode == (int)KeyList.Space))
			{
				FinishSplashScreen();
			}
		}

		private void FinishSplashScreen()
		{
			GetTree().ChangeSceneTo(GD.Load<PackedScene>("res://scenes/menus/MainMenu.tscn"));
			QueueFree();
		}
	}
}

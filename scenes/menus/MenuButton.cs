using Godot;

namespace Ancient
{
    public class MenuButton : TextureButton
    {
        [Export(PropertyHint.File, "*.tscn")]
        private string nextScenePath;
        private bool hovered = false;
        private float hoverAmount = 0f;
        private float timeFrac = 0f;
        private Vector2 origin;

        public override void _Ready()
        {
            base._Ready();

            origin = RectPosition;
            Connect("mouse_entered", this, nameof(SetHovered), new Godot.Collections.Array(true));
            Connect("mouse_exited", this, nameof(SetHovered), new Godot.Collections.Array(false));
            Connect("pressed", this, nameof(Clicked));
        }

        private void SetHovered(bool shouldHover)
        {
            hovered = shouldHover;

            if (shouldHover)
                GlobalSound.UIClick();
        }

        private void Clicked()
        {
            if (!nextScenePath.Empty())
            {
                GetTree().ChangeScene(nextScenePath);

                Owner.QueueFree();
            }
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            timeFrac += delta;
            if (timeFrac > 1f)
                timeFrac--;

            if (hovered)
            {
                hoverAmount += delta * 10f;
                if (hoverAmount > 1f)
                    hoverAmount = 1f;
            }
            else if (hoverAmount > 0f)
            {
                hoverAmount -= delta * 10f;
                if (hoverAmount < 0f)
                    hoverAmount = 0f;
            }

            if (hoverAmount <= 0f)
                return;

            float yTimeOffset = timeFrac + .5f;
            if (yTimeOffset > 1f)
                yTimeOffset--;

            Vector2 shake = new Vector2(GameWorld.ShakeCurve.InterpolateBaked(timeFrac), GameWorld.ShakeCurve.InterpolateBaked(yTimeOffset)) * hoverAmount;
            RectPosition = origin + shake;
        }
    }
}
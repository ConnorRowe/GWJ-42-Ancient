using Godot;
using System.Collections.Generic;

namespace Ancient
{
    public class MainMenu : Node2D
    {
        private static readonly PackedScene foodScene = GD.Load<PackedScene>("res://scenes/FoodPlant.tscn");
        private float timeFrac = 0f;
        private List<FoodPlant> food = new List<FoodPlant>();
        private MainMenuChar player;
        private YSort ySort;
        private HSlider masterSlider;
        private HSlider musicSlider;
        private HSlider sfxSlider;
        private CheckButton fullscreenToggle;
        private Control settingsControls;
        private TextureButton playButton;
        private TextureButton settingsButton;
        private TextureRect logo;
        private int masterBusId;
        private int musicBusId;
        private int sfxBusId;

        public override void _Ready()
        {
            base._Ready();

            player = GetNode<MainMenuChar>("YSort/MainMenuChar");
            ySort = GetNode<YSort>("YSort");
            masterSlider = GetNode<HSlider>("SettingsControls/MasterSlider");
            musicSlider = GetNode<HSlider>("SettingsControls/MusicSlider");
            sfxSlider = GetNode<HSlider>("SettingsControls/SFXSlider");
            fullscreenToggle = GetNode<CheckButton>("SettingsControls/FullscreenToggle");
            logo = GetNode<TextureRect>("Logo");

            masterBusId = AudioServer.GetBusIndex("Master");
            musicBusId = AudioServer.GetBusIndex("Music");
            sfxBusId = AudioServer.GetBusIndex("SFX");

            masterSlider.Value = GetBusVol(masterBusId);
            musicSlider.Value = GetBusVol(musicBusId);
            sfxSlider.Value = GetBusVol(sfxBusId);
            fullscreenToggle.Pressed = OS.WindowFullscreen;

            masterSlider.Connect("value_changed", this, nameof(MasterChanged));
            musicSlider.Connect("value_changed", this, nameof(MusicChanged));
            sfxSlider.Connect("value_changed", this, nameof(SFXChanged));
            fullscreenToggle.Connect("toggled", this, nameof(FullscreenToggled));

            masterSlider.Connect("mouse_entered", this, nameof(UIClick));
            musicSlider.Connect("mouse_entered", this, nameof(UIClick));
            sfxSlider.Connect("mouse_entered", this, nameof(UIClick));
            fullscreenToggle.Connect("mouse_entered", this, nameof(UIClick));

            playButton = GetNode<TextureButton>("PlayButton");
            settingsButton = GetNode<TextureButton>("SettingsButton");
            settingsControls = GetNode<Control>("SettingsControls");

            settingsButton.Connect("pressed", this, nameof(ToggleSettingsMenu), new Godot.Collections.Array(true));
            GetNode("SettingsControls/SettingsButton").Connect("pressed", this, nameof(ToggleSettingsMenu), new Godot.Collections.Array(false));
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            timeFrac += delta;
            if (timeFrac > 1f)
                timeFrac--;
        }

        public override void _Input(InputEvent evt)
        {
            base._Input(evt);

            if (evt is InputEventMouseButton emb && emb.Pressed)
            {
                FoodPlant newFood = foodScene.Instance<FoodPlant>();
                ySort.AddChild(newFood);
                newFood.Position = emb.Position;
                food.Add(newFood);

                if (food.Count == 1)
                    player.TargetFood = food[0];
            }
        }

        private void ToggleSettingsMenu(bool toggle)
        {
            settingsControls.Visible = toggle;
            playButton.Visible = settingsButton.Visible = !toggle;
            logo.Visible = !toggle;
        }

        public void RemoveFood(FoodPlant foodToRemove)
        {
            food.Remove(foodToRemove);
        }

        public int FoodCount()
        {
            return food.Count;
        }

        public FoodPlant NextFood()
        {
            return food[0];
        }

        private void UIClick()
        {
            GlobalSound.UIClick();
        }

        private float GetBusVol(int busId)
        {
            return GD.Db2Linear(AudioServer.GetBusVolumeDb(busId));
        }

        private void SetBusVol(int busId, float vol)
        {
            AudioServer.SetBusVolumeDb(busId, GD.Linear2Db(vol));
        }

        private void MasterChanged(float vol)
        {
            SetBusVol(masterBusId, vol);
        }

        private void MusicChanged(float vol)
        {
            SetBusVol(musicBusId, vol);
        }

        private void SFXChanged(float vol)
        {
            SetBusVol(sfxBusId, vol);
        }

        private void FullscreenToggled(bool toggle)
        {
            OS.WindowFullscreen = toggle;
        }
    }
}

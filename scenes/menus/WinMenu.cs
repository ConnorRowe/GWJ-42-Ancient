using Godot;
using System;

namespace Ancient
{
    public class WinMenu : Node2D
    {
        public override void _Ready()
        {
            base._Ready();

            TimeSpan time = TimeSpan.FromSeconds(EarthWorld.SpeedRunStartTime);
            var speedRunTime = GetNode<HandDrawnMass>("SpeedRunTime");
            speedRunTime.DrawKG = false;
            speedRunTime.Text = time.ToString(@"mm\:ss\:fff");
        }
    }
}
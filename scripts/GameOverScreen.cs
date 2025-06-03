using Godot;
using System;

namespace Asteroids.Scripts;

public partial class GameOverScreen : Control
{
    private void OnRestartButtonPressed()
    {
        // Emit a signal to restart the game
        GetTree().ReloadCurrentScene();
    }
}
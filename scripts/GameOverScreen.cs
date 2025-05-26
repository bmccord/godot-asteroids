using Godot;
using System;

public partial class GameOverScreen : Control
{
    private void OnRestartButtonPressed()
    {
        // Emit a signal to restart the game
        GetTree().ReloadCurrentScene();
    }
}
using Godot;
using System;
using GodotUtilities;

namespace Asteroids.Scripts;

[Scene]
public partial class Hud : Control
{
    [Node]
    private Label _score;
    [Node]
    private HBoxContainer _lives;
    private PackedScene _uiLifeScene;

    public int Score
    {
        set => _score.Text = "SCORE: " + value;
    }
    
    public override void _Notification(int what) {
        if (what == NotificationSceneInstantiated) {
            WireNodes(); // this is a generated method
        }
    }

    public override void _Ready()
    {
        
        _uiLifeScene = GD.Load<PackedScene>("res://scenes/ui_life.tscn");
    }

    public void UpdateLives(int amount)
    {
        foreach (var life in _lives.GetChildren())
        {
            life.QueueFree();
        }

        for (int i = 0; i < amount; i++)
        {
            var life = _uiLifeScene.Instantiate();
            _lives.AddChild(life);
        }
    }
}
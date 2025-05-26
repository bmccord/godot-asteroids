using Godot;
using System;

public partial class Hud : Control
{
    private Label _score;
    private HBoxContainer _livesContainer;
    private PackedScene _uiLifeScene;

    public int Score
    {
        set => _score.Text = "SCORE: " + value;
    }

    public override void _Ready()
    {
        _score = GetNode<Label>("Score");
        _livesContainer = GetNode<HBoxContainer>("Lives");
        _uiLifeScene = GD.Load<PackedScene>("res://scenes/ui_life.tscn");
    }

    public void UpdateLives(int amount)
    {
        foreach (var life in _livesContainer.GetChildren())
        {
            life.QueueFree();
        }

        for (int i = 0; i < amount; i++)
        {
            var life = _uiLifeScene.Instantiate();
            _livesContainer.AddChild(life);
        }
    }
}
using Godot;
using System;
using GodotUtilities;

namespace Asteroids.Scripts;

[Scene]
public partial class Game : Node2D
{
    [Node]
    private Node _lasers;
    [Node]
    private Player _player;
    [Node]
    private Node _asteroids;
    [Node]  
    private Node2D _playerSpawnPosition;
    [Node ("UI/GameOverScreen")]
    private Control _gameOverScreen;
    [Node ("PlayerSpawnPosition/PlayerSpawnArea")]
    private PlayerSpawnArea _playerSpawnArea;
    [Node ("UI/HUD")]
    private Hud _hud;

    private PackedScene _asteroidScene;


    private int _score = 0;

    private int Score
    {
        get => _score;
        set
        {
            _score = value;
            _hud.Score = _score;
        }
    }

    private int _lives = 3;

    private int Lives
    {
        set
        {
            _lives = value;
            _hud.UpdateLives(_lives);
        }
        get => _lives;
    }
    
    public override void _Notification(int what) {
        if (what == NotificationSceneInstantiated) {
            WireNodes(); // this is a generated method
        }
    }
    
    public override void _Ready()
    {
        _asteroidScene = GD.Load<PackedScene>("res://scenes/asteroid.tscn");

        _player.LaserShot += OnPlayerLaserShot;
        _player.Died += OnPlayerDied;
        _gameOverScreen.Visible = false;

        foreach (Asteroid asteroid in _asteroids.GetChildren())
        {
            asteroid.Exploded += OnAsteroidExploded;
        }

        Score = 0;
        Lives = 3;
    }

    private async void OnPlayerDied()
    {
        _player.GlobalPosition = _playerSpawnPosition.GlobalPosition;
        GetNode<AudioStreamPlayer>("PlayerDieSound").Play();
        Lives -= 1;

        if (Lives <= 0)
        {
            await ToSignal(GetTree().CreateTimer(2), SceneTreeTimer.SignalName.Timeout);
            _gameOverScreen.Visible = true;
        }
        else
        {
            await ToSignal(GetTree().CreateTimer(1), SceneTreeTimer.SignalName.Timeout);
            while (!_playerSpawnArea.IsEmpty)
            {
                await ToSignal(GetTree().CreateTimer(0.1), SceneTreeTimer.SignalName.Timeout);
            }

            _player.Respawn(_playerSpawnPosition.GlobalPosition);
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("restart"))
        {
            GetTree().ReloadCurrentScene();
        }
    }

    private void OnAsteroidExploded(Vector2 position, string size, int points)
    {
        GetNode<AudioStreamPlayer>("AsteroidHitSound").Play();
        var asteroidSize = Enum.Parse(typeof(Asteroid.AsteroidType), size);
        Score += points;

        for (int i = 0; i < 2; i++)
        {
            switch (asteroidSize)
            {
                case Asteroid.AsteroidType.Large:
                    SpawnAsteroid(position, Asteroid.AsteroidType.Medium);
                    break;
                case Asteroid.AsteroidType.Medium:
                    SpawnAsteroid(position, Asteroid.AsteroidType.Small);
                    break;
                case Asteroid.AsteroidType.Small:
                    // Do nothing
                    break;
            }
        }
    }

    private void SpawnAsteroid(Vector2 position, Asteroid.AsteroidType size)
    {
        var asteroid = _asteroidScene.Instantiate<Asteroid>();
        asteroid.GlobalPosition = position;
        asteroid.Size = size;
        asteroid.Exploded += OnAsteroidExploded;
        _asteroids.CallDeferred(Node.MethodName.AddChild, asteroid);
    }

    private void OnPlayerLaserShot(Laser laser)
    {
        GetNode<AudioStreamPlayer>("LaserSound").Play();
        _lasers.AddChild(laser);
    }
}
using Godot;
using System;
using System.Threading.Tasks;
using GodotUtilities;

namespace Asteroids.Scripts;

[Scene]
public partial class Player : CharacterBody2D
{
    [Signal]
    public delegate void LaserShotEventHandler(Laser laser);

    [Signal]
    public delegate void DiedEventHandler();

    [Export] private float _acceleration = 10.0f;
    [Export] private float _maxSpeed = 350.0f;
    [Export] private float _rotationSpeed = 250.0f;

    [Node]
    private Node2D _muzzle;
    [Node]
    private Sprite2D _playerSprite;
    [Node]
    private CollisionShape2D _playerCollisionShape;
    
    private PackedScene _laserScene = GD.Load<PackedScene>("res://scenes/laser.tscn");

    private bool _shootCooldown = false;
    private float _rateOfFire = 0.15f;

    private bool _alive = true;

    public override void _Notification(int what) {
        if (what == NotificationSceneInstantiated) {
            WireNodes(); // this is a generated method
        }
    }

    public override async void _Process(double delta)
    {
        if (!_alive)
        {
            return;
        }

        if (!Input.IsActionPressed("shoot")) return;
        if (_shootCooldown) return;
        _shootCooldown = true;
        ShootLaser();
        await ToSignal(GetTree().CreateTimer(_rateOfFire), SceneTreeTimer.SignalName.Timeout);
        _shootCooldown = false;
    }

    private void ShootLaser()
    {
        var l = _laserScene.Instantiate<Laser>();
        l.GlobalPosition = _muzzle.GlobalPosition;
        l.Rotation = Rotation;

        EmitSignal("LaserShot", l);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_alive)
        {
            return;
        }

        var inputVector = new Vector2(0, Input.GetAxis("move_forward", "move_backward"));

        Velocity += inputVector.Rotated(Rotation) * _acceleration;
        Velocity = Velocity.LimitLength(_maxSpeed);

        if (Input.IsActionPressed("rotate_right"))
        {
            Rotate(float.DegreesToRadians(_rotationSpeed * (float)delta));
        }

        if (Input.IsActionPressed("rotate_left"))
        {
            Rotate(float.DegreesToRadians(-_rotationSpeed * (float)delta));
        }

        if (inputVector == Vector2.Zero)
        {
            Velocity = Velocity.MoveToward(Vector2.Zero, 3);
        }

        MoveAndSlide();

        var screenSize = GetViewportRect().Size;
        if (GlobalPosition.Y < 0)
        {
            SetGlobalPosition(new Vector2(GlobalPosition.X, screenSize.Y));
        }
        else if (GlobalPosition.Y > screenSize.Y)
        {
            SetGlobalPosition(new Vector2(GlobalPosition.X, 0));
        }

        if (GlobalPosition.X < 0)
        {
            SetGlobalPosition(new Vector2(screenSize.X, GlobalPosition.Y));
        }
        else if (GlobalPosition.X > screenSize.X)
        {
            SetGlobalPosition(new Vector2(0, GlobalPosition.Y));
        }
    }

    private void Die()
    {
        if (!_alive) return;
        _alive = false;
        _playerSprite.Visible = false;
        _playerCollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        EmitSignal("Died");
    }

    public void Respawn(Vector2 position)
    {
        if (_alive) return;
        _alive = true;
        GlobalPosition = position;
        Velocity = Vector2.Zero;
        _playerSprite.Visible = true;
        _playerCollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
    }
}
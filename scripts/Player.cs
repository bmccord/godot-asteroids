using Godot;
using System;
using System.Threading.Tasks;

public partial class Player : CharacterBody2D
{
    [Signal]
    public delegate void LaserShotEventHandler(Laser laser);

    [Signal]
    public delegate void DiedEventHandler();

    [Export] private float _acceleration = 10.0f;
    [Export] private float _maxSpeed = 350.0f;
    [Export] private float _rotationSpeed = 250.0f;

    private Node2D _muzzle;
    private PackedScene _laserScene = GD.Load<PackedScene>("res://scenes/laser.tscn");
    private Sprite2D _sprite;
    private CollisionShape2D _collisionShape;

    private bool _shootCooldown = false;
    private float _rateOfFire = 0.15f;

    private bool _alive = true;

    public override void _Ready()
    {
        _muzzle = GetNode<Node2D>("Muzzle");
        _sprite = GetNode<Sprite2D>("Sprite2D");
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
    }

    public override async void _Process(double delta)
    {
        if (!_alive)
        {
            return;
        }

        if (Input.IsActionPressed("shoot"))
        {
            if (!_shootCooldown)
            {
                _shootCooldown = true;
                ShootLaser();
                await ToSignal(GetTree().CreateTimer(_rateOfFire), SceneTreeTimer.SignalName.Timeout);
                _shootCooldown = false;
            }
        }
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

    public void Die()
    {
        if (_alive)
        {
            _alive = false;
            _sprite.Visible = false;
            _collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
            EmitSignal("Died");
        }
    }

    public void Respawn(Vector2 position)
    {
        if (!_alive)
        {
            _alive = true;
            GlobalPosition = position;
            Velocity = Vector2.Zero;
            _sprite.Visible = true;
            _collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
        }
    }
}
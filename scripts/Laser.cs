using Godot;
using System;

public partial class Laser : Area2D
{
    private Vector2 _movementVector = new Vector2(0, -1);
    [Export] private float _speed = 500.0f;
    
    private void OnAreaEntered(Area2D area)
    {
        if (area is Asteroid asteroid)
        {
            asteroid.Explode();
            QueueFree();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += _movementVector.Rotated(Rotation) * _speed * (float)delta;
    }

    private void OnScreenExit()
    {
        QueueFree();
    }
}

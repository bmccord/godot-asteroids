using Godot;
using GodotUtilities;

namespace Asteroids.Scripts;

[Scene]
public partial class Asteroid : Area2D
{
    [Signal]
    public delegate void ExplodedEventHandler(Vector2 position, string size, int points);

    private Vector2 _movementVector = new Vector2(0, -1);
    private float _speed = 50.0f;

    public enum AsteroidType
    {
        Small,
        Medium,
        Large
    }

    [Node]
    private Sprite2D _asteroidSprite;
    [Node]
    private CollisionShape2D _asteroidCollisionShape;

    private int Points
    {
        get
        {
            return Size switch
            {
                AsteroidType.Small => 25,
                AsteroidType.Medium => 50,
                AsteroidType.Large => 100,
                _ => 0
            };
        }
    }

    [Export] public AsteroidType Size = AsteroidType.Large;

    public override void _Notification(int what) {
        if (what == NotificationSceneInstantiated) {
            WireNodes(); // this is a generated method
        }
    }
    
    public override void _Ready()
    {
        // Randomize the rotation of the asteroid
        Rotation = (float)GD.RandRange(0, Mathf.Pi * 2);

        switch (Size)
        {
            case AsteroidType.Small:
                _speed = GD.RandRange(100, 200);
                _asteroidSprite.Texture = GD.Load<Texture2D>("res://assets/sprites/meteor_small.png");
                _asteroidCollisionShape.SetDeferred(CollisionShape2D.PropertyName.Shape,
                    GD.Load<Shape2D>("res://resources/asteroid_cshape_small.tres"));
                break;
            case AsteroidType.Medium:
                _speed = GD.RandRange(100, 150);
                _asteroidSprite.Texture = GD.Load<Texture2D>("res://assets/sprites/meteor_medium.png");
                _asteroidCollisionShape.SetDeferred(CollisionShape2D.PropertyName.Shape,
                    GD.Load<Shape2D>("res://resources/asteroid_cshape_medium.tres"));
                break;
            case AsteroidType.Large:
                _speed = GD.RandRange(50, 100);
                _asteroidSprite.Texture = GD.Load<Texture2D>("res://assets/sprites/meteor_large.png");
                _asteroidCollisionShape.SetDeferred(CollisionShape2D.PropertyName.Shape,
                    GD.Load<Shape2D>("res://resources/asteroid_cshape_large.tres"));
                break;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += _movementVector.Rotated(Rotation) * _speed * (float)delta;

        float radius = 0f;

        if (_asteroidCollisionShape.Shape is CircleShape2D circleShape)
        {
            radius = circleShape.Radius;
        }

        var screenSize = GetViewportRect().Size;
        if (GlobalPosition.Y + radius < 0)
        {
            GlobalPosition = new Vector2(GlobalPosition.X, screenSize.Y + radius);
        }
        else if (GlobalPosition.Y - radius > screenSize.Y)
        {
            GlobalPosition = new Vector2(GlobalPosition.X, -radius);
        }

        if (GlobalPosition.X + radius < 0)
        {
            GlobalPosition = new Vector2(screenSize.X, GlobalPosition.Y + radius);
        }
        else if (GlobalPosition.X - radius > screenSize.X)
        {
            GlobalPosition = new Vector2(-radius, GlobalPosition.Y);
        }
    }

    public void Explode()
    {
        EmitSignal("Exploded", GlobalPosition, Size.ToString(), Points);
        QueueFree();
    }

    private static void OnBodyEntered(Node body)
    {
        if (body is Player player)
        {
            player.CallDeferred(Player.MethodName.Die);
        }
    }
}
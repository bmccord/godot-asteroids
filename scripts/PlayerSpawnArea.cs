using Godot;
using System;

namespace Asteroids.Scripts;

public partial class PlayerSpawnArea : Area2D
{
    public bool IsEmpty => !HasOverlappingAreas() && !HasOverlappingBodies();
}
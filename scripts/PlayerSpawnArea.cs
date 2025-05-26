using Godot;
using System;

public partial class PlayerSpawnArea : Area2D
{
    public bool IsEmpty => !HasOverlappingAreas() && !HasOverlappingBodies();
}
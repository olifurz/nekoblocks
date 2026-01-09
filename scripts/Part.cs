using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class Part : Node3D
{
    private Vector3 _size = Vector3.One;
    [Export]
    public Vector3 Size
    {
        get => _size;
        set
        {
            _size = value;
            if (IsInsideTree()) Update();
        }
    }

    private int[] _surfaces = new int[6] { 1, 2, 0, 0, 0, 0 };
    [Export]
    public int[] Surfaces
    {
        get => _surfaces;
        set
        {
            _surfaces = value;
            if (IsInsideTree()) Update();
        }
    }

    private Color _brickColor;
    [Export]
    public Color BrickColor
    {
        get => _brickColor;
        set
        {
            _brickColor = value;
            if (IsInsideTree()) Update();
        }
    }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

    }

    public void Update()
    {
        var meshInstance = GetNodeOrNull<MeshInstance3D>("Mesh");
        var collider = GetNodeOrNull<CollisionShape3D>("Collider");

        if (meshInstance == null || collider == null) return;

        if (meshInstance.Mesh is BoxMesh box) box.Size = _size;
        if (collider.Shape is BoxShape3D shape) shape.Size = _size;

        meshInstance.SetInstanceShaderParameter("brick_color", _brickColor);
        meshInstance.SetInstanceShaderParameter("surf_top", _surfaces[0]);
        meshInstance.SetInstanceShaderParameter("surf_bottom", _surfaces[1]);
        meshInstance.SetInstanceShaderParameter("surf_front", _surfaces[2]);
        meshInstance.SetInstanceShaderParameter("surf_back", _surfaces[3]);
        meshInstance.SetInstanceShaderParameter("surf_left", _surfaces[4]);
        meshInstance.SetInstanceShaderParameter("surf_right", _surfaces[5]);
    }
}

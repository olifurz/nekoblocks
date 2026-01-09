using Godot;

namespace Nekoblocks.scripts;

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
            Update();
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
            Update();
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
            Update();
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

        if (meshInstance.Mesh is BoxMesh box)
        {
            var uniqueBox = (BoxMesh)box.Duplicate();
            uniqueBox.Size = _size;
            meshInstance.Mesh = uniqueBox;
        }

        if (collider.Shape is BoxShape3D shape)
        {
            var uniqueShape = (BoxShape3D)shape.Duplicate();
            uniqueShape.Size = _size;
            collider.Shape = uniqueShape;
        }

        meshInstance.SetInstanceShaderParameter("brick_color", _brickColor);
        meshInstance.SetInstanceShaderParameter("surf_top", _surfaces[0]);
        meshInstance.SetInstanceShaderParameter("surf_bottom", _surfaces[1]);
        meshInstance.SetInstanceShaderParameter("surf_front", _surfaces[2]);
        meshInstance.SetInstanceShaderParameter("surf_back", _surfaces[3]);
        meshInstance.SetInstanceShaderParameter("surf_left", _surfaces[4]);
        meshInstance.SetInstanceShaderParameter("surf_right", _surfaces[5]);
    }
}
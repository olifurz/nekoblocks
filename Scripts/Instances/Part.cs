using System;
using System.Collections.Generic;
using Godot;

namespace Nekoblocks.Instances;

[Tool]
public partial class Part : RigidBody3D
{
	private bool _anchored = true;
	private bool _castShadow = true;
	private bool _canCollide = true;
	private Vector3 _size = new(4, 1, 2);
	private Color _colour = new(0.5f, 0.5f, 0.5f);
	private float _transparency;
	private Shape _shape = Shape.Brick;
	private Dictionary<Surface, SurfaceType> _surfaces = new()
	{
		{ Surface.Top, SurfaceType.Studs },
		{ Surface.Bottom, SurfaceType.Inlets },
		{ Surface.Front, SurfaceType.Smooth },
		{ Surface.Back, SurfaceType.Smooth },
		{ Surface.Left, SurfaceType.Smooth },
		{ Surface.Right, SurfaceType.Smooth },
	};
	
	[Export] public bool Anchored { get => _anchored; set => SetAnchored(value); }
	[Export] public bool CanCollide { get => _canCollide; set => SetCanCollide(value); }
	[Export] public bool CastShadow { get => _castShadow; set => SetCastShadow(value); }
	[Export] public Vector3 Size { get => _size; set => SetSize(value); }
	[Export] public Color Color { get => _colour; set => SetColour(value); }
	[Export] public float Transparency { get => _transparency; set => SetTransparency(value); }
	[Export] public Shape Shape { get => _shape; set => SetShape(value); }
	public Dictionary<Surface, SurfaceType> Surfaces { get => _surfaces; set => SetSurfaces(value); }

	public MeshInstance3D GetMeshInstance() => GetNodeOrNull<MeshInstance3D>("Mesh");
	public CollisionShape3D GetCollider() => GetNodeOrNull<CollisionShape3D>("Collider");
	
	public override void _Ready()
	{
		ContactMonitor = true;
		UpdateMesh();
		UpdateCollider();
	}

	public override void _PhysicsProcess(double delta)
	{
		
	}
	private void UpdateMesh()
	{
		var meshInstance = GetMeshInstance();
		if (meshInstance == null) return;
		
		meshInstance.Mesh = _shape switch
		{
			Shape.Brick => new BoxMesh { Size = _size },
			Shape.Slope => new PrismMesh { Size = _size, LeftToRight = 0 },
			Shape.Cylinder => new CylinderMesh { BottomRadius = _size.X / 2, TopRadius = _size.X / 2, Height = _size.Y },
			Shape.Sphere => new SphereMesh { Radius = _size.X / 2, Height = _size.X },
			Shape.Capsule => new CapsuleMesh { Radius = _size.X / 2, Height = _size.Y },
			Shape.Torus => new TorusMesh { OuterRadius = _size.X },
			Shape.Head => GD.Load<Mesh>("res://Models/head.obj"),
			_ => throw new ArgumentOutOfRangeException()
		};

		meshInstance.Scale = _shape == Shape.Head ? _size : Vector3.One;

		if (meshInstance.GetSurfaceOverrideMaterial(0) != null) return;

		var shaderMaterial = GD.Load<ShaderMaterial>("res://Materials/surface.tres");
		shaderMaterial.Shader = GD.Load<Shader>("res://Shaders/surface.gdshader");
		meshInstance.SetSurfaceOverrideMaterial(0, shaderMaterial);

		SetSurfaces(_surfaces);
		SetColour(_colour);
	}

	private void UpdateCollider()
	{
		var collider = GetCollider();
		if (collider == null) return;
		
		collider.Shape = _shape switch
		{
			Shape.Brick => new BoxShape3D { Size = _size },
			Shape.Slope => new BoxShape3D { Size = _size }, // TODO: Slope collider
			Shape.Cylinder => new CylinderShape3D() { Radius = _size.X / 2 },
			Shape.Sphere => new SphereShape3D() { Radius = _size.X / 2 },
			Shape.Capsule => new CapsuleShape3D() { Radius = _size.X / 2, Height = _size.Y / 2},
			_ => new BoxShape3D { Size = _size }
		};
		
		collider.Disabled = !_canCollide;
	}

	public void SetAnchored(bool anchored)
	{
		_anchored = anchored;
		// Freeze = anchored;
	}
	
	public void SetCanCollide(bool canCollide)
	{
		_canCollide = canCollide;
		var collider = GetCollider();
		if (collider == null) return;
		collider.Disabled = !canCollide;
	}
	
	public void SetCastShadow(bool castShadow)
	{
		_castShadow = castShadow;
		var meshInstance = GetMeshInstance();
		var volume = _size.X * _size.Y * _size.Z;
		if (volume < 0.5f)
			meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
		else
			meshInstance.CastShadow = castShadow ? GeometryInstance3D.ShadowCastingSetting.On : GeometryInstance3D.ShadowCastingSetting.Off;
	}
	
	/// <summary>
	/// Set the mesh & collider size
	/// </summary>
	/// <param name="size">Size in Vector3</param>
	public void SetSize(Vector3 size)
	{
		_size = size;
		UpdateMesh();
		UpdateCollider();
	}
	
	/// <summary>
	/// Set the colour of the Part
	/// </summary>
	/// <param name="colour">Colour to use</param>
	/// <exception cref="NullReferenceException"></exception>
	public void SetColour(Color colour)
	{
		_colour = colour; 
		var mesh = GetMeshInstance();
		mesh?.SetInstanceShaderParameter("colour", colour);
	}

	/// <summary>
	/// Set transparency of Part
	/// </summary>
	/// <param name="transparency">Transparency (0 - 1)</param>
	public void SetTransparency(float transparency)
	{
		_transparency = transparency;
		Mathf.Clamp(transparency, 0, 1);
	}
	
	/// <summary>
	/// Set the shape of the part mesh
	/// </summary>
	/// <param name="shape"></param>
	public void SetShape(Shape shape)
	{
		_shape = shape;
		UpdateMesh();
		UpdateCollider();
	}

	/// <summary>
	/// Set the surface type of a certain surface
	/// </summary>
	/// <param name="surface"></param>
	/// <param name="surfaceType"></param>
	public void SetSurface(Surface surface, SurfaceType surfaceType)
	{
		_surfaces[surface] = surfaceType;
		var mesh = GetMeshInstance();
		mesh?.SetInstanceShaderParameter($"surf_{surface.ToString().ToLower()}", (int)surfaceType);
	}

	/// <summary>
	/// Set the surface types of all surfaces at once
	/// </summary>
	/// <param name="surfaces"></param>
	public void SetSurfaces(Dictionary<Surface, SurfaceType> surfaces)
	{
		_surfaces = surfaces;

		var meshInstance = GetMeshInstance();
		meshInstance.SetInstanceShaderParameter("surf_top", (int)surfaces[Surface.Top]);
		meshInstance.SetInstanceShaderParameter("surf_bottom", (int)surfaces[Surface.Bottom]);
		meshInstance.SetInstanceShaderParameter("surf_front", (int)surfaces[Surface.Front]);
		meshInstance.SetInstanceShaderParameter("surf_back", (int)surfaces[Surface.Back]);
		meshInstance.SetInstanceShaderParameter("surf_left", (int)surfaces[Surface.Left]);
		meshInstance.SetInstanceShaderParameter("surf_right", (int)surfaces[Surface.Right]);
	}
}

public enum Shape
{
	Brick,
	Slope,
	Cylinder,
	Sphere,
	Capsule,
	Torus,
	Head
}
public enum Surface
{
	Top,
	Bottom,
	Front,
	Back,
	Left,
	Right
}

public enum SurfaceType
{
	Smooth,
	Studs,
	Inlets,
	Weld,
	Glue,
	Universal
}

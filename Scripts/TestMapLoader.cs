using System.Collections.Generic;
using System.IO;
using Godot;
using RobloxFiles;
using FileAccess = Godot.FileAccess;
using Part = Nekoblocks.Instances.Part;

namespace Nekoblocks.Scripts;

public partial class TestMapLoader : Node
{
	private PackedScene _partPrefab;

	private RobloxFile _data;

	private readonly Dictionary<int, int> _surfaceMappings = new()
	{
		{ 0, 0 }, // Smooth (0) -> Smooth (0)
		{ 3, 1 }, // Studs (3) -> Stud (1)
		{ 4, 2 }, // Inlet (4) -> Inlet (2)
		{ 2, 3 }, // Weld (2) -> Weld (3)
		{ 1, 4 }, // Glue (1) -> Glue (4)
		{ 5, 5 }, // Universal (5) -> Universal (5)
		{ 10, 0 } // SmoothNoOutlines (10) -> Smooth (0)
	};
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_partPrefab = GD.Load<PackedScene>("res://Scenes/Prefabs/Part.tscn");


		using var file = FileAccess.Open("res://test.rbxl", FileAccess.ModeFlags.Read);
		if (file == null) throw new FileNotFoundException("Test file not found");

		_data = RobloxFile.Open(file.GetBuffer((long)file.GetLength()));
		
		CreateParts();
	}

	private void CreateParts()
	{
		foreach (var descendent in _data.GetDescendantsOfType<RobloxFiles.Part>())
		{
			var newPart = _partPrefab.Instantiate<Part>();
			newPart.Name = descendent.Name;

			var cf = descendent.CFrame;
			newPart.SetPosition(new Vector3(cf.X, cf.Y, cf.Z));
			
			var rot = cf.Rotation.ToEulerAngles();
			newPart.SetRotation(new Vector3(rot.Pitch, rot.Yaw, rot.Roll));
			
			newPart.Size = new Vector3(descendent.Size.X, descendent.Size.Y, descendent.Size.Z);
			
			var c = descendent.Color;
			newPart.BrickColor = new Color(c.R, c.G, c.B);
			
			int[] robloxSurfaces = {
				(int)descendent.TopSurface, (int)descendent.BottomSurface,
				(int)descendent.FrontSurface, (int)descendent.BackSurface,
				(int)descendent.LeftSurface, (int)descendent.RightSurface
			};
			for (var i = 0; i < 6; i++)
				newPart.Surfaces[i] = _surfaceMappings.GetValueOrDefault(robloxSurfaces[i], 0);
			
			AddChild(newPart);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Godot;
using Nekoblocks.Instances;
using RobloxFiles;
using RobloxFiles.Enums;
using FileAccess = Godot.FileAccess;
using Part = Nekoblocks.Instances.Part;
using SurfaceType = Nekoblocks.Instances.SurfaceType;

namespace Nekoblocks.Scripts;

public class RobloxMapLoader
{
    private PackedScene _partPrefab;
    private RobloxFile _data;

    private readonly Dictionary<int, SurfaceType> _surfaceMappings = new()
    {
        { 0, SurfaceType.Smooth }, { 3, SurfaceType.Studs }, { 4, SurfaceType.Inlets },
        { 2, SurfaceType.Weld }, { 1, SurfaceType.Glue }, { 5, SurfaceType.Universal },
        { 10, SurfaceType.Smooth }
    };

    private readonly Dictionary<PartType, Shape> _shapeMappings = new()
    {
        { PartType.Block, Shape.Brick },
        { PartType.Ball, Shape.Sphere },
        { PartType.Cylinder, Shape.Cylinder },
        { PartType.Wedge, Shape.Slope },
        { PartType.CornerWedge, Shape.Slope }
    };

    public void Load(string filePath)
    {
        _partPrefab = GD.Load<PackedScene>("res://Scenes/Prefabs/Part.tscn");

        using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        if (file == null) throw new FileNotFoundException("File not found");

        _data = RobloxFile.Open(file.GetBuffer((long)file.GetLength()));
        
        foreach (var robloxChild in _data.FindFirstChildOfClass<Workspace>().GetChildren())
        {
            ProcessRecursive(robloxChild, World.Workspace.Instance);
        }
    }

    private void ProcessRecursive(Instance robloxObject, Node godotParent)
    {
        Node nextParent = godotParent;

        switch (robloxObject)
        {
            case RobloxFiles.Part robloxPart:
            {
                var partNode = CreatePart(robloxPart);
                godotParent.AddChild(partNode);
            
                var cf = robloxPart.CFrame;
                partNode.GlobalPosition = new Vector3(cf.X, cf.Y, cf.Z);
                var rot = cf.Rotation.ToEulerAngles();
                partNode.GlobalRotation = new Vector3(rot.Pitch, rot.Yaw, rot.Roll);
            
                nextParent = partNode;
                break;
            }
            case Folder:
            case Model:
            {
                Node3D groupNode = new Node3D { Name = robloxObject.Name };
                godotParent.AddChild(groupNode);
                nextParent = groupNode;
                break;
            }
        }
        
        // Keep recursing through children
        foreach (var child in robloxObject.GetChildren())
        {
            ProcessRecursive(child, nextParent);
        }
    }

    private Part CreatePart(RobloxFiles.Part part)
    {
        var newPart = _partPrefab.Instantiate<Part>();
        newPart.Name = part.Name;
        newPart.CanCollide = part.CanCollide;
        newPart.CastShadow = part.CastShadow;
        newPart.Transparency = part.Transparency;
        newPart.Anchored = part.Anchored;
        
        newPart.SetSize(new Vector3(part.Size.X, part.Size.Y, part.Size.Z));
        var c = part.Color;
        newPart.SetColour(new Color(c.R, c.G, c.B));
        newPart.SetShape(_shapeMappings.GetValueOrDefault(part.Shape, Shape.Brick));

        int[] robloxSurfaces = {
            (int)part.TopSurface, (int)part.BottomSurface,
            (int)part.FrontSurface, (int)part.BackSurface,
            (int)part.LeftSurface, (int)part.RightSurface
        };
        var surfaceKeys = (Surface[])Enum.GetValues(typeof(Surface));

        for (var i = 0; i < surfaceKeys.Length; i++)
        {
            if (i >= robloxSurfaces.Length) break;
            var mappedValue = _surfaceMappings.GetValueOrDefault(robloxSurfaces[i], SurfaceType.Smooth);
            newPart.SetSurface(surfaceKeys[i], mappedValue);
        }

        return newPart;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Nekoblocks.LocalPlayer;

public partial class LocalPlayerSounds : Node
{
    private readonly List<AudioStreamPlayer> _playerPool = [];
    private readonly int _poolSize = 8;
    
    private readonly Dictionary<PlayerSound, AudioStream> _soundCache = new();

    public override void _Ready()
    {
        SetupPool();
        LoadSoundsFromFolder("res://Sounds/");
    }

    private void SetupPool()
    {
        for (var i = 0; i < _poolSize; i++)
        {
            var player = new AudioStreamPlayer { Bus = "SFX" };
            AddChild(player);
            _playerPool.Add(player);
        }
    }

    private void LoadSoundsFromFolder(string path)
    {
        using var dir = DirAccess.Open(path);
        if (dir == null) 
        {
            GD.PrintErr($"Directory not found: {path}");
            return;
        }

        dir.ListDirBegin();
        string fileName = dir.GetNext();

        while (fileName != "")
        {
            if (!dir.CurrentIsDir() && !fileName.EndsWith(".import"))
            {
                // Get name without extension
                string nameOnly = fileName.GetBaseName();
                
                // Pascal case as enum values have a capital letter
                if (Enum.TryParse(nameOnly.ToPascalCase(), out PlayerSound soundEnum))
                {
                    var stream = GD.Load<AudioStream>(path + fileName);
                    _soundCache[soundEnum] = stream;
                }
            }
            fileName = dir.GetNext();
        }
    }

    public void PlayOneShot(PlayerSound sound)
    {
        if (!_soundCache.TryGetValue(sound, out var stream)) return;

        var availablePlayer = _playerPool.FirstOrDefault(p => !p.Playing);
        if (availablePlayer == null) return;
        
        availablePlayer.Stream = stream;
        availablePlayer.Play();
    }
}

public enum PlayerSound
{
    Scroll,
}
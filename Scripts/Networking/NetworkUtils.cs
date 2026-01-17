using System;
using System.ComponentModel;
using System.Linq;
using Godot;

namespace Nekoblocks.Networking;

public static class NetworkUtils
{
    /// <summary>
    /// Check if the runtime is a Host or Client and not a headless Server
    /// </summary>
    public static bool IsPlayer => DisplayServer.GetName() != "headless";

    
    /// <summary>
    /// Get a Player based off an id
    /// </summary>
    /// <param name="id">ID to query with</param>
    /// <returns></returns>
    public static Player GetPlayerFromId(int id) => GetPlayer(GetUsernameFromId(id));
    
    /// <summary>
    /// Get a Player based off a username
    /// </summary>
    /// <param name="username">Username to query with</param>
    /// <returns></returns>
    public static Player GetPlayer(string username)
    {
        return GetPlayers().FirstOrDefault(p => p.Username == username);
    }

    /// <summary>
    /// Get an array of all Players
    /// </summary>
    /// <returns></returns>
    public static Player[] GetPlayers()
    {
        var playersContainer = GetSceneRoot().GetNode("%Players");
        return playersContainer == null ? throw new Exception("Players object is missing from the game!") : playersContainer.GetChildren().OfType<Player>().ToArray();
    }

    /// <summary>
    /// Fetch the Player object that belongs to the local player
    /// </summary>
    /// <returns>Player</returns>
    public static Player GetLocalPlayer()
    {
        return !IsPlayer ? throw new Exception("Attempted to get the local player on a server") : GetPlayers().FirstOrDefault(p => p.IsMultiplayerAuthority());
    }
    
    
    
    /// <summary>
    /// Fetch the username from the players list based on id
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Username</returns>
    public static string GetUsernameFromId(int id) // This feels like a stupid solution
    {
        // Treat server as 1
        var searchId = (id == 0) ? 1 : id;
        
        var players = GetSceneRoot().GetNode("%Players");
        var player = players.GetChildren()
            .OfType<Player>()
            .FirstOrDefault(p => p.Id == searchId);
        return player?.Username;
    }
    private static Node GetSceneRoot()
    {
        var tree = Engine.GetMainLoop() as SceneTree;
        return tree?.CurrentScene;
    }
}
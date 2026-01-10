using System;
using Godot;
using Nekoblocks.Scripts.Player;
using Nekoblocks.Scripts.World;

namespace Nekoblocks.Scripts.LocalPlayer;

public partial class LocalPlayer : CharacterBody3D
{
	[Export] public LocalPlayerCamera Camera;
	[Export] public LocalPlayerSounds Sounds;
	
	public float WalkSpeed = 16.0f;
	public float JumpVelocity = 15.0f;
	private float _friction = 100.0f;
	
	public override void _Ready()
	{
		
	}
	public override void _Process(double delta)
	{
		
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		
		if (!IsOnFloor())
		{
			velocity.Y -= GetNode<Workspace>("%Workspace").Gravity * (float)delta;
		}
		else
		{
			if (Input.IsActionJustPressed("player_jump"))
			{
				velocity.Y = JumpVelocity;
			}
			else
			{
				velocity.Y = 0;
			}
		}
		
		Vector2 inputDir = Input.GetVector("player_move_left", "player_move_right", "player_move_forward", "player_move_back");
		Vector3 direction = Camera.GlobalBasis * new Vector3(inputDir.X, 0, inputDir.Y);
		direction.Y = 0;

		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * WalkSpeed;
			velocity.Z = direction.Z * WalkSpeed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, _friction * (float)delta);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, _friction * (float)delta);
		}
		
		Velocity = velocity;
		MoveAndSlide();
	}

	private static float Deg2Rad(float degrees)
	{
		return degrees * (float)(Math.PI / 180);
	}

}

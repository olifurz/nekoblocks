using System;
using Godot;

namespace Nekoblocks.LocalPlayer;

public partial class LocalPlayerCharacter : CharacterBody3D
{
	[Export] public LocalPlayerCamera Camera;
	[Export] public LocalPlayerSounds Sounds;
	[Export] public AnimationPlayer AnimationPlayer;
	
	public float WalkSpeed = 16.0f;
	public float JumpVelocity = 35.0f;
	private float _friction = 100.0f;
	private float _mass = 15.0f;

	private Vector3 _velocity;
	
	public override void _Ready()
	{
		
	}
	public override void _Process(double delta)
	{
		
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!IsMultiplayerAuthority()) return;
		
		_velocity = Velocity;
		var nextAnim = "player_idle";
		
		if (!IsOnFloor())
		{
			_velocity.Y -= -GetGravity().Y * _mass * (float)delta;
			nextAnim = "player_jump"; 
		}
		else
		{
			if (Input.IsActionJustPressed("player_jump"))
			{
				_velocity.Y = JumpVelocity;
				nextAnim = "player_jump";
			}
			else
			{
				_velocity.Y = 0;
			}
		}
		
		Vector2 inputDir = Input.GetVector("player_move_left", "player_move_right", "player_move_forward", "player_move_back");
		Vector3 direction = Camera.GlobalBasis * new Vector3(inputDir.X, 0, inputDir.Y);
		direction.Y = 0;
		
		if (direction.Length() > 0)
			direction = direction.Normalized();

		if (direction != Vector3.Zero)
		{
			_velocity.X = direction.X * WalkSpeed;
			_velocity.Z = direction.Z * WalkSpeed;
			
			// Calculate the angle to look at based on X and Z movement
			// Fuck knows why this has to be negative, are we even calculating movement direction right?
			var targetAngle = Mathf.Atan2(-direction.X, -direction.Z);
			var newAngle = (float)Mathf.LerpAngle(Rotation.Y, targetAngle, delta * 10);
			Rotation = new Vector3(Rotation.X, newAngle, Rotation.Z);
			
			if (IsOnFloor()) 
			{
				nextAnim = "player_walk";
			}
		}
		else
		{
			_velocity.X = Mathf.MoveToward(Velocity.X, 0, _friction * (float)delta);
			_velocity.Z = Mathf.MoveToward(Velocity.Z, 0, _friction * (float)delta);
		} 
		
		ChangeAnimation(nextAnim);
		Velocity = _velocity;
		MoveAndSlide();
	}
	
	private void ChangeAnimation(string newAnim)
	{
		if (AnimationPlayer.CurrentAnimation == newAnim) return;

		switch (newAnim)
		{
			case "player_jump" when !IsOnFloor() && !AnimationPlayer.IsPlaying():
				// Special handling so that the player jump animation doesn't repeat whilst in the air
				return;
			case "player_walk":
				AnimationPlayer.SpeedScale = 1 + WalkSpeed / 15;
				break;
			default:
				AnimationPlayer.SpeedScale = 1;
				break;
		}


		AnimationPlayer.Stop();
		AnimationPlayer.Play(newAnim);
	}

	private static float Deg2Rad(float degrees)
	{
		return degrees * (float)(Math.PI / 180);
	}

}

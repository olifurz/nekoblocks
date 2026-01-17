using System;
using Godot;
using Nekoblocks.World;

namespace Nekoblocks.LocalPlayer;

public partial class Character : CharacterBody3D
{
	[Export] public CharacterCamera Camera;
	[Export] public AnimationPlayer AnimationPlayer;
	
	private AudioStreamPlayer3D _walkPlayer;
	
	public float WalkSpeed = 16.0f;
	private float _targetWalkSpeed;
	public float JumpVelocity = 35.0f;
	private float _friction = 100.0f;
	private float _mass = 15.0f;

	private Vector3 _velocity;
	private Vector2 _inputDir;

	private bool _jump;

	public override void _Ready()
	{
		_walkPlayer = new AudioStreamPlayer3D();
		_walkPlayer.Stream = GD.Load<AudioStream>("res://Sounds/walk2.wav");
		_walkPlayer.MaxDistance = 50;
		_walkPlayer.Finished += () => _walkPlayer.StreamPaused = false;
		AddChild(_walkPlayer);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!IsMultiplayerAuthority()) return;
		var wasInAir = !IsOnFloor();
		
		_velocity = Velocity;
		_targetWalkSpeed = WalkSpeed;

		Vector3 direction = Camera.GlobalBasis * new Vector3(_inputDir.X, 0, _inputDir.Y);
		direction.Y = 0;
		
		if (_jump && IsOnFloor())
		{
			_velocity.Y = JumpVelocity;
		}
		
		else if (!IsOnFloor())
		{
			// Apply Gravity
			_velocity.Y -= -GetGravity().Y * _mass * (float)delta;
		}
		else
		{
			_velocity.Y = 0;
		}
		
		if (direction.Length() > 0)
			direction = direction.Normalized();
		
		
		if (direction != Vector3.Zero)
		{
			_velocity.X = direction.X * _targetWalkSpeed;
			_velocity.Z = direction.Z * _targetWalkSpeed;
			
			// Calculate the angle to look at based on X and Z movement
			var targetAngle = Mathf.Atan2(-direction.X, -direction.Z);
			var newAngle = (float)Mathf.LerpAngle(Rotation.Y, targetAngle, delta * 10);
			Rotation = new Vector3(Rotation.X, newAngle, Rotation.Z);
		}
		else
		{
			_velocity.X = Mathf.MoveToward(Velocity.X, 0, _friction * (float)delta);
			_velocity.Z = Mathf.MoveToward(Velocity.Z, 0, _friction * (float)delta);
		} 
		
		Velocity = _velocity;
		MoveAndSlide();
		ProcessAnimations(direction);

		if (wasInAir && IsOnFloor())
		{
			SoundManager.Instance.PlayAtPosition("res://Sounds/land.wav", GlobalPosition, 0.5f, 100);
		}
		
		var shouldPlay = direction.Length() > 0.1f && IsOnFloor();
		switch (shouldPlay)
		{
			case true when !_walkPlayer.Playing:
				_walkPlayer.Play();
				break;
			case false when _walkPlayer.Playing:
				_walkPlayer.Stop();
				break;
		}
	}

	private void ProcessAnimations(Vector3 direction)
	{
		var targetAnim = "player_idle";
		AnimationPlayer.SpeedScale = 1;

		if (!IsOnFloor())
		{
			targetAnim = "player_jump";
		}
		else if (direction.Length() > 0.1f)
		{
			targetAnim = "player_walk";
			AnimationPlayer.SpeedScale = 1 + WalkSpeed / 15;
		}
		
		if (AnimationPlayer.CurrentAnimation != targetAnim)
		{
			AnimationPlayer.Play(targetAnim, customBlend: 0.1f);
		}
		
		_jump = false;
	}
	public override void _UnhandledInput(InputEvent @event)
	{
		_inputDir = Input.GetVector("player_move_left", "player_move_right", "player_move_forward", "player_move_back");
		
		if (@event.IsActionPressed("player_jump") && IsOnFloor())
		{
			_jump = true;
			SoundManager.Instance.PlayAtPosition("res://Sounds/jump.wav", GlobalPosition, 0.5f, 100);
		}
	}

	private static float Deg2Rad(float degrees)
	{
		return degrees * (float)(Math.PI / 180);
	}

}

using System;
using Godot;
using Nekoblocks.Scripts.LocalPlayer;

namespace Nekoblocks.Scripts.Player;

public partial class LocalPlayerCamera : Camera3D
{
	// https://en.wikipedia.org/wiki/Spherical_coordinate_system

	private LocalPlayer.LocalPlayerCharacter _localPlayerCharacter;
	[Export] public Node3D Head;
	
	private float _camDistance = 10f;
	public float Theta { get; private set; }
	public float Phi { get; private set; }

	private float _camSensitivity = 0.2f;
	private Vector2 _mouseDelta;
	private Tween _zoomTween;
	private float _targetZoom;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Theta = Phi = Deg2Rad(90);
		Phi = Deg2Rad(60);
		_localPlayerCharacter = GetParent<LocalPlayer.LocalPlayerCharacter>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		UpdateCamera();
	}
	
	private void UpdateCamera()
	{
		var target = new Vector3(Head.GlobalPosition.X, Head.GlobalPosition.Y - 0.5f, Head.GlobalPosition.Z);

		Phi = Math.Clamp(Phi, 0.05f, (float)Math.PI - 0.05f);
		Theta = (Theta + 2 * (float)Math.PI) % (2 * (float)Math.PI);

		if (Input.IsActionPressed("camera_move_left")) Theta -= 0.025f;
		if (Input.IsActionPressed("camera_move_right")) Theta += 0.025f;

		float x = target.X + (float)(_camDistance * Math.Sin(Phi) * Math.Cos(Theta));
		float y = target.Y + (float)(_camDistance * Math.Cos(Phi));
		float z = target.Z + (float)(_camDistance * Math.Sin(Phi) * Math.Sin(Theta));

		GlobalPosition = new Vector3(x, y, z);
		LookAt(target);
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		if (Input.IsActionJustPressed("camera_move"))
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
		else if (Input.IsActionJustReleased("camera_move"))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
		if (Input.IsActionJustPressed("camera_snap_left"))
		{
			Theta += Deg2Rad(45);
			_localPlayerCharacter.Sounds.PlayOneShot(PlayerSound.Scroll);
		}
		if (Input.IsActionJustPressed("camera_snap_right"))
		{
			Theta += Deg2Rad(-45);
			_localPlayerCharacter.Sounds.PlayOneShot(PlayerSound.Scroll);
		}

		float zoomDelta = 0;
		if (@event.IsActionPressed("camera_zoom_in")) zoomDelta = -5f;
		if (@event.IsActionPressed("camera_zoom_out")) zoomDelta = 5f;

		if (zoomDelta != 0)
		{
			_targetZoom = Mathf.Clamp(_targetZoom + zoomDelta, 2, 80);
		
			AnimateZoom();
		}
		
		switch (@event)
		{
			case InputEventMouseMotion mouseMotion:
				_mouseDelta = mouseMotion.Relative;
				if (Input.MouseMode == Input.MouseModeEnum.Captured)
				{
					Theta += Deg2Rad(_mouseDelta.X * _camSensitivity);
					Phi -= Deg2Rad(_mouseDelta.Y * _camSensitivity);
				}
				break;
		}
	}
	
	private void AnimateZoom()
	{
		if (Math.Abs(_targetZoom - _camDistance) > 0.05)
		{
			_zoomTween?.Kill();
			_zoomTween = GetTree().CreateTween();
	
			_localPlayerCharacter.Sounds.PlayOneShot(PlayerSound.Scroll);

			_zoomTween.TweenProperty(this, nameof(_camDistance), _targetZoom, 0.15f)
				.SetTrans(Tween.TransitionType.Quint)
				.SetEase(Tween.EaseType.Out);
		}
	}
	
	private static float Deg2Rad(float degrees)
	{
		return degrees * (float)(Math.PI / 180);
	}

	private static float Rad2Deg(float radians)
	{
		return radians * 180 / (float)Math.PI;
	}
}

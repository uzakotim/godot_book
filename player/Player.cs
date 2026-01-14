using Godot;
using System;

public partial class Player : CharacterBody3D
{
    private Node3D body;
    private Vector3 rotation;
    private Node3D cameraPivot;
    
    [Export(PropertyHint.Range, "0,0.5")]
    private float cameraSensitivity_H = 0.05f;
    [Export(PropertyHint.Range, "0,0.5")]
    private float cameraSensitivity_V = 0.05f;
    
    private Vector3 maxSpringRotation = new Vector3(30, 30, 0);

    public float ConvertDegreesToRadians(float degrees) =>
        degrees * ((float)Math.PI / 180);

    public override void _Ready()
    {
        cameraPivot = GetNode<Node3D>("CameraPivot");
        body = GetNode<Node3D>("Body");
        rotation = body.Rotation;

        Input.MouseMode = Input.MouseModeEnum.Captured;

        // Clamp initial rotation
        rotation = cameraPivot.Rotation;
        rotation.X = Mathf.Clamp(rotation.X, -ConvertDegreesToRadians(maxSpringRotation.X), ConvertDegreesToRadians(maxSpringRotation.X));
        rotation.Y = Mathf.Clamp(rotation.Y, -ConvertDegreesToRadians(maxSpringRotation.Y), ConvertDegreesToRadians(maxSpringRotation.Y));
        rotation.Z = Mathf.Clamp(rotation.Z, -ConvertDegreesToRadians(maxSpringRotation.Z), ConvertDegreesToRadians(maxSpringRotation.Z));
        cameraPivot.Rotation = rotation;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            cameraPivot.RotateY(ConvertDegreesToRadians(-mouseMotion.Relative.X * cameraSensitivity_H));
            cameraPivot.RotateX(ConvertDegreesToRadians(-mouseMotion.Relative.Y * cameraSensitivity_V));

            // Clamp vertical rotation to avoid flipping
            Vector3 camRot = cameraPivot.Rotation;
            camRot.X = Mathf.Clamp(camRot.X, -ConvertDegreesToRadians(maxSpringRotation.X), ConvertDegreesToRadians(maxSpringRotation.X));
            cameraPivot.Rotation = camRot;
        }
    }

    public const float Speed = 5.0f;
    public const float JumpVelocity = 4.5f;

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        // Add gravity
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        // Jump
        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        // Movement
        Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        // Rotate movement by camera Y
        direction = new Basis(Vector3.Up, cameraPivot.Rotation.Y) * direction;

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;

            rotation.Y = Mathf.LerpAngle(rotation.Y, Mathf.Atan2(velocity.X, velocity.Z), 0.15f);
            body.Rotation = rotation;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}
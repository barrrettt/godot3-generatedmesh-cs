using Godot;
using System;

public class Camera : Spatial{
    Vector2 velocity = new Vector2();
    [Export] public float Speed = 0.1f;
    private Godot.Camera camera;


    public override void _Ready()    {
        camera = GetNode("Camera")as Godot.Camera;
    }

    public override void _PhysicsProcess(float delta){
        GetInput();
        Translate (new Vector3(velocity.x,0,velocity.y));        
    }

    public void GetInput(){
        velocity = new Vector2();
        if (Input.IsActionPressed("ui_right")) velocity.x += 1;
        if (Input.IsActionPressed("ui_left")) velocity.x -= 1;
        if (Input.IsActionPressed("ui_down")) velocity.y += 1;
        if (Input.IsActionPressed("ui_up")) velocity.y -= 1;
        velocity = velocity.Normalized() * Speed;
    }

}

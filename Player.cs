using SFML.Graphics;
using SFML.System;
using System;
using static Consts;

/*
 * The player that moves around the screen
 */

class Player {

    public const int SIZE = 5; //the radius of player circle
    private static readonly Color playerColor = Color.Cyan;
    private static readonly Color rayColor = Color.Yellow;

    public Player()
    {
        Position = new Vector2f(0, 0);
        Angle = 0.0f;
    }

    public void MoveForward(float d)
    {
        if (d < 0.0f) throw new ArgumentException("Distance can't be negative");

        Vector2f delta = new Vector2f(  //calculate unit vector and scale by d
            MathF.Cos(Angle) * d,
            MathF.Sin(Angle) * d
        );

        Position += delta; 
    }

    //same as forward except we subtract delta
    public void MoveBackward(float d)
    {
        if (d < 0.0f) throw new ArgumentException("Distance can't be negative");

        Vector2f delta = new Vector2f(
            MathF.Cos(Angle) * d,
            MathF.Sin(Angle) * d
        );

        Position -= delta;
    }

    public void Turn(float rad)
    {
        // normalize angle between [0, 2_PI)

        this.Angle += rad;

        if (this.Angle < 0.0f) this.Angle = PI_2 + this.Angle;
        else if (this.Angle >= PI_2) this.Angle -= PI_2;
    }

    public void Draw()
    {
        CircleShape circle = new CircleShape(SIZE) {
            Position = this.Position, 
            Origin = new Vector2f(SIZE, SIZE)
        };

        circle.FillColor = playerColor;

        var window = AssetManager.GetWindow();
        window.Draw(circle);
    }

    public Vector2f Position { get; set; }
    public float Angle { get; set; }    //player angle [0, 2PI)
}
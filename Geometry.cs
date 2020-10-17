using SFML.System;
using SFML.Graphics;
using System;


/*  A line segment defined by two points
 */
struct LineSegment {
    public Vector2f start;
    public Vector2f end;

    public LineSegment(Vector2f start, Vector2f end)
    {
        this.start = start;
        this.end = end;
    }

    public void Draw(Color color)
    {
        Vertex start = new Vertex(this.start, color);
        Vertex end = new Vertex(this.end, color);

        var window = AssetManager.GetWindow();
        window.Draw(new Vertex[] { start, end }, PrimitiveType.Lines);
    }


    public float Length()
    {
        float x = end.X - start.X;
        float y = end.Y - start.Y;

        return MathF.Sqrt(x * x + y * y);
    }
}

/*
 * A ray defined by a starting point and direction
 */

struct Ray {
    public Vector2f start;
    public Vector2f direction;

    //calculate angle using direction vector
    public float Angle {
        get {
            return MathF.Atan(direction.Y / direction.X);
        }
        set {
            float x = MathF.Cos(value);
            float y = MathF.Sin(value);

            direction = new Vector2f(x, y);
        }
    }

    // calculate intersection of ray with line segment
    // algortihm from wiki
    // https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection#Given_two_points_on_each_line
    public bool IntersectWithSegment(LineSegment segment, out Vector2f point)
    {
        point = new Vector2f(0.0f, 0.0f);

        //for ray
        float x1 = start.X;
        float y1 = start.Y;
        float x2 = direction.X + x1;
        float y2 = direction.Y + y1;

        //for segment
        float x3 = segment.start.X;
        float y3 = segment.start.Y;
        float x4 = segment.end.X;
        float y4 = segment.end.Y;

        float den = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

        if (MathF.Abs(den) < 0.01f) return false;

        float num1 = (x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4);
        float num2 = (x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3);

       

        float t1 = num1 / den;
        float t2 = -num2 / den;



        /* t is the path parameter
         * for ray, t =[0, inf) (infinite in one direction)
         * for line segment t = [0, 1.0]
         */



        if (t1 >= 0.0f && (0.0f <= t2 && t2 <= 1.0f)) {
            float x = x1 + t1 * (x2 - x1);
            float y = y1 + t1 * (y2 - y1);

            point = new Vector2f(x, y);

            return true;
        }
        else {
            return false;
        }
    }
}
using SFML.System;
using SFML.Window;
using SFML.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;

class RayCastingEngine {

    private static RayCastingEngine instance = null;

    public static RayCastingEngine GetInstance()
    {
        if (instance == null) instance = new RayCastingEngine();
        return instance;
    }

    private RayCastingEngine()
    {
        var window = AssetManager.InitWindow();
        window.SetVerticalSyncEnabled(true);
        window.Closed += (obj, args) => window.Close();

        //enable / disable blocks
        window.MouseButtonPressed += (obj, args) =>
        {
            if (args.Button == Mouse.Button.Left) {
                int x = args.X / Consts.BLOCK_SIZE; // calculate grid pos
                int y = args.Y / Consts.BLOCK_SIZE;

                if (0 <= x && x < Consts.GRID_COLS &&
                    0 <= y && y < Consts.GRID_ROWS) {
                    grid[y, x] = !grid[y, x];
                }
            }
        };

        player = new Player();
        player.Position = new Vector2f(300.0f, 300.0f);
        grid = new bool[Consts.GRID_ROWS, Consts.GRID_COLS];

        for (int y = 0; y < Consts.GRID_ROWS; ++y) {
            for (int x = 0; x < Consts.GRID_COLS; ++x) {
                //for boundary blocks
                if (x == 0 || y == 0 || x == Consts.GRID_COLS - 1 || y == Consts.GRID_ROWS -1) {
                    grid[y, x] = true;
                }
            }
        }
    }

    public void Run()
    {
        var window = AssetManager.GetWindow();
        while (window.IsOpen) {
            window.DispatchEvents();
            HandleInput();
            Render();
        }
    }

    private void HandleInput()
    {
        if (Keyboard.IsKeyPressed(Keyboard.Key.W)) {
            player.MoveForward(1.0f);
        }
        else if (Keyboard.IsKeyPressed(Keyboard.Key.S)) {
            player.MoveBackward(1.0f);
        }
        if (Keyboard.IsKeyPressed(Keyboard.Key.A)) {
            player.Turn(-0.05f);
        }
        else if (Keyboard.IsKeyPressed(Keyboard.Key.D)) {
            player.Turn(0.05f);
        }
    }

    private void Render()
    {
        var window = AssetManager.GetWindow();
        window.Clear();

        const int MAX_RAYS = 100;
        const float FOV = MathF.PI / 3.0f; //field of view (60 deg)

        for (int i = 0; i < MAX_RAYS; ++i) {

            float angle = (-FOV / 2.0f) + FOV * i / MAX_RAYS; // calculate angle [-FOV / 2, +FOV / 2]
            bool boundary = false; // if hit boundary
            float minDist = float.PositiveInfinity;
            LineSegment closest = new LineSegment();
            Ray ray = new Ray(); //cast ray with angle
            ray.start = player.Position;
            ray.Angle = player.Angle + angle;

            //for each grid block
            for (int y = 0; y < Consts.GRID_ROWS; ++y) {
                for (int x = 0; x < Consts.GRID_COLS; ++x) {
                    if (grid[y, x]) { //if grid block is there
                        var segs = BlockToSegs(x, y);
                        foreach (LineSegment s in segs) { //convert square into segments
                            Vector2f point;
                            //if ray intersects with segment
                            if (ray.IntersectWithSegment(s, out point)) { 
                                LineSegment temp = new LineSegment(player.Position, point);
                                float len = temp.Length();
                                if (len < minDist) {

                                    //update minDist and hit point
                                    minDist = len;
                                    closest = temp;
                                   
                                    //if hit boundary 
                                    if (x == 0 || y == 0 ||
                                        x == Consts.GRID_COLS - 1 ||
                                        y == Consts.GRID_ROWS - 1) {
                                        boundary = true;
                                    }
                                    else {
                                        boundary = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            closest.Draw(Color.Yellow); //draw ray
            minDist *= MathF.Cos(angle); //correct fish eye effect by projecting parallel to player angle 

            //linehight max half of win height
            float lineHeight = (Consts.BLOCK_SIZE * Consts.WIN_WIDTH / 2) / minDist;
            if (lineHeight >= Consts.WIN_HEIGHT) lineHeight = Consts.WIN_HEIGHT / 2.0f;

            float lineWidth = Consts.WIN_WIDTH / 2.0f / MAX_RAYS;
            float yOffset = Consts.WIN_HEIGHT - lineHeight / 2.0f; //shift lines to centre

            RectangleShape pixLine = new RectangleShape(new Vector2f(lineWidth, lineHeight));
            pixLine.Position = new Vector2f(i * lineWidth + Consts.WIN_WIDTH / 2.0f, Consts.WIN_HEIGHT / 2.0f - lineHeight);

            if (boundary) pixLine.FillColor = Color.Black; //for boundary, special color
            else pixLine.FillColor = Color.Red;

            window.Draw(pixLine);
        }

        //draw 2d map
        for (int y = 0; y < Consts.GRID_ROWS; ++y) {
            for (int x = 0; x < Consts.GRID_COLS; ++x) {
                if (grid[y, x]) {
                    RectangleShape rectangle = new RectangleShape(new Vector2f(Consts.BLOCK_SIZE, Consts.BLOCK_SIZE));
                    rectangle.Position = new Vector2f(x * Consts.BLOCK_SIZE, y * Consts.BLOCK_SIZE);
                    rectangle.FillColor = Color.Red;
                    window.Draw(rectangle);
                }
            }
        }

        player.Draw();
        window.Display();
    }


    /*
     * fundtion to convert a 2D square in 2d map to a set of 4 line segments
     */
    LineSegment[] BlockToSegs(int x, int y)
    {
        float len = Consts.BLOCK_SIZE;
        Vector2f topLeft = new Vector2f(x * len, y * len);
        Vector2f topRight = topLeft + new Vector2f(len, 0.0f);
        Vector2f botLeft = topLeft + new Vector2f(0.0f, len);
        Vector2f botRight = botLeft + new Vector2f(len, 0.0f);

        return new LineSegment[] {
            new LineSegment(topLeft, topRight),
            new LineSegment(topLeft, botLeft),
            new LineSegment(topRight, botRight),
            new LineSegment(botLeft, botRight)
        };
    }

    private Player player;
    bool[,] grid; // the map as a grid
}
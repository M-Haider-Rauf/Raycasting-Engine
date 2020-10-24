using SFML.System;
using SFML.Window;
using SFML.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;

//different block colors

class RayCastingEngine {

    private static RayCastingEngine instance = null;
    private static Color[] blockColors = {
        new Color(), Color.Blue/*(wall color)*/,
        Color.Red, Color.Magenta, 
        //other colors
    };

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

                if (x > 0 && y > 0  && x < Consts.GRID_COLS - 1 && y < Consts.GRID_ROWS - 1) {
                    ++grid[y, x];
                }

                if (grid[y, x] == blockColors.Length) grid[y, x] = 0;
                else if (grid[y, x] == 1) ++grid[y, x];
            }
        };

        player = new Player();
        player.Position = new Vector2f(300.0f, 300.0f);

        grid = new int[Consts.GRID_ROWS, Consts.GRID_COLS];

        for (int y = 0; y < Consts.GRID_ROWS; ++y) {
            for (int x = 0; x < Consts.GRID_COLS; ++x) {
                //for boundary blocks
                if (x == 0 || y == 0 || x == Consts.GRID_COLS - 1 || y == Consts.GRID_ROWS -1) {
                    grid[y, x] = 1;
                }
                else {
                    grid[y, x] = 0;
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
            player.Turn(-0.02f);
        }
        else if (Keyboard.IsKeyPressed(Keyboard.Key.D)) {
            player.Turn(0.02f);
        }
    }

    private void Render()
    {
        var window = AssetManager.GetWindow();
        window.Clear();

        // draw sky
        RectangleShape bgScene = new RectangleShape(new Vector2f(Consts.WIN_WIDTH / 2, Consts.WIN_HEIGHT / 2));
        bgScene.Position = new Vector2f(Consts.WIN_WIDTH / 2, 0.0f);
        bgScene.FillColor = new Color(135, 206, 235);
        window.Draw(bgScene);

        //draw ground
        bgScene.FillColor = new Color(124, 252, 0);
        bgScene.Position = new Vector2f(Consts.WIN_WIDTH / 2, Consts.WIN_HEIGHT / 2);
        window.Draw(bgScene);

        const int MAX_RAYS = 120;
        const float FOV = MathF.PI / 3.0f;

        for (int i = 0; i < MAX_RAYS; ++i) {

            float angle = (-FOV / 2.0f) + FOV * i / MAX_RAYS; // calculate angle [-FOV / 2, +FOV / 2]
            float minDist = float.PositiveInfinity;
            LineSegment closest = new LineSegment();
            Ray ray = new Ray(); //cast ray with angle
            ray.start = player.Position;
            ray.Angle = player.Angle + angle;
            int hitBlock = 0; //hit block type
            bool shading = false;

            //for each grid block
            for (int y = 0; y < Consts.GRID_ROWS; ++y) {
                for (int x = 0; x < Consts.GRID_COLS; ++x) {
                    if (grid[y, x] > 0) { //if grid block is there
                        var segs = BlockToSegs(x, y);
                        for(int k = 0; k < 4; ++k) { //convert square into segments

                            LineSegment s = segs[k];

                            Vector2f point;
                            //if ray intersects with segment
                            if (ray.IntersectWithSegment(s, out point)) { 
                                LineSegment temp = new LineSegment(player.Position, point);
                                float len = temp.Length();
                                if (len < minDist) {

                                    //update minDist and hit point
                                    minDist = len;
                                    closest = temp;

                                    hitBlock = grid[y, x];

                                    if (k > 1) shading = false; //shading based on ver / hor 
                                    else shading = true;
                                }
                            }
                        }
                    }
                }
            }

            closest.Draw(Color.Yellow); //draw ray
            minDist *= MathF.Cos(angle); //correct fish eye effect by projecting parallel to player angle

            //linehight max half of win height
            float lineHeight = (Consts.WIN_HEIGHT / minDist) * Consts.BLOCK_SIZE * 0.9f;
            if (lineHeight >= Consts.WIN_HEIGHT) lineHeight = Consts.WIN_HEIGHT;

            int lineWidth = Consts.WIN_WIDTH / 2 / MAX_RAYS;
            float yOffset = Consts.WIN_HEIGHT - lineHeight / 2.0f; //shift lines to centre

            RectangleShape pixLine = new RectangleShape(new Vector2f(lineWidth, lineHeight));
            pixLine.Position = new Vector2f(i * lineWidth + Consts.WIN_WIDTH / 2.0f, (Consts.WIN_HEIGHT - lineHeight) / 2.0f);

            Color pixColor = blockColors[hitBlock];

            //darken color shade
            if (shading) {
                pixColor.R /= 2; pixColor.G /= 2; pixColor.B /= 2;
            }

            pixLine.FillColor = pixColor;
            window.Draw(pixLine);
        }

        //draw 2d map
        for (int y = 0; y < Consts.GRID_ROWS; ++y) {
            for (int x = 0; x < Consts.GRID_COLS; ++x) {
                if (grid[y, x] > 0) {
                    RectangleShape rectangle = new RectangleShape(new Vector2f(Consts.BLOCK_SIZE, Consts.BLOCK_SIZE));
                    rectangle.Position = new Vector2f(x * Consts.BLOCK_SIZE, y * Consts.BLOCK_SIZE);
                    rectangle.FillColor = blockColors[grid[y, x]];

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
            new LineSegment(botLeft, botRight),
            new LineSegment(topLeft, botLeft),
            new LineSegment(topRight, botRight)
        };
    }

    private Player player;
    int[,] grid; // the map as a grid
}
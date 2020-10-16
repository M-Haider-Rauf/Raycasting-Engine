using SFML.Graphics;
using SFML.Window;
using SFML.System;


/*
 *  Wanted to include other things here....
 */
static class AssetManager {

    public static RenderWindow GetWindow()
    {
        return window;
    }

    public static RenderWindow InitWindow()
    {
        if (window == null) window = new RenderWindow(new VideoMode(Consts.WIN_WIDTH, Consts.WIN_HEIGHT), "SFML.NET", Styles.Close);
        return window;
    }

    static RenderWindow window;
}
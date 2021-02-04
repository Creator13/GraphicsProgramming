using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GraphicsProgrammingProject {
public class Game1 : Game {
    private GraphicsDeviceManager _graphics;

    private SpriteBatch _spriteBatch;
    // private Lesson currentLesson;

    public Game1() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize() {
        // currentLesson.Initialize();
        _graphics.PreferredBackBufferWidth = 750;
        _graphics.PreferredBackBufferHeight = 750;
        _graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        effect = new BasicEffect(_graphics.GraphicsDevice);
    }

    protected override void Update(GameTime gameTime) {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    private VertexPositionColor[] vertices = {
        // FRONT
        new VertexPositionColor(new Vector3(-.5f, .5f, .5f), Color.Red),
        new VertexPositionColor(new Vector3(.5f, -.5f, .5f), Color.Green),
        new VertexPositionColor(new Vector3(-.5f, -.5f, .5f), Color.Blue),
        new VertexPositionColor(new Vector3(.5f, .5f, .5f), Color.Yellow),

        // BACK
        new VertexPositionColor(new Vector3(-.5f, .5f, -.5f), Color.Red),
        new VertexPositionColor(new Vector3(.5f, -.5f, -.5f), Color.Green),
        new VertexPositionColor(new Vector3(-.5f, -.5f, -.5f), Color.Blue),
        new VertexPositionColor(new Vector3(.5f, .5f, -.5f), Color.Yellow)
    };

    private int[] indices = {
        // FRONT
        0, 1, 2,
        0, 3, 1,
        
        // BACK
        4, 6, 5,
        4, 5, 7,
        
        // TOP
        4,3,0,
        4,7,3,
        
        //BOTTOM
        6,2,1,
        6,1,5,
        
        // LEFT
        0, 2, 6,
        0, 6, 4,
        
        // RIGHT
        3, 5, 1,
        3, 7, 5,
    };

    private BasicEffect effect;

    protected override void Draw(GameTime gameTime) {
        var device = _graphics.GraphicsDevice;
        device.Clear(Color.Black);

        effect.World = Matrix.Identity * 
                       Matrix.CreateRotationX((float) gameTime.TotalGameTime.TotalSeconds) *
                       Matrix.CreateRotationY((float) gameTime.TotalGameTime.TotalSeconds) * 
                       Matrix.CreateRotationZ((float) gameTime.TotalGameTime.TotalSeconds);
        effect.View = Matrix.CreateLookAt(-Vector3.Forward * 2, Vector3.Zero, Vector3.Up);
        effect.Projection =
            Matrix.CreatePerspectiveFieldOfView((MathF.PI / 180f) * 65f, device.Viewport.AspectRatio, .1f, 100f);

        effect.VertexColorEnabled = true;
        foreach (var pass in effect.CurrentTechnique.Passes) {
            pass.Apply();
            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0,
                indices.Length / 3);
        }

        base.Draw(gameTime);
    }
}
}

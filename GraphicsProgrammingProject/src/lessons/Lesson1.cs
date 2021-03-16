using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GraphicsProgrammingProject.Lessons {
public class Lesson1 : Lesson {
    private readonly VertexPositionColor[] vertices = {
        // FRONT
        new(new Vector3(-.5f, .5f, .5f), Color.Red),
        new(new Vector3(.5f, -.5f, .5f), Color.Green),
        new(new Vector3(-.5f, -.5f, .5f), Color.Blue),
        new(new Vector3(.5f, .5f, .5f), Color.Yellow),

        // BACK
        new(new Vector3(-.5f, .5f, -.5f), Color.Red),
        new(new Vector3(.5f, -.5f, -.5f), Color.Green),
        new(new Vector3(-.5f, -.5f, -.5f), Color.Blue),
        new(new Vector3(.5f, .5f, -.5f), Color.Yellow)
    };

    private readonly int[] indices = {
        // FRONT
        0, 1, 2,
        0, 3, 1,

        // BACK
        4, 6, 5,
        4, 5, 7,

        // TOP
        4, 3, 0,
        4, 7, 3,

        //BOTTOM
        6, 2, 1,
        6, 1, 5,

        // LEFT
        0, 2, 6,
        0, 6, 4,

        // RIGHT
        3, 5, 1,
        3, 7, 5,
    };

    private BasicEffect effect;

    public override void Draw(GameTime gameTime, GraphicsDeviceManager graphics, SpriteBatch spriteBatch) {
        var device = graphics.GraphicsDevice;
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
    }

    public override void LoadContent(ContentManager content, GraphicsDeviceManager graphics, SpriteBatch spriteBatch) {
        effect = new BasicEffect(graphics.GraphicsDevice);
    }
}
}

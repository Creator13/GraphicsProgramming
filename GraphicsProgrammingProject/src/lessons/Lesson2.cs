using System;
using System.Runtime.InteropServices;
using GraphicsProgrammingProject.Lessons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GraphicsProgramming {
public class Lesson2 : Lesson {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct VertexPositionColorNormal : IVertexType {
        private Vector3 Position;
        private Color Color;
        private Vector3 Normal;
        private Vector2 Texture;

        private static readonly VertexDeclaration _vertexDeclaration = new(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration => _vertexDeclaration;

        public VertexPositionColorNormal(Vector3 position, Color color, Vector3 normal, Vector2 texture) {
            Position = position;
            Color = color;
            Normal = normal;
            Texture = texture;
        }
    }

    private readonly VertexPositionColorNormal[] vertices = {
        //FRONT
        new(new Vector3(-1f, 1f, 1f), Color.Red, Vector3.Forward, new Vector2(0, 1)),
        new(new Vector3(1f, -1f, 1f), Color.Red, Vector3.Forward, new Vector2(1, 0)),
        new(new Vector3(-1f, -1f, 1f), Color.Red, Vector3.Forward, new Vector2(0, 0)),
        new(new Vector3(1f, 1f, 1f), Color.Red, Vector3.Forward, new Vector2(1, 1)),

        //BACK
        new(new Vector3(-1f, 1f, -1f), Color.Green, Vector3.Backward, new Vector2(0, 0)),
        new(new Vector3(1f, -1f, -1f), Color.Green, Vector3.Backward, new Vector2(1, 1)),
        new(new Vector3(-1f, -1f, -1f), Color.Green, Vector3.Backward, new Vector2(0, 1)),
        new(new Vector3(1f, 1f, -1f), Color.Green, Vector3.Backward, new Vector2(1, 0)),

        //LEFT
        new(new Vector3(-1f, 1f, -1f), Color.Blue, Vector3.Left, new Vector2(0, 0)),
        new(new Vector3(-1f, -1f, 1f), Color.Blue, Vector3.Left, new Vector2(1, 1)),
        new(new Vector3(-1f, -1f, -1f), Color.Blue, Vector3.Left, new Vector2(1, 0)),
        new(new Vector3(-1f, 1f, 1f), Color.Blue, Vector3.Left, new Vector2(0, 1)),

        //RIGHT
        new(new Vector3(1f, 1f, -1f), Color.Cyan, Vector3.Right, new Vector2(1, 0)),
        new(new Vector3(1f, -1f, 1f), Color.Cyan, Vector3.Right, new Vector2(0, 1)),
        new(new Vector3(1f, -1f, -1f), Color.Cyan, Vector3.Right, new Vector2(0, 0)),
        new(new Vector3(1f, 1f, 1f), Color.Cyan, Vector3.Right, new Vector2(1, 1)),

        //TOP
        new(new Vector3(-1f, 1f, 1f), Color.Magenta, Vector3.Up, new Vector2(1, 1)),
        new(new Vector3(1f, 1f, -1f), Color.Magenta, Vector3.Up, new Vector2(0, 0)),
        new(new Vector3(-1f, 1f, -1f), Color.Magenta, Vector3.Up, new Vector2(1, 0)),
        new(new Vector3(1f, 1f, 1f), Color.Magenta, Vector3.Up, new Vector2(0, 1)),

        //BOTTOM
        new(new Vector3(-1f, -1f, 1f), Color.Yellow, Vector3.Down, new Vector2(0, 1)),
        new(new Vector3(1f, -1f, -1f), Color.Yellow, Vector3.Down, new Vector2(1, 0)),
        new(new Vector3(-1f, -1f, -1f), Color.Yellow, Vector3.Down, new Vector2(0, 0)),
        new(new Vector3(1f, -1f, 1f), Color.Yellow, Vector3.Down, new Vector2(1, 1)),
    };

    private readonly int[] indices = {
        //FRONT
        //triangle 1
        0, 1, 2,
        //triangle 2
        0, 3, 1,

        //BACK
        //triangle 1
        4, 6, 5,
        //triangle 2
        4, 5, 7,

        //LEFT
        //triangle 1
        8, 9, 10,
        //triangle 2
        8, 11, 9,

        //RIGHT
        //triangle 1
        12, 14, 13,
        //triangle 2
        12, 13, 15,

        //TOP
        //triangle 1
        16, 18, 17,
        //triangle 2
        16, 17, 19,

        //BOTTOM
        //triangle 1
        20, 21, 22,
        //triangle 2
        20, 23, 21
    };

    private Effect myEffect;
    private Texture2D crateTexture, crateNormal, crateSpecular;
    private Vector3 lightPosition = Vector3.Right * 2 + Vector3.Up * 2 + Vector3.Backward * 2;
    private float ambient;

    public override void LoadContent(ContentManager Content, GraphicsDeviceManager graphics, SpriteBatch spriteBatch) {
        myEffect = Content.Load<Effect>("shader/live2");
        crateTexture = Content.Load<Texture2D>("texture/crate");
        crateNormal = Content.Load<Texture2D>("texture/crateNormal");
        crateSpecular = Content.Load<Texture2D>("texture/crateSpecular");

        ambient = .1f;
    }

    public override void Draw(GameTime gameTime, GraphicsDeviceManager graphics, SpriteBatch spriteBatch) {
        var device = graphics.GraphicsDevice;

        var time = (float) gameTime.TotalGameTime.TotalSeconds;
        lightPosition = new Vector3(MathF.Cos(time), 1, MathF.Sin(time)) * 2;

        var cameraPos = -Vector3.Forward * 10 + Vector3.Up * 5 + Vector3.Right * 5;

        var worldMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);
        var viewMatrix = Matrix.CreateLookAt(cameraPos, Vector3.Zero, Vector3.Up);

        myEffect.Parameters["World"].SetValue(worldMatrix);
        myEffect.Parameters["View"].SetValue(viewMatrix);
        myEffect.Parameters["Projection"].SetValue(Matrix.CreatePerspectiveFieldOfView(
            MathF.PI / 180f * 25f, device.Viewport.AspectRatio, 0.001f, 1000f));

        myEffect.Parameters["MainTex"].SetValue(crateTexture);
        myEffect.Parameters["NormalTex"].SetValue(crateNormal);
        myEffect.Parameters["SpecularTex"].SetValue(crateSpecular);

        myEffect.Parameters["CameraPosition"].SetValue(cameraPos);
        myEffect.Parameters["LightPosition"].SetValue(lightPosition);
        myEffect.Parameters["Ambient"].SetValue(ambient);

        myEffect.CurrentTechnique.Passes[0].Apply();

        device.Clear(Color.Black);
        device.DrawUserIndexedPrimitives(
            PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);
    }
}
}

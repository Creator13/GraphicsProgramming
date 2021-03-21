﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GraphicsProgrammingProject.Lessons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GraphicsProgramming
{
class Homework5 : Lesson
{
    private Effect effect;

    private Texture2D heightmap,
        underwater,
        dirt,
        grass,
        rock,
        snow,
        plantTexture,
        dirt_norm,
        dirt_spec,
        water,
        foam,
        waterNormal;

    private TextureCube sky;
    private Model cube, sphere, star, plantModel;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vert : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 Binormal;
        public Vector3 Tangent;
        public Vector2 Texture;

        static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(24, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0),
            new VertexElement(36, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
            new VertexElement(48, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration => _vertexDeclaration;


        public Vert(Vector3 position, Vector3 normal, Vector3 binormal, Vector3 tangent, Vector2 texture)
        {
            Position = position;
            Normal = normal;
            Binormal = binormal;
            Tangent = tangent;
            Texture = texture;
        }
    }

    private Vert[] vertices;
    private int[] indices;

    private List<Vector3> grassPositions;

    private int mouseX, mouseY;

    Vector3 cameraPos = Vector3.Up * 128f;
    Quaternion cameraRotation = Quaternion.Identity;
    float yaw, pitch;

    private RenderTarget2D rt;
    private Texture2D backbuffer;
    private Color[] backbufferPixels;

    public override void Initialize()
    {
        mouseX = Mouse.GetState().X;
        mouseY = Mouse.GetState().Y;
    }

    public override void LoadContent(ContentManager content, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
    {
        effect = content.Load<Effect>("shader/homework5");
        heightmap = content.Load<Texture2D>("heightmap/heightmap");
        underwater = content.Load<Texture2D>("texture/sand");
        dirt = content.Load<Texture2D>("texture/dirt_diff");
        grass = content.Load<Texture2D>("texture/grass");
        rock = content.Load<Texture2D>("texture/rock");
        snow = content.Load<Texture2D>("texture/snow");
        plantTexture = content.Load<Texture2D>("texture/plant");
        waterNormal = content.Load<Texture2D>("texture/waternormal");

        plantModel = content.Load<Model>("models/plant");
        foreach (var mesh in plantModel.Meshes)
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = effect;
            }
        }

        star = content.Load<Model>("models/starry");
        foreach (var mesh in star.Meshes)
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = effect;
            }
        }

        sphere = content.Load<Model>("Lesson3/uv_sphere");
        foreach (var mesh in sphere.Meshes)
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = effect;
            }
        }

        cube = content.Load<Model>("Lesson3/cube");
        foreach (var mesh in cube.Meshes)
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = effect;
            }
        }

        GeneratePlane(2, 600);

        rt = new RenderTarget2D(graphics.GraphicsDevice, graphics.PreferredBackBufferWidth,
            graphics.PreferredBackBufferHeight, false, graphics.PreferredBackBufferFormat,
            graphics.PreferredDepthStencilFormat);

        backbuffer = new Texture2D(graphics.GraphicsDevice, graphics.PreferredBackBufferWidth,
            graphics.PreferredBackBufferHeight, false, graphics.PreferredBackBufferFormat);

        backbufferPixels = new Color[graphics.PreferredBackBufferWidth * graphics.PreferredBackBufferHeight];
    }

    private void GeneratePlane(float gridSize = 8.0f, float height = 128f, float grassPercent = 0.007f)
    {
        // Get pixels
        var pixels = new Color[heightmap.Width * heightmap.Height];
        heightmap.GetData(pixels);

        //Generate vertices & indices
        vertices = new Vert[pixels.Length];
        indices = new int[heightmap.Width * heightmap.Height * 6];
        
        // Grass
        grassPositions = new List<Vector3>();
        var random = new Random();

        // for loops
        for (var y = 0; y < heightmap.Height; ++y)
        {
            for (var x = 0; x < heightmap.Width; ++x)
            {
                var index = y * heightmap.Width + x;

                var r = pixels[index].R / 255f * height;

                // smooth if not at edges
                if (y < heightmap.Height - 1 && x < heightmap.Width - 1)
                {
                    r += pixels[index + 1].R / 255f;
                    r += pixels[index + heightmap.Width].R / 255f;
                    r += pixels[index + heightmap.Width + 1].R / 255f;
                    r *= .25f;
                }

                // add vertex for current
                vertices[index] = new Vert(
                    new Vector3(gridSize * x, r, gridSize * y),
                    Vector3.Up, Vector3.Up, Vector3.Up,
                    new Vector2(x / (float) heightmap.Width, y / (float) heightmap.Height)
                );
                
                // if not edge
                if (y < heightmap.Height - 2 && x < heightmap.Width - 2)
                {
                    // add indices fro two triangles (bottom right)
                    var right = y * heightmap.Width + (x + 1); // index +1 
                    var bottom = (y + 1) * heightmap.Width + x; // index + width
                    var bottomRight = (y + 1) * heightmap.Width + (x + 1); // index + width + 1

                    // tri 1
                    indices[index * 6 + 0] = index;
                    indices[index * 6 + 1] = bottomRight;
                    indices[index * 6 + 2] = bottom;
                    // tri 2
                    indices[index * 6 + 3] = index;
                    indices[index * 6 + 4] = right;
                    indices[index * 6 + 5] = bottomRight;
                    
                    // Add grass
                    if (random.NextDouble() < grassPercent)
                    {
                        grassPositions.Add(vertices[index].Position);
                    }
                }
            }
        }

        //Calculate normals
        for (var y = 0; y < heightmap.Height - 1; ++y)
        {
            for (var x = 0; x < heightmap.Width - 1; ++x)
            {
                var index = y * heightmap.Width + x;

                var right = y * heightmap.Width + x + 1;
                var bottom = (y + 1) * heightmap.Width + x;

                var vRight = Vector3.Normalize(vertices[right].Position - vertices[index].Position);
                var vDown = Vector3.Normalize(vertices[bottom].Position - vertices[index].Position);

                vertices[index].Normal = Vector3.Cross(vRight, vDown);
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        var delta = (float) gameTime.ElapsedGameTime.TotalSeconds;
        float speed = 100;

        var keyState = Keyboard.GetState();

        if (keyState.IsKeyDown(Keys.LeftShift))
        {
            speed *= 2;
        }

        if (keyState.IsKeyDown(Keys.W))
        {
            cameraPos += delta * speed * Vector3.Transform(Vector3.Forward, cameraRotation);
        }
        else if (keyState.IsKeyDown(Keys.S))
        {
            cameraPos -= delta * speed * Vector3.Transform(Vector3.Forward, cameraRotation);
        }

        if (keyState.IsKeyDown(Keys.A))
        {
            cameraPos += delta * speed * Vector3.Transform(Vector3.Left, cameraRotation);
        }
        else if (keyState.IsKeyDown(Keys.D))
        {
            cameraPos += delta * speed * Vector3.Transform(Vector3.Right, cameraRotation);
        }

        if (keyState.IsKeyDown(Keys.E))
        {
            cameraPos += delta * speed * Vector3.Transform(Vector3.Up, cameraRotation);
        }
        else if (keyState.IsKeyDown(Keys.Q))
        {
            cameraPos += delta * speed * Vector3.Transform(Vector3.Down, cameraRotation);
        }

        var mState = Mouse.GetState();
        var deltaX = mState.X - mouseX;
        var deltaY = mState.Y - mouseY;

        var sensitivity = 0.01f;

        yaw -= deltaX * sensitivity;
        pitch -= deltaY * sensitivity;

        pitch = Math.Clamp(pitch, -MathF.PI * .5f, MathF.PI * .5f);

        cameraRotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0);

        mouseX = mState.X;
        mouseY = mState.Y;

        if (mState.RightButton == ButtonState.Pressed)
        {
            yaw = 0;
            pitch = 0;
        }
    }

    public override void Draw(GameTime gameTime, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
    {
        var device = graphics.GraphicsDevice;

        // device.SetRenderTarget(rt);

        device.Clear(Color.Black);

        var time = (float) gameTime.TotalGameTime.TotalSeconds;

        // Build & Set Matrices
        var World = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);
        var View = Matrix.CreateLookAt(cameraPos, cameraPos + Vector3.Transform(Vector3.Forward, cameraRotation),
            Vector3.Transform(Vector3.Up, cameraRotation));
        var Projection =
            Matrix.CreatePerspectiveFieldOfView((MathF.PI / 180f) * 65f, device.Viewport.AspectRatio, 0.01f, 2000f);

        effect.Parameters["World"].SetValue(World);
        effect.Parameters["View"].SetValue(View);
        effect.Parameters["Projection"].SetValue(Projection);

        effect.Parameters["Time"].SetValue(time);

        // Lighting Parameters
        effect.Parameters["LightDirection"].SetValue(Vector3.Normalize(Vector3.Down + Vector3.Right * 2));
        effect.Parameters["Ambient"].SetValue(new Vector3(.25f, .20f, .15f));
        effect.Parameters["CameraPosition"].SetValue(cameraPos);

        // Textures
        effect.Parameters["UnderwaterTex"].SetValue(underwater);
        effect.Parameters["DirtTex"].SetValue(dirt);
        effect.Parameters["GrassTex"].SetValue(grass);
        effect.Parameters["RockTex"].SetValue(rock);
        effect.Parameters["SnowTex"].SetValue(snow);

        // Render Sky
        device.RasterizerState = RasterizerState.CullNone;
        device.DepthStencilState = DepthStencilState.None;
        effect.CurrentTechnique = effect.Techniques["SkyBox"];

        RenderModel(cube, Matrix.CreateTranslation(cameraPos));

        // Render Terrain
        device.RasterizerState = RasterizerState.CullCounterClockwise;
        device.DepthStencilState = DepthStencilState.Default;
        effect.CurrentTechnique = effect.Techniques["Terrain"];
        effect.Parameters["World"].SetValue(World);

        effect.CurrentTechnique.Passes[0].Apply();
        device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0,
            indices.Length / 3);

        // Render star
        device.BlendState = BlendState.AlphaBlend;
        effect.CurrentTechnique = effect.Techniques["Unlit"];

        var color = Color.Crimson;
        color.A = 128;
        effect.Parameters["BaseColor"].SetValue(color.ToVector4());

        var starMatrix = Matrix.CreateScale(.25f) * Matrix.CreateTranslation(500, 50, 500) * World;
        
        device.RasterizerState = RasterizerState.CullClockwise;
        RenderModel(star, starMatrix);
        device.RasterizerState = RasterizerState.CullCounterClockwise;
        RenderModel(star, starMatrix);

        device.BlendState = BlendState.Opaque;

        // Render grass
        device.RasterizerState = RasterizerState.CullNone;

        effect.CurrentTechnique = effect.Techniques["Clip"];
        effect.Parameters["GrassTex"].SetValue(plantTexture);

        foreach (var pos in grassPositions)
        {
            RenderModel(plantModel,
                World * Matrix.CreateReflection(new Plane(Vector3.Zero, Vector3.Up)) * Matrix.CreateScale(.05f) * Matrix.CreateTranslation(pos + Vector3.Up * 7));
        }

        device.RasterizerState = RasterizerState.CullCounterClockwise;

        // Copy backbuffer to Texture2D
        device.GetBackBufferData(backbufferPixels);
        backbuffer.SetData(backbufferPixels);

        // Render sphere with transparent technique
        effect.CurrentTechnique = effect.Techniques["HeatDistort"];

        effect.Parameters["GrassTex"].SetValue(backbuffer);
        effect.Parameters["UnderwaterTex"].SetValue(waterNormal);

        device.RasterizerState = RasterizerState.CullNone;
        device.DepthStencilState = DepthStencilState.Default;
        RenderModel(sphere,
            World * Matrix.CreateTranslation(Vector3.Right * 512 - Vector3.Forward * 512 + Vector3.Up * 200));

        device.RasterizerState = RasterizerState.CullCounterClockwise;
    }

    void RenderModel(Model m, Matrix parentMatrix)
    {
        var transforms = new Matrix[m.Bones.Count];
        m.CopyAbsoluteBoneTransformsTo(transforms);

        effect.CurrentTechnique.Passes[0].Apply();

        foreach (var mesh in m.Meshes)
        {
            effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] * parentMatrix);

            mesh.Draw();
        }
    }
}
}

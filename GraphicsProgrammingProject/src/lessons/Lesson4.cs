using System;
using System.Runtime.InteropServices;
using GraphicsProgrammingProject.Lessons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GraphicsProgramming
{
class Lesson4 : Lesson
{
    private Effect effect;

    private Texture2D heightmap,
        underwater,
        dirt,
        grass,
        rock,
        snow,
        plant,
        dirt_norm,
        dirt_spec,
        water,
        foam,
        waterNormal;

    private TextureCube sky;
    private Model cube, sphere;

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

    public override void LoadContent(ContentManager Content, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
    {
        effect = Content.Load<Effect>("shader/live4");
        heightmap = Content.Load<Texture2D>("heightmap/heightmap");
        underwater = Content.Load<Texture2D>("texture/sand");
        dirt = Content.Load<Texture2D>("texture/dirt_diff");
        grass = Content.Load<Texture2D>("texture/grass");
        rock = Content.Load<Texture2D>("texture/rock");
        snow = Content.Load<Texture2D>("texture/snow");
        plant = Content.Load<Texture2D>("texture/plant");
        waterNormal = Content.Load<Texture2D>("texture/waternormal");
        
        sphere = Content.Load<Model>("Lesson3/uv_sphere");
        foreach (ModelMesh mesh in sphere.Meshes)
        {
            foreach (ModelMeshPart meshPart in mesh.MeshParts)
            {
                meshPart.Effect = effect;
            }
        }

        cube = Content.Load<Model>("Lesson3/cube");
        foreach (ModelMesh mesh in cube.Meshes)
        {
            foreach (ModelMeshPart meshPart in mesh.MeshParts)
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

    private void GeneratePlane(float gridSize = 8.0f, float height = 128f)
    {
        // Get pixels
        var pixels = new Color[heightmap.Width * heightmap.Height];
        heightmap.GetData(pixels);

        //Generate vertices & indices
        vertices = new Vert[pixels.Length];
        indices = new int[heightmap.Width * heightmap.Height * 6];

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
        float delta = (float) gameTime.ElapsedGameTime.TotalSeconds;
        float speed = 100;

        KeyboardState keyState = Keyboard.GetState();

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

        MouseState mState = Mouse.GetState();
        int deltaX = mState.X - mouseX;
        int deltaY = mState.Y - mouseY;

        float sensitivity = 0.01f;

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
        GraphicsDevice device = graphics.GraphicsDevice;

        // device.SetRenderTarget(rt);

        device.Clear(Color.Black);

        float r = (float) gameTime.TotalGameTime.TotalSeconds;

        // Build & Set Matrices
        Matrix World = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);
        Matrix View = Matrix.CreateLookAt(cameraPos, cameraPos + Vector3.Transform(Vector3.Forward, cameraRotation),
            Vector3.Transform(Vector3.Up, cameraRotation));
        Matrix Projection =
            Matrix.CreatePerspectiveFieldOfView((MathF.PI / 180f) * 65f, device.Viewport.AspectRatio, 0.01f, 2000f);

        effect.Parameters["World"].SetValue(World);
        effect.Parameters["View"].SetValue(View);
        effect.Parameters["Projection"].SetValue(Projection);
        
        effect.Parameters["Time"].SetValue(r);

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

        device.SetRenderTarget(null);

        // Copy backbuffer to Texture2D
        device.GetBackBufferData(backbufferPixels);
        backbuffer.SetData(backbufferPixels);

        // Set rt as shader variable

        // Render sphere with transparent technique
        // device.BlendState = BlendState.AlphaBlend;
        effect.CurrentTechnique = effect.Techniques["HeatDistort"];

        effect.Parameters["GrassTex"].SetValue(backbuffer);
        effect.Parameters["UnderwaterTex"].SetValue(waterNormal);

        device.RasterizerState = RasterizerState.CullNone;
        device.DepthStencilState = DepthStencilState.Default;
        RenderModel(sphere,
            World * Matrix.CreateTranslation(Vector3.Right * 512 - Vector3.Forward * 512 + Vector3.Up * 200));

        // device.RasterizerState = RasterizerState.CullCounterClockwise;
        // RenderModel(sphere,
        //     World * Matrix.CreateTranslation(Vector3.Right * 512 - Vector3.Forward * 512 + Vector3.Up * 200));

        device.BlendState = BlendState.Opaque;
        device.RasterizerState = RasterizerState.CullCounterClockwise;
    }

    void RenderModel(Model m, Matrix parentMatrix)
    {
        Matrix[] transforms = new Matrix[m.Bones.Count];
        m.CopyAbsoluteBoneTransformsTo(transforms);

        effect.CurrentTechnique.Passes[0].Apply();

        foreach (ModelMesh mesh in m.Meshes)
        {
            effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] * parentMatrix);

            mesh.Draw();
        }
    }
}
}

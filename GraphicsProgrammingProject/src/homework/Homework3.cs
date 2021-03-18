using System;
using GraphicsProgrammingProject;
using GraphicsProgrammingProject.Lessons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GraphicsProgramming
{
public class Homework3 : Lesson
{
    private PostProcessing postProcessing;
    
    private Effect myEffect;
    private Vector3 lightPosition = Vector3.Right * 2 + Vector3.Up * 2 + Vector3.Backward * 2;

    private Model sphere, cube, asteroid;
    private Texture2D day, night, clouds, moon, mars, moonmoon;
    private TextureCube sky;

    private float yaw, pitch;
    private int prevX, prevY;

    private int previousScrollValue = 0;
    private float cameraDistance = 15;

    public override void Update(GameTime gameTime)
    {
        var mouseState = Mouse.GetState();
        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            // update yaw & pitch
            yaw -= (mouseState.X - prevX) * .01f;
            pitch -= (mouseState.Y - prevY) * .01f;

            pitch = MathF.Min(MathF.Max(pitch, -MathF.PI * .45f), MathF.PI * .45f);
        }

        prevX = mouseState.X;
        prevY = mouseState.Y;

        if (mouseState.ScrollWheelValue != previousScrollValue)
        {
            var deltaScroll = (int) -((mouseState.ScrollWheelValue - previousScrollValue) * (1 / 120f));
            cameraDistance = Math.Clamp(cameraDistance + deltaScroll, 5, 25);
        }

        previousScrollValue = mouseState.ScrollWheelValue;
    }

    public override void LoadContent(ContentManager content, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
    {
        myEffect = content.Load<Effect>("shader/homework3");

        day = content.Load<Texture2D>("Lesson3/day");
        night = content.Load<Texture2D>("Lesson3/night");
        clouds = content.Load<Texture2D>("Lesson3/clouds");
        moon = content.Load<Texture2D>("Lesson3/2k_moon");
        mars = content.Load<Texture2D>("Lesson3/2k_mars");
        moonmoon = content.Load<Texture2D>("Lesson3/2k_makemake");
        sky = content.Load<TextureCube>("Lesson3/sky_cube");

        sphere = content.Load<Model>("Lesson3/uv_sphere");
        foreach (var mesh in sphere.Meshes)
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = myEffect;
            }
        }

        cube = content.Load<Model>("Lesson3/cube");
        foreach (var mesh in cube.Meshes)
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = myEffect;
            }
        }

        asteroid = content.Load<Model>("Lesson3/asteroid");
        foreach (var mesh in cube.Meshes)
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = myEffect;
            }
        }

        postProcessing = new PostProcessing(graphics);
        postProcessing.AddEffect(content.Load<Effect>("shader/fxaa"));
        postProcessing.AddTechnique("FXAA", true);
    }

    public override void Draw(GameTime gameTime, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
    {
        var device = graphics.GraphicsDevice;

        var time = (float) gameTime.TotalGameTime.TotalSeconds;
        // lightPosition = new Vector3(MathF.Cos(time), 0, MathF.Sin(time)) * 200;
        lightPosition = Vector3.Left * 200;

        // var cameraPos = -Vector3.Forward * 10 + Vector3.Up * 5 + Vector3.Right * 5;
        var cameraPos = -Vector3.Forward * cameraDistance;
        cameraPos = Vector3.Transform(cameraPos, Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0));

        var worldMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);
        var viewMatrix = Matrix.CreateLookAt(cameraPos, Vector3.Zero, Vector3.Up);

        myEffect.Parameters["World"].SetValue(worldMatrix);
        myEffect.Parameters["View"].SetValue(viewMatrix);
        myEffect.Parameters["Projection"].SetValue(Matrix.CreatePerspectiveFieldOfView(
            MathF.PI / 180f * 25f, device.Viewport.AspectRatio, 0.001f, 1000f));

        myEffect.Parameters["DayTex"].SetValue(day);
        myEffect.Parameters["NightTex"].SetValue(night);
        myEffect.Parameters["CloudsTex"].SetValue(clouds);
        myEffect.Parameters["MoonTex"].SetValue(moon);
        myEffect.Parameters["SkyTex"].SetValue(sky);

        myEffect.Parameters["CameraPosition"].SetValue(cameraPos);
        myEffect.Parameters["LightPosition"].SetValue(lightPosition);

        myEffect.Parameters["Time"].SetValue(time);

        myEffect.CurrentTechnique.Passes[0].Apply();

        device.Clear(Color.Black);

        // Sky
        myEffect.CurrentTechnique = myEffect.Techniques["Sky"];
        device.DepthStencilState = DepthStencilState.None;
        device.RasterizerState = RasterizerState.CullNone;
        RenderModel(cube, Matrix.CreateTranslation(cameraPos));
        device.DepthStencilState = DepthStencilState.Default;
        device.RasterizerState = RasterizerState.CullCounterClockwise;

        // Earth
        myEffect.CurrentTechnique = myEffect.Techniques["Earth"];
        RenderModel(sphere,
            Matrix.CreateScale(.01f) * Matrix.CreateRotationZ(time) * Matrix.CreateRotationY(MathF.PI / 180 * 23) *
            worldMatrix);

        // Moon
        myEffect.CurrentTechnique = myEffect.Techniques["Moon"];
        RenderModel(sphere,
            Matrix.CreateTranslation(Vector3.Down * 8) * Matrix.CreateScale(.0033f) * Matrix.CreateRotationZ(time) *
            worldMatrix);
        
        // Moon 2
        myEffect.CurrentTechnique = myEffect.Techniques["Moon"];
        myEffect.Parameters["MoonTex"].SetValue(mars);
        RenderModel(sphere,
            Matrix.CreateTranslation(Vector3.Down * 19) * Matrix.CreateScale(.0025f) *
            Matrix.CreateRotationZ(time * .8f) * Matrix.CreateRotationX(-25 * (MathF.PI / 180f)) *
            worldMatrix);
        
        // Moon around moon
        myEffect.CurrentTechnique = myEffect.Techniques["Moon"];
        myEffect.Parameters["MoonTex"].SetValue(moonmoon);
        RenderModel(sphere,
            Matrix.CreateTranslation(Vector3.Down * 4) * Matrix.CreateScale(.5f) *
            Matrix.CreateRotationZ(time * 1.5f) * Matrix.CreateRotationY(78 * (MathF.PI / 180f)) *
            Matrix.CreateTranslation(Vector3.Down * 19) * Matrix.CreateScale(0.0025f) *
            Matrix.CreateRotationZ(time * .8f) * Matrix.CreateRotationX(-25 * (MathF.PI / 180f)) * 
            worldMatrix);
        
        postProcessing.Apply(device, spriteBatch);
    }

    private void RenderModel(Model m, Matrix parentMatrix)
    {
        var transforms = new Matrix[m.Bones.Count];
        m.CopyAbsoluteBoneTransformsTo(transforms);

        myEffect.CurrentTechnique.Passes[0].Apply();

        foreach (var mesh in m.Meshes)
        {
            myEffect.Parameters["World"].SetValue(parentMatrix * transforms[mesh.ParentBone.Index]);

            mesh.Draw();
        }
    }
}
}

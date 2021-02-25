using System;
using System.Runtime.InteropServices;
using GraphicsProgrammingProject.Lessons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GraphicsProgramming {
public class Lesson3 : Lesson {
    private Effect myEffect;
    private Vector3 lightPosition = Vector3.Right * 2 + Vector3.Up * 2 + Vector3.Backward * 2;

    private Model sphere, cube;
    private Texture2D day, night, clouds, moon;
    private TextureCube sky;

    private float yaw, pitch;
    private int prevX, prevY;

    public override void Update(GameTime gameTime) {
        var mouseState = Mouse.GetState();
        if (mouseState.LeftButton == ButtonState.Pressed) {
            // update yaw & pitch
            yaw -= (mouseState.X - prevX) * .01f;
            pitch -= (mouseState.Y - prevY) * .01f;

            pitch = MathF.Min(MathF.Max(pitch, -MathF.PI * .45f), MathF.PI * .45f);
        }

        prevX = mouseState.X;
        prevY = mouseState.Y;
    }

    public override void LoadContent(ContentManager Content, GraphicsDeviceManager graphics, SpriteBatch spriteBatch) {
        myEffect = Content.Load<Effect>("shader/live3");

        day = Content.Load<Texture2D>("Lesson3/day");
        night = Content.Load<Texture2D>("Lesson3/night");
        clouds = Content.Load<Texture2D>("Lesson3/clouds");
        moon = Content.Load<Texture2D>("Lesson3/2k_moon");
        sky = Content.Load<TextureCube>("Lesson3/sky_cube");

        sphere = Content.Load<Model>("Lesson3/uv_sphere");
        foreach (var mesh in sphere.Meshes) {
            foreach (var meshPart in mesh.MeshParts) {
                meshPart.Effect = myEffect;
            }
        }
        
        cube = Content.Load<Model>("Lesson3/cube");
        foreach (var mesh in cube.Meshes) {
            foreach (var meshPart in mesh.MeshParts) {
                meshPart.Effect = myEffect;
            }
        }
    }

    public override void Draw(GameTime gameTime, GraphicsDeviceManager graphics, SpriteBatch spriteBatch) {
        var device = graphics.GraphicsDevice;

        var time = (float) gameTime.TotalGameTime.TotalSeconds;
        // lightPosition = new Vector3(MathF.Cos(time), 0, MathF.Sin(time)) * 200;
        lightPosition = Vector3.Left * 200;

        // var cameraPos = -Vector3.Forward * 10 + Vector3.Up * 5 + Vector3.Right * 5;
        var cameraPos = -Vector3.Forward * 10;
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
        RenderModel(sphere, Matrix.CreateTranslation(Vector3.Down * 8) *Matrix.CreateScale(.0033f) * Matrix.CreateRotationZ(time) * worldMatrix);
    }

    private void RenderModel(Model m, Matrix parentMatrix) {
        var transforms = new Matrix[m.Bones.Count];
        m.CopyAbsoluteBoneTransformsTo(transforms);

        myEffect.CurrentTechnique.Passes[0].Apply();

        foreach (var mesh in m.Meshes) {
            myEffect.Parameters["World"].SetValue(parentMatrix * transforms[mesh.ParentBone.Index]);

            mesh.Draw();
        }
    }
}
}

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GraphicsProgrammingProject
{
public class PostProcessing
{
    private class TechniqueState
    {
        public string name;
        public bool active;
    }

    private readonly Dictionary<Effect, EffectParameter[]> effects = new();
    private readonly List<TechniqueState> techniques = new ();

    private readonly RenderTarget2D[] renderTargets;
    private readonly Texture2D backbuffer;
    private readonly Color[] backbufferPixels;

    private readonly Vector2 screenSize;

    private List<string> ActiveTechniques =>
        techniques.Where(technique => technique.active).Select(technique => technique.name).ToList();

    private List<Effect> ActiveEffects => effects.Keys
                                                 .Where(effect => effect.Techniques.Any(technique =>
                                                     ActiveTechniques.Contains(technique.Name))).ToList();

    public PostProcessing(GraphicsDeviceManager graphics)
    {
        screenSize = new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

        renderTargets = new[]
        {
            new RenderTarget2D(graphics.GraphicsDevice,
                (int) screenSize.X, (int) screenSize.Y,
                false,
                graphics.PreferredBackBufferFormat, graphics.PreferredDepthStencilFormat),
            new RenderTarget2D(graphics.GraphicsDevice,
                (int) screenSize.X, (int) screenSize.Y,
                false,
                graphics.PreferredBackBufferFormat, graphics.PreferredDepthStencilFormat)
        };

        backbuffer = new Texture2D(graphics.GraphicsDevice,
            graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight,
            false, graphics.PreferredBackBufferFormat);

        backbufferPixels = new Color[(int) screenSize.X * (int) screenSize.Y];
    }

    public void AddEffect(Effect effect, params EffectParameter[] parameters)
    {
        effects.Add(effect, parameters);
    }

    public void AddTechnique(string name, bool startActive = false)
    {
        techniques.Add(new TechniqueState {name = name, active = startActive});
    }

    public void AddTechniqueAtStart(string name, bool startActive = false)
    {
        techniques.Insert(0, new TechniqueState {name = name, active = startActive});
    }

    public void ToggleTechnique(string name)
    {
        var index = techniques.IndexOf(techniques.Find(t => t.name == name));
        
        if (index < 0)
        {
            AddTechniqueAtStart(name, true);
            return;
        }

        techniques[index].active = !techniques[index].active;
    }

    public void Apply(GraphicsDevice device, SpriteBatch spriteBatch)
    {
        device.GetBackBufferData(backbufferPixels);
        backbuffer.SetData(backbufferPixels);

        var drawIteration = 0;

        foreach (var effect in ActiveEffects)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, effect);

            // Set effect parameters
            effect.Parameters["ScreenSize"].SetValue(new Vector2(1280, 720));

            // Perform techniques
            foreach (var technique in GetActiveEffectTechniques(effect))
            {
                effect.CurrentTechnique = effect.Techniques[technique];
                device.SetRenderTarget(renderTargets[drawIteration % renderTargets.Length]);
                spriteBatch.Draw(
                    drawIteration == 0 ? backbuffer : renderTargets[(drawIteration - 1) % renderTargets.Length],
                    Vector2.Zero, Color.White);

                drawIteration++;
            }

            spriteBatch.End();
        }

        device.SetRenderTarget(null);
        spriteBatch.Begin();

        // Copy last written rendertarget to screen
        // Offset renderTarget selection by -1 to undo the drawIteration++ at the end of the last technique drawing
        spriteBatch.Draw(drawIteration == 0 ? backbuffer : renderTargets[(drawIteration - 1) % renderTargets.Length],
            Vector2.Zero, Color.White);

        spriteBatch.End();
    }

    private IEnumerable<string> GetActiveEffectTechniques(Effect effect)
    {
        return effect.Techniques.Select(technique => technique.Name).Intersect(ActiveTechniques);
    }
}
}

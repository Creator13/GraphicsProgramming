using System;
using GraphicsProgramming;
using GraphicsProgrammingProject.Lessons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GraphicsProgrammingProject {
public class Game1 : Game {
    private readonly GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    private readonly Lesson currentLesson;

    public Game1() {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        currentLesson = new Lesson4();
    }

    protected override void Initialize() {
        graphics.PreferredBackBufferWidth = 1280;
        graphics.PreferredBackBufferHeight = 720;
        graphics.ApplyChanges();

        currentLesson.Initialize();

        base.Initialize();
    }

    protected override void LoadContent() {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        currentLesson.LoadContent(Content, graphics, spriteBatch);
    }

    protected override void Update(GameTime gameTime) {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape)) {
            Exit();
        }

        currentLesson.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        currentLesson.Draw(gameTime, graphics, spriteBatch);

        base.Draw(gameTime);
    }
}
}

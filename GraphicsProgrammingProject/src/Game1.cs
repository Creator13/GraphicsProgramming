using System;
using System.Collections.Generic;
using GraphicsProgramming;
using GraphicsProgrammingProject.Homework;
using GraphicsProgrammingProject.Lessons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GraphicsProgrammingProject {
public class Game1 : Game {
    private readonly GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    private readonly Type[] lessons =
    {
        typeof(Homework2),
        typeof(Homework3),
        typeof(Homework5),
        typeof(Homework6)
    };
    private int currentLessonIndex = 0;
    private Lesson currentLesson;

    private KeyboardState oldKeyState;
    
    public Game1() {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        LoadCurrentLesson();
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
            Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        var currentKeyState = Keyboard.GetState();

        if (currentKeyState.IsKeyDown(Keys.Down) && !oldKeyState.IsKeyDown(Keys.Down))
        {
            currentLessonIndex = (currentLessonIndex - 1) < 0 ? lessons.Length - 1 : currentLessonIndex - 1;
            LoadCurrentLesson();
        }
        if (currentKeyState.IsKeyDown(Keys.Up) && !oldKeyState.IsKeyDown(Keys.Up))
        {
            currentLessonIndex = (currentLessonIndex + 1) % lessons.Length;
            LoadCurrentLesson();
        }

        oldKeyState = currentKeyState;

        currentLesson.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        currentLesson.Draw(gameTime, graphics, spriteBatch);

        base.Draw(gameTime);
    }

    private void LoadCurrentLesson()
    {
        currentLesson = (Lesson) Activator.CreateInstance(lessons[currentLessonIndex]);
        Initialize();
    }
}
}

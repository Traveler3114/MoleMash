using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace MoleMash
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;

        Texture2D _ghostTexture;
        Texture2D _scaledGhostTextureBig;
        Texture2D _scaledGhostTextureMedium;
        Texture2D _scaledGhostTextureSmall;
        Texture2D _heartTexture;
        Texture2D _scaledHeartTexture; // Add this line

        List<Ghost> _ghostsBig;
        List<Ghost> _ghostsMedium;
        List<Ghost> _ghostsSmall;

        float _timeSinceLastSpawnBig;
        float _timeSinceLastSpawnMedium;
        float _timeSinceLastSpawnSmall;

        float _spawnIntervalBig = 5f;
        float _spawnIntervalMedium = 4f;
        float _spawnIntervalSmall = 3f;

        float _elapsedGameTime;

        Player _player;

        int screenWidth;
        int screenHeight;
        int headerHeight;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Force the game to run at 60 FPS
            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
            IsFixedTimeStep = true;
        }

        protected override void Initialize()
        {
            _ghostsBig = new List<Ghost>();
            _ghostsMedium = new List<Ghost>();
            _ghostsSmall = new List<Ghost>();

            _player = new Player(this) { Health = 5 }; // Initialize player health to 5

            _timeSinceLastSpawnBig = 0f;
            _timeSinceLastSpawnMedium = 0f;
            _timeSinceLastSpawnSmall = 0f;

            _elapsedGameTime = 0f;

            TouchPanel.EnabledGestures = GestureType.Tap;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _ghostTexture = Content.Load<Texture2D>("Ghost");

            _scaledGhostTextureBig = ScaleTexture(_ghostTexture, 0.05f);
            _scaledGhostTextureMedium = ScaleTexture(_ghostTexture, 0.03f);
            _scaledGhostTextureSmall = ScaleTexture(_ghostTexture, 0.015f);

            _font = Content.Load<SpriteFont>("Arial");

            _heartTexture = Content.Load<Texture2D>("Heart");
            _scaledHeartTexture = ScaleTexture(_heartTexture, 0.15f); // Scale the heart texture

            screenWidth = GraphicsDevice.Viewport.Width;
            screenHeight = GraphicsDevice.Viewport.Height;
            headerHeight = 100;
        }

        private Texture2D ScaleTexture(Texture2D originalTexture, float scaleFactor)
        {
            int newWidth = (int)(originalTexture.Width * scaleFactor);
            int newHeight = (int)(originalTexture.Height * scaleFactor);

            RenderTarget2D renderTarget = new RenderTarget2D(GraphicsDevice, newWidth, newHeight);

            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.Transparent);

            _spriteBatch.Begin();
            _spriteBatch.Draw(originalTexture, new Rectangle(0, 0, newWidth, newHeight), Color.White);
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            return renderTarget;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _elapsedGameTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            _timeSinceLastSpawnBig += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _timeSinceLastSpawnMedium += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _timeSinceLastSpawnSmall += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_timeSinceLastSpawnBig >= _spawnIntervalBig)
            {
                SpawnGhost(_ghostsBig, _scaledGhostTextureBig);
                _timeSinceLastSpawnBig = 0f- _spawnIntervalBig; 
            }

            if (_elapsedGameTime >= 20f && _timeSinceLastSpawnMedium >= _spawnIntervalMedium)
            {
                SpawnGhost(_ghostsMedium, _scaledGhostTextureMedium);
                _timeSinceLastSpawnMedium = 0f - _spawnIntervalMedium;
            }

            if (_elapsedGameTime >= 40f && _timeSinceLastSpawnSmall >= _spawnIntervalSmall)
            {
                SpawnGhost(_ghostsSmall, _scaledGhostTextureSmall);
                _timeSinceLastSpawnSmall = 0f - _spawnIntervalSmall;
            }

            //CINI MI SE DA NE RADI PROVJERI
            if (_elapsedGameTime >= 60f && (_elapsedGameTime - 60f) % 10f < (float)gameTime.ElapsedGameTime.TotalSeconds)
            {
                _spawnIntervalBig = Math.Max(0.1f, _spawnIntervalBig - 0.05f);
                _spawnIntervalMedium = Math.Max(0.1f, _spawnIntervalMedium - 0.05f);
                _spawnIntervalSmall = Math.Max(0.1f, _spawnIntervalSmall - 0.05f);
            }

            CheckForTouchInput();

            DestroyExpiredGhosts(_ghostsBig, _spawnIntervalBig);
            DestroyExpiredGhosts(_ghostsMedium, _spawnIntervalMedium);
            DestroyExpiredGhosts(_ghostsSmall, _spawnIntervalSmall);

            base.Update(gameTime);
        }

        private void SpawnGhost(List<Ghost> ghostList, Texture2D texture)
        {
            Random rand = new Random();
            Vector2 position = new Vector2(rand.Next(0, screenWidth - texture.Width),
                                          rand.Next(0, screenHeight - headerHeight - texture.Height));

            Ghost newGhost = new Ghost(position, texture, 0f);
            ghostList.Add(newGhost);
        }

        private void DestroyExpiredGhosts(List<Ghost> ghostList, float spawnInterval)
        {
            for (int i = ghostList.Count - 1; i >= 0; i--)
            {
                ghostList[i].TimeAlive += 1f / 60f;

                if (ghostList[i].TimeAlive >= spawnInterval)
                {
                    ghostList.RemoveAt(i);
                    _player.Health -= 1;
                }
            }
        }

        private void CheckForTouchInput()
        {
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();
                if (gesture.GestureType == GestureType.Tap)
                {
                    bool ghostTouched = false;

                    ghostTouched |= CheckTouchOnGhosts(_ghostsBig, gesture.Position, "Big");
                    ghostTouched |= CheckTouchOnGhosts(_ghostsMedium, gesture.Position, "Medium");
                    ghostTouched |= CheckTouchOnGhosts(_ghostsSmall, gesture.Position, "Small");

                    if (!ghostTouched)
                    {
                        _player.Health -= 1; // Decrease health if no ghost was touched
                    }
                }
            }
        }

        private bool CheckTouchOnGhosts(List<Ghost> ghostList, Vector2 touchPosition, string ghostType)
        {
            for (int i = ghostList.Count - 1; i >= 0; i--)
            {
                if (ghostList[i].Rectangle.Contains(touchPosition.ToPoint()))
                {
                    ghostList.RemoveAt(i);
                    if (ghostType == "Big")
                    {
                        _timeSinceLastSpawnBig = 0f;
                    }
                    else if (ghostType == "Medium")
                    {
                        _timeSinceLastSpawnMedium = 0f;
                    }
                    else if (ghostType == "Small")
                    {
                        _timeSinceLastSpawnSmall = 0f;
                    }
                    return true; // Ghost was touched
                }
            }
            return false; // No ghost was touched
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Cyan); // Change this line to set a different background color

            _spriteBatch.Begin();

            // Draw header background
            _spriteBatch.Draw(CreateRectangleTexture(GraphicsDevice, screenWidth, headerHeight, Color.White), new Vector2(0, 0), Color.White);

            // Draw health information in the header
            for (int i = 0; i < _player.Health; i++)
            {
                _spriteBatch.Draw(_scaledHeartTexture, new Vector2(10 + i * (_scaledHeartTexture.Width + 5), 10), Color.White);
            }

            // Draw game content below the header
            DrawGhosts(_ghostsBig);
            DrawGhosts(_ghostsMedium);
            DrawGhosts(_ghostsSmall);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private Texture2D CreateRectangleTexture(GraphicsDevice graphicsDevice, int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; ++i) data[i] = color;
            texture.SetData(data);
            return texture;
        }

        private void DrawGhosts(List<Ghost> ghostList)
        {
            foreach (var ghost in ghostList)
            {
                _spriteBatch.Draw(ghost.Texture, ghost.Position, Color.White);
            }
        }
    }
}

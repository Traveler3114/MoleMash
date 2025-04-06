using Android.OS;
using Android.Views;
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
        Texture2D _scaledHeartTexture;

        Texture2D _immunityTexture;
        Texture2D _scaledImmunityTexture;

        Texture2D _slowTimeTexture;
        Texture2D _scaledSlowTimeTexture;

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

        // Immunity fields
        private float _timeSinceLastImmunitySpawn;
        private float _immunityDuration = 20f;
        private float _immunityTimeRemaining;
        private Vector2 _immunityPosition;
        private bool _isImmunityVisible;
        private float _nextImmunitySpawnInterval;

        // SlowTime fields
        private float _timeSinceLastSlowTimeSpawn;
        private float _slowTimeDuration = 20f;
        private float _slowTimeTimeRemaining;
        private Vector2 _slowTimePosition;
        private bool _isSlowTimeVisible;
        private float _nextSlowTimeSpawnInterval;

        private float _originalSpawnIntervalBig;
        private float _originalSpawnIntervalMedium;
        private float _originalSpawnIntervalSmall;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Force the game to run at 60 FPS
            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
            IsFixedTimeStep = true;
        }

        private float GetRandomTime(int a,int b)
        {
            Random rand = new Random();
            return (float)rand.Next(a, b); // Random time between 5 and 20 seconds
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

            _timeSinceLastImmunitySpawn = 0f;
            _player.bHasImmunity = false;
            _immunityTimeRemaining = 0f;
            _isImmunityVisible = false;
            _nextImmunitySpawnInterval = 30f + GetRandomTime(5,21);

            _timeSinceLastSlowTimeSpawn = 0f;
            _player.bSlowerTime = false;
            _slowTimeTimeRemaining = 0f;
            _isSlowTimeVisible = false;
            _nextSlowTimeSpawnInterval = 50f + GetRandomTime(15,31);

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

            _immunityTexture = Content.Load<Texture2D>("Infinity");
            _scaledImmunityTexture = ScaleTexture(_immunityTexture, 0.15f); // Scale the immunity texture

            _slowTimeTexture = Content.Load<Texture2D>("Clock");
            _scaledSlowTimeTexture = ScaleTexture(_slowTimeTexture, 0.1f); // Scale the slow time texture

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
            //JEBENI KOD ZA FULLSCREEN
            //NE DIRAJ!!!!
            if (GraphicsDevice != null && (GraphicsDevice.Viewport.X != 0 ||
                GraphicsDevice.Viewport.Width != GraphicsDevice.Adapter.CurrentDisplayMode.Width ||
                GraphicsDevice.Viewport.Width != GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width))
            {
                Viewport view = GraphicsDevice.Viewport;
                view.X = 0;
                view.Width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                GraphicsDevice.Viewport = view;
            }
            screenWidth = GraphicsDevice.Viewport.Width;
            screenHeight = GraphicsDevice.Viewport.Height;
            //NE DIRAJ!!!!
            //JEBENI KOD ZA FULLSCREEN

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _elapsedGameTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            _timeSinceLastSpawnBig += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _timeSinceLastSpawnMedium += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _timeSinceLastSpawnSmall += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_timeSinceLastSpawnBig >= _spawnIntervalBig)
            {
                SpawnGhost(_ghostsBig, _scaledGhostTextureBig);
                _timeSinceLastSpawnBig = 0f - _spawnIntervalBig;
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

            if (_elapsedGameTime >= 60f && (_elapsedGameTime - 60f) % 10f < (float)gameTime.ElapsedGameTime.TotalSeconds)
            {
                if (!_player.bSlowerTime)
                {
                    _spawnIntervalBig = Math.Max(0.1f, _spawnIntervalBig - 0.05f);
                    _spawnIntervalMedium = Math.Max(0.1f, _spawnIntervalMedium - 0.05f);
                    _spawnIntervalSmall = Math.Max(0.1f, _spawnIntervalSmall - 0.05f);
                }
            }

            _timeSinceLastImmunitySpawn += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_timeSinceLastImmunitySpawn >= _nextImmunitySpawnInterval)
            {
                SpawnImmunity();
                _timeSinceLastImmunitySpawn = 0f;
                _nextImmunitySpawnInterval = 30f + GetRandomTime(5,21);
            }

            if (_player.bHasImmunity)
            {
                _immunityTimeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_immunityTimeRemaining <= 0f)
                {
                    _player.bHasImmunity = false;
                }
            }

            _timeSinceLastSlowTimeSpawn += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_timeSinceLastSlowTimeSpawn >= _nextSlowTimeSpawnInterval)
            {
                SpawnSlowTime();
                _timeSinceLastSlowTimeSpawn = 0f;
                _nextSlowTimeSpawnInterval = 30f + GetRandomTime(15,31);
            }

            if (_player.bSlowerTime)
            {
                _slowTimeTimeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_slowTimeTimeRemaining <= 0f)
                {
                    _player.bSlowerTime = false;
                    _spawnIntervalBig = _originalSpawnIntervalBig;
                    _spawnIntervalMedium = _originalSpawnIntervalMedium;
                    _spawnIntervalSmall = _originalSpawnIntervalSmall;
                }
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
                                          rand.Next(0, screenHeight - texture.Height));

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
                    if (!_player.bHasImmunity)
                    {
                        _player.Health -= 1;
                    }
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
                        if (_isImmunityVisible && new Rectangle(_immunityPosition.ToPoint(), new Point(_scaledImmunityTexture.Width, _scaledImmunityTexture.Height)).Contains(gesture.Position.ToPoint()))
                        {
                            ActivateImmunity();
                            _isImmunityVisible = false;
                        }
                        else if (_isSlowTimeVisible && new Rectangle(_slowTimePosition.ToPoint(), new Point(_scaledSlowTimeTexture.Width, _scaledSlowTimeTexture.Height)).Contains(gesture.Position.ToPoint()))
                        {
                            ActivateSlowTime();
                            _isSlowTimeVisible = false;
                        }
                        else if (!_player.bHasImmunity)
                        {
                            _player.Health -= 1; // Decrease health if no ghost was touched and immunity is not active
                        }
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

        private void SpawnImmunity()
        {
            Random rand = new Random();
            _immunityPosition = new Vector2(rand.Next(0, screenWidth - _scaledImmunityTexture.Width),
                                                rand.Next(0, screenHeight - headerHeight - _scaledImmunityTexture.Height));
            _isImmunityVisible = true;
        }

        private void ActivateImmunity()
        {
            _player.bHasImmunity = true;
            _immunityTimeRemaining = _immunityDuration;
        }

        private void SpawnSlowTime()
        {
            Random rand = new Random();
            _slowTimePosition = new Vector2(rand.Next(0, screenWidth - _scaledSlowTimeTexture.Width),
                                            rand.Next(0, screenHeight - headerHeight - _scaledSlowTimeTexture.Height));
            _isSlowTimeVisible = true;
        }

        private void ActivateSlowTime()
        {
            _player.bSlowerTime = true;
            _slowTimeTimeRemaining = _slowTimeDuration;

            _originalSpawnIntervalBig = _spawnIntervalBig;
            _originalSpawnIntervalMedium = _spawnIntervalMedium;
            _originalSpawnIntervalSmall = _spawnIntervalSmall;

            _spawnIntervalBig = 3f;
            _spawnIntervalMedium = 2f;
            _spawnIntervalSmall = 1f;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Cyan); // Change this line to set a different background color

            _spriteBatch.Begin();

            // Draw header background
            _spriteBatch.Draw(CreateRectangleTexture(GraphicsDevice, screenWidth, headerHeight, Color.White), new Vector2(0, 0), Color.White);

            if (_player.bHasImmunity)
            {
                // Draw infinity icon and countdown timer
                _spriteBatch.Draw(_scaledImmunityTexture, new Vector2((screenWidth - _scaledImmunityTexture.Width) / 2, 10), Color.White);
                _spriteBatch.DrawString(_font, _immunityTimeRemaining.ToString("0"), new Vector2((screenWidth - _scaledImmunityTexture.Width) / 2 + _scaledImmunityTexture.Width + 10, 10), Color.Black);

                if (_player.bSlowerTime)
                {
                    // Draw slow time icon and countdown timer below immunity icon
                    _spriteBatch.Draw(_scaledSlowTimeTexture, new Vector2((screenWidth - _scaledSlowTimeTexture.Width) / 2, 10 + _scaledImmunityTexture.Height + 10), Color.White);
                    _spriteBatch.DrawString(_font, _slowTimeTimeRemaining.ToString("0"), new Vector2((screenWidth - _scaledSlowTimeTexture.Width) / 2 + _scaledSlowTimeTexture.Width + 10, 10 + _scaledImmunityTexture.Height + 10), Color.Black);
                }
            }
            else
            {
                // Draw hearts
                int heartCount = _player.Health;
                int spacing = 5;
                int totalWidth = heartCount * (_scaledHeartTexture.Width + spacing) - spacing; // No spacing after last heart
                int startX = (screenWidth - totalWidth) / 2;

                for (int i = 0; i < heartCount; i++)
                {
                    Vector2 position = new Vector2(startX + i * (_scaledHeartTexture.Width + spacing), 10); // Y = 10 is still top
                    _spriteBatch.Draw(_scaledHeartTexture, position, Color.White);
                }

                if (_player.bSlowerTime)
                {
                    // Draw slow time icon and countdown timer below hearts
                    _spriteBatch.Draw(_scaledSlowTimeTexture, new Vector2((screenWidth - _scaledSlowTimeTexture.Width) / 2, 10 + _scaledHeartTexture.Height + 10), Color.White);
                    _spriteBatch.DrawString(_font, _slowTimeTimeRemaining.ToString("0"), new Vector2((screenWidth - _scaledSlowTimeTexture.Width) / 2 + _scaledSlowTimeTexture.Width + 10, 10 + _scaledHeartTexture.Height + 10), Color.Black);
                }
            }

            // Draw immunity icon if visible
            if (_isImmunityVisible)
            {
                _spriteBatch.Draw(_scaledImmunityTexture, _immunityPosition, Color.White);
            }

            // Draw slow time icon if visible
            if (_isSlowTimeVisible)
            {
                _spriteBatch.Draw(_scaledSlowTimeTexture, _slowTimePosition, Color.White);
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

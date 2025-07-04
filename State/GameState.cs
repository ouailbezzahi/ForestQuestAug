using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ForestQuest.Entities;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace ForestQuest.State
{
    public class GameState : State
    {
        private Texture2D _houseTile;
        private Texture2D _treeTile;
        private Texture2D _grassTile;
        private Player _player;
        private Song _backgroundMusic;
        private SoundEffect _playerMoveSound;
        private SoundEffectInstance _playerMoveSoundInstance;
        private float _footstepTimer = 0f;
        private bool _isMultiplayer;

        private int[,] _backgroundTiles = new int[,]
        {
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 1, 1, 2, 2, 2, 2, 2, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 }
        };

        public GameState(Game1 game, ContentManager content, GraphicsDevice graphicsDevice, bool isMultiplayer)
            : base(game, content, graphicsDevice)
        {
            _isMultiplayer = isMultiplayer;
        }

        public override void LoadContent()
        {
            // Load tile textures
            _houseTile = _content.Load<Texture2D>("Background/House/Slice 10");
            _treeTile = _content.Load<Texture2D>("Background/Tree/Slice 12");
            _grassTile = _content.Load<Texture2D>("Background/Grass/Slice 21");

            // Load audio
            _backgroundMusic = _content.Load<Song>("Audio/background_music");
            _playerMoveSound = _content.Load<SoundEffect>("Audio/footsteps");

            // Create sound effect instance
            _playerMoveSoundInstance = _playerMoveSound.CreateInstance();
            _playerMoveSoundInstance.IsLooped = false;

            // Play background music
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(_backgroundMusic);

            // Initialize player
            _player = new Player(new Vector2(15 * 32, 15 * 32)); // Start in the middle of the map
            _player.LoadContent(_content);
        }

        public override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            // Update player
            _player.Update(keyboardState, gameTime, _graphicsDevice.Viewport, _backgroundTiles);

            // Footstep sound timer
            const float footstepInterval = 0.3f;
            _footstepTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Check if player is moving
            bool isMoving = keyboardState.IsKeyDown(Keys.Z) || keyboardState.IsKeyDown(Keys.Q) ||
                            keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.D);

            if (isMoving && _footstepTimer >= footstepInterval)
            {
                if (_playerMoveSoundInstance.State != SoundState.Playing)
                {
                    _playerMoveSoundInstance.Play();
                }
                _footstepTimer = 0f;
            }
            else if (!isMoving && _playerMoveSoundInstance.State == SoundState.Playing)
            {
                _playerMoveSoundInstance.Stop();
            }

            // Placeholder for multiplayer logic
            if (_isMultiplayer)
            {
                // Add multiplayer-specific logic here in the future
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _graphicsDevice.Clear(Color.CornflowerBlue);

            // Use player's camera transform
            spriteBatch.Begin(transformMatrix: _player.CameraTransform);

            int tileSize = 32;
            float grassScale = 2f;

            // First pass: Draw grass tiles (2)
            for (int y = 0; y < _backgroundTiles.GetLength(0); y++)
            {
                for (int x = 0; x < _backgroundTiles.GetLength(1); x++)
                {
                    if (_backgroundTiles[y, x] == 2)
                    {
                        Vector2 position = new Vector2(x * tileSize, y * tileSize);
                        Vector2 scale = new Vector2(grassScale, grassScale);
                        spriteBatch.Draw(
                            _grassTile,
                            position,
                            null,
                            Color.White,
                            0f,
                            Vector2.Zero,
                            scale,
                            SpriteEffects.None,
                            0f
                        );
                    }
                }
            }

            // Second pass: Draw houses (0) and trees (1)
            for (int y = 0; y < _backgroundTiles.GetLength(0); y++)
            {
                for (int x = 0; x < _backgroundTiles.GetLength(1); x++)
                {
                    Texture2D tileTexture = null;
                    Vector2 position = new Vector2(x * tileSize, y * tileSize);
                    Vector2 scale = Vector2.One;

                    switch (_backgroundTiles[y, x])
                    {
                        case 0:
                            tileTexture = _houseTile;
                            break;
                        case 1:
                            tileTexture = _treeTile;
                            break;
                    }

                    if (tileTexture != null)
                    {
                        spriteBatch.Draw(
                            tileTexture,
                            position,
                            null,
                            Color.White,
                            0f,
                            Vector2.Zero,
                            scale,
                            SpriteEffects.None,
                            0f
                        );
                    }
                }
            }

            // Draw player
            _player.Draw(spriteBatch);

            spriteBatch.End();
        }
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ForestQuest.Entities;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media; // Toegevoegd voor Song en MediaPlayer

namespace ForestQuest
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _houseTile;
        private Texture2D _treeTile;
        private Texture2D _grassTile;

        private Player _player; // Gebruik de Player-klasse

        private Song _backgroundMusic; // Gewijzigd van SoundEffect naar Song
        private SoundEffect _playerMoveSound; // Geluid voor spelerbeweging

        private float _footstepTimer = 0f;

        // Vooraf gedefinieerde achtergrond array
        private int[,] _backgroundTiles = new int[,]
        {
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2 },
            { 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 1, 1, 2, 0, 2, 2, 2, 2, 0, 2, 2, 1, 2, 2 },
            { 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2 },
            { 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2 },
            { 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2 },
            { 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2 },
            { 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 1, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 }
        };

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Stel de resolutie in en sta resizing toe
            _graphics.PreferredBackBufferWidth = 800; // Breedte van het venster
            _graphics.PreferredBackBufferHeight = 600; // Hoogte van het venster
            Window.AllowUserResizing = true; // Sta resizing toe
        }

        private SoundEffectInstance _playerMoveSoundInstance; // Instance voor voetstapgeluiden

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Laad de tile textures
            _houseTile = Content.Load<Texture2D>("Background/House/Slice 10");
            _treeTile = Content.Load<Texture2D>("Background/Tree/Slice 12");
            _grassTile = Content.Load<Texture2D>("Background/Grass/Slice 21");

            // Laad audio
            _backgroundMusic = Content.Load<Song>("Audio/background_music");
            _playerMoveSound = Content.Load<SoundEffect>("Audio/footsteps");

            // Maak een instance voor voetstapgeluiden
            _playerMoveSoundInstance = _playerMoveSound.CreateInstance();
            _playerMoveSoundInstance.IsLooped = false; // Voetstapgeluiden mogen niet loopen

            // Speel achtergrondmuziek af met MediaPlayer
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(_backgroundMusic);

            // Initialiseer de speler
            _player = new Player(new Vector2(0, 0));
            _player.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            // Bereken de totale breedte en hoogte van de achtergrond
            int tileSize = 32;
            int backgroundWidth = _backgroundTiles.GetLength(1) * tileSize;
            int backgroundHeight = _backgroundTiles.GetLength(0) * tileSize;

            // Bereken de offset om de achtergrond te centreren
            int offsetX = (GraphicsDevice.Viewport.Width - backgroundWidth) / 2;
            int offsetY = (GraphicsDevice.Viewport.Height - backgroundHeight) / 2;

            // Update de speler met de gecentreerde achtergrond
            _player.Update(keyboardState, gameTime, GraphicsDevice.Viewport, _backgroundTiles, offsetX, offsetY);

            // Timer voor voetstapgeluiden
            const float footstepInterval = 0.3f; // Interval tussen voetstapgeluiden in seconden
            _footstepTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Controleer of de speler beweegt
            bool isMoving = keyboardState.IsKeyDown(Keys.Z) || keyboardState.IsKeyDown(Keys.Q) ||
                            keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.D);

            if (isMoving && _footstepTimer >= footstepInterval)
            {
                if (_playerMoveSoundInstance.State != SoundState.Playing) // Controleer of het geluid niet al speelt
                {
                    _playerMoveSoundInstance.Play();
                }
                _footstepTimer = 0f; // Reset de timer
            }
            else if (!isMoving && _playerMoveSoundInstance.State == SoundState.Playing)
            {
                // Stop het geluid als de speler niet beweegt
                _playerMoveSoundInstance.Stop();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            int tileSize = 32; // Tile-grootte
            float grassScale = 2f; // Schaal voor gras

            // Bereken de totale breedte en hoogte van de achtergrond
            int backgroundWidth = _backgroundTiles.GetLength(1) * tileSize;
            int backgroundHeight = _backgroundTiles.GetLength(0) * tileSize;

            // Bereken de offset om de achtergrond te centreren
            int offsetX = (GraphicsDevice.Viewport.Width - backgroundWidth) / 2;
            int offsetY = (GraphicsDevice.Viewport.Height - backgroundHeight) / 2;

            // Eerste pass: Teken alleen gras (2)
            for (int y = 0; y < _backgroundTiles.GetLength(0); y++)
            {
                for (int x = 0; x < _backgroundTiles.GetLength(1); x++)
                {
                    if (_backgroundTiles[y, x] == 2) // Alleen gras tekenen
                    {
                        Vector2 position = new Vector2(offsetX + x * tileSize, offsetY + y * tileSize);
                        Vector2 scale = new Vector2(grassScale, grassScale);
                        _spriteBatch.Draw(
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

            // Tweede pass: Teken huizen (0) en bomen (1)
            for (int y = 0; y < _backgroundTiles.GetLength(0); y++)
            {
                for (int x = 0; x < _backgroundTiles.GetLength(1); x++)
                {
                    Texture2D tileTexture = null;
                    Vector2 position = new Vector2(offsetX + x * tileSize, offsetY + y * tileSize);
                    Vector2 scale = Vector2.One; // Standaard schaal

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
                        _spriteBatch.Draw(
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

            // Teken de speler
            _player.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
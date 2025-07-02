using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ForestQuest.Entities;

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

        // Vooraf gedefinieerde achtergrond array
        private int[,] _backgroundTiles = new int[,]
        {
    { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
    { 2, 2, 1, 2, 2, 1, 1, 2, 1, 2, 2, 1, 2, 2, 2 },
    { 2, 1, 2, 2, 0, 2, 1, 2, 2, 1, 0, 2, 1, 2, 2 },
    { 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2 },
    { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2 },
    { 2, 1, 2, 2, 2, 1, 2, 2, 2, 1, 2, 2, 2, 2, 2 },
    { 2, 2, 1, 2, 0, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2 },
    { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2 },
    { 2, 1, 2, 2, 1, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2 },
    { 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2 },
    { 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2 },
    { 2, 1, 2, 0, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2 },
    { 2, 2, 2, 2, 1, 2, 2, 2, 1, 2, 2, 2, 1, 2, 2 },
    { 2, 2, 1, 2, 2, 1, 2, 2, 2, 1, 2, 2, 2, 1, 2 },
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

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Laad de tile textures
            _houseTile = Content.Load<Texture2D>("Background/House/Slice 10");
            _treeTile = Content.Load<Texture2D>("Background/Tree/Slice 12");
            _grassTile = Content.Load<Texture2D>("Background/Grass/Slice 21");

            // Initialiseer de speler
            _player = new Player(new Vector2(400, 300)); // Startpositie
            _player.LoadContent(Content); // Laad de speler texture
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            // Update de speler met achtergrondtiles voor collision
            _player.Update(keyboardState, gameTime, GraphicsDevice.Viewport, _backgroundTiles);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            int tileSize = 32; // Halveer de tile-grootte
            float grassScale = 2f; // Pas de schaal aan om tiles visueel te verkleinen

            // Eerste pass: Teken alleen gras (2)
            for (int y = 0; y < _backgroundTiles.GetLength(0); y++)
            {
                for (int x = 0; x < _backgroundTiles.GetLength(1); x++)
                {
                    if (_backgroundTiles[y, x] == 2) // Alleen gras tekenen
                    {
                        Vector2 position = new Vector2(x * tileSize, y * tileSize);
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
                    Vector2 position = new Vector2(x * tileSize, y * tileSize);
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

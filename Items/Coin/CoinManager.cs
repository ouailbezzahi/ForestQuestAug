using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ForestQuest.Items.Coin
{
    public class CoinManager
    {
        private List<AnimatedCoin> _coins = new List<AnimatedCoin>();
        private Random _random = new Random();
        private int _spawnCount = 10;
        private int _tileSize = 32;
        private int _mapWidth;
        private int _mapHeight;

        public List<AnimatedCoin> Coins => _coins;

        public CoinManager(ContentManager content, int mapWidth, int mapHeight)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            SpawnCoins(content);
        }

        private void SpawnCoins(ContentManager content)
        {
            _coins.Clear();
            for (int i = 0; i < _spawnCount; i++)
            {
                int x = _random.Next(1, _mapWidth - 2) * _tileSize + 4; // +4 voor marge
                int y = _random.Next(1, _mapHeight - 2) * _tileSize + 4;
                _coins.Add(new AnimatedCoin(content, new Vector2(x, y)));
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (var coin in _coins)
            {
                coin.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var coin in _coins)
            {
                coin.Draw(spriteBatch);
            }
        }
    }
} 
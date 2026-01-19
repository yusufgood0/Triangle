using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TextureArray
{
    internal class TextureHolder
    {
        public enum EndEffect
        {
            Loop,
            OneTime,
            Stop,
            SuppressUpdate
        }
        (Texture2D texture, EndEffect EndEffect)[] _textures;
        int _currentSpriteSheet;
        float _currentColumn; // allow float for changing between frames
        int _defaultFrame;
        Rectangle _drawRect;

        public int X
        {
            get { return _drawRect.X; }
            set {  _drawRect.X = value; }
        }
        public int Y
        {
            get { return _drawRect.Y; }
            set {  _drawRect.Y = value; }
        }
        public int width
        {
            get { return _drawRect.Width; }
            set {  _drawRect.Width = value; }
        }
        public int height
        {
            get { return _drawRect.Height; }
            set {  _drawRect.Height = value; }
        }
        Texture2D Texture => _textures[i].texture;
        int i => _currentSpriteSheet;
        int Width => _textures[i].texture.Width;
        int Height => _textures[i].texture.Height;
        int Size => Height; // size of each frame, assume each frame is square
        int Frame => (int)_currentColumn;
        Rectangle SourceRectangle => new Rectangle(Frame * Size, 0, Size, Size);
        EndEffect CurrentEndEffect => _textures[i].EndEffect;
        bool SuppressUpdate => CurrentEndEffect == EndEffect.SuppressUpdate;
        int maxColumn => Width / Height; // the number of columns
        public TextureHolder(Texture2D textures, EndEffect areOneTime, Rectangle drawRect, int SpriteSheet = 0, int defaultFrame = 0)
        {
            _textures = new (Texture2D texture, EndEffect oneTimeTexture)[1];
            _textures[0] = (textures, areOneTime);

            _drawRect = drawRect;
            _currentSpriteSheet = SpriteSheet;
            _defaultFrame = defaultFrame;
        }
        public TextureHolder(Texture2D[] textures, EndEffect[] areOneTime, Rectangle drawRect, int SpriteSheet = 0, int defaultFrame = 0)
        {
            for (int i = 0; i < _textures.Length; i++)
            {
                _textures[i] = (textures[i], areOneTime[i]);
            }

            _drawRect = drawRect;
            _currentSpriteSheet = SpriteSheet;
            _defaultFrame = defaultFrame;
        }
        public TextureHolder((String name, EndEffect oneTimeTexture)[] textures, Rectangle drawRect, ContentManager Content, int SpriteSheet = 0, int defaultFrame = 0)
        {
            Texture2D[] loadedTextures = Array.ConvertAll(textures, t => Content.Load<Texture2D>(t.name)).ToArray();
            _textures = new (Texture2D texture, EndEffect oneTimeTexture)[textures.Length];
            for (int i = 0; i < _textures.Length; i++)
            {
                _textures[i] = (loadedTextures[i], textures[i].oneTimeTexture);
            }

            _drawRect = drawRect;
            _currentSpriteSheet = SpriteSheet;
            _defaultFrame = defaultFrame;
        }
        public int CurrentSpriteSheet
        {
            get { return _currentSpriteSheet; }
            set { _currentSpriteSheet = value; }
        }
        public float CurrentColumn
        {
            get { return _currentColumn; }
            set { _currentColumn = value; }
        }
        public void Update(float amount)
        {
            if (SuppressUpdate) return; // prevent negative frame
            SetColumn(_currentColumn + amount);
        }
        public void SetColumn(float amount)
        {
            if (amount < 0)
            {
                _currentColumn = -1;
            }
            _currentColumn = amount;
            if (_currentColumn >= maxColumn) // check if we exceed the max column
            {
                _currentColumn = 0; // reset to first frame
                ApplyEndEffect(_textures[i].EndEffect);
            }
        }
        public void SetSpriteSheet(int index)
        {
            _currentSpriteSheet = index;
            _currentColumn = 0f;
        }
        public void ApplyEndEffect(EndEffect effect)
        {
            switch (effect)
            {
                case EndEffect.Loop:
                    // Do nothing, will loop automatically
                    break;
                case EndEffect.OneTime:
                    // Reset to default frame
                    _currentColumn = _defaultFrame;
                    break;
                case EndEffect.Stop:
                    // Stay on the last frame
                    _currentColumn = maxColumn - 1;
                    break;
            }
        }
        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            _drawRect.X = (int)position.X;
            _drawRect.Y = (int)position.Y;
            Draw(spriteBatch);
        }
        public void Draw(SpriteBatch spriteBatch, Rectangle rect)
        {
            _drawRect = rect;
            Draw(spriteBatch);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (_currentColumn < 0) return; // do not draw if negative frame
            spriteBatch.Begin(depthStencilState: spriteBatch.GraphicsDevice.DepthStencilState);
            spriteBatch.Draw(Texture, _drawRect, SourceRectangle, Color.White);
            spriteBatch.End();
        }
        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            spriteBatch.Begin(depthStencilState: spriteBatch.GraphicsDevice.DepthStencilState);
            spriteBatch.Draw(texture, _drawRect, Color.Red * 0.2f);
            spriteBatch.End();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SlimeGame.GameAsset;
using SlimeGame.GameAsset.Exp;
using SlimeGame.GameAsset.SpriteSheets;
using SlimeGame.Input;

namespace SlimeGame.Drawing.TextureDrawers
{
    internal class UIManager
    {
        //TextureHolder[] _elementDisplay;
        Element[] _elements;
        SpriteSheetManager _healthDisplay;
        Slider _expDisplay;
        Slider _manaDisplay;
        string level = "1";
        Vector2 levelDrawPos;
        Vector2 _crosshairPos;
        int _crosshairSize;
        SpriteFont _spriteFont;
        SpriteBatch _spriteBatch;
        Rectangle _ControlRect;
        Vector2 _screenSize;
        int _score;
        Texture2D _NotchTexture;

        public UIManager(TextureHolder[] elementDisplay, SpriteSheetManager healthDisplay, Slider expDisplay, Slider manaDisplay, SpriteFont spriteFont, SpriteBatch spritebatch, Texture2D texture)
        {
            _healthDisplay = healthDisplay;
            _expDisplay = expDisplay;
            _manaDisplay = manaDisplay;
            _spriteFont = spriteFont;
            _spriteBatch = spritebatch;
            int width = _spriteBatch.GraphicsDevice.Viewport.Width;
            int height = _spriteBatch.GraphicsDevice.Viewport.Height;

            int size = Math.Min(width, height);
            _crosshairSize = (int)(size * 0.01f);
            Vector2 crosshairSizeVector = new Vector2(_crosshairSize, _crosshairSize);
            Vector2 center = new Vector2(width, height) / 2;
            _crosshairPos = center - crosshairSizeVector / 2;

            int controlYSize = 200;
            _ControlRect = new Rectangle(0, height - controlYSize, 0, controlYSize);

            _screenSize = new(width, height);

            _NotchTexture = texture;
        }

        public void Update(Player player, SpellBook spellbook, float manaPercent)
        {
            _elements = spellbook.Elements;


            _healthDisplay.CheckForIncrementUpdates();
            _healthDisplay.SetTotalOpacity(player.Health / (float)player.MaxHealth);
            _healthDisplay.SetNumberOfInstances((int)Math.Ceiling(player.MaxHealth));

            player.LevelManager.SyncSlider(ref _expDisplay);
            level = player.LevelManager.Level.ToString();
            Rectangle rect = _expDisplay.Rectangle;
            int lineSpacing = _spriteFont.LineSpacing;
            levelDrawPos.Y = rect.Y + rect.Height + lineSpacing / 2;
            levelDrawPos.X = rect.X + rect.Width / 2;

            _manaDisplay.Percent = manaPercent;

            _score = player.LevelManager.LifeTimeExperience;
        }
        public void RotateLastHeart(int PlayerHealth)
        {
            _healthDisplay.SetInstanceCell(PlayerHealth - 1, (0, 0));
        }
        public void Draw(Texture2D pixel, MasterInput input)
        {
            for (int i = 0; i < SpellBook.SpellDictionary.Count; i++)
            {
                var element = SpellBook.SpellList.ElementAt(i);
                SpellBook.Spell spell = element;
                var e = spell.Elements;
                Color[] colors;
                if (spell.Active == true)
                {
                    colors = new Color[] { SpellBook.GetElementColor(e.b), SpellBook.GetElementColor(e.a), SpellBook.GetElementColor(e.c) };
                }
                else
                {
                    colors = new Color[] { Color.AntiqueWhite, Color.AntiqueWhite, Color.AntiqueWhite };
                }
                //byte R = (byte)((colors[0].R + colors[0].R + colors[0].R)/3);
                //byte G = (byte)((colors[0].G + colors[0].G + colors[0].G)/3);
                //byte B = (byte)((colors[0].B + colors[0].R + colors[0].B)/3);
                int size = (int)(_screenSize.Y * 0.08f);
                (int width, int height) = (size, size);
                (int X, int Y) = ((int)_screenSize.X - width, 100);

                for (int j = 0; j < 3; j++)
                {
                    Rectangle rect = new Rectangle(
                        X,
                        Y + height * i,
                        width,
                        height
                        );
                    _spriteBatch.Begin(depthStencilState: _spriteBatch.GraphicsDevice.DepthStencilState);
                    _spriteBatch.Draw(_NotchTexture, rect, new Rectangle(_NotchTexture.Width / 3 * j, 0, _NotchTexture.Width / 3, _NotchTexture.Height), colors[j]);
                    _spriteBatch.End();
                }
            }
            //Elements
            int spacing = 10;
            int currentElementsFullWidth = (int)(_screenSize.X * 0.3f);
            int currentElementsCellWidth = (int)((currentElementsFullWidth - spacing * 2f) / 3f);
            Vector2 currentElementsPos = new(_screenSize.X / 2 - currentElementsFullWidth / 2, _expDisplay.Rectangle.Y - currentElementsCellWidth);
            TextureHolder[] currentElementsHolder = TripleElementDisplay.Draw(_spriteBatch, _elements, currentElementsCellWidth, (int)currentElementsPos.X, (int)currentElementsPos.Y, spacing);

            for (int i = 0; i < currentElementsHolder.Length; i++)
            {
                float opacity = (MathF.Sin((float)(DateTime.Now.Date - DateTime.Now).TotalSeconds) + 1) * 0.1f;
                Color color = Color.Gold * opacity;
                if (i == currentElementsHolder.Length - 1)
                    currentElementsHolder[i].Draw(_spriteBatch, pixel, color);
            }

            // Spell's and controls
            Element[] elements = new Element[] { Element.Fire, Element.Earth, Element.Air };
            KeybindActions[] elementActions = new KeybindActions[] { KeybindActions.AddElementFire, KeybindActions.AddElementEarth, KeybindActions.AddElementAir };
            TextureHolder[] holder = TripleElementDisplay.Draw(_spriteBatch, elements, (int)((_ControlRect.Height - spacing * 2f) / 3f), _ControlRect.X, _ControlRect.Y, spacing, true);


            _spriteBatch.Begin(depthStencilState: _spriteBatch.GraphicsDevice.DepthStencilState);
            for (int i = 0; i < holder.Length; i++)
            {
                TextureHolder rect = holder[i];
                Vector2 pos = new(rect.X + rect.width, rect.Y);
                string text = input.GetKeyBindName(elementActions[i]);
                _spriteBatch.DrawString(_spriteFont, text, pos, Color.White);
            }

            _healthDisplay.DrawAllInstancesFraction(_spriteBatch);
            // Levels
            Vector2 lvlDrawPos = levelDrawPos;
            lvlDrawPos.X -= _spriteFont.MeasureString(level).X / 2;
            _spriteBatch.DrawString(_spriteFont, level, lvlDrawPos, _expDisplay.Color);


            // crosshair
            Rectangle rect1 = new Rectangle((int)_crosshairPos.X + _crosshairSize / 3, (int)_crosshairPos.Y, _crosshairSize / 3, _crosshairSize);
            Rectangle rect2 = new Rectangle((int)_crosshairPos.X, (int)_crosshairPos.Y + _crosshairSize / 3, _crosshairSize, _crosshairSize / 3);
            _spriteBatch.Draw(pixel, rect1, Color.White);
            _spriteBatch.Draw(pixel, rect2, Color.White);

            // Exp
            _expDisplay.Draw(_spriteBatch, pixel);

            // Score
            string score = "score: " + _score.ToString();
            Vector2 scorePos = _screenSize - _spriteFont.MeasureString(score);
            _spriteBatch.DrawString(_spriteFont, score, scorePos, Color.White);

            //_manaDisplay.Draw(_spriteBatch, pixel);
            _spriteBatch.End();
        }


    }
}

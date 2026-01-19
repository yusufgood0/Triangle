using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SlimeGame.GameAsset;
using SlimeGame.GameAsset.SpriteSheets;
using TextureArray;

namespace SlimeGame
{
    internal class UIManager
    {
        TextureHolder[] _elementDisplay;
        SpriteSheetManager _healthDisplay;
        Slider _expDisplay;
        Slider _manaDisplay;
        String level = "1";
        Vector2 levelDrawPos;
        SpriteFont _spriteFont;
        SpriteBatch _spriteBatch;

        public UIManager(TextureHolder[] elementDisplay, SpriteSheetManager healthDisplay, Slider expDisplay, Slider manaDisplay, SpriteFont spriteFont, SpriteBatch spritebatch)
        {
            _elementDisplay = elementDisplay;
            _healthDisplay = healthDisplay;
            _expDisplay = expDisplay;
            _manaDisplay = manaDisplay;
            _spriteFont = spriteFont;
            _spriteBatch = spritebatch;
        }

        public void Update(Player player, SpellBook spellbook, ExpManager expManager, float manaPercent)
        {
            foreach (var element in _elementDisplay)
            {
                element.SetColumn(-1);
            }
            Element[] elements = spellbook.Elements;
            for (int i = 0; i < elements.Length; i++)
            {
                int column;
                switch (elements[i])
                {
                    case Element.Fire:
                        column = 0;
                        break;
                    case Element.Water:
                        column = 1;
                        break;
                    case Element.Earth:
                        column = 5;
                        break;
                    case Element.Air:
                        column = 3;
                        break;
                    default:
                        column = -1;
                        break;
                }
                _elementDisplay[i].SetColumn(column);
            }

            _healthDisplay.CheckForIncrementUpdates();
            _healthDisplay.SetTotalOpacity(player.Health / (float)Player.MaxHealth);
            _healthDisplay.SetNumberOfInstances((int)Math.Ceiling(Player.MaxHealth));

            expManager.SyncSlider(ref _expDisplay);
            level = expManager.Level.ToString();
            Rectangle rect = _expDisplay.Rectangle;
            int lineSpacing = _spriteFont.LineSpacing;
            levelDrawPos.Y = rect.Y + rect.Height + lineSpacing / 2;
            levelDrawPos.X = rect.X + rect.Width / 2;

            _manaDisplay.Percent = manaPercent;
        }
        public void RotateLastHeart(int PlayerHealth)
        {
            _healthDisplay.SetInstanceCell(PlayerHealth - 1, (0, 0));
        }
        public void Draw(Texture2D pixel)
        {
            foreach (var element in _elementDisplay)
            {
                element.Draw(_spriteBatch, pixel);
                element.Draw(_spriteBatch);
            }

            _spriteBatch.Begin(depthStencilState: _spriteBatch.GraphicsDevice.DepthStencilState);
            _healthDisplay.DrawAllInstancesFraction(_spriteBatch);

            _spriteBatch.DrawString(_spriteFont, level, levelDrawPos, _expDisplay.Color);

            _expDisplay.Draw(_spriteBatch, pixel);
            _manaDisplay.Draw(_spriteBatch, pixel);
            _spriteBatch.End();
        }


    }
}

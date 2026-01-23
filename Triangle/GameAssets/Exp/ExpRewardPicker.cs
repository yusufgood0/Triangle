using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.GameAsset;
using SlimeGame.Input;
using SpriteFont_inside_a_rect;

namespace SlimeGame.GameAsset.Exp
{
    internal class ExpRewardPicker
    {
        List<ExpReward> _rewards = GetAllRewards();
        Random _random = new Random();
        const int AMOUNT_OF_HEALTH_REWARDS = 500;
        List<ExpReward> _selection;
        Rectangle[] _drawRects;
        Texture2D _selectionTexture;
        float _scale = 1f;

        public ExpRewardPicker(Point screenSize, float xPaddingPercent, float yPaddingPercent, Texture2D texture, SpriteFont spriteFont)
        {
            _selectionTexture = texture;
            _drawRects = new Rectangle[3];
            int width = screenSize.X / 3;
            int xPadding = (int)(xPaddingPercent * screenSize.X);
            int yPadding = (int)(yPaddingPercent * screenSize.Y);

            int finalWidth = width - xPadding * 2;
            int finalHeight = screenSize.Y - yPadding * 2;

            for (int i = 0; i < 3; i++)
            {
                _drawRects[i] = new Rectangle(
                    xPadding + width * i,
                    yPadding,
                    finalWidth,
                    finalHeight
                    );
            }
            _scale = FindNewScale(spriteFont);
            SetRandomSelection();
        }
        /// <summary>
        /// Finds the recommended scale based off the length of descriptions and stuff
        /// </summary>
        /// <param name="descriptionFont"></param>
        /// <exception cref="Exception"></exception>
        public float FindNewScale(SpriteFont descriptionFont)
        {
            Rectangle rect = _drawRects[0];
            Rectangle descrptionRect = new Rectangle(
                    (int)(rect.X + rect.Width * 0.15f),
                    (int)(rect.Y + rect.Height * 0.4f),
                    (int)(rect.Width * 0.75f),
                    (int)(rect.Height * 0.5f)
                    );
            float smallestScale = float.MaxValue;
            foreach (ExpReward reward in _rewards)
            {
                TextRect textRect = new TextRect(descrptionRect, reward.Description, descriptionFont);
                smallestScale = Math.Min(textRect._scale, smallestScale);
            }
            return smallestScale;
        }
        public void ActivateAllRewards(Player player)
        {
            foreach (var reward in _rewards)
            {
                reward.Activate(player);
            }
        }
        public void Draw(SpriteBatch spritebatch, SpriteFont titleFont, SpriteFont descriptionFont, Point screenSize)
        {
            for (int i = 0; i < _selection.Count; i++)
            {
                Rectangle rect = _drawRects[i];
                Rectangle titleRect = new Rectangle(
                    (int)(rect.X + rect.Width * 0.1f),
                    (int)(rect.Y + rect.Height * 0.1f),
                    (int)(rect.Width * 0.8f),
                    (int)(rect.Height * 0.15f)
                    );
                (int OffsetX, int OffsetY) = (titleRect.X, titleRect.Y + titleRect.Height);
                Rectangle descrptionRect = new Rectangle(
                    (int)(rect.X + rect.Width * 0.15f),
                    (int)(rect.Y + rect.Height * 0.4f),
                    (int)(rect.Width * 0.75f),
                    (int)(rect.Height * 0.5f)
                    );
                Vector2 titlePos = new(screenSize.X / 2 - titleFont.MeasureString("Pick to unlock a spell").X / 2, 50);
                if (_selection != null)
                {
                    ExpReward reward = _selection[i];

                    spritebatch.Begin(depthStencilState: spritebatch.GraphicsDevice.DepthStencilState);
                    spritebatch.Draw(_selectionTexture, rect, Color.White);
                    TextRect textRect = new TextRect(titleRect, reward.Name, titleFont, _scale);
                    TextRect textRect2 = new TextRect(descrptionRect, reward.Description, descriptionFont, _scale);
                    textRect.Draw(spritebatch, Color.Black);
                    textRect2.Draw(spritebatch, Color.Black);
                    spritebatch.DrawString(titleFont, "Pick to unlock a spell", titlePos, Color.White);
                    spritebatch.End();

                    if (reward is SpellReward)
                    {
                        SpellReward spellReward = (SpellReward)reward;
                        int spacing = 10;
                        TripleElementDisplay.Draw(spritebatch, spellReward.Spell.Elements, (int)((descrptionRect.Width - spacing * 2f) / 3f), OffsetX, OffsetY, spacing, false);
                    }
                }

            }
        }
        public bool PickSelection(MasterInput input, Player player)
        {
            if (input.OnLeftPress)
                for (int i = 0; i < _drawRects.Length; i++)
                {
                    Rectangle rect = _drawRects[i];
                    if (rect.Contains(input.MouseState.Position))
                    {
                        return PickSelection(i, player);
                    }
                }
            return false;
        }
        public bool PickSelection(int selection, Player assetManager)
        {
            if (selection >= _selection.Count) return false;
            ExpReward selected = _selection[selection];
            selected.Activate(assetManager);
            _rewards.Remove(selected);
            //_selection = null;
            SetRandomSelection();
            return true;
        }
        public void SetRandomSelection()
        {
            HashSet<ExpReward> selected = new(3);
            Random rng = _random;

            // Group spell rewards by complexity (1–3)
            List<ExpReward>[] complexities = new List<ExpReward>[3]
            {
            new(),
            new(),
            new()
            };

            foreach (SpellReward reward in _rewards.OfType<SpellReward>())
            {
                int index = Math.Clamp(reward.Complexity - 1, 0, 2);
                complexities[index].Add(reward);
            }

            int targetCount = Math.Min(3, _rewards.Count);


            // Optional heart reward (20% chance)
            if (rng.Next(1, 6) == 1 || !_rewards.Any(i => i is not HealthReward))
            {
                HealthReward heart = _rewards.OfType<HealthReward>().FirstOrDefault();
                if (heart != null)
                {
                    selected.Add(heart);
                }
            }


            // Shuffle each complexity list
            foreach (var list in complexities)
            {
                list.Sort((a, b) => rng.Next(-1, 2));
            }

            // Round-robin pick across complexities
            while (selected.Count < targetCount)
            {
                bool added = false;

                for (int i = 0; i < complexities.Length && selected.Count < targetCount; i++)
                {
                    if (complexities[i].Count == 0) continue;

                    ExpReward reward = complexities[i][0];
                    complexities[i].RemoveAt(0);

                    if (selected.Add(reward))
                    {
                        added = true;
                    }
                }

                // Nothing left to add
                if (!added) break;
            }

            _selection = selected.ToList();

            foreach (var reward in _selection)
            {
                Debug.WriteLine(reward.Name);
            }
        }

        static List<ExpReward> GetAllRewards()
        {
            List<ExpReward> returnList = new List<ExpReward>();
            returnList.AddRange(GetSpellRewards());
            returnList.AddRange(GetHealthRewards());
            return returnList;
        }
        static IEnumerable<SpellReward> GetSpellRewards()
        {
            foreach (var reward in SpellBook.SpellDictionary)
            {
                SpellBook.Spell spell = reward.Value as SpellBook.Spell;
                SpellReward expReward = new SpellReward(spell);
                yield return expReward;
            }
        }
        static IEnumerable<HealthReward> GetHealthRewards()
        {
            for (int i = 0; i < AMOUNT_OF_HEALTH_REWARDS; i++)
            {
                yield return new HealthReward();
            }
        }
    }
}

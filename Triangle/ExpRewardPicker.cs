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

namespace SlimeGame
{
    internal class ExpRewardPicker
    {
        List<ExpReward> _rewards = GetAllRewards();
        Random _random = new Random();
        const int AMOUNT_OF_HEALTH_REWARDS = 5;
        List<ExpReward> _selection;
        Rectangle[] _drawRects;
        Texture2D _selectionTexture;


        public ExpRewardPicker(Point screenSize, float xPaddingPercent, float yPaddingPercent, Texture2D texture)
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
            SetRandomSelection();
        }
        public void ActivateAllRewards(Player player)
        {
            foreach (var reward in _rewards)
            {
                reward.Activate(player);
            }
        }
        public void Draw(SpriteBatch spritebatch, SpriteFont font)
        {
            for (int i = 0; i < _drawRects.Length; i++)
            {

                Rectangle rect = _drawRects[i];
                spritebatch.Begin(depthStencilState: spritebatch.GraphicsDevice.DepthStencilState);
                spritebatch.Draw(_selectionTexture, rect, Color.White);
                if (_selection != null)
                {
                    ExpReward reward = _selection[i];
                    Vector2 upperThirdCenter = new Vector2(rect.X + rect.Width * 0.5f - font.MeasureString(reward.Name).X / 2, rect.Y + rect.Height * 0.2f);
                    spritebatch.DrawString(font, reward.Name, upperThirdCenter, Color.Maroon);
                }
                spritebatch.End();
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
                        PickSelection(i, player);
                        return true;
                    }
                }
            return false;
        }
        public void PickSelection(int selection, Player objectManager)
        {
            ExpReward selected = _selection[selection];
            selected.Activate(objectManager);
            _rewards.Remove(selected);
            //_selection = null;
            SetRandomSelection();
        }
        public void SetRandomSelection()
        {
            HashSet<ExpReward> returnValues = new();

            while (returnValues.Count < 3)
            {
                bool addHeart = 1 == _random.Next(1, 5) && _rewards.Count > 0;
                if (addHeart)
                {
                    returnValues.Add(_rewards.Find(i => i is HealthReward));
                    continue;
                }
                List<ExpReward>[] complexities = new List<ExpReward>[3];
                for (int i = 0; i < 3; i++)
                {
                    complexities[i] = new List<ExpReward>();
                }
                foreach (SpellReward reward in _rewards.FindAll(i => i is SpellReward))
                {
                    complexities[reward.Complexity - 1].Add(reward);
                }
                for (int i = 0; i < complexities.Length; i++)
                {
                    List<ExpReward> complexity = complexities[i];
                    if (complexity.Count() == 1 && returnValues.Contains(complexity[0]))
                    {
                        continue;
                    }
                    if (complexity.Count > 0)
                    {
                        ExpReward reward = complexity[_random.Next(0, complexity.Count)];
                        returnValues.Add(reward);
                        break;
                    }
                }
            }
            foreach (var reward in returnValues.ToList())
            {
                Debug.WriteLine(reward.Name);
            }

            _selection = returnValues.ToList();
        }
        static List<ExpReward> GetAllRewards()
        {
            List<ExpReward> returnList = new List<ExpReward>();
            returnList.AddRange(GetSpellRewards());
            returnList.AddRange(GetHealthRewards());
            return returnList;
        }
        static IEnumerable<ExpReward> GetSpellRewards()
        {
            foreach (var reward in SpellBook.SpellDictionary)
            {
                SpellBook.Spell spell = reward.Value as SpellBook.Spell;
                ExpReward expReward = new SpellReward(spell);
                yield return expReward;
            }
        }
        static IEnumerable<ExpReward> GetHealthRewards()
        {
            for (int i = 0; i < AMOUNT_OF_HEALTH_REWARDS; i++)
            {
                yield return new HealthReward();
            }
        }
    }
}

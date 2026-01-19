using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.GameAsset;
using SlimeGame.Input;

namespace SlimeGame
{
    internal class ExpManager
    {
        int _exp;
        int _level;
        int _levelsToIncrease = 1;
        ExpRewardPicker _rewardPicker;

        const float EXP_GROWTH_RATE = 1.5f;
        const int BASE_EXP = 500;
        const int MAX_LEVEL = 20;
        int ExpToNextLevel => (int)(BASE_EXP * Math.Pow(EXP_GROWTH_RATE, _level - 1));
        public int Level => _level;
        public float ExpProgress => (float)_exp / ExpToNextLevel;
        public bool LevelUpMenu => _levelsToIncrease > 0;

        public ExpManager(Point screenSize, float xPaddingPercent, float yPaddingPercent, Texture2D texture)
        {
            _level = 1;
            _exp = 0;
            _rewardPicker = new ExpRewardPicker(screenSize, xPaddingPercent, yPaddingPercent, texture);
        }
        public int AddExp(int amount)
        {
            if (_level >= MAX_LEVEL)
            {
                return 0;
            }

            _exp += amount;
            int levelIncrease = 0;
            while (_exp >= ExpToNextLevel)
            {
                _exp -= ExpToNextLevel;
                _level++;
                levelIncrease++;
            }
            _levelsToIncrease += levelIncrease;
            return levelIncrease;
        }
        public int AddExp(int amount, ref Slider slider)
        {
            int levelIncrease = AddExp(amount);
            SyncSlider(ref slider);
            return levelIncrease;
        }
        public void SyncSlider(ref Slider slider)
        {
            slider.Percent = ExpProgress;
        }
        public void RefreshAwards()
            =>_rewardPicker.SetRandomSelection();
        public void DrawLevelUpMenu(SpriteBatch spritebatch, SpriteFont font)
            => _rewardPicker.Draw(spritebatch, font);
        public void UpdateLevelUpMenu(MasterInput mouseState, Player player)
        {
            if (_rewardPicker.PickSelection(mouseState, player))
            {
                _levelsToIncrease -= 1;
            }
        }
        public void CheatAllRewards(Player player)
        {
            _rewardPicker.ActivateAllRewards(player);
        }

    }
}

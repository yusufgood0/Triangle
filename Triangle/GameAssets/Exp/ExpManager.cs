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

namespace SlimeGame.GameAsset.Exp
{
    internal class ExpManager
    {
        int _exp;
        int _level;
        int _rewardsOwed = 1;
        public int LifeTimeExperience;
        ExpRewardPicker _rewardPicker;

        const float EXP_GROWTH_RATE = 1.5f;
        const int BASE_EXP = 500;
        const int MAX_LEVEL = 20;
        public int ExpToNextLevel => (int)(BASE_EXP * Math.Pow(EXP_GROWTH_RATE, _level - 1));
        public int Level => _level;
        public float ExpProgress => (float)_exp / ExpToNextLevel;
        public bool LevelUpMenu => _rewardsOwed > 0;

        public ExpManager(Point screenSize, float xPaddingPercent, float yPaddingPercent, Texture2D texture, SpriteFont spriteFont)
        {
            _level = 1;
            _exp = 0;
            _rewardPicker = new ExpRewardPicker(screenSize, xPaddingPercent, yPaddingPercent, texture, spriteFont);
        }
        public int AddExp(int amount)
        {
            LifeTimeExperience += amount;
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
            _rewardsOwed += levelIncrease;
            return levelIncrease;
        }
        public int AddExp(int amount, ref Slider slider)
        {
            LifeTimeExperience += amount;
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
        public void DrawLevelUpMenu(SpriteBatch spritebatch, SpriteFont titleFont, SpriteFont descriptionFont, Point screenSize)
            => _rewardPicker.Draw(spritebatch, titleFont, descriptionFont, screenSize);
        public void UpdateLevelUpMenu(MasterInput mouseState, Player player)
        {
            if (_rewardPicker.PickSelection(mouseState, player))
            {
                _rewardsOwed -= 1;
            }
        }
        public void CheatAllRewards(Player player)
        {
            _rewardPicker.ActivateAllRewards(player);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimeGame.GameAsset.Projectiles;
using Microsoft.Xna.Framework;
using SlimeGame.GameAsset.IFrames;
using SlimeGame.Generation;
using static SlimeGame.GameAsset.SpellBook;
using SlimeGame.GameAssets.Enemies;

namespace SlimeGame.GameAsset.StatusEffects
{
    public enum BindType
    {
        ClampPosition,
        ReferenceToPlayer,
        DirectionToPlayer,
    }
    internal class EnemyBind : StatusEffect
    {
        public DateTime ExpireTime { get; }
        Enemy Enemy { get; }
        BindType BindType { get; }
        Vector3 ClampPosition { get; }
        public EnemyBind(Enemy Enemy, float duration, Vector3 ClampPosition, BindType BindType)
        {
            ExpireTime = Game1.PlayingGameTime.AddSeconds(duration);
            this.BindType = BindType;
            this.Enemy = Enemy;
            this.ClampPosition = ClampPosition;
        }
        public bool Update(AssetManager assetManager)
        {
            if (!assetManager.Enemies.Any(i => i.Equals(Enemy)))
            {
                StatusEffect.CancelEffect(this);
				return true;
			}
            Player player = assetManager.Player;
            switch (BindType)
            {
                case BindType.ClampPosition:
                    Enemy.SetPosition(ClampPosition);
                    break;
                case BindType.ReferenceToPlayer:
                    Enemy.SetPosition(player.Position + ClampPosition);
                    break;
                case BindType.DirectionToPlayer:
                    Vector3 vector = player.dirVector * ClampPosition.Length();
                    Enemy.SetPosition(player.Position + vector);
                    IFrameInstance.AddIFrame(new IFrameInstance(new object(), 0.5f, IFrameType.Universal));
                    break;
            }
            return false;
        }
    }
}

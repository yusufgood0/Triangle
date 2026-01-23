using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SlimeGame.GameAsset.IFrames
{
    public enum IFrameType
    {
        Universal,
        Local,
        SingleSource
    }
    internal class IFrameInstance
    {
        public static List<IFrameInstance> ActiveIFrames = new();
        public static object universalFiller = new object();
        public object DamageSource;
        public DateTime DeleteTime;
        public IFrameType FrameType;
        public IFrameInstance(object damageSource, float durationSeconds, IFrameType type)
        {
            DamageSource = type == IFrameType.Universal ? universalFiller : damageSource;
            DeleteTime = Game1.PlayingGameTime.AddSeconds(durationSeconds);
            FrameType = type;
        }
        public bool IsExpired => Game1.PlayingGameTime >= DeleteTime;
        public static void AddIFrame(IFrameInstance iframe)
        {
            if (iframe.IsImmune())
            {
                foreach (var active in ActiveIFrames)
                {
                    if (active.DamageSource.Equals(iframe.DamageSource))
                    {
                        ActiveIFrames.Remove(active);
                        break;
                    }
                }
            }
            ActiveIFrames.Add(iframe);
        }
        public static void CullExpiredIframes()
        {
            ActiveIFrames.RemoveAll(i => i.IsExpired);
        }
        public bool IsImmune()
        {
            if (ActiveIFrames.Any(i => i.FrameType == IFrameType.Universal))
            {
                return true;
            }
            switch (FrameType)
            {
                case IFrameType.Local:
                    return ActiveIFrames.Any(i => i.DamageSource.GetType() == DamageSource.GetType());
                case IFrameType.SingleSource:
                    return ActiveIFrames.Any(i => i.DamageSource.Equals(DamageSource));
                default:
                    return false;
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace SlimeGame.GameAssets
{
    internal class ErrorMessage
    {
        private static List<ErrorMessage> Errors = new();
        private static SpriteBatch SpriteBatch;
        private static SpriteFont Font;
        private const float Duration = 3.0f;
        private const float FadeTime = 1.0f;
        private static Color drawColor = Color.Red;
        private static Vector2 startPos = default;
        private static Vector2 endPos = default;
        private static SoundEffectInstance SoundEffectInstance;

        private DateTime _createTime;
        private string _message;
        private float _offset;
        private ErrorMessage(string message)
        {
            _message = message;
            _offset = Font.MeasureString(message).X / 2;
            _createTime = Game1.PlayingGameTime;
        }
        public static void Init(SpriteBatch SpriteBatch, SpriteFont Font, Vector2 startPos, Vector2 endPos, SoundEffectInstance sfx)
        {
            ErrorMessage.startPos = startPos;
            ErrorMessage.endPos = endPos;
            ErrorMessage.SpriteBatch = SpriteBatch;
            ErrorMessage.Font = Font;
            SoundEffectInstance = sfx;
        }
        public static void New(string message)
        {
            Errors.Add(new ErrorMessage(message));
            SoundEffectInstance.Stop();
            SoundEffectInstance.Play();
        }
        public static void DrawAll()
        {
            if (SpriteBatch == null) throw new Exception("Did not initialize error message class");
            Errors.RemoveAll(i => (float)(Game1.PlayingGameTime - i._createTime).TotalSeconds > Duration + FadeTime);
            foreach (var error in Errors)
            {
                TimeSpan span = Game1.PlayingGameTime - error._createTime;
                float lifespan = (float)span.TotalSeconds;
                Color color;
                Vector2 drawPos;

                if (lifespan < Duration)
                {
                    drawPos = startPos;
                    drawPos.X -= error._offset;
                    SpriteBatch.Begin(depthStencilState: SpriteBatch.GraphicsDevice.DepthStencilState);
                    SpriteBatch.DrawString(Font, error._message, drawPos, drawColor);
                    SpriteBatch.End();
                    continue;
                }

                float t = Math.Clamp((lifespan - Duration) / FadeTime, 0, 1);

                color = drawColor * (1-t);
                drawPos = Vector2.Lerp(startPos, endPos, t);
                drawPos.X -= error._offset;

                SpriteBatch.Begin(depthStencilState: SpriteBatch.GraphicsDevice.DepthStencilState);
                SpriteBatch.DrawString(Font, error._message, drawPos, color);
                SpriteBatch.End();
            }
        }
    }
}

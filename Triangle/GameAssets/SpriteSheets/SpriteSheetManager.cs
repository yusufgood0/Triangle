using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SlimeGame.GameAsset.SpriteSheets
{
    internal class SpriteSheetManager
    {
        private Texture2D _spritesheet;
        private (int X, int Y) _offset;
        private (int X, int Y) _pixelSize;
        private (int X, int Y) _maxCellPosition;
        private List<SpriteSheetInstance> spriteInstances;
        private FinishBehavior[] _finishBehaviors;
        private float _totalOpacity = 1.0f;
        private DateTime _lastFrameUpdate = Game1.PlayingGameTime;
        private int _fps = 15;
        private DateTime _lastFrameReset = Game1.PlayingGameTime;
        private int _instanceResetsPerSecond = 10;
        private int SelectedInstance = 0;
        public bool Animate = false;

        public enum FinishBehavior
        {
            Loop,
            Stop,
            GoToDefault,
            TurnInvisible,
            WaitOnFirstFrame,
            Nothing
        }

        private float? _scale;
        private (int Width, int Height)? _drawSize;

        public float BrightInstanceCount => _totalOpacity * InstanceCount;
        private bool UseRectangle => _drawSize != null;
        private (int Width, int Height) DrawSize => ((int, int))_drawSize;
        public int InstanceCount => spriteInstances.Count();
        public SpriteSheetManager(
            Texture2D spritesheet,
            FinishBehavior[] finishBehaviors,
            (int X, int Y)[] DrawPositions,
            (int X, int Y) Offset,
            (int X, int Y) PixelSize,
            (int X, int Y) MaxPixel,
            (int Width, int Height)? drawSize = null,
            float? scale = null
            )
        {
            _spritesheet = spritesheet;
            _offset = Offset;
            _pixelSize = PixelSize;
            _maxCellPosition = MaxPixel;

            _drawSize = drawSize;
            _scale = scale;
            _finishBehaviors = finishBehaviors;

            spriteInstances = new List<SpriteSheetInstance>();
            for (int i = 0; i < DrawPositions.Length; i++)
            {
                spriteInstances.Add(new SpriteSheetInstance(DrawPositions[i], (0, 0)));
            }
        }
        public void SetNumberOfInstances(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("Instance count cannot be negative.");
            }
            if (count == spriteInstances.Count()) return;

            while (spriteInstances.Count() < count)
            {
                var drawPos1 = spriteInstances[^1].DrawPosition;
                var drawPos2 = spriteInstances[^2].DrawPosition;
                var newDrawPos = (drawPos1.X + (drawPos1.X - drawPos2.X), drawPos1.Y + (drawPos1.Y - drawPos2.Y));
                spriteInstances.Add(new SpriteSheetInstance(newDrawPos, (0, 0)));
            }
            while (spriteInstances.Count() > count)
            {
                spriteInstances.RemoveAt(spriteInstances.Count() - 1);
            }
        }
        public void SetTotalOpacity(float opacity)
        {
            _totalOpacity = opacity;
            for (int i = 0; i < InstanceCount; i++)
            {
                if (i < (int)BrightInstanceCount)
                {
                    spriteInstances[i].Brightness = 1f;
                }
                else if (i == (int)BrightInstanceCount)
                {
                    spriteInstances[i].Brightness = BrightInstanceCount - (int)BrightInstanceCount;
                }
                else
                {
                    spriteInstances[i].Brightness = 0f;
                }
            }
        }
        public void CheckForIncrementUpdates()
        {
            if ((Game1.PlayingGameTime - _lastFrameUpdate).TotalSeconds > 1f / _fps)
            {
                UpdateAllCellIncrement((1, 0));
                _lastFrameUpdate = Game1.PlayingGameTime;
            }
            if ((Game1.PlayingGameTime - _lastFrameReset).TotalSeconds > 1f / _instanceResetsPerSecond && BrightInstanceCount > 1 && Animate)
            {
                SetInstanceCell(SelectedInstance, (0, 0));
                SelectedInstance = (SelectedInstance + 1) % (int)BrightInstanceCount;
                _lastFrameReset = Game1.PlayingGameTime;
            }
        }
        public SpriteSheetInstance GetInstance(int index)
        {
            if (index < 0 || index >= spriteInstances.Count())
            {
                throw new IndexOutOfRangeException("SpriteSheetInstance index out of range.");
            }
            return spriteInstances[index];
        }
        public void DrawAllInstances(SpriteBatch spriteBatch)
        {
            Rectangle sourceRectangle;
            Rectangle drawRect;
            Vector2 drawPosition;

            foreach (var instance in spriteInstances)
            {
                sourceRectangle = new Rectangle(
                    _offset.X + instance.CurrentCell.X * _pixelSize.X,
                    _offset.Y + instance.CurrentCell.Y * _pixelSize.Y,
                    _pixelSize.X,
                    _pixelSize.Y);
                Color color = instance.ColorMask * instance.Brightness;
                color.A = 255;
                if (UseRectangle)
                {
                    (int Width, int Height) drawSize = DrawSize;
                    drawRect = new(instance.DrawPosition.X, instance.DrawPosition.Y, drawSize.Width, drawSize.Height);
                    spriteBatch.Draw(
                        _spritesheet,
                        drawRect,
                        sourceRectangle,
                        color,
                        0f,
                        Vector2.Zero,
                        SpriteEffects.None,
                        0f);
                }
                else
                {
                    drawPosition = new(instance.DrawPosition.X, instance.DrawPosition.Y);

                    spriteBatch.Draw(
                        _spritesheet,
                        drawPosition,
                        sourceRectangle,
                        color,
                        0f,
                        Vector2.Zero,
                        (float)_scale,
                        SpriteEffects.None,
                        0f);
                }

            }
        }
        public void DrawAllInstancesFraction(SpriteBatch spriteBatch)
        {
            Rectangle sourceRectangle;
            Rectangle drawRect;
            Vector2 drawPosition;
            (int Width, int Height) halfDrawSize = (DrawSize.Width / 2, DrawSize.Height / 2);
            (int Width, int Height) halfPixelSize = (_pixelSize.X / 2, _pixelSize.Y / 2);

            // draw only black silhouettes
            foreach (var instance in spriteInstances)
            {
                sourceRectangle = new Rectangle(
                    _offset.X + instance.CurrentCell.X * _pixelSize.X,
                    _offset.Y + instance.CurrentCell.Y * _pixelSize.Y,
                    _pixelSize.X,
                    _pixelSize.Y);
                if (UseRectangle)
                {
                    (int Width, int Height) drawSize = DrawSize;
                    drawRect = new(instance.DrawPosition.X, instance.DrawPosition.Y, drawSize.Width, drawSize.Height);
                    spriteBatch.Draw(
                        _spritesheet,
                        drawRect,
                        sourceRectangle,
                        Color.Black,
                        0f,
                        Vector2.Zero,
                        SpriteEffects.None,
                        0f);
                }
                else
                {
                    drawPosition = new(instance.DrawPosition.X, instance.DrawPosition.Y);

                    spriteBatch.Draw(
                        _spritesheet,
                        drawPosition,
                        sourceRectangle,
                        Color.Black,
                        0f,
                        Vector2.Zero,
                        (float)_scale,
                        SpriteEffects.None,
                        0f);
                }
            }

            // draw only colored silhouettes
            foreach (var instance in spriteInstances)
            {
                if (instance.Brightness == 1f)
                {
                    sourceRectangle = new Rectangle(
                    _offset.X + instance.CurrentCell.X * _pixelSize.X,
                    _offset.Y + instance.CurrentCell.Y * _pixelSize.Y,
                    _pixelSize.X,
                    _pixelSize.Y);

                    if (UseRectangle)
                    {
                        (int Width, int Height) drawSize = DrawSize;
                        drawRect = new(instance.DrawPosition.X, instance.DrawPosition.Y, drawSize.Width, drawSize.Height);
                        spriteBatch.Draw(
                            _spritesheet,
                            drawRect,
                            sourceRectangle,
                            Color.White,
                            0f,
                            Vector2.Zero,
                            SpriteEffects.None,
                            0f);
                    }
                    else
                    {
                        drawPosition = new(instance.DrawPosition.X, instance.DrawPosition.Y);

                        spriteBatch.Draw(
                            _spritesheet,
                            drawPosition,
                            sourceRectangle,
                            Color.White,
                            0f,
                            Vector2.Zero,
                            (float)_scale,
                            SpriteEffects.None,
                            0f);
                    }
                }
                else if (instance.Brightness > 0f)
                {
                    int fractional = (int)(instance.Brightness * 4);
                    for (int y = 0; y < 2; y++)
                    {
                        for (int x = 0; x < 2; x++)
                        {
                            fractional -= 1;
                            if (fractional < 0)
                                continue;

                            sourceRectangle = new Rectangle(
                                _offset.X + instance.CurrentCell.X * _pixelSize.X + halfPixelSize.Width * x,
                                _offset.Y + instance.CurrentCell.Y * _pixelSize.Y + halfPixelSize.Height * y,
                                halfPixelSize.Width,
                                halfPixelSize.Height);

                            if (UseRectangle)
                            {
                                drawRect = new(
                                    instance.DrawPosition.X + halfDrawSize.Width * x,
                                    instance.DrawPosition.Y + halfDrawSize.Height * y,
                                    halfDrawSize.Width,
                                    halfDrawSize.Height);

                                spriteBatch.Draw(
                                    _spritesheet,
                                    drawRect,
                                    sourceRectangle,
                                    Color.White,
                                    0f,
                                    Vector2.Zero,
                                    SpriteEffects.None,
                                    0f);
                            }
                        }
                    }
                }
            }
        }
        public void UpdateInstanceCell(int index, (int X, int Y) newCell)
        {
            if (index < 0 || index >= spriteInstances.Count())
            {
                throw new IndexOutOfRangeException("SpriteSheetInstance index out of range.");
            }
            if (newCell.X < 0 || newCell.X >= _maxCellPosition.X ||
                newCell.Y < 0 || newCell.Y >= _maxCellPosition.Y)
            {
                throw new ArgumentOutOfRangeException("New cell position is out of range.");
            }
            spriteInstances[index].CurrentCell = newCell;
        }
        public void UpdateAllCellIncrement((int X, int Y) increment)
        {
            for (int i = 0; i < spriteInstances.Count(); i++)
            {
                UpdateInstanceCellIncrement(i, increment);
            }
        }
        public void UpdateInstanceCellIncrement(int index, (int X, int Y) increment)
        {
            if (index < 0 || index >= spriteInstances.Count())
            {
                throw new IndexOutOfRangeException("SpriteSheetInstance index out of range.");
            }
            ref (int X, int Y) currentCell = ref spriteInstances[index].CurrentCell;
            if (_finishBehaviors[currentCell.Y] == FinishBehavior.Nothing) return;

            currentCell.X = currentCell.X + increment.X;
            currentCell.Y = currentCell.Y + increment.Y;

            if (currentCell.X >= _maxCellPosition.X && currentCell.Y < _finishBehaviors.Length)
            {
                switch (_finishBehaviors[currentCell.Y])
                {
                    case FinishBehavior.Loop:
                        currentCell.X = 0;
                        break;
                    case FinishBehavior.Stop:
                        currentCell.X = _maxCellPosition.X - 1;
                        break;
                    case FinishBehavior.GoToDefault:
                        currentCell.X = 0;
                        currentCell.Y = 0;
                        break;
                    case FinishBehavior.TurnInvisible:
                        spriteInstances[index].IsVisible = false;
                        break;
                    case FinishBehavior.WaitOnFirstFrame:
                        currentCell.X = 0;
                        break;
                }
            }
            if (_finishBehaviors[currentCell.Y] == FinishBehavior.WaitOnFirstFrame && currentCell.X == 0)
            {
                currentCell.X = currentCell.X - 1;
            }
        }
        public void SetInstanceVisibility(int index, bool isVisible)
        {
            if (index < 0 || index >= spriteInstances.Count())
            {
                throw new IndexOutOfRangeException("SpriteSheetInstance index out of range.");
            }
            spriteInstances[index].IsVisible = isVisible;
        }
        public void SetInstanceCell(int index, (int X, int Y) cell)
        {
            if (index < 0 || index >= spriteInstances.Count())
            {
                throw new IndexOutOfRangeException("SpriteSheetInstance index out of range.");
            }
            spriteInstances[index].CurrentCell = cell;
        }
    }
}

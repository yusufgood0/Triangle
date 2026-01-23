using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SlimeGame.Drawing.Models;
using SlimeGame.Drawing.TextureDrawers;
using SlimeGame.GameAsset.Projectiles;
using SlimeGame.GameAsset.StatusEffects;
using SlimeGame.GameAssets;
using SlimeGame.GameAssets.Enemies;
using SlimeGame.Generation;
using SlimeGame.Input;

namespace SlimeGame.GameAsset
{
    public enum Element
    {
        Earth = 1000,
        //Water = 100,
        Fire = 10,
        Air = 1,
    }
    internal class SpellBook
    {
        Element?[] _elements = new Element?[_maxElements];
        DateTime _lastCastTime = DateTime.MinValue;
        const int _maxElements = 3;
        static Random rnd;
        const float cooldown = 0f;
        static List<Spell> _spellList = new();
        static readonly Dictionary<int, Spell> Spells = new Dictionary<int, Spell>();
        public static Dictionary<int, Spell> SpellDictionary => Spells;
        public static List<Spell> SpellList => _spellList;
        float TimeSinceLastCast => (float)(Game1.PlayingGameTime - _lastCastTime).TotalSeconds;
        bool OnCooldown => TimeSinceLastCast < cooldown;
        public Element[] Elements
        {
            get
            {
                List<Element> elems = new List<Element>();
                for (int i = 0; i < _maxElements; i++)
                {
                    if (_elements[i] != null)
                    {
                        elems.Add((Element)_elements[i]);
                    }
                }
                return elems.ToArray();
            }
        }
        public SpellBook(Random rnd, ContentManager Content)
        {
            if (Spells.Count == 0)
            {
                IEnumerable<Type> spellTypes =
                    Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .Where(t =>
                                t.IsClass &&
                                !t.IsAbstract &&
                                typeof(Spell).IsAssignableFrom(t));

                List<Spell> allSpells = spellTypes
                    .Select(t => (Spell)Activator.CreateInstance(t))
                    .ToList();

                foreach (Spell spell in allSpells)
                {
                    Spells.Add(spell.SpellIdentity, spell);
                }
                foreach (Spell spell in allSpells)
                {
                    SpellList.Add(spell);
                }
            }


            SpellBook.rnd = rnd;
            Spell.LoadAllSpells(Content);
        }
        public static int MaxElements => _maxElements;
        public int ElementsCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _maxElements; i++)
                {
                    if (_elements[i] != null)
                    {
                        count++;
                    }
                }
                return count;
            }
        }
        public Color[] ElementColors
        {
            get
            {
                Color[] colors = new Color[_maxElements];
                for (int i = 0; i < _maxElements; i++)
                {
                    colors[i] = _elements[i] == null ? Color.Black : GetElementColor((Element)_elements[i]);
                }
                return colors;
            }
        }
        static public Color GetElementColor(Element element)
        {
            return element switch
            {
                Element.Earth => new Color(139, 69, 19),
                //Element.Water => new Color(0, 191, 255),
                Element.Fire => new Color(255, 69, 0),
                //Element.Air => new Color(211, 211, 211),
                Element.Air => new Color(255, 255, 150),
                _ => Color.White,
            };
        }
        public void AddElement(Element elementToAdd)
        {
            if (OnCooldown)
            {
                return;
            }
            for (int i = 0; i < _maxElements; i++)
            {
                if (_elements[i] == null)
                {
                    _elements[i] = elementToAdd;
                    if (i < _maxElements - 1)
                    {
                        return;
                    }
                }
            }

        }
        public void TryCast(AssetManager objectManager)
        {
            if (ElementsCount != 3)
            {
                return;
            }

            CastSpell(objectManager);
            ClearElements();
        }
        public void ClearElements()
        {
            for (int j = 0; j < _elements.Length; j++)
            {
                _elements[j] = null;
            }
        }
        private void CastSpell(AssetManager objectManager)
        {
            int spellIdentity = ConvertSpellIndex(
                (Element)_elements[0],
                (Element)_elements[1],
                (Element)_elements[2]
                );
            if (Spells.ContainsKey(spellIdentity))
            {
                _lastCastTime = Game1.PlayingGameTime;
                StatusEffect cooldownEffect = new StatusEffects.SpellCooldown(Spells[spellIdentity]);

                if (!StatusEffect.ContainSpell(Spells[spellIdentity]))
                {
                    //cooldownEffect.AddEffect(); // Enable to get spell specific cooldowns
                    if (Spells[spellIdentity].TryCastSpell(objectManager))
                    {
                        objectManager.Player.Recoil(Spells[spellIdentity].Recoil);
                    }
                    else
                    {
                        ErrorMessage.New("Cast Failed!");
                    }
                }

            }
        }
        public static int ConvertSpellIndex((Element a, Element b, Element c) i) => ConvertSpellIndex(i.a, i.b, i.c);
        public static int ConvertSpellIndex(Element a, Element b, Element c)
        {
            return (int)a + (int)b + (int)c;
        }
        public abstract class Spell
        {
            public int SpellIdentity => ConvertSpellIndex(Elements);
            public abstract (Element a, Element b, Element c) Elements { get; }
            public abstract float CoolDown { get; }
            public abstract float Recoil { get; }
            public abstract string Description { get; }
            public virtual (string Name, float Volume)[] SoundEffectsLoads => null;

            protected static SoundEffectInstance defaultSpellCastSoundEffect;
            private SoundEffectInstance[] SoundEffects;
            protected SoundEffectInstance RandomSoundEffect => SoundEffects != null && SoundEffects.Length > 0 ? SoundEffects[rnd.Next(0, SoundEffects.Length)] : null;
            public static void LoadAllSpells(ContentManager Content)
            {
                foreach (var spell in SpellBook.Spells)
                {
                    spell.Value.LoadSpell(Content);
                }

                SoundEffect soundEffect = Content.Load<SoundEffect>("castSpellMagic");
                defaultSpellCastSoundEffect = soundEffect.CreateInstance();
                defaultSpellCastSoundEffect.Volume = .01f;
            }
            public void LoadSpell(ContentManager Content)
            {
                Active = false;

                if (SoundEffectsLoads == null) return;
                SoundEffects = new SoundEffectInstance[SoundEffectsLoads.Length];
                for (int i = 0; i < SoundEffectsLoads.Length; i++)
                {
                    SoundEffect soundEffect = Content.Load<SoundEffect>(SoundEffectsLoads[i].Name);
                    SoundEffects[i] = soundEffect.CreateInstance();
                    SoundEffects[i].Volume = SoundEffectsLoads[i].Volume;
                }
            }

            public bool Active;
            public bool TryCastSpell(AssetManager objectManager)
            {
                if (Active)
                {
                    CastSpell(objectManager);
                    SoundEffectInstance soundEffectInstance = RandomSoundEffect;
                    defaultSpellCastSoundEffect.Stop();
                    objectManager.PlaySound(Spell.defaultSpellCastSoundEffect);
                    if (soundEffectInstance != null)
                    {
                        objectManager.PlaySound(soundEffectInstance, objectManager.Player.Position);
                    }
                    return true;
                }
                return false;
            }
            public abstract void CastSpell(AssetManager objectManager);
        }
        public abstract class DurationSpell : Spell, StatusEffect
        {
            DateTime StatusEffect.ExpireTime { get => _expireTime; }
            public DateTime _expireTime;
            public abstract float Duration { get; }
            public override void CastSpell(AssetManager objectManager)
            {
                // debug.writeline($"Casting {this.ToString()}");
                StatusEffect.AddEffect(this);
                _expireTime = Game1.PlayingGameTime.AddSeconds(Duration);
            }
            public abstract bool Update(AssetManager objectManager);
            bool StatusEffect.EffectEnd(AssetManager objectManager) => this.EffectEnd(objectManager);
            public virtual bool EffectEnd(AssetManager objectManager) => false;
            public void CancelEffect(AssetManager objectManager)
            {
                this.CancelEffect();
            }
            private void CancelEffect() => ((StatusEffect)this).CancelEffect();
        }
        // FFF
        internal class FireBall : Spell
        {
            public override (Element a, Element b, Element c) Elements => (Element.Fire, Element.Fire, Element.Fire);
            public override float CoolDown => 2f;
            public override float Recoil => 3f;
            public override string Description => "Hurl a condensed sphere of flame straight ahead, detonating on impact with searing force. A simple but reliable spell, Fireball rewards precision and timing over brute chaos. 'The first spell most learn, and the last many underestimate.'";

            public override void CastSpell(AssetManager objectManager)
            {
                Player player = objectManager.Player;
                // debug.writeline($"Casting {this.ToString()}");
                objectManager.Projectiles.Add(
                new FireBallProjectile(player.EyePos, player.dirVector, 20, 1)
                );
            }
        }
        // FFA
        internal class Fire_Burst : DurationSpell
        {
            public override (Element a, Element b, Element c) Elements => (Element.Fire, Element.Fire, Element.Air);
            public override float CoolDown => 4f;
            public override float Recoil => 2f;
            public override float Duration => 2f;
            public override string Description => "Release a storm of living flame that seeks out the nearest foe and refuses to let go. Once bound, the fire bends mid-flight, orbiting and converging until its target is reduced to ash, or the spell ends. 'Run if you must-the fire already knows where you are.'";
            private Enemy Enemy = null;
            public override void CastSpell(AssetManager objectManager)
            {
                Player player = objectManager.Player;
                Enemy target;
                float LowestDistance = float.MaxValue;

                for (int i = 0; i < 10; i++)
                {
                    objectManager.Add(new FireBurstProjectile(player.EyePos, player.dirVector, 100));
                }

                if (objectManager.Enemies.Count == 0) return;

                target = objectManager.Enemies[0];
                float greatestDot = default;
                foreach (Enemy enemy in objectManager.Enemies)
                {
                    Vector3 targetPos = enemy.Position;
                    //Vector3 target = enemy.Position;
                    Vector3 dir = Vector3.Normalize(targetPos - objectManager.Player.Position);
                    float dot = Vector3.Dot(dir, objectManager.Player.dirVector);
                    dot = MathF.Max(dot, greatestDot);

                    if (greatestDot == default)
                    {
                        greatestDot = dot;
                        target = enemy;
                    }
                }

                Enemy = target;
                base.CastSpell(objectManager);
            }
            public override bool Update(AssetManager objectManager)
            {
                List<Enemy> enemies = objectManager.Enemies;
                List<Projectile> projectiles = objectManager.Projectiles;
                if (Enemy.Health <= 0) CancelEffect(objectManager);

                foreach (Projectile flameBurstProjectile in projectiles)
                {
                    if (!(flameBurstProjectile is FireBurstProjectile)) continue;
                    Projectile projectile = flameBurstProjectile;
                    float length = projectile.Velocity.Length();
                    projectile.Velocity = Vector3.Lerp(projectile.Velocity, Enemy.Position - projectile.Position, 0.001f);
                    projectile.Velocity = (projectile.Velocity / length) * Math.Clamp(length, 30, 100);
                }
                return false;
            }
            public override bool EffectEnd(AssetManager objectManager)
            {
                for (int i = 0; i < objectManager.Projectiles.Count; i++)
                {
                    if (objectManager.Projectiles[i] is FireBurstProjectile)
                    {
                        objectManager.Projectiles.RemoveAt(i);
                        i--;
                    }
                }
                return false;
            }
        }
        // FAA
        internal class Flame_Thrower : DurationSpell
        {
            public override (Element a, Element b, Element c) Elements => (Element.Fire, Element.Air, Element.Air);
            public override float CoolDown => 2f;
            public override float Recoil => 0f;
            public override float Duration => 0.75f;
            public override string Description => "Unleash a continuous stream of volatile fire, flooding the space ahead with burning embers. The recoil pushes you backward as the flames roar outward, turning close quarters into an inferno. 'Fire does not ask permission-it only spreads.'";
            public override (string Name, float Volume)[] SoundEffectsLoads
                => new (string Name, float Volume)[]
                {
                    ("flameThrower", .01f)
                };
            public override bool Update(AssetManager objectManager)
            {
                Player player = objectManager.Player;
                objectManager.Add(new FlameThrowerProjectile(player.Position, player.dirVector, 50 * (float)rnd.NextDouble(), rnd));
                player.ChangeSpeed(-player.dirVector);
                return false;
            }
        }
        // AAA
        internal class Dash : DurationSpell
        {
            public override (Element a, Element b, Element c) Elements => (Element.Air, Element.Air, Element.Air);
            public override float CoolDown => 0.5f;
            public override float Recoil => 1f;
            public override float Duration => 1f;
            public override string Description => "Explode forward in a burst of compressed air, instantly repositioning yourself. Perfect for evasion or aggression, Dash turns momentum into survival. 'The wind carries those who hesitate the least.'";
            const int DASH_SPEED = 50;
            Vector3 _dashDir;
            bool _hit = false;
            public override void CastSpell(AssetManager objectManager)
            {
                // debug.writeline($"Casting {this.ToString()}");
                _dashDir = objectManager.Player.dirVector;
                base.CastSpell(objectManager);
            }
            public override bool Update(AssetManager objectManager)
            {
                _dashDir = Vector3.Lerp(_dashDir, objectManager.Player.dirVector, 0.1f);
                _dashDir.Normalize();
                objectManager.Player.SetSpeed(_dashDir * DASH_SPEED);

                BoundingBox hitbox = new BoundingBox(
                    objectManager.Player.Position - new Vector3(500),
                    objectManager.Player.Position + new Vector3(500)
                    );
                foreach (var enemy in objectManager.Enemies)
                {
                    BoundingBox hurtbox = enemy.Hitbox;
                    BoundingBox modelBox = enemy.models[0].BoundingBox;
                    float dot = Vector3.Dot((modelBox.Max + modelBox.Min) / 2 - objectManager.Player.Position, _dashDir);
                    if (hurtbox.Intersects(hitbox) && dot > 0.8f)
                    {
                        Vector3 normal = Vector3.Normalize(DASH_SPEED * objectManager.Player.Position - enemy.Position);
                        objectManager.Player.SetSpeed(normal * 20);
                        _hit = true;

                        enemy.EnemyIsHit(Math.Clamp((Duration - ((StatusEffect)this).TimeUntilExpire) * 120f, 25, 100), objectManager);
                        enemy.Knockback(_dashDir, 40);
                        CancelEffect(objectManager);

                        break;
                    }
                }

                Particle particle = new Particle(objectManager.Player.Position - objectManager.Player.dirVector * 100, SpellBook.GetElementColor(Element.Air), objectManager.Player.Speed, rnd);
                objectManager.Add(particle);
                return false;
            }
            public override bool EffectEnd(AssetManager objectManager)
            {
                if (!_hit) objectManager.Player.SetSpeed(objectManager.Player.Speed * 0.3f);
                objectManager.Player.Stun(0.5f);

                for (int x = 0; x < 20; x++)
                {
                    objectManager.Add(new Particle(
                        objectManager.Player.Position,
                        SpellBook.GetElementColor(Element.Air),
                        new(rnd.Next(-50, 50), -rnd.Next(50), rnd.Next(-50, 50)),
                        rnd
                    ));
                }

                return false;
            }
        }
        // EAA
        internal class Rock_UpperCut : DurationSpell
        {
            public override (Element a, Element b, Element c) Elements => (Element.Air, Element.Air, Element.Earth);
            public override float CoolDown => 0.5f;
            public override float Recoil => 1.5f;
            public override float Duration => 0.5f;
            public override string Description => "Launch yourself upward with crushing force, dragging enemies into your rising assault. As you soar the sky, the air blasts a thunderous gust-leaving foes stunned and broken. 'Strike the sky-then burst a gust of fiery'";
            public override void CastSpell(AssetManager objectManager)
            {
                foreach (var i in StatusEffect._activeEffects)
                {
                    if (i is Rock_Smash)
                    {
                        StatusEffect.CancelEffect(i);
                        break;
                    }
                }
                Player player = objectManager.Player;
                if (player.IsOnGround(objectManager.ChunkManager))
                {
                    player.ChangeSpeed(-player.Speed + Vector3.Down * 100f);
                }
                else
                {
                    player.ChangeSpeed(-player.Speed + Vector3.Down * 30f);
                }
                base.CastSpell(objectManager);
            }
            public override bool Update(AssetManager objectManager)
            {
                Player player = objectManager.Player;
                List<Enemy> enemies = objectManager.Enemies;
                Vector3 vector3 = player.Position + player.dirVector * 400;
                BoundingBox hitBox = new BoundingBox(
                    vector3 - new Vector3(400),
                    vector3 + new Vector3(400)
                    );
                player.ChangeSpeed(Vector3.Down * 3f);
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i].Hitbox.Intersects(hitBox))
                    {
                        Vector3 randomVector = new Vector3(rnd.Next(-20, 20), rnd.Next(-20, 20), rnd.Next(-20, 20));
                        objectManager.Add(new Particle(enemies[i].Position, enemies[i].models[0].Color, player.Speed + randomVector, rnd));
                        if (enemies[i].EnemyIsHit(Vector3.Zero, 0.75f, objectManager))
                        {
                            i--;
                            continue;
                        }
                        float distance = 200 + enemies[i].Height;
                        StatusEffect.AddEffect(new EnemyBind(enemies[i], 0.05f, Vector3.Forward * distance, BindType.DirectionToPlayer));
                    }
                }
                objectManager.Add(new Particle(player.Position, GetElementColor(Element.Earth), Vector3.Zero, rnd));
                return false;
            }
            public override bool EffectEnd(AssetManager objectManager)
            {
                Player player = objectManager.Player;
                List<Enemy> enemies = objectManager.Enemies;

                player.ChangeSpeed(-player.Speed);
                player.Stun(0.5f);

                Vector3 vector3 = player.Position + player.dirVector * 400;
                BoundingBox hitBox = new BoundingBox(
                    vector3 - new Vector3(300),
                    vector3 + new Vector3(300)
                    );
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i].Hitbox.Intersects(hitBox))
                    {
                        enemies[i].Knockback(Vector3.Down, 10);

                        if (enemies[i].EnemyIsHit(45, objectManager))
                        {
                            i--;
                            continue;
                        }
                    }
                }
                for (int x = 0; x < 20; x++)
                {
                    objectManager.Add(new Particle(
                        player.Position,
                        SpellBook.GetElementColor(Element.Earth),
                        new(rnd.Next(-50, 50), -rnd.Next(50), rnd.Next(-50, 50)),
                        rnd
                    ));
                }
                return false;
            }
        }
        // EEA
        internal class Rock_Smash : DurationSpell
        {
            public override (Element a, Element b, Element c) Elements => (Element.Air, Element.Earth, Element.Earth);
            public override float CoolDown => 1.5f;
            public override float Recoil => 1.5f;
            public override float Duration => 100f;
            public override string Description => "Charge forward encased in stone and momentum, pulling enemies helplessly into your path. The harder you move, the harder the earth strikes when you collide with the ground. 'The mountain does not dodge. It advances.'";
            private Vector3 Foreward { get => _foreward; set => _foreward = value != Vector3.Zero ? Vector3.Normalize(value) : _foreward; }
            private Vector3 _foreward = Vector3.Forward;
            private Vector3 greatestSpeed = Vector3.Zero;

            public override void CastSpell(AssetManager objectManager)
            {
                foreach (var i in StatusEffect._activeEffects)
                {
                    if (i is Rock_UpperCut)
                    {
                        StatusEffect.CancelEffect(i);
                        break;
                    }
                }
                // debug.writeline($"Casting {this.ToString()}");
                Vector3 foreward = objectManager.Player.Speed;
                foreward.Y = 0;
                Foreward = foreward;
                base.CastSpell(objectManager);
            }
            public override bool Update(AssetManager objectManager)
            {
                Player player = objectManager.Player;
                List<Enemy> enemies = objectManager.Enemies;
                Vector3 hitBoxCenter = player.Position;
                BoundingBox hitBox = new BoundingBox(
                    hitBoxCenter - new Vector3(600),
                    hitBoxCenter + new Vector3(600)
                    );
                player.ChangeSpeed(Vector3.Up * 3 + Foreward);

                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i].Hitbox.Intersects(hitBox))
                    {
                        float distance = 200 + enemies[i].Height;
                        StatusEffect.AddEffect(new EnemyBind(enemies[i], 0.1f, Vector3.Forward * distance, BindType.DirectionToPlayer));
                    }
                }

                if (player.Speed.Length() > greatestSpeed.Length())
                {
                    greatestSpeed = player.Speed;
                }

                if (player.IsOnGround(objectManager.ChunkManager, 10))
                {
                    float Speed = greatestSpeed.Length();
                    Vector3 newSpeed = -player.dirVector * Speed;
                    newSpeed.Y = -greatestSpeed.Y * 0.25f;
                    player.ChangeSpeed(-player.Speed + newSpeed);
                    CancelEffect(objectManager);
                    return true;
                }
                return false;
            }
            public override bool EffectEnd(AssetManager objectManager)
            {
                Player player = objectManager.Player;
                List<Enemy> enemies = objectManager.Enemies;
                Vector3 hitBoxCenter = player.Position;
                BoundingBox hitBox = new BoundingBox(
                            hitBoxCenter - new Vector3(1000),
                            hitBoxCenter + new Vector3(1000)
                            );

                // Apply damage to enemies inside
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i].Hitbox.Intersects(hitBox))
                    {
                        if (enemies[i].EnemyIsHit(player.Speed.Length() / 2.5f, objectManager))
                        {
                            i--;
                        }
                    }
                }

                // Recoil off ground
                Vector3 newSpeed = player.Speed;
                newSpeed.Y *= 0.6f;
                newSpeed.Y *= player.Speed.Y > 0 ? -1 : 1;
                player.ChangeSpeed(-player.Speed + newSpeed);

                // Particles for flare
                for (int x = 0; x < 20; x++)
                {
                    objectManager.Add(new Particle(
                        player.Position,
                        SpellBook.GetElementColor(Element.Earth),
                        new(rnd.Next(-50, 50), -rnd.Next(50), rnd.Next(-50, 50)),
                        rnd
                    ));
                }
                return false;
            }
        }
        // EEE
        internal class Rock_Domain : DurationSpell
        {
            public override (Element a, Element b, Element c) Elements => (Element.Earth, Element.Earth, Element.Earth);
            public override float CoolDown => 0f;
            public override float Recoil => 0f;
            public override float Duration => Length * 2;
            public override string Description => "Manifest a growing sphere of living earth that bends movement and crushes all within its reach. Enemies are dragged and battered as your domain expands, be warned, even you cannot escape its grasp. 'Within the earth's embrace, all stand equal, slow, heavy, and doomed.'";
            public override (string Name, float Volume)[] SoundEffectsLoads
                => new (string Name, float Volume)[]
                {
                    ("Domain", .1f)
                };
            private const float GrowthRate = MaxSize / Length;
            private const float Length = 5f;
            private const float MinSize = 10f;
            private const float MaxSize = 7500;
            private const float StunDelay = 3;
            private const float StunDuration = 1;
            private DateTime LastUpdate;
            private DateTime StunTimer;
            private float Size;
            private Vector3 center;
            private Vector3 speed;
            private float opacity;
            public override void CastSpell(AssetManager objectManager)
            {
                Size = MinSize;
                LastUpdate = Game1.PlayingGameTime;
                StunTimer = Game1.PlayingGameTime.AddSeconds(StunDelay);
                center = objectManager.Player.Position;
                opacity = 255;
                speed = Vector3.Zero;
                if (StatusEffect._activeEffects.Contains(this)) this.CancelEffect(objectManager);
                base.CastSpell(objectManager);
            }
            public override bool Update(AssetManager objectManager)
            {
                TimeSpan timeDifferenceSinceLastUpdate = Game1.PlayingGameTime - LastUpdate;
                LastUpdate = Game1.PlayingGameTime;
                center += speed * (float)timeDifferenceSinceLastUpdate.TotalSeconds;
                float GrowSize = (float)timeDifferenceSinceLastUpdate.TotalSeconds * GrowthRate;
                float SizeSquared = Size * Size;

                objectManager.Player.SpeedMultiplier *= 1.2F;

                bool stun = false;
                if (StunTimer < Game1.PlayingGameTime)
                {
                    StunTimer = StunTimer.AddSeconds(StunDelay);
                    stun = true;
                }

                for (int i = 0; i < objectManager.Enemies.Count; i++)
                {
                    Enemy enemy = objectManager.Enemies[i];

                    float enemyDistanceSquared = Vector3.DistanceSquared(enemy.Position, center);
                    if (enemyDistanceSquared < SizeSquared)
                    {
                        Vector3 randomVector;
                        enemy.Knockback(Vector3.Up, 1f);
                        //enemy.Knockback(center - enemy.Position, 0.1f);

                        if (stun)
                        {
                            enemy.StunnedFor(1f);
                            enemy.Knockback(Vector3.Up, 100f);

                            for (int j = 0; j < 20; j++)
                            {
                                int x = 15;
                                randomVector = new Vector3(rnd.Next(-x, x), rnd.Next(-x, x), rnd.Next(-x, x));
                                objectManager.Add(new Particle(enemy.Position, Color.BlueViolet, randomVector, rnd));
                            }

                            if (enemy.EnemyIsHit(Math.Max(30, 0), objectManager))
                            {
                                i--;
                                continue;
                            }
                            objectManager.ChunkManager.CreateCrater(center, (int)(Size / objectManager.ChunkManager.TileSize), 300, SpellBook.GetElementColor(Element.Earth));
                        }

                        randomVector = new Vector3(rnd.Next(-5, 5), rnd.Next(-5, 5), rnd.Next(-5, 5));
                        objectManager.Add(new Particle(enemy.Position, SpellBook.GetElementColor(Element.Earth), randomVector, rnd));
                    }

                }
                Player player = objectManager.Player;
                float playerDistanceSquared = Vector3.DistanceSquared(player.Position, center);
                if (playerDistanceSquared < SizeSquared && SizeSquared > MinSize * MinSize)
                {
                    player.SpeedMultiplier *= 1 - playerDistanceSquared / SizeSquared;
                }
                else
                {
                    Vector3 distance = center - player.Position;
                    if (distance != Vector3.Zero)
                    {
                        distance.Normalize();
                        player.Position = player.Position + distance * Size;
                    }
                }

                speed = Vector3.Lerp(speed, player.Position - center, 0.001f);

                Size += GrowSize;
                if (Size >= MaxSize)
                {
                    Size = MaxSize;
                    opacity *= 0.995f;
                    if (opacity < 25)
                    {
                        CancelEffect(objectManager);
                    }
                }

                Color color = SpellBook.GetElementColor(Element.Earth);
                color.A = (byte)opacity;
                Sphere sphere = new Sphere(center, Size, 20, color);
                objectManager.Add(sphere);

                return false;
            }
        }
        // EFF
        internal class Meteor : DurationSpell
        {
            public override (Element a, Element b, Element c) Elements => (Element.Earth, Element.Earth, Element.Fire);
            public override float CoolDown => 0f;
            public override float Recoil => 0f;
            public override float Duration => 1f;
            public override string Description => "Mark a distant point and call down flaming destruction from the heavens. " + "After a brief silence, the sky breaks open and meteors fall without mercy." + "'When the earth looks to the sky, something always answers.'";
            const int Delay = 50;
            int CurrentPositionInCycle = 0;
            Vector3 target;

            public override void CastSpell(AssetManager objectManager)
            {
                Vector3 ray = objectManager.Player.Position;
                Vector3 rayMovement = objectManager.Player.dirVector * 50;
                for (int i = 0; i < 100; i++)
                {
                    ray += rayMovement;
                    if (objectManager.ChunkManager.IsPointAboveTerrain(ray))
                    {
                        while (objectManager.ChunkManager.IsPointAboveTerrain(ray))
                        {
                            ray -= objectManager.Player.dirVector;
                        }
                        break;
                    }
                    //foreach (var enemy in objectManager.Enemies)
                    //{
                    //    if (enemy.BoundingBox.Contains(ray) == ContainmentType.Contains)
                    //    {
                    //        ray = enemy.Position;
                    //        i = 100;
                    //        break;
                    //    }
                    //}

                }
                target = ray;
                //for (int i = 0; i < 100; i++)
                //{
                //    Vector2 angle;
                //    angle.X = (float)rnd.NextDouble() * MathHelper.TwoPi;
                //    angle.Y = -(float)rnd.NextDouble() * MathHelper.Pi * 0.2f;
                //    Vector3 direction = General.angleToVector3(angle);
                //    Particle meteor = new Particle(
                //    target + direction * 200,
                //    Color.Red,
                //    -direction,
                //    rnd
                //    );
                //}
                if (StatusEffect._activeEffects.Contains(this)) this.CancelEffect(objectManager);
                base.CastSpell(objectManager);
            }
            public override bool Update(AssetManager objectManager)
            {
                //Color color = Color.Purple;
                //color.A = 255;
                //Sphere sphere = new Sphere(target, 1000, 20, color);
                //objectManager.Add(sphere);

                float width = 500;
                float height = 100;
                Vector3 pos = target;
                //pos.Y = Math.Min(pos.Y, objectManager.ChunkManager.HeightAtPosition(target));
                Cube Cube = new Cube(pos - new Vector3(width / 2, height, width / 2), width, height, width, Color.Purple, new((Game1.PlayingGameTime.Second * 0.01f) % MathF.Tau, 0));
                Sphere Sphere = new Sphere(target, 1000, 20, Color.Red);
                objectManager.Add(Cube);


                CurrentPositionInCycle++;
                if (CurrentPositionInCycle >= Delay)
                {
                    CurrentPositionInCycle = 0;
                }
                else
                {
                    return false;
                }
                Vector2 angle;
                angle.X = (float)rnd.NextDouble() * MathHelper.TwoPi;
                angle.Y = (float)rnd.NextDouble() - MathHelper.Pi / 2;
                Vector3 position = target + General.angleToVector3(angle) * (20000 * (float)rnd.NextDouble() + 20000f);

                angle.Y += (float)rnd.NextDouble() * 0.1f;
                Vector3 direction = -General.angleToVector3(angle);
                {
                    MeteorProjectile meteor = new MeteorProjectile(
                        position,
                        direction,
                        300 * (float)rnd.NextDouble() + 600,
                        1f + (float)rnd.NextDouble() * 2f
                        );

                    objectManager.Add((Projectile)meteor);
                }



                return false;
            }
        }
        // EEF
        internal class MoshPit : DurationSpell
        {
            public override (Element a, Element b, Element c) Elements => (Element.Earth, Element.Fire, Element.Fire);
            public override float CoolDown => 0f;
            public override float Recoil => 0f;
            public override float Duration => 5;
            public override string Description => "Manifest a growing sphere of living earth that bends movement and crushes all within its reach. Enemies are dragged and battered as your domain expands, be warned, even you cannot escape its grasp. 'Within the earth's embrace, all stand equal. Slow, heavy, and doomed.'";
            public override (string Name, float Volume)[] SoundEffectsLoads
                => new (string Name, float Volume)[]
                {
                    ("Explosion", .03f),
                    ("Explosion (1)", .03f),
                    ("Explosion (2)", .03f),
                    ("Explosion (3)", .03f),
                    ("Explosion (4)", .03f),
                    ("Explosion (5)", .03f),
                };

            private const int Size = 1500;
            private Vector3 center;
            private List<(Vector3 Pos, float GrowRate, float Size, byte R, byte G)> bubbles = new();
            public override void CastSpell(AssetManager objectManager)
            {
                center = objectManager.Player.Position;
                if (StatusEffect._activeEffects.Contains(this)) this.CancelEffect(objectManager);
                base.CastSpell(objectManager);
            }
            public override bool Update(AssetManager objectManager)
            {
                float SizeSquared = Size * Size;

                //objectManager.Player.SpeedMultiplier *= 0.5F;

                for (int i = 0; i < objectManager.Enemies.Count; i++)
                {
                    Enemy enemy = objectManager.Enemies[i];
                    Vector3 enemypos = enemy.Position;
                    enemypos.Y = center.Y;
                    float enemyDistanceSquared = Vector3.DistanceSquared(enemy.Position, center);
                    if (enemyDistanceSquared < SizeSquared * 1.5f)
                    {
                        enemy.Knockback(Vector3.Up, 0.35f);
                        enemy.Knockback(center - enemypos, 0.65f);

                        if (enemy.EnemyIsHit(3, objectManager))
                        {
                            i--;
                            continue;
                        }
                        Vector3 randomVector = new Vector3(rnd.Next(-5, 5), rnd.Next(-5, 5), rnd.Next(-5, 5));
                        objectManager.Add(new Particle(enemy.Position, SpellBook.GetElementColor(Element.Earth), randomVector, rnd));
                    }
                }
                Player player = objectManager.Player;
                float playerDistanceSquared = Vector3.DistanceSquared(player.Position, center);


                var chunkManager = objectManager.ChunkManager;
                float angle = MathF.Tau * (float)rnd.NextDouble();
                Vector3 offset = General.angleToVector3(new Vector2(angle, 0));
                Color color = Color.Black;
                color.R = (byte)rnd.Next(255);
                color.G = (byte)rnd.Next((int)(color.R * 0.7f));

                if (rnd.NextDouble() < 0.05f)
                {
                    (Vector3 Pos, float GrowRate, int Size, byte R, byte G) bubble;
                    bubble.Pos = center + offset * (float)rnd.NextDouble() * Size;
                    bubble.GrowRate = ((float)rnd.NextDouble() - 0.5f) * 10;
                    bubble.Size = 0;
                    bubble.R = color.R;
                    bubble.G = color.G;
                    bubbles.Add(bubble);
                    objectManager.PlaySound(RandomSoundEffect, bubble.Pos);
                    for (int i = 0; i < 40; i++)
                    {
                        Vector3 particlePos = bubble.Pos + new Vector3(rnd.Next(-50, 50), rnd.Next(-50, 50), rnd.Next(-50, 50));
                        Particle particle = new Particle(particlePos, color, (particlePos - bubble.Pos) / 10 + Vector3.Down * 20f, rnd);
                        particle.Float(200, rnd);
                        objectManager.Add(particle);
                    }

                }
                for (int i = 0; i < bubbles.Count; i++)
                {
                    var bubble = bubbles[i];
                    Color bubbleColor = new Color(bubble.R, bubble.G, 0);
                    chunkManager.CreateCrater(bubble.Pos, 5, bubble.GrowRate, bubbleColor);

                    Vector3 movementVector = General.angleToVector3(new((float)(Math.Tau * rnd.NextDouble()), (float)(Math.PI * rnd.NextDouble())));
                    bubble.Pos.Y = objectManager.ChunkManager.HeightAtPosition(bubble.Pos);
                    Particle particle = new Particle(bubble.Pos, bubbleColor, movementVector * (float)rnd.NextDouble() * 50, rnd);
                    objectManager.Add(particle);
                    if (rnd.NextDouble() < 0.005f)
                    {
                        bubbles.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
                return false;
            }
        }

    }
}

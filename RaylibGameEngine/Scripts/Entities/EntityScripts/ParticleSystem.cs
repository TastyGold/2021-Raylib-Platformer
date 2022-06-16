using System.Numerics;
using System;
using System.Collections.Generic;
using Engine;
using Raylib_cs;
using MathExtras;

namespace Levels
{
    public partial class EntityManagement
    {
        public class ParticleSystem : Entity
        {
            //EntityInfo
            public override byte GetEntityID() => 3;
            public override Vector2 GetPositionOffset() => new Vector2(0);

            //Data
            public List<Particles.Particle> particles = new List<Particles.Particle>();

            //Emission
            public bool active = true;
            public Clock.Timestamp lastEmitTime = Clock.Now;

            //Methods
            public void EmitParticle(Particles.Particle newParticle, Vector2 position)
            {
                newParticle.position = position;
                particles.Add(newParticle);
            }
            public override void RunBehaviour()
            {
                particles.ForEach(p => { if (p.active) p.Update(Clock.DeltaTime); });
            }
            public override void Draw()
            {
                particles.ForEach(p => p.Draw());
            }

            //Constructor
            public ParticleSystem()
            {
                particles = new List<Particles.Particle>();
                OrderInLayer = 150;
            }
        }

        public static class Particles
        {
            public static Random rand = new Random();

            //Abstract particle class
            public abstract class Particle
            {
                //Data
                public Vector2 position;
                public int age;
                public bool active = true;

                //Methods
                public abstract void Update(float deltaTime);
                public abstract void Draw();

                //Constructors
                public Particle()
                {

                }
                public Particle(Vector2 startPosition, byte startNumber = 0)
                {
                    position = startPosition;
                    age = startNumber;
                }
            }

            public class Moth : Particle
            {
                //Texture data
                public static SubSpriteSheet spriteSheet = new SubSpriteSheet(ResourceTextures.particleSheet, 8, 8, 4, 1, 0, 8);

                //Data
                public Vector2 velocity = Vector2.Zero;
                public Vector2 acceleration = Vector2.Zero;
                public Vector2 targetPosition;
                public Vectex wanderArea;
                public Player.PlayerCharacter playerRef;

                //Methods
                public override void Update(float deltaTime)
                {
                    //Animation
                    age += 1;
                    if (age >= 8) age = 0;

                    //Wander
                    if (rand.Next(0, 150) == 0) targetPosition = wanderArea.RandomPositionInside(rand);

                    acceleration = targetPosition - position;
                    if (Math.Abs(playerRef.Position.X - position.X) < 2f)
                    {
                        if (Math.Abs(playerRef.Position.Y - position.Y) < 2.5f)
                        {
                            Vector2 dist = (position - playerRef.Position);
                            dist.Y = Math.Abs(dist.Y);
                            float sqrMagnitude = dist.LengthSquared();
                            acceleration += dist / Math.Max(sqrMagnitude, 0.35f);
                        }
                    }

                    //Update position
                    velocity += acceleration * deltaTime;
                    velocity *= 1 - deltaTime;
                    position += velocity * deltaTime;
                }

                public override void Draw()
                {
                    int spriteNumber = age > 3 ? 0 : 1;
                    if (velocity.X < 0) spriteNumber += 2;

                    Rectangle destRec = Rendering.GetScreenRect(position - new Vector2(0.25f), Vector2.One * 0.5f);
                    Raylib.DrawTexturePro(spriteSheet.texture, spriteSheet.GetSourceRec(spriteNumber), destRec, Vector2.Zero, 0, Color.WHITE);
                    Rendering.CountDrawCall(spriteSheet.texture.id);

                    if (Gameplay.drawHitboxes)
                    {
                        //Debug (may cause lag)
                        Raylib.DrawLineV(position * Screen.scalar * Vect.FlipY, (position + acceleration) * Screen.scalar * Vect.FlipY, Color.DARKBLUE);
                        Raylib.DrawLineV(position * Screen.scalar * Vect.FlipY, (position + velocity) * Screen.scalar * Vect.FlipY, Color.RED);
                        Rendering.CountDrawCallSimple(2);
                    }
                }

                //Constructors
                public Moth(Vectex wanderArea, Player.PlayerCharacter playerRef)
                {
                    this.wanderArea = wanderArea;
                    this.position = wanderArea.RandomPositionInside(rand);
                    targetPosition = wanderArea.RandomPositionInside(rand);
                    age = rand.Next(0, 8);
                    this.playerRef = playerRef;
                }
            }
        }
    }
}
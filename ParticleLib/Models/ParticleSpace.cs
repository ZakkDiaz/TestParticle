using QuadTrees;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticleLib.Models
{
    public partial class ParticleSpace2D
    {
        private PointF from;
        private PointF to;
        private int density;

        //private object _particleLock;

        //List<Particle> particles;

        bool subspaceFilled = false;
        //ParticleSpace2D subSpaceTL;
        //ParticleSpace2D subSpaceTR;
        //ParticleSpace2D subSpaceBL;
        //ParticleSpace2D subSpaceBR;
        //ParticleSpace2D parent;
        bool lockedSpace = false;
        int depth;
        Quad spaceQuad;

        //QuadTree<Particle> spaceTree;
        QuadTreeRect<Particle> particles;

        public ParticleSpace2D(PointF _from, PointF _to, int _depth = 1, int _density = 1, Quad quad = Quad.Global)
        {
            from = _from;
            to = _to;
            density = _density;
            //particles = new List<Particle>();
            depth = _depth;
            spaceQuad = quad;
            //spaceTree = new QuadTree<Particle>(null, Quad.Global);
            ////parent = _parent;

            //spaceTree.Init((_from.X, _from.Y), (_to.X, _to.Y), _depth, new List<Particle>());

            //_particleLock = new object();

            particles = new QuadTreeRect<Particle>(from.X, from.Y, to.X - from.X, to.Y - from.Y);

        }

        public void AddParticle(Particle particle, bool bypassLock = false, bool forceLock = false)
        {
            particles.Add(particle);


            //spaceTree.Add(particle);
            //if(spaceTree.Is(particle))
            //particles.Add(particle);
            //for(var i = 0; i <= density; i++)
            //{
            //    if (particles[i] == null)
            //    {
            //        particles[i] = particle;
            //        break;
            //    }
            //}

            //var overDensity = particles.Count(p => p != null) > density;
            //if (overDensity)
            //{
            //    var particleXGroup = particles.GroupBy(g => (g.dimenisons[0].pos, g.dimenisons[1].pos));
            //    var particleStacks = particleXGroup.Count(pg => pg.Count() > 1);

            //    if (particleStacks == 0 && depth < 5 && (!(lockedSpace || bypassLock) || forceLock))
            //        FillSubSpace();
            //}

        }

        //public void UpdateParticle(Particle particle, bool hasExpired)
        //{
        //    //lock(_particleLock)
        //    //    GetAndUpdateQuadOfParticle(particle, hasExpired);
        //}



        //private void FillSubSpace(bool bypassLock = false)
        //{
        //    //subspaceFilled = true;
        //    //foreach (var particle in particles)
        //    //{
        //    //    Quad quad = GetQuadForParticle(particle);
        //    //    AddParticleToSubspace(particle, quad, bypassLock);
        //    //}
        //    //particles = new List<Particle>();
        //}

        //public void ProcessSubspace()
        //{
        //    //lock (_particleLock)
        //        FillSubSpace(true);
        //}

        //private void AddParticleToSubspace(Particle particle, Quad quad, bool bypassLock = false)
        //{
        //    //var factor = (float)Math.Pow(2, depth);
        //    //var wx = to.X / factor;
        //    //var mx = ((to.X / factor) + wx)/2;
        //    //var lx = from.X;
        //    //var rx = (to.X / factor) + wx;

        //    //var wy = to.Y / factor;
        //    //var my = ((to.Y / factor) + wy)/2;
        //    //var ty = from.Y;
        //    //var by = (to.Y / factor) + wy;

        //    //var TLF = new PointF(lx, ty);
        //    //var TLT = new PointF(mx, my);

        //    //var TRF = new PointF(mx, ty);
        //    //var TRT = new PointF(rx, my);

        //    //var BRF = TLT;
        //    //var BRT = new PointF(rx, by);

        //    //var BLF = new PointF(lx, my);
        //    //var BLT = new PointF(mx, by);

        //    //switch (quad)
        //    //{
        //    //    case Quad.TopRight:
        //    //        if (subSpaceTR == null) //upper right
        //    //            subSpaceTR = new ParticleSpace2D(TRF, TRT, depth + 1, density, quad, this);
        //    //        subSpaceTR.AddParticle(particle, bypassLock);
        //    //        break;
        //    //    case Quad.TopLeft:
        //    //        if (subSpaceTL == null) //top left
        //    //            subSpaceTL = new ParticleSpace2D(TLF, TLT, depth + 1, density, quad, this);
        //    //        subSpaceTL.AddParticle(particle, bypassLock);
        //    //        break;
        //    //    case Quad.BottomRight:
        //    //        if (subSpaceBR == null) //bottom right
        //    //            subSpaceBR = new ParticleSpace2D(BRF, BRT, depth + 1, density, quad, this);
        //    //        subSpaceBR.AddParticle(particle, bypassLock);
        //    //        break;
        //    //    case Quad.BottomLeft:
        //    //        if (subSpaceBL == null) //bottom left
        //    //            subSpaceBL = new ParticleSpace2D(BLF, BLT, depth + 1, density, quad, this);
        //    //        subSpaceBL.AddParticle(particle, bypassLock);
        //    //        break;
        //    //}
        //}

        public void ProcessTimestep(long diff, (float, float) focus, Point BOUNDS)
        {
            Parallel.ForEach(particles, (p) => { p.ProcessTimestep(diff, focus, BOUNDS);
            });

            var pList = particles.ToList();
            List<Particle> toRemove = new List<Particle>();
            List<Particle> toAdd = new List<Particle>();
            int dist = BOUNDS.X/2;
            foreach(var p in pList)
            {  
                particles.GetObjects(new RectangleF(p.pos().Item1, p.pos().Item2, dist, dist), (p2) => {
                    if(p != null && p2 != null)
                    if (p != p2)
                    {
                        var angleRads = Particle.AngleFor(p2.pos(), p.pos());
                        var dist = p.DistanceFrom(p2);
                        var mt = (p.mass + p2.mass);
                        if (p.Intersects(p2))
                        {
                            var vTot = (float)Math.Sqrt(Math.Pow(Math.Abs(p.vel().X*p.mass/ mt), 2) + Math.Pow(Math.Abs(p.vel().Y * p.mass/ mt), 2));
                            var v2Tot = (float)Math.Sqrt(Math.Pow(Math.Abs(p2.vel().X * p.mass/ mt), 2) + Math.Pow(Math.Abs(p2.vel().Y * p.mass/ mt), 2));
                            if (p.isAsorb)
                            {
                                var np = new Particle(10f, mt, (p.pos().Item1 + p2.pos().Item1)/2, (p.pos().Item2 + p2.pos().Item1) / 2, p.duration + p2.duration, 0, 0, false, p.isSeeking, 0, vTot, v2Tot);
                                toRemove.Add(p);
                                toRemove.Add(p2);
                                toAdd.Add(np);
                            }
                            else
                            {
                                //var absV = vTot + v2Tot;
                                //var vt_ratio = vTot / absV;
                                //var vt2_ratio = v2Tot / absV;

                                //var v1Ang = p.angle();
                                //var v2Ang = p2.angle();

                                //var p1Dif = v1Ang - angleRads;
                                //var p2Dif = v2Ang - angleRads;


                                //var p1Vals = ((float)Math.Cos(p1Dif) * (p2.mass), (float)Math.Sin(p1Dif) * (p2.mass));
                                //var p2Vals = ((float)Math.Sin(p2Dif) * (p.mass), (float)Math.Cos(p2Dif) * (p.mass));

                                //var vTot_r = Math.Sqrt(Math.Pow(Math.Abs(p1Vals.Item1), 2) + Math.Pow(Math.Abs(p1Vals.Item2), 2));
                                //var v2Tot_r = Math.Sqrt(Math.Pow(Math.Abs(p2Vals.Item1), 2) + Math.Pow(Math.Abs(p2Vals.Item2), 2));
                                //var absV_r = vTot_r + v2Tot_r;

                                //var p1Scaled = ((float)(p1Vals.Item1 * (vTot / absV_r) * vt_ratio), (float)(p1Vals.Item2 * (vTot / absV_r) * vt_ratio));
                                //var p2Scaled = ((float)(p2Vals.Item1 * (v2Tot_r / absV_r) * vt_ratio), (float)(p2Vals.Item2 * (v2Tot_r / absV_r) * vt_ratio));

                                //p.SetSpeed(p1Scaled);
                                //p2.SetSpeed(p2Scaled);

                                //p.ProcessTimestep(diff, focus);
                                //p2.ProcessTimestep(diff, focus);
                            }

                        } else if (p.isGravitational && p2.isGravitational)
                        {
                            var g = 1;
                            float forceMult = (float)(((p.mass * p2.mass)) / Math.Pow(dist, 1.4)) * g;
                            p.AddForce((float)angleRads, -forceMult * (p2.mass / mt) * diff * p._deltaStep);
                            p2.AddForce((float)angleRads, (forceMult * (p.mass / mt) * diff * p2._deltaStep));
                        }
                    }
                });
            }

            foreach (var p in toRemove)
                particles.Remove(p);

            foreach(var p in toAdd)
                if (p.mass > 0)
                    particles.Add(p);

            foreach (var p in particles)
            {
                if(p.mass > 0)
                particles.Move(p);
            }
        }

        private void GetAndUpdateQuadOfParticle(Particle particle, bool hasExpired)
        {
            //var expectedQuad = GetQuadForParticle(particle);
            //if (particles.Contains(particle))
            //{
            //    if (hasExpired)
            //        particles.Remove(particle);
            //    else if (IsParticleInQuad(Quad.NONE, particle))
            //    {
            //        if (parent != null)
            //        {
            //            particles.Remove(particle);
            //            if (!particles.Any())
            //                parent.RemoveSubspace(this);

            //            parent.AddParticle(particle);
            //        }
            //    }
                
            //    return;
            //}
            
            //bool found = false;
            //switch (expectedQuad)
            //{
            //    case Quad.TopLeft:
            //        if (subSpaceTL != null)
            //        {
            //            subSpaceTL.GetAndUpdateQuadOfParticle(particle, hasExpired);
            //            found = true;
            //        }
            //        break;
            //    case Quad.TopRight:
            //        if (subSpaceTR != null)
            //        {
            //            subSpaceTR.GetAndUpdateQuadOfParticle(particle, hasExpired);
            //            found = true;
            //        }
            //        break;
            //    case Quad.BottomLeft:
            //        if (subSpaceBL != null)
            //        {
            //            subSpaceBL.GetAndUpdateQuadOfParticle(particle, hasExpired);
            //            found = true;
            //        }
            //        break;
            //    case Quad.BottomRight:
            //        if (subSpaceBR != null)
            //        {
            //            subSpaceBR.GetAndUpdateQuadOfParticle(particle, hasExpired);
            //            found = true;
            //        }
            //        found = true;
            //        break;
            //}

            //if (found)
            //    return;

            //if (expectedQuad != Quad.TopLeft && subSpaceTL != null)
            //    subSpaceTL.GetAndUpdateQuadOfParticle(particle, hasExpired);
            //if (expectedQuad != Quad.TopRight && subSpaceTR != null)
            //    subSpaceTR.GetAndUpdateQuadOfParticle(particle, hasExpired);
            //if (expectedQuad != Quad.BottomLeft && subSpaceBL != null)
            //    subSpaceBL.GetAndUpdateQuadOfParticle(particle, hasExpired);
            //if (expectedQuad != Quad.BottomRight && subSpaceBR != null)
            //    subSpaceBR.GetAndUpdateQuadOfParticle(particle, hasExpired);

        }

        public List<Particle> GetParticles()
        {
            List<Particle> _particles = new List<Particle>();

            _particles.AddRange(particles.GetAllObjects());

            //if (subSpaceTR != null)
            //    _particles.AddRange(subSpaceTR.GetParticles());
            //if (subSpaceTL != null)
            //    _particles.AddRange(subSpaceTL.GetParticles());
            //if (subSpaceBR != null)
            //    _particles.AddRange(subSpaceBR.GetParticles());
            //if (subSpaceBL != null)
            //    _particles.AddRange(subSpaceBL.GetParticles());

            return _particles;
        }

        private void RemoveSubspace(ParticleSpace2D particleSpace2D)
        {
            switch(particleSpace2D.spaceQuad)
            {
                //case Quad.TopLeft:
                //    subSpaceTL = null;
                //    break;
                //case Quad.TopRight:
                //    subSpaceTR = null;
                //    break;
                //case Quad.BottomLeft:
                //    subSpaceBL = null;
                //    break;
                //case Quad.BottomRight:
                //    subSpaceBR = null;
                //    break;
            }
        }

        //public bool ContainsParticle(Particle particle)
        //{
        //    //return particles.Contains(particle);
        //    //if(subSpaceTR != null && subSpaceTR.ContainsParticle(particle))
        //}

        private Quad GetQuadForParticle(Particle particle)
        {
            if (particle == null)
                return Quad.NONE;
            var isTop = particle.dimenisons[1].pos < to.Y / 2;
            var isRight = particle.dimenisons[0].pos > to.X / 2;
            if (isTop)
            {
                if (isRight)
                    return Quad.TopRight;
                else
                    return Quad.TopLeft;
            }
            else if (isRight)
                return Quad.BottomRight;
            else
                return Quad.BottomLeft;
        }

        //private bool IsParticleInQuad(Quad quad, Particle particle)
        //{
        //    //var x = particle.dimenisons[0].pos;
        //    //var y = particle.dimenisons[1].pos;


        //    //var factor = (float)Math.Pow(2, depth);
        //    //var wx = to.X / factor;
        //    //var mx = ((to.X / factor) + wx) / 2;
        //    //var lx = from.X;
        //    //var rx = (to.X / factor) + wx;

        //    //var wy = to.Y / factor;
        //    //var my = ((to.Y / factor) + wy) / 2;
        //    //var ty = from.Y;
        //    //var by = (to.Y / factor) + wy;

        //    //switch (quad)
        //    //{
        //    //    case Quad.TopLeft:
        //    //        return x > lx && x < mx && y > by && y < my;
        //    //    case Quad.TopRight:
        //    //        return x > mx && x < rx && y > by && y < my;
        //    //    case Quad.BottomLeft:
        //    //        return x > lx && x < mx && y > my && y < by;
        //    //    case Quad.BottomRight:
        //    //        return x > mx && x < rx && y > my && y < by;
        //    //    case Quad.Global:
        //    //        return x > lx && x < rx && y > ty && y < by;
        //    //    case Quad.NONE:
        //    //        return x < lx || x > rx || y < ty || y > by;
        //    //}
        //    //return false;
        //}

        void RemoveParticle(Particle particle)
        {
            //particles.Remove(particle);
            //for (var i = 0; i < particles.Count; i++)
            //{
            //    if (particles[i] == particle)
            //        particles[i] = null;
            //}
        }

        public RectangleF[] GetRects(bool isSeed = false)
        {
            List<RectangleF> rectArs = new List<RectangleF>();
            rectArs.Add(particles.QuadRect);
            return rectArs.ToArray();
        }
    }

}

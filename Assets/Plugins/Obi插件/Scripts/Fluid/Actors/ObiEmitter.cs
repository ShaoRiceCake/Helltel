using UnityEngine;
using System.Collections.Generic;
using System;

namespace Obi
{

    [AddComponentMenu("Physics/Obi/Obi Emitter", 850)]
    [ExecuteInEditMode]
    public class ObiEmitter : ObiActor
    {

        public delegate void EmitterParticleCallback(ObiEmitter emitter, int particleIndex);

        public event EmitterParticleCallback OnEmitParticle;
        public event EmitterParticleCallback OnKillParticle;

        public enum EmissionMethod
        {
            /// <summary>  
            /// Continously emits particles until there are no particles left to emit.
            /// </summary>
            STREAM,

            /// <summary>  
            /// Emits a single burst of particles from the emitter, and does not emit any more until
            /// all alive particles have died.
            /// </summary>
            BURST,

            /// <summary>  
            /// Will not automatically emit particles. The user needs to call EmitParticle() manually.
            /// </summary>
            MANUAL
        }

        public ObiEmitterBlueprintBase emitterBlueprint;

        /// <summary>  
        /// The base actor blueprint used by this actor.
        /// </summary>
        /// This is the same as <see cref="emitterBlueprint"/>.
        public override ObiActorBlueprint sourceBlueprint
        {
            get { return emitterBlueprint; }
        }

        [Tooltip("Filter used for collision detection.")]
        [SerializeField] private int filter = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 1);

        /// <summary>  
        /// Emission method used by this emitter.
        /// </summary>
        /// Can be either STREAM or BURST. 
        [Tooltip("Changes how the emitter behaves. Available modes are Stream and Burst.")]
        public EmissionMethod emissionMethod = EmissionMethod.STREAM;

        /// <summary>  
        /// Minimum amount of inactive particles available before the emitter is allowed to resume emission.
        /// </summary>
        [Range(0, 1)]
        public float minPoolSize = 0.5f;

        /// <summary>  
        /// Speed (in meters/second) at which fluid is emitter.
        /// </summary>
        /// Note this affects both the speed and the amount of particles emitted per second, to ensure flow is as smooth as possible.
        /// Set it to zero to deactivate emission.
        [Tooltip("Speed (in meters/second) of emitted particles. Setting it to zero will stop emission. Large values will cause more particles to be emitted.")]
        public float speed = 0.25f;

        /// <summary>  
        /// Particle lifespan in seconds.
        /// </summary>
        /// Particles older than this value will become inactive and go back to the solver's emission pool, making them available for reuse.
        [Tooltip("Lifespan of each particle.")]
        public float lifespan = 4;

        /// <summary>  
        /// Amount of randomness added to particle direction when emitted.
        /// </summary>
        [Range(0, 1)]
        [Tooltip("Amount of randomization applied to particle emit direction.")]
        public float randomDirection = 0;

        /// <summary>  
        /// Amount of emitter velocity inherited by the fluid.
        /// </summary>
        [Range(0, 1)]
        [Tooltip("Amount of emitter velocity inherited by the fluid.")]
        public float inheritVelocity = 0;

        /// <summary>  
        /// Use the emitter shape color to tint particles upon emission.
        /// </summary>
        [Tooltip("Spawned particles are tinted by the corresponding emitter shape's color.")]
        public bool useShapeColor = true;

        [HideInInspector] [SerializeField] private List<ObiEmitterShape> emitterShapes = new List<ObiEmitterShape>();
        private IEnumerator<EmitPoint> distEnumerator;

        private EmittedParticleData emissionData;

        [NonSerialized] private ObiNativeEmitPointList emitPoints;

        private float unemittedBursts = 0;
        private bool manualKill = false;

        /// <summary>  
        /// Collision filter value used by fluid particles.
        /// </summary>
        public int Filter
        {
            set
            {
                if (filter != value)
                {
                    filter = value;
                    UpdateFilter();
                }
            }
            get { return filter; }
        }


        /// <summary>  
        /// Whether the emitter is currently emitting particles.
        /// </summary>
        public bool isEmitting { get; private set; } = false;

        /// <summary>
        /// Whether to use simplices (triangles, edges) for contact generation.
        /// </summary>
        public override bool surfaceCollisions
        {
            get
            {
                return false;
            }
            set
            {
                if (m_SurfaceCollisions != value)
                    m_SurfaceCollisions = false;
            }
        }

        /// <summary>  
        /// Whether this actor makes use of particle anisotropy
        /// </summary>
        /// In case of fluid, this is true as particles adapt their shape to fit the fluid's surface.
        public override bool usesAnisotropicParticles
        {
            get { return true; }
        }


        protected override void Awake()
        {
            base.Awake();
            emitPoints = new ObiNativeEmitPointList();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            emitPoints?.Dispose();
            emitPoints = null;
        }

        internal override void LoadBlueprint()
        {
            base.LoadBlueprint();

            UpdateEmitter();

            // bring existing orientation data from GPU right away:
            ObiFluidEmitterBlueprint fluidMaterial = emitterBlueprint as ObiFluidEmitterBlueprint;

            if (fluidMaterial == null)
                solver.orientations.Readback(false);
            else
            {
                solver.angularVelocities.Readback(false);
                if (fluidMaterial.miscibility > 0)
                    solver.userData.Readback(false);
            }
            solver.life.Readback(false);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            UpdateEmitter();
        }

        public override void RequestReadback()
        {
            base.RequestReadback();

            ObiFluidEmitterBlueprint fluidMaterial = emitterBlueprint as ObiFluidEmitterBlueprint;

            if (fluidMaterial == null)
                solver.orientations.Readback();
            else
            {
                solver.angularVelocities.Readback();
                if (fluidMaterial.miscibility > 0)
                    solver.userData.Readback();
            }
            solver.life.Readback();
        }

        /// <summary>  
        /// Adds a shape trough which to emit particles. This is called automatically by <see cref="ObiEmitterShape"/>.
        /// </summary>
        public void AddShape(ObiEmitterShape shape)
        {
            if (!emitterShapes.Contains(shape))
            {
                emitterShapes.Add(shape);

                if (solver != null)
                {
                    shape.particleSize = (emitterBlueprint != null) ? emitterBlueprint.GetParticleSize(m_Solver.parameters.mode) : 0.1f;
                    shape.GenerateDistribution();
                    distEnumerator = GetDistributionEnumerator();
                }
            }
        }

        /// <summary>  
        /// Removes a shape trough which to emit particles. This is called automatically by <see cref="ObiEmitterShape"/>.
        /// </summary>
        public void RemoveShape(ObiEmitterShape shape)
        {
            emitterShapes.Remove(shape);
            if (solver != null)
            {
                distEnumerator = GetDistributionEnumerator();
            }
        }

        /// <summary>  
        /// Updates the spawn point distribution of all shapes used by this emitter.
        /// </summary>
        public void UpdateEmitter()
        {
            if (solver != null)
            {
                // Update all shape's distribution:
                for (int i = 0; i < emitterShapes.Count; ++i)
                {
                    emitterShapes[i].particleSize = (emitterBlueprint != null) ? emitterBlueprint.GetParticleSize(m_Solver.parameters.mode) : 0.1f;
                    emitterShapes[i].GenerateDistribution();
                }
                distEnumerator = GetDistributionEnumerator();

                // generate and store data to set to emitted particles:
                CacheEmissionData();
            }
        }

        private IEnumerator<EmitPoint> GetDistributionEnumerator()
        {

            // In case there are no shapes, emit using the emitter itself as a single-point shape.
            if (emitterShapes.Count == 0)
            {
                while (true)
                {
                    Matrix4x4 l2sTransform = actorLocalToSolverMatrix;
                    yield return new EmitPoint(l2sTransform.GetColumn(3), l2sTransform.GetColumn(2), Color.white);
                }
            }

            // Emit distributing emission among all shapes:
            while (true)
            {
                for (int j = 0; j < emitterShapes.Count; ++j)
                {
                    ObiEmitterShape shape = emitterShapes[j];

                    if (shape.distribution.Count == 0)
                        yield return new EmitPoint(shape.ShapeLocalToSolverMatrix.GetColumn(3), shape.ShapeLocalToSolverMatrix.GetColumn(2), Color.white);

                    for (int i = 0; i < shape.distribution.Count; ++i)
                        yield return shape.distribution[i].GetTransformed(shape.ShapeLocalToSolverMatrix, shape.PreviousShapeLocalToSolverMatrix, shape.color, shape.deltaTime);

                }
            }

        }

        public override void SetSelfCollisions(bool selfCollisions)
        {
            if (solver != null && isLoaded)
            {
                ObiUtils.ParticleFlags particleFlags = ObiUtils.ParticleFlags.Fluid;
                if (emitterBlueprint != null && !(emitterBlueprint is ObiFluidEmitterBlueprint))
                    particleFlags = 0;

                for (int i = 0; i < solverIndices.count; i++)
                {
                    int group = ObiUtils.GetGroupFromPhase(m_Solver.phases[solverIndices[i]]);
                    m_Solver.phases[solverIndices[i]] = ObiUtils.MakePhase(group, (selfCollisions ? ObiUtils.ParticleFlags.SelfCollide : 0) | particleFlags);
                }
            }
        }

        private void UpdateFilter()
        {
            if (solver != null && isLoaded)
            {
                for (int i = 0; i < solverIndices.count; i++)
                    m_Solver.filters[solverIndices[i]] = filter;
            }
        }

        private void CacheEmissionData()
        {
            ObiFluidEmitterBlueprint fluidMaterial = emitterBlueprint as ObiFluidEmitterBlueprint;

            // calculate rest distance and mass:
            float restDistance = (emitterBlueprint != null) ? emitterBlueprint.GetParticleSize(m_Solver.parameters.mode) : 0.1f;
            float pmass = (emitterBlueprint != null) ? emitterBlueprint.GetParticleMass(m_Solver.parameters.mode) : 0.1f;
            pmass *= m_MassScale;

            // calculate particle physical radius and volume:
            emissionData.radius = restDistance * 0.5f;
            emissionData.volume = 1 / Mathf.Pow(Mathf.Abs(emissionData.radius * 2), 3 - (int)m_Solver.parameters.mode);

            // calculate fluid smoothing radius:
            float smoothingRadius;
            if (emitterBlueprint != null)
                smoothingRadius = fluidMaterial != null ? fluidMaterial.GetSmoothingRadius(m_Solver.parameters.mode) : 0;
            else
                smoothingRadius = 1f / (10 * Mathf.Pow(1, 1 / (m_Solver.parameters.mode == Oni.SolverParameters.Mode.Mode3D ? 3.0f : 2.0f)));

            emissionData.fluidMaterial = fluidMaterial != null ? new Vector4(smoothingRadius, fluidMaterial.polarity, fluidMaterial.viscosity, fluidMaterial.vorticity) : Vector4.zero;
            emissionData.fluidInterface = fluidMaterial != null ? new Vector4(fluidMaterial.atmosphericDrag, fluidMaterial.atmosphericPressure, fluidMaterial.buoyancy, fluidMaterial.miscibility) : new Vector4(0, 0, -1, 0);
            emissionData.userData = fluidMaterial != null ? fluidMaterial.userData : Vector4.zero;
            emissionData.invMass = 1 / pmass;

            ObiUtils.ParticleFlags particleFlags = ObiUtils.ParticleFlags.Fluid;
            if (emitterBlueprint != null && fluidMaterial == null)
                particleFlags = 0;

            emissionData.phase = ObiUtils.MakePhase(groupID, ObiUtils.ParticleFlags.SelfCollide | particleFlags);
        }

       /* protected override void SwapWithFirstInactiveParticle(int actorIndex)
        {
            base.SwapWithFirstInactiveParticle(actorIndex);
            life.Swap(actorIndex, activeParticleCount);
        }*/

        /// <summary>
        /// Activates one particle. Specialized implementation, optimized to activate large amounts of particles per step. Does not
        /// flag active particles and simplices as dirty (which requires the solver to rebuild them), instead it appends to their end.
        /// </summary>
        /// <returns>
        /// True if a particle could be activated. False if there are no particles to activate.
        /// </returns> 
        /// This operation preserves the relative order of all particles.
        public override bool ActivateParticle()
        {
            if (activeParticleCount >= particleCount)
                return false;

            int index = solverIndices[m_ActiveParticleCount];

            m_Solver.points.Add(index);
            m_Solver.simplices.Add(index);
            m_Solver.activeParticles.Add(index);

            m_Solver.m_SimplexCounts.pointCount++;
            m_ActiveParticleCount++;

            m_Solver.cellCoords.Add(default);

            return true;
        }

        /// <summary>  
        /// Asks the emitter to emit a new particle. Returns whether the emission was succesful.
        /// </summary>
        /// <param name="offset"> Distance from the emitter surface at which the particle should be emitted.</param>
        /// <returns>
        /// If at least one particle was in the emission pool and it could be emitted, will return true. False otherwise.
        /// </returns> 
        public bool EmitParticle(float offset)
        {
            if (emitPoints.count >= particleCount - activeParticleCount)
                return false;

            // move on to next emission point:
            distEnumerator.MoveNext();
            EmitPoint distributionPoint = distEnumerator.Current;

            // randomize spawn direction:
            distributionPoint.direction = Vector3.Lerp(distributionPoint.direction, UnityEngine.Random.onUnitSphere, randomDirection);

            // offset spawn position, then scale direction by speed to get velocity:
            distributionPoint.position += distributionPoint.direction * offset;
            distributionPoint.direction *= speed;

            emitPoints.Add(distributionPoint);

            return true;
        }

        /// <summary>  
        /// Asks the emiter to kill a particle. Returns whether it was succesful.
        /// </summary>
        /// <returns>
        /// True if the particle could be killed. False if it was already inactive.
        /// </returns> 
        public bool KillParticle(int index)
        {
            if (index >= 0 && index < solverIndices.count && solver.life[solverIndices[index]] > 0)
            {
                solver.life[solverIndices[index]] = 0;
                manualKill = true;
                return true;
            }

            return false;
        }

        /// <summary>  
        /// Kills all particles in the emitter, and returns them to the emission pool.
        /// </summary>
        public void KillAll()
        {
            for (int i = 0; i < activeParticleCount; ++i)
                solver.life[solverIndices[i]] = 0;
            manualKill = true;
        }

        private int GetDistributionPointsCount()
        {
            int size = 0;
            for (int i = 0; i < emitterShapes.Count; ++i)
                size += emitterShapes[i].distribution.Count;
            return Mathf.Max(1, size);
        }

        private void EmitParticles()
        {
            // clamp amount of emitted particles so we don't emit more particles than available in the indices array.
            int emitCount = Mathf.Min(Mathf.Max(0, particleCount - activeParticleCount), emitPoints.count);

            for (int i = 0; i < emitCount; ++i)
            {
                int index = activeParticleCount;
                if (!ActivateParticle())
                    continue;

                int solverIndex = solverIndices[index];

                solver.life[solverIndex] = lifespan;

                m_Solver.startPositions[solverIndex] = m_Solver.endPositions[solverIndex] = m_Solver.positions[solverIndex] = emitPoints[i].position;
                m_Solver.velocities[solverIndex] = emitPoints[i].direction + emitPoints[i].velocity * inheritVelocity;
                m_Solver.angularVelocities[solverIndex] = Vector4.zero;
                m_Solver.orientations[solverIndex] = Quaternion.LookRotation(emitPoints[i].direction);

                float radius = emissionData.radius;
                if (emitterBlueprint is ObiGranularEmitterBlueprint)
                {
                    float randomRadius = UnityEngine.Random.Range(0, radius * 2 / 100.0f * (emitterBlueprint as ObiGranularEmitterBlueprint).randomness);
                    radius = Mathf.Max(0.001f, radius - randomRadius);
                }

                m_Solver.principalRadii[solverIndex] = new Vector4(radius, radius, radius, 1); // set active particle radius W to 1.
                m_Solver.invMasses[solverIndex] = m_Solver.invRotationalMasses[solverIndex] = emissionData.invMass;

                m_Solver.fluidMaterials[solverIndex] = emissionData.fluidMaterial;
                m_Solver.fluidInterface[solverIndex] = emissionData.fluidInterface;
                m_Solver.userData[solverIndex] = emissionData.userData;
                m_Solver.phases[solverIndex] = emissionData.phase;
                m_Solver.filters[solverIndex] = filter;
                m_Solver.fluidData[solverIndex] = new Vector4(emissionData.volume, 0, 0, 0);

                // inject invmass in 4th component of position:
                var pos = m_Solver.positions[solverIndex];
                pos.w = m_Solver.invMasses[solverIndex];
                m_Solver.positions[solverIndex] = pos;

                if (useShapeColor)
                    m_Solver.colors[solverIndex] = (Vector4)emitPoints[i].color;

                OnEmitParticle?.Invoke(this, activeParticleCount - 1);

                isEmitting = true;
            }

            // In case any particles have been emitted, notify the solver implementation
            // that the amount of active particles and simplices has changed:
            // TODO: do this in solver, as other emitters might flag as dirty after this one, and it might
            // not be necessary to do this.
            if (!m_Solver.dirtyActiveParticles && emitCount > 0 && isEmitting)
            {
                m_Solver.implementation.SetActiveParticles(m_Solver.activeParticles);
                m_Solver.implementation.SetSimplices(m_Solver.simplices, m_Solver.simplexCounts);
            }
        }

        private void UpdateLifetime(float deltaTime)
        {
            for (int i = activeParticleCount - 1; i >= 0; --i)
            {
                solver.life[solverIndices[i]] -= deltaTime;

                if (solver.life[solverIndices[i]] <= 0)
                {
                    DeactivateParticle(i);
                    OnKillParticle?.Invoke(this, activeParticleCount);
                }
            }
        }

        public override void SimulationStart(float timeToSimulate, float substepTime)
        {
            base.SimulationStart(timeToSimulate, substepTime);

            UnityEngine.Profiling.Profiler.BeginSample("Emitter lifecycle");

            // wait for data to arrive from the GPU.
            ObiFluidEmitterBlueprint fluidMaterial = emitterBlueprint as ObiFluidEmitterBlueprint;
            if (fluidMaterial == null)
                solver.orientations.WaitForReadback();
            else
            {
                solver.angularVelocities.WaitForReadback();
                if (fluidMaterial.miscibility > 0)
                    solver.userData.WaitForReadback();
            }
            solver.life.WaitForReadback();

            // cache a per-shape matrix that transforms from shape local space to solver space.
            for (int j = 0; j < emitterShapes.Count; ++j)
                emitterShapes[j].UpdateLocalToSolverMatrix(timeToSimulate);

            // Update lifetime and kill dead particles:
            if (!float.IsInfinity(lifespan) || manualKill)
            {
                UpdateLifetime(timeToSimulate);
                manualKill = false;
            }

            int emissionPoints = GetDistributionPointsCount();
            int pooledParticles = particleCount - activeParticleCount;

            if (pooledParticles == 0)
            {
                if (isEmitting)
                    distEnumerator = GetDistributionEnumerator();
                isEmitting = false;
            }

            if (isEmitting || pooledParticles > Mathf.FloorToInt(minPoolSize * particleCount))
            {
                // SimulationStart might be called before our awake, so we need to lazy initialize emitPoints:
                if (emitPoints == null)                    emitPoints = new ObiNativeEmitPointList();

                // stream emission:
                switch (emissionMethod)
                {
                    case EmissionMethod.STREAM:

                        // number of bursts per simulation step:
                        float particleSize = ((emitterBlueprint != null) ? emitterBlueprint.GetParticleSize(m_Solver.parameters.mode) : 0.1f);
                        float burstCount = speed * timeToSimulate / particleSize;

                        // Emit new particle burst:
                        unemittedBursts += burstCount;

                        emitPoints.EnsureCapacity((int)unemittedBursts * emissionPoints);

                        while (unemittedBursts >= 1)
                        {
                            unemittedBursts -= 1;

                            for (int i = 0; i < emissionPoints; ++i)
                                EmitParticle(unemittedBursts * particleSize);
                        }

                        EmitParticles();
                        emitPoints.Clear();

                        break;

                    case EmissionMethod.BURST:

                        // single burst when there's no active particles:
                        if (activeParticleCount == 0)
                        {
                            emitPoints.EnsureCapacity(emissionPoints);

                            for (int i = 0; i < emissionPoints; ++i)
                                EmitParticle(0);

                            EmitParticles();
                            emitPoints.Clear();
                        }

                        break;

                    default:
                        EmitParticles();
                        emitPoints.Clear();
                        break;
                }
            }

            UnityEngine.Profiling.Profiler.EndSample();
        }
    }
}

using UnityEngine;

namespace Obi
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ObiCollider))]
    public class ObiVoidZone : ObiForceZone
    {
        public override void UpdateIfNeeded()
        {
            var fc = ObiColliderWorld.GetInstance().forceZones[handle.index];
            fc.type = ForceZone.ZoneType.Void;
            fc.mode = mode;
            fc.intensity = intensity + intensityVariation;
            fc.minDistance = minDistance;
            fc.maxDistance = maxDistance;
            fc.falloffPower = falloffPower;
            fc.damping = damping;
            fc.dampingDir = dampingDir;
            ObiColliderWorld.GetInstance().forceZones[handle.index] = fc;
        }
    }
}


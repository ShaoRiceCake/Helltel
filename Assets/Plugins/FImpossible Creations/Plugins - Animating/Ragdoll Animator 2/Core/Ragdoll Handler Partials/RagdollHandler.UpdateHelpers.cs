namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
        public void PreCalibrate()
        {
            //foreach (var extraBone in skeletonFillExtraBonesList) extraBone.Calibrate();

            if( ApplyPositions )
            {
                foreach( var chain in chains ) chain.Calibrate();
            }
            else
            {
                _playmodeAnchorBone.BoneProcessor.Calibrate();
                foreach( var chain in chains ) chain.CalibrateJustRotation();
            }
        }

        /// <summary>
        /// (Runtime) Updating joint dynamic parameters like connected mass scale etc.
        /// </summary>
        public void RefreshAllChainsDynamicParameters()
        {
            bool fall = IsFallingOrSleep;

            foreach( var chain in chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    bone.RefreshDynamicPhysicalParameters( chain, fall );
                    bone.RefreshJointLimitSwitch( chain );
                }
            }
        }

        /// <summary>
        /// (Runtime) Updating joint dynamic parameters like connected mass scale etc.
        /// </summary>
        public void RefreshAllChainsRigidbodyOptimizationParameters()
        {
            foreach( var chain in chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    bone.RefreshRigidbodyOptimizationParameters( this );
                }
            }
        }
    }
}
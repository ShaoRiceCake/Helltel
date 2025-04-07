using UnityEngine;

namespace Obi
{
    public static class FluidRenderingUtils
    {
        public struct FluidRenderTargets
        {
            public int refraction;
            public int transmission;
            public int foam;
            public int surface;
        }

        public static Color thicknessBufferClear = new Color(1,1,1,0);
        public static Color transmissionBufferClear = new Color(1, 1, 1, 0);
    }

}
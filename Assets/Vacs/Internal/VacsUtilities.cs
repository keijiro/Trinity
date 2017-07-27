using UnityEngine;

namespace Vacs
{
    public static class VacsComputeExtensionMethods
    {
        public static int GetKernelThreadGroupSizeX(this ComputeShader compute, int kernel)
        {
            uint cx, cy, cz;
            compute.GetKernelThreadGroupSizes(kernel, out cx, out cy, out cz);
            return (int)cx;
        }
    }
}

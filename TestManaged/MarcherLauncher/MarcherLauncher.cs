﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarcherLauncher
{
    using ManagedCuda;
    using ManagedCuda.VectorTypes;
    class MarcherLauncher
    {
#region static
        const string moduleName = @"..\..\..\Marcher\Debug\march3.ptx";
        const string kernelName = "simpleKernel";

        static CudaContext context = new CudaContext();
        static CudaKernel kernel;

        static MarcherLauncher()
        {
            var cumodule = context.LoadModule(moduleName);
            kernel = new CudaKernel(kernelName, cumodule, context);
            kernel.SetConstantVariable("d_edgeTable", Tables.EDGE_TABLE);
            kernel.SetConstantVariable("d_triTable", Tables.TRI_TABLE);
        }
#endregion
#region config
        public dim3 dimensions { get; set; }
        public float isoValue { get; set; }
        public float3 minValue { get; set; }
        public float3 stepSize { get; set; }
#endregion
#region results
        public float3[] triangles { get; set; }
        public uint[] count { get; set; }
#endregion


        public void march()
        {
            uint N = prod(dimensions);
            CudaDeviceVariable<float3> d_result = new CudaDeviceVariable<float3>(15 * N);
            CudaDeviceVariable<uint> d_count = new CudaDeviceVariable<uint>(N);

            kernel.Run(isoValue, dimensions, minValue, stepSize, d_result.DevicePointer, d_count.DevicePointer);

            triangles = new float3[15 * N];
            count = new uint[N];

            d_result.CopyToHost(triangles);
            d_count.CopyToHost(count);
        }

        static void Main(string[] args)
        {
            MarcherLauncher launcher = new MarcherLauncher();
            launcher.dimensions = new dim3(20, 20, 20);
            launcher.isoValue = 0;
            launcher.minValue = new float3(-1.5f, -1.5f, -1.5f);
            launcher.stepSize = new float3(0.2f, 0.2f, 0.2f);



            launcher.march();            
            
            foreach (uint c in launcher.count)
            {
                if (c != 0)
                    Console.WriteLine("{0}", c);
            }
        }

        static uint prod(dim3 d)
        {
            return d.x * d.y * d.z;
        }
    }
}
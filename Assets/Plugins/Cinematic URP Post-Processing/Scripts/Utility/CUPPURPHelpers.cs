using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PRISM.Utils
{

    public class CUPPURPHelpers
    {
        public static void BlitFullscreenMesh(CommandBuffer cmd, int mainTexShaderID, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material material, int passIndex = 0)
            {
                cmd.SetGlobalTexture(mainTexShaderID, source);
                cmd.SetRenderTarget(destination, 0, CubemapFace.Unknown, -1);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material, 0, passIndex);
            }

            public static void BlitFullscreenMesh(CommandBuffer cmd, int mainTexShaderID, RenderTargetIdentifier source, RenderTargetIdentifier destination, Rect viewportRect, Material material, int passIndex = 0)
            {
                cmd.SetGlobalTexture(mainTexShaderID, source);
                cmd.SetRenderTarget(destination, 0, CubemapFace.Unknown, -1);
                cmd.SetViewport(viewportRect);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material, 0, passIndex);
            }

            public static void BlitFullscreenMesh(CommandBuffer cmd, int mainTexShaderID, RenderTargetIdentifier source, RenderTargetIdentifier[] destinations, Material material, int passIndex = 0)
            {
                cmd.SetGlobalTexture(mainTexShaderID, source);
                cmd.SetRenderTarget(destinations, destinations[0], 0, CubemapFace.Unknown, -1);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material, 0, passIndex);
            }

            public static void BlitFullscreenMesh(CommandBuffer cmd, int mainTexShaderID, RenderTargetIdentifier source, RenderTargetIdentifier[] destinations, Rect viewportRect, Material material, int passIndex = 0)
            {
                cmd.SetGlobalTexture(mainTexShaderID, source);
                cmd.SetRenderTarget(destinations, destinations[0], 0, CubemapFace.Unknown, -1);
                cmd.SetViewport(viewportRect);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material, 0, passIndex);
            }

            public static void BlitFullscreenMeshWithClear(CommandBuffer cmd, int mainTexShaderID, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material material, Color clearColor, int passIndex = 0)
            {
                cmd.SetGlobalTexture(mainTexShaderID, source);
                cmd.SetRenderTarget(destination, 0, CubemapFace.Unknown, -1);
                cmd.ClearRenderTarget(false, true, clearColor);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material, 0, passIndex);
            }
        }

}

using UnityEngine;

namespace XMLGame.Effect.PostEffects
{
    public sealed class VerticalFogComponent : XMLPostEffectsComponentRenderTexture<VerticalFogModel>
    {
        private Matrix4x4 frustumCorners = Matrix4x4.identity;
        private Camera m_Camera = null;

        public override bool active
        {
            get
            {
                return model.enabled
                       && !context.interrupted;
            }
        }

        public void Prepare(RenderTexture source, Material uberMaterial)
        {
            m_Camera = model.camera;
            if (m_Camera)
            {
                float fovWHalf = m_Camera.fieldOfView * 0.5f;

                Vector3 toRight = m_Camera.transform.right * m_Camera.nearClipPlane * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * m_Camera.aspect;
                Vector3 toTop = m_Camera.transform.up * m_Camera.nearClipPlane * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

                Vector3 topLeft = m_Camera.transform.forward * m_Camera.nearClipPlane - toRight + toTop;
                float camScale = topLeft.magnitude * m_Camera.farClipPlane / m_Camera.nearClipPlane;

                topLeft.Normalize();
                topLeft *= camScale;

                Vector3 topRight = (m_Camera.transform.forward * m_Camera.nearClipPlane + toRight + toTop);
                topRight.Normalize();
                topRight *= camScale;

                Vector3 bottomRight = (m_Camera.transform.forward * m_Camera.nearClipPlane + toRight - toTop);
                bottomRight.Normalize();
                bottomRight *= camScale;

                Vector3 bottomLeft = (m_Camera.transform.forward * m_Camera.nearClipPlane - toRight - toTop);
                bottomLeft.Normalize();
                bottomLeft *= camScale;

                frustumCorners.SetRow(0, topLeft);
                frustumCorners.SetRow(1, topRight);
                frustumCorners.SetRow(2, bottomRight);
                frustumCorners.SetRow(3, bottomLeft);
            }

            uberMaterial.SetMatrix(ShaderPropertyID._frustumCornersWS, frustumCorners);
            uberMaterial.SetColor(ShaderPropertyID._verticalFogColor, model.verticalFogColor);
            uberMaterial.SetFloat(ShaderPropertyID._verticalFogStart, model.verticalFogStart);
            uberMaterial.SetFloat(ShaderPropertyID._verticalFogDensity, model.verticalFogDensity);
            uberMaterial.EnableKeyword("VERTICALFOG");
        }
    }
}

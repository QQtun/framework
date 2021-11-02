using UnityEngine;

namespace XMLGame.Effect.PostEffects
{
    public sealed class DepthOfFieldComponent : XMLPostEffectsComponentRenderTexture<DepthOfFieldModel>
    {
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

            var material = context.materialFactory.Get("Hidden/PostEffects/DepthOfField");
            material.shaderKeywords = null;

            Mathf.Clamp(model.focalDistance, model.camera.nearClipPlane, model.camera.farClipPlane);

            RenderTexture temp1 = context.renderTextureFactory.Get(source.width >> model.downSample, source.height >> model.downSample, 0, source.format);
            RenderTexture temp2 = context.renderTextureFactory.Get(source.width >> model.downSample, source.height >> model.downSample, 0, source.format);

            Graphics.Blit(source, temp1);

            material.SetVector(ShaderPropertyID._offsets, new Vector4(0, model.samplerScale, 0, 0));
            Graphics.Blit(temp1, temp2, material, 0);
            //material.SetVector(ShaderPropertyID._offsets, new Vector4(model.samplerScale, 0, 0, 0));
            //Graphics.Blit(temp2, temp1, material, 0);

            context.renderTextureFactory.Release(temp1);

            uberMaterial.SetTexture(ShaderPropertyID._BlurTex, temp2);
            float dis = model.focalDistance;
            if (model.focalObj)
            {
                dis = Vector3.Distance(model.transform.position, model.focalObj.position);
            }
            uberMaterial.SetFloat(ShaderPropertyID._focalDistance, FocalDistance01(dis));
            uberMaterial.SetFloat(ShaderPropertyID._nearBlurScale, model.nearBlurScale);
            uberMaterial.SetFloat(ShaderPropertyID._farBlurScale, model.farBlurScale);
            uberMaterial.EnableKeyword("DEPTHOFFIELD");
            
            context.renderTextureFactory.Release(temp2);
        }

        //计算设置的焦点被转换到01空间中的距离，以便shader中通过这个01空间的焦点距离与depth比较  
        private float FocalDistance01(float distance)
        {
            return model.camera.WorldToViewportPoint((distance - model.camera.nearClipPlane) * model.camera.transform.forward + model.camera.transform.position).z / (model.camera.farClipPlane - model.camera.nearClipPlane);
        }  
    }
}

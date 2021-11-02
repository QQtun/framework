using UnityEngine;

namespace XMLGame.Effect.PostEffects
{
    public sealed class FilterColorComponent : XMLPostEffectsComponentRenderTexture<FilterColorModel>
    {
        public override bool active
        {
            get
            {
                return model.enabled
                       && !context.interrupted;
            }
        }

        public void Prepare(Material uberMaterial, Color _filterColor, Color _StandardColor, float Intersity)
        {
            switch (model.fmMode)
            {
                case XMLPostEffects.FilterMirror.Defalut:
                    {
                        //uberMaterial.SetVector(ShaderPropertyID._filterMirrorColor, new Vector4(_filterColor.r, _filterColor.g, _filterColor.b, Intersity));
                        //uberMaterial.SetVector(ShaderPropertyID._filterStandardColor, new Vector4(_StandardColor.r, _StandardColor.g, _StandardColor.b, 0));
                        //uberMaterial.EnableKeyword("FILTERCOLOR");
                    }
                    break;
                case XMLPostEffects.FilterMirror.OldPic:             // 老照片
                    {
                        uberMaterial.SetVector(ShaderPropertyID._filterMirrorColor, new Vector4(116.0f / 255.0f, 67.0f / 255.0f, 15.0f / 255.0f, 2.5f));
                        //uberMaterial.SetVector(ShaderPropertyID._filterStandardColor, new Vector4(1, 1, 1, 0));
                        uberMaterial.EnableKeyword("FILTERCOLOR");
                    }
                    break;
                case XMLPostEffects.FilterMirror.ABaoColor:             // 阿宝色
                    {
                        uberMaterial.SetVector(ShaderPropertyID._filterMirrorColor, new Vector4(152.0f / 255.0f, 199.0f / 255.0f, 208.0f / 255.0f, 1.8f));
                        //uberMaterial.SetVector(ShaderPropertyID._filterStandardColor, new Vector4(1, 1, 1, 0));
                        uberMaterial.EnableKeyword("FILTERCOLOR");
                    }
                    break;
                case XMLPostEffects.FilterMirror.Blue:                // 蓝色调
                    {
                        uberMaterial.SetVector(ShaderPropertyID._filterMirrorColor, new Vector4(90.0f / 255.0f, 110.0f / 255.0f, 240.0f / 255.0f, 3.0f));
                        //uberMaterial.SetVector(ShaderPropertyID._filterStandardColor, new Vector4(1, 1, 1, 0));
                        uberMaterial.EnableKeyword("FILTERCOLOR");
                    }
                    break;
                case XMLPostEffects.FilterMirror.Green:               // 绿色调
                    {
                        uberMaterial.SetVector(ShaderPropertyID._filterMirrorColor, new Vector4(68.0f / 255.0f, 184.0f / 255.0f, 39.0f / 255.0f, 2.0f));
                        //uberMaterial.SetVector(ShaderPropertyID._filterStandardColor, new Vector4(1, 1, 1, 0));
                        uberMaterial.EnableKeyword("FILTERCOLOR");
                    }
                    break;
                case XMLPostEffects.FilterMirror.HuangHun:             // 黄昏
                    {
                        uberMaterial.SetVector(ShaderPropertyID._filterMirrorColor, new Vector4(242.0f / 255.0f, 83.0f / 255.0f, 36.0f / 255.0f, 2.0f));
                        //uberMaterial.SetVector(ShaderPropertyID._filterStandardColor, new Vector4(1, 1, 1, 0));
                        uberMaterial.EnableKeyword("FILTERCOLOR");
                    }
                    break;
                case XMLPostEffects.FilterMirror.BangWan:               // 傍晚
                    {
                        uberMaterial.SetVector(ShaderPropertyID._filterMirrorColor, new Vector4(14.0f / 255.0f, 14.0f / 255.0f, 54.0f / 255.0f, 6.0f));
                        //uberMaterial.SetVector(ShaderPropertyID._filterStandardColor, new Vector4(1, 1, 1, 0));
                        uberMaterial.EnableKeyword("FILTERCOLOR");
                    }
                    break;
                case XMLPostEffects.FilterMirror.Night:               // 夜晚
                    {
                        uberMaterial.SetVector(ShaderPropertyID._filterMirrorColor, new Vector4(2.0f / 255.0f, 2.0f / 255.0f, 19.0f / 255.0f, 13.29f));
                        //uberMaterial.SetVector(ShaderPropertyID._filterStandardColor, new Vector4(1, 1, 1, 0));
                        uberMaterial.EnableKeyword("FILTERCOLOR");
                    }
                    break;
                case XMLPostEffects.FilterMirror.BlackWhite:         // 黑白
                    {
                        uberMaterial.SetVector(ShaderPropertyID._filterMirrorColor, new Vector4(208.0f / 255.0f, 231.0f / 255.0f, 238.0f / 255.0f, 0));
                        uberMaterial.EnableKeyword("FILTERGRAY");
                    }
                    break;
                case XMLPostEffects.FilterMirror.Snooper:               // 夜视镜
                    {
                        uberMaterial.SetVector(ShaderPropertyID._filterMirrorColor, new Vector4(_filterColor.r, _filterColor.g, _filterColor.b, Intersity));
                        uberMaterial.SetVector(ShaderPropertyID._filterStandardColor, new Vector4(_StandardColor.r, _StandardColor.g, _StandardColor.b, 0));
                        uberMaterial.EnableKeyword("FILTERSNOOPER");
                    }
                    break;
                case XMLPostEffects.FilterMirror.Carve:               // 浮雕
                    {
                        //uberMaterial.SetVector(ShaderPropertyID._filterStandardColor, new Vector4(0,0,0,4));

                        //     half3 color1 = tex2D(_MainTex, i.uvSPR+_MainTex_TexelSize.wz).rgb;
                        //     half3 newColor = abs(color1 - color + fixed3(0.5, 0.5, 0.5));
                        //     color = min(fixed3(1,1,1), newColor);
                        //     color = max(fixed3(0,0,0), color);
                        //     color = color.r * 0.3 + color.g * 0.59 + color.b * 0.11;
                    }
                    break;
                    
                default:
                    break;
            }
        }
    }
}

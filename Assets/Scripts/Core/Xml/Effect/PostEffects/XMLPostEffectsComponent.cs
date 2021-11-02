using UnityEngine;
using UnityEngine.Rendering;


namespace XMLGame.Effect.PostEffects
{
    public abstract class XMLPostEffectsComponentBase
    {
        public XMLPostEffectsContext context;
    
        public virtual DepthTextureMode GetCameraFlags()
        {
            return DepthTextureMode.None;
        }

        public abstract bool active { get; }

        public virtual void OnEnable()
        { }

        public virtual void OnDisable()
        { }

        public abstract XMLPostEffectsModel GetModel();

    }

    public abstract class XMLPostEffectsComponent<T> : XMLPostEffectsComponentBase
    where T : XMLPostEffectsModel
    {
        public T model { get; internal set; }

        public virtual void Init(XMLPostEffectsContext pcontext, T pmodel)
        {
            context = pcontext;
            model = pmodel;
        }

        public override XMLPostEffectsModel GetModel()
        {
            return model;
        }
    }


    public abstract class XMLPostEffectsComponentCommandBuffer<T> : XMLPostEffectsComponent<T>
        where T : XMLPostEffectsModel
    {
        public abstract CameraEvent GetCameraEvent();

        public abstract string GetName();

        public abstract void PopulateCommandBuffer(CommandBuffer cb);
    }

    public abstract class XMLPostEffectsComponentRenderTexture<T> : XMLPostEffectsComponent<T>
        where T : XMLPostEffectsModel
    {
        public virtual void Prepare(Material material)
        { }
    }
}

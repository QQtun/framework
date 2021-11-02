using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

namespace HighlightingSystem
{
    [DisallowMultipleComponent]
    public partial class Highlighter : MonoBehaviour
    {
        public enum Mode
        {
            None,
            Occluder,
            Highlight,
            Distortion,
            
        }

        // Constants (don't touch this!)
        #region Constants
        // Highlighting modes rendered in that order
        static private readonly Mode[] renderingOrder = new Mode[] { Mode.Occluder, Mode.Highlight, Mode.Distortion };
        #endregion

        #region Static Fields
        // List of all highlighters in scene
        static private HashSet<Highlighter> highlighters = new HashSet<Highlighter>();
        #endregion

        #region Public Fields
        [HideInInspector]
        public bool highlight;

        [HideInInspector]
        public bool distortion;

        [HideInInspector]
        public bool occluder;

        #endregion

        #region Private Fields
        // Cached transform component reference
        private Transform tr;

        // Cached Renderers
        private List<HighlighterRenderer> highlightableRenderers = new List<HighlighterRenderer>();

        // Renderers reinitialization is required flag
        private bool renderersDirty;

        public static int _renderMode = 0;

        // Static list to prevent unnecessary memory allocations when grabbing renderer components
        static private List<Component> sComponents = new List<Component>(4);

        // Highlighting mode
        private Mode mode;

        static private Shader _opaqueShader;
        static public Shader opaqueShader
        {
            get
            {
                if (_opaqueShader == null)
                {
                    _opaqueShader = Shader.Find("Hidden/Highlighted/Opaque");
                }
                return _opaqueShader;
            }
        }

        static private Shader _transparentShader;
        static public Shader transparentShader
        {
            get
            {
                if (_transparentShader == null)
                {
                    _transparentShader = Shader.Find("Hidden/Highlighted/Transparent");
                }
                return _transparentShader;
            }
        }

//         static private Shader _dissolveShader;
//         static public Shader dissolveShader
//         {
//             get
//             {
//                 if (_dissolveShader == null)
//                 {
//                     _dissolveShader = Shader.Find("Hidden/Highlighted/Dissolve");
//                 }
//                 return _dissolveShader;
//             }
//         }
// 
//         static private Shader _glowMaskShader;
//         static public Shader glowMaskShader
//         {
//             get
//             {
//                 if (_glowMaskShader == null)
//                 {
//                     _glowMaskShader = Shader.Find("Hidden/Highlighted/GlowMask");
//                 }
//                 return _glowMaskShader;
//             }
//         }
        
        static private Shader _distortionShader;
        static public Shader distortionShader
        {
            get
            {
                if (_distortionShader == null)
                {
                    _distortionShader = Shader.Find("Hidden/Highlighted/Distortion");
                }
                return _distortionShader;
            }
        }

//         static private Shader _emissionShader;
//         static public Shader emissionShader
//         {
//             get
//             {
//                 if (_emissionShader == null)
//                 {
//                     _emissionShader = Shader.Find("Hidden/Highlighted/Emission");
//                 }
//                 return _emissionShader;
//             }
//         }

//         static private Shader _glowShader;
//         static public Shader glowShader
//         {
//             get
//             {
//                 if (_glowShader == null)
//                 {
//                     _glowShader = Shader.Find("Hidden/Highlighted/Glow");
//                 }
//                 return _glowShader;
//             }
//         }        
// 
//         static private Shader _discardShader;
//         static public Shader discardShader
//         {
//             get
//             {
//                 if (_discardShader == null)
//                 {
//                     _discardShader = Shader.Find("Hidden/Highlighted/Discard");
//                 }
//                 return _discardShader;
//             }
//         }      

        private Material _opaqueMaterial;
        private Material opaqueMaterial
        {
            get
            {
                if (_opaqueMaterial == null)
                {
                    _opaqueMaterial = new Material(opaqueShader);

                    // Make sure that ShaderPropertyIDs is initialized
                    ShaderPropertyID.Initialize();

                }
                return _opaqueMaterial;
            }
        }

        #endregion

        // 
        private void Awake()
        {
            ShaderPropertyID.Initialize();
            tr = GetComponent<Transform>();
            renderersDirty = true;
        }

        // 
        private void OnEnable()
        {
            highlighters.Add(this);
            renderersDirty = true;
        }

        // 
        private void OnDisable()
        {
            highlighters.Remove(this);
            renderersDirty = true;
        }

        // 
        private void Update()
        {

        }

        // Clear cached renderers
        private void ClearRenderers()
        {
            for (int i = highlightableRenderers.Count - 1; i >= 0; i--)
            {
                HighlighterRenderer renderer = highlightableRenderers[i];
                renderer.SetState(false);
            }
            highlightableRenderers.Clear();
        }

        // This method defines the way in which renderers are initialized
        private void UpdateRenderers()
        {
            if (renderersDirty)
            {
                ClearRenderers();

                // Find all renderers which should be controlled with this Highlighter component
                List<Renderer> renderers = new List<Renderer>();
                _renderMode = 0;
                GrabRenderers(tr, renderers);

                // Cache found renderers
                for (int i = 0, imax = renderers.Count; i < imax; i++)
                {
                    GameObject rg = renderers[i].gameObject;
                    HighlighterRenderer renderer = rg.GetComponent<HighlighterRenderer>();
                    if (renderer == null) { renderer = rg.AddComponent<HighlighterRenderer>(); }
                    renderer.SetState(true);

                    renderer.Initialize(opaqueMaterial, transparentShader, distortionShader);
                    highlightableRenderers.Add(renderer);
                }

                renderersDirty = false;
            }
        }

        // Recursively follows hierarchy of objects from t, searches for Renderers and adds them to the list. 
        // Breaks if HighlighterBlocker or another Highlighter component found
        private void GrabRenderers(Transform t, List<Renderer> renderers)
        {
            GameObject g = t.gameObject;

            // Find all Renderers of all types on current GameObject g and add them to the renderers list
            for (int i = 0, imax = types.Count; i < imax; i++)
            {
                g.GetComponents(types[i], sComponents);
                for (int j = 0, jmax = sComponents.Count; j < jmax; j++)
                {
                    HighlighterBase highLightType = sComponents[j].transform.GetComponent<HighlighterBase>();
                    if (highLightType is HighlighterDistortion)
                    {
                        _renderMode = 1 << (int)Mode.Distortion;
                    }

                    renderers.Add(sComponents[j] as Renderer);
                }
            }
            sComponents.Clear();

            // Return if transform t doesn't have any children
            int childCount = t.childCount;
            if (childCount == 0) { return; }

            // Recursively cache renderers on all child GameObjects
            for (int i = 0; i < childCount; i++)
            {
                Transform childTransform = t.GetChild(i);

                // Do not cache Renderers of this childTransform in case it has it's own Highlighter component
                Highlighter h = childTransform.GetComponent<Highlighter>();
                if (h != null) { continue; }

                GrabRenderers(childTransform, renderers);
            }
        }

        // 
        private Mode FillBufferInternal(CommandBuffer buffer, Mode m)
        {
            bool isHighlight = highlight && !distortion && !occluder;
            bool isDistortion = distortion;
            bool isOccluder = occluder;

            // Update mode
            mode = Mode.None;
            if (isHighlight)
            {
                mode = Mode.Highlight;
            }
            else if (isDistortion)
            {
                mode = Mode.Distortion;
            }
            else if (isOccluder)
            {
                mode = Mode.Occluder;
            }

            if (mode == Mode.None || mode != m) { return mode; }


            // Fill CommandBuffer with this highlighter rendering commands
            for (int i = highlightableRenderers.Count - 1; i >= 0; i--)
            {
                // To avoid null-reference exceptions when cached renderer has been removed but ReinitMaterials wasn't been called
                HighlighterRenderer renderer = highlightableRenderers[i];
                if (renderer == null)
                {
                    highlightableRenderers.RemoveAt(i);
                }
                // Try to fill buffer
                else if (!renderer.FillBuffer(buffer, m == Mode.Distortion))
                {
                    highlightableRenderers.RemoveAt(i);
                    renderer.SetState(false);
                }
            }
            return mode;
        }

        // Fill CommandBuffer with highlighters rendering commands
        static public void FillBuffer(CommandBuffer buffer)
        {
            HighlightingBase._nRenderMode = 0;
            for (int i = 0; i < renderingOrder.Length; i++)
            {
                Mode mode = renderingOrder[i];

                var e = highlighters.GetEnumerator();
                while (e.MoveNext())
                {
                    Highlighter highlighter = e.Current;
                    Mode m = highlighter.FillBufferInternal(buffer, mode);
                    if (m == Mode.Distortion)
                        HighlightingBase._nRenderMode |= (1 << (int)m);
                }
            }
        }

        static public void FillRenderers(CommandBuffer buffer)
        {
            for (int i = 0; i < renderingOrder.Length; i++)
            {
                Mode mode = renderingOrder[i];

                var e = highlighters.GetEnumerator();
                while (e.MoveNext())
                {
                    Highlighter highlighter = e.Current;
                    highlighter.UpdateRenderers();
                }
            }
        }

    }
}
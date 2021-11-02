using System;
using System.Collections.Generic;
using UnityEngine;

namespace XMLGame.Effect.PostEffects
{
    public abstract class XMLPostEffectsModel
    {
        bool m_Enabled;
        public bool enabled
        {
            get { return m_Enabled; }
            set
            {
                m_Enabled = value;

                if (value)
                    OnValidate();
            }
        }

        public abstract void Reset();

        public virtual void OnValidate()
        { }
    }
}
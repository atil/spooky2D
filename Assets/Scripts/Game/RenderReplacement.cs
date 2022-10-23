using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Game
{
    public class RenderReplacement : MonoBehaviour
    {
        [SerializeField] private RenderTexture _replacement;

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            Graphics.Blit(_replacement, (RenderTexture)null);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using PRISM.Utils;

namespace PRISM.Utils
{

    public class CUPPLUTCreator : MonoBehaviour
    {
        Texture2D tex2D;
        public int dims;

        void Start()
        {
           // CreateIdentityLut();
        }

        [ContextMenu("CreateLUT")]
        void CreateIdentityLut()
        {
            int dim = dims;
            var newC = new Color[dim * dim * dim];
            var oneOverDim = 1.0f / (1.0f * dim - 1.0f);
            for (var i = 0; i < dim; i++)
            {
                for (var j = 0; j < dim; j++)
                {
                    for (var k = 0; k < dim; k++)
                    {
                        newC[i + (j * dim) + (k * dim * dim)] = new Color((i * 1.0f) * oneOverDim, Mathf.Abs(((k * 1.0f) * oneOverDim) - 1.0f), (j * 1.0f) * oneOverDim, 1.0f);
                    }
                }
            }

            tex2D = new Texture2D(dim * dim, dim, TextureFormat.RGB24, false);
            tex2D.SetPixels(newC);
            tex2D.Apply();
            var bytes = tex2D.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/../3DLUT" + (dim * dim).ToString() + "ll" + ".png", bytes);
        }

    }
}
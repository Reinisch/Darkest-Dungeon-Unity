using UnityEngine;
using System.Collections;
using System.IO;
using System.ComponentModel.Design.Serialization;

using UnityStandardAssets.ImageEffects;

public enum LutTextureOrientation { Horizontal, Vertical }

public class LutConverter : MonoBehaviour
{
    public Material material;

    public Texture2D firstGradeLookup;
    public Texture2D secondGradeLookup;
    public Texture2D defaultGradeLookup;

    public LutTextureOrientation orientation;

    Texture3D firstGrade3DLUT;
    Texture3D secondGrade3DLUT;
    Texture3D defaultGrade3DLUT;

    public void Start()
    {
        if (orientation == LutTextureOrientation.Vertical)
        {
            firstGrade3DLUT = ConvertVertical(firstGradeLookup);
            secondGrade3DLUT = ConvertVertical(secondGradeLookup);
            defaultGrade3DLUT = ConvertVertical(defaultGradeLookup);
            material.SetTexture("_FirstColourGradeLUT", firstGrade3DLUT);
            material.SetTexture("_SecondColourGradeLUT", secondGrade3DLUT);
            material.SetTexture("_DesatColourGradeLUT", defaultGrade3DLUT);
            //AssetDatabase.CreateAsset(firstGrade3DLUT, "Assets/Assets/Colours/evening.asset");
            //AssetDatabase.CreateAsset(secondGrade3DLUT, "Assets/Assets/Colours/night.asset");
            //AssetDatabase.CreateAsset(defaultGrade3DLUT, "Assets/Assets/Colours/default.asset");
        }
        else
        {
            firstGrade3DLUT = ConvertHorizontal(firstGradeLookup);
            secondGrade3DLUT = ConvertHorizontal(secondGradeLookup);
            defaultGrade3DLUT = ConvertHorizontal(defaultGradeLookup);
            material.SetTexture("_FirstColourGradeLUT", firstGrade3DLUT);
            material.SetTexture("_SecondColourGradeLUT", secondGrade3DLUT);
            material.SetTexture("_DesatColourGradeLUT", defaultGrade3DLUT);
        }
    }

    Texture3D ConvertVertical(Texture2D texture)
    {
        int dim = 16;
        var c = texture.GetPixels();
        var newC = new Color[c.Length];

        for (int z = 0; z < 16; z++)
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    newC[z * 256 + y * 16 + x] = texture.GetPixel(x, 15 - y + (15 - z) * 16);

        Texture3D colorGradeLUT = new Texture3D(dim, dim, dim, TextureFormat.RGBA32, false);
        colorGradeLUT.SetPixels(newC);
        colorGradeLUT.Apply();
        return colorGradeLUT;
    }

    Texture3D ConvertHorizontal(Texture2D texture)
    {
        int dim = 16;
        var c = texture.GetPixels();
        var newC = new Color[c.Length];

        for(int i = 0; i < dim; i++) {
                    for(int j = 0; j < dim; j++) {
                        for(int k = 0; k < dim; k++) {
                            int j_ = dim-j-1;
                            newC[i + (j*dim) + (k*dim*dim)] = c[k*dim+i+j_*dim*dim];
                        }
                    }
                }

        Texture3D colorGradeLUT = new Texture3D(dim, dim, dim, TextureFormat.RGBA32, false);
        colorGradeLUT.SetPixels(newC);
        colorGradeLUT.Apply();
        return colorGradeLUT;
    }
}

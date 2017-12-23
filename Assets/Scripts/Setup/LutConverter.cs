using UnityEngine;

public enum LutTextureOrientation { Horizontal, Vertical }

public class LutConverter : MonoBehaviour
{
    [SerializeField]
    private Material material;
    [SerializeField]
    private Texture2D firstGradeLookup;
    [SerializeField]
    private Texture2D secondGradeLookup;
    [SerializeField]
    private Texture2D defaultGradeLookup;
    [SerializeField]
    private LutTextureOrientation orientation;

    private Texture3D firstGrade3DLUT;
    private Texture3D secondGrade3DLUT;
    private Texture3D defaultGrade3DLUT;

    private void Start()
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

    private Texture3D ConvertVertical(Texture2D texture)
    {
        int dim = 16;
        var c = texture.GetPixels();
        var newC = new Color[c.Length];

        for (int z = 0; z < dim; z++)
            for (int y = 0; y < dim; y++)
                for (int x = 0; x < dim; x++)
                    newC[z * dim * dim + y * dim + x] = texture.GetPixel(x, dim - 1 - y + (dim - 1 - z) * dim);

        Texture3D colorGradeLUT = new Texture3D(dim, dim, dim, TextureFormat.RGBA32, false);
        colorGradeLUT.SetPixels(newC);
        colorGradeLUT.Apply();
        return colorGradeLUT;
    }

    private Texture3D ConvertHorizontal(Texture2D texture)
    {
        int dim = 16;
        var c = texture.GetPixels();
        var newC = new Color[c.Length];

        for(int i = 0; i < dim; i++)
            for(int j = 0; j < dim; j++)
                for(int k = 0; k < dim; k++)
                    newC[i + (j * dim) + (k * dim * dim)] = c[k * dim + i + dim - j - 1 * dim * dim];

        Texture3D colorGradeLUT = new Texture3D(dim, dim, dim, TextureFormat.RGBA32, false);
        colorGradeLUT.SetPixels(newC);
        colorGradeLUT.Apply();
        return colorGradeLUT;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TexturePacker : MonoBehaviour
{
	public bool generateLightMapUVs = true;

	public bool generateColorTextures = true;

	public Color generatedMaterialColor = Color.white;

	private Dictionary<Shader, List<Material>> shaderToMaterial = new Dictionary<Shader, List<Material>>();

	private Dictionary<Shader, Material> generatedMaterials = new Dictionary<Shader, Material>();

	private Dictionary<Material, Rect> generatedUVs = new Dictionary<Material, Rect>();

	private Dictionary<Material, Rect> generatedUV2s = new Dictionary<Material, Rect>();

	private void Start()
	{
		List<Component> list = new List<Component>();
		list.AddRange(GetComponentsInChildren(typeof(MeshFilter)));
		for (int i = 0; i < list.Count; i++)
		{
			Renderer renderer = list[i].GetComponent<Renderer>();
			Vector2[] uv = ((MeshFilter)list[i]).mesh.uv2;
			if ((uv == null || uv.Length == 0) && generateLightMapUVs)
			{
				Vector2[] uv2 = ((MeshFilter)list[i]).mesh.uv;
				uv = new Vector2[uv2.Length];
				Array.Copy(uv2, uv, uv2.Length);
				((MeshFilter)list[i]).mesh.uv2 = uv;
			}
			bool flag = true;
			if (!(renderer != null) || !renderer.enabled || !(renderer.material != null))
			{
				continue;
			}
			Material[] sharedMaterials = renderer.sharedMaterials;
			if (sharedMaterials == null)
			{
				continue;
			}
			Material[] array = sharedMaterials;
			foreach (Material material in array)
			{
				if (generateColorTextures)
				{
					if (material.mainTexture == null)
					{
						Texture2D texture2D = new Texture2D(4, 4);
						for (int k = 0; k < texture2D.height; k++)
						{
							for (int l = 0; l < texture2D.width; l++)
							{
								texture2D.SetPixel(l, k, material.color);
							}
						}
						texture2D.Apply();
						material.SetTexture("_MainTex", texture2D);
						material.color = Color.white;
					}
					else
					{
						flag = false;
					}
				}
				if (((material.HasProperty("_LightMap") && (((MeshFilter)list[i]).mesh.uv2.Length != 0 || generateLightMapUVs) && material.GetTexture("_LightMap") != null) || !material.HasProperty("_LightMap")) && material.mainTextureScale == new Vector2(1f, 1f) && material.mainTextureOffset == Vector2.zero && material.shader != null && material.mainTexture != null)
				{
					if (shaderToMaterial.ContainsKey(material.shader))
					{
						shaderToMaterial[material.shader].Add(material);
						continue;
					}
					shaderToMaterial[material.shader] = new List<Material>();
					shaderToMaterial[material.shader].Add(material);
				}
			}
			if (generateColorTextures && flag)
			{
				Vector2[] uv3 = ((MeshFilter)list[i]).mesh.uv;
				for (int m = 0; m < uv3.Length; m++)
				{
					ref Vector2 reference = ref uv3[m];
					reference = new Vector2(0.5f, 0.5f);
				}
				((MeshFilter)list[i]).mesh.uv = uv3;
			}
		}
		foreach (Shader key in shaderToMaterial.Keys)
		{
			Texture2D texture2D2 = new Texture2D(1024, 1024);
			Texture2D[] array2 = new Texture2D[shaderToMaterial[key].Count];
			generatedMaterials[key] = new Material(key);
			for (int n = 0; n < array2.Length; n++)
			{
				array2[n] = shaderToMaterial[key][n].mainTexture as Texture2D;
			}
			Rect[] array3 = texture2D2.PackTextures(array2, 0, 2048);
			texture2D2.Apply();
			generatedMaterials[key].CopyPropertiesFromMaterial(shaderToMaterial[key][0]);
			generatedMaterials[key].mainTexture = texture2D2;
			generatedMaterials[key].color = generatedMaterialColor;
			for (int num = 0; num < array2.Length; num++)
			{
				if (shaderToMaterial[key][num].HasProperty("_LightMap"))
				{
					array2[num] = shaderToMaterial[key][num].GetTexture("_LightMap") as Texture2D;
				}
			}
			texture2D2 = new Texture2D(1024, 1024);
			Rect[] array4 = texture2D2.PackTextures(array2, 0, 2048);
			texture2D2.Apply();
			if (generatedMaterials[key].HasProperty("_LightMap"))
			{
				generatedMaterials[key].SetTexture("_LightMap", texture2D2);
			}
			for (int num2 = 0; num2 < array2.Length; num2++)
			{
				generatedUVs[shaderToMaterial[key][num2]] = array3[num2];
				generatedUV2s[shaderToMaterial[key][num2]] = array4[num2];
			}
		}
		for (int num3 = 0; num3 < list.Count; num3++)
		{
			int subMeshCount = ((MeshFilter)list[num3]).mesh.subMeshCount;
			Material[] sharedMaterials2 = list[num3].gameObject.GetComponent<Renderer>().sharedMaterials;
			Vector2[] uv2 = ((MeshFilter)list[num3]).mesh.uv;
			Vector2[] uv = ((MeshFilter)list[num3]).mesh.uv2;
			for (int num4 = 0; num4 < subMeshCount; num4++)
			{
				if (!generatedUVs.ContainsKey(sharedMaterials2[num4]))
				{
					continue;
				}
				Rect rect = generatedUVs[sharedMaterials2[num4]];
				Rect rect2 = generatedUV2s[sharedMaterials2[num4]];
				int[] array5 = DeleteDuplicates(((MeshFilter)list[num3]).mesh.GetTriangles(num4)) as int[];
				sharedMaterials2[num4] = generatedMaterials[list[num3].gameObject.GetComponent<Renderer>().sharedMaterials[num4].shader];
				int[] array6 = array5;
				foreach (int num6 in array6)
				{
					ref Vector2 reference2 = ref uv2[num6];
					reference2 = new Vector2(uv2[num6].x * rect.width + rect.x, uv2[num6].y * rect.height + rect.y);
					if (uv != null && uv.Length != 0)
					{
						ref Vector2 reference3 = ref uv[num6];
						reference3 = new Vector2(uv[num6].x * rect2.width + rect2.x, uv[num6].y * rect2.height + rect2.y);
					}
				}
			}
			list[num3].gameObject.GetComponent<Renderer>().sharedMaterials = sharedMaterials2;
			((MeshFilter)list[num3]).mesh.uv = uv2;
			if (uv != null && uv.Length != 0)
			{
				((MeshFilter)list[num3]).mesh.uv2 = uv;
			}
		}
	}

	public static Array DeleteDuplicates(Array arr)
	{
		if (arr.Rank != 1)
		{
			throw new ArgumentException("Multiple-dimension arrays are not supported");
		}
		Hashtable hashtable = new Hashtable(arr.Length * 2);
		ArrayList arrayList = new ArrayList();
		foreach (object item in arr)
		{
			if (!hashtable.Contains(item))
			{
				arrayList.Add(item);
				hashtable.Add(item, null);
			}
		}
		return arrayList.ToArray(arr.GetType().GetElementType());
	}
}

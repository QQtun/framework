using System.Collections.Generic;
using UnityEngine;
using System;

namespace Core.Framework.Utility
{
    public static class MeshHelper
    {
        private static Dictionary<string, Mesh[]> sCachedSubmeshes = new Dictionary<string, Mesh[]>();
        private static Dictionary<int, int> sOld2new = new Dictionary<int, int>();
        private static Dictionary<int, int> sNew2old = new Dictionary<int, int>();

        public static Mesh CreateMesh(Mesh oldMesh, int subIndex)
        {
            int[] oldIndices = oldMesh.GetIndices(subIndex);
            sOld2new.Clear();
            sNew2old.Clear();
            int vertexIndex = 0;
            for (int i = 0; i < oldIndices.Length; ++i)
            {
                if (sOld2new.ContainsKey(oldIndices[i]))
                    continue;
                sOld2new.Add(oldIndices[i], vertexIndex);
                sNew2old.Add(vertexIndex, oldIndices[i]);
                ++vertexIndex;
            }
            int[] newIndices = new int[oldIndices.Length];
            for (int i = 0; i < newIndices.Length; ++i)
            {
                newIndices[i] = sOld2new[oldIndices[i]];
            }
            Vector3[] oldMeshVerticles = oldMesh.vertices;
            Vector3[] oldMeshNormals = oldMesh.normals;
            Vector2[] oldMeshUV = oldMesh.uv;
            BoneWeight[] oldMeshWeights = oldMesh.boneWeights;
            Vector3[] newVertices = new Vector3[vertexIndex];
            Vector3[] newNormal = new Vector3[vertexIndex];
            Vector2[] newUV = new Vector2[vertexIndex];
            BoneWeight[] newWeights = new BoneWeight[vertexIndex];
            Matrix4x4[] oldMeshBindPoses = oldMesh.bindposes;
            for (int i = 0; i < vertexIndex; ++i)
            {
                int oldIndex = sNew2old[i];
                newVertices[i] = oldMeshVerticles[oldIndex];
                newNormal[i] = oldMeshNormals[oldIndex];
                newUV[i] = oldMeshUV[oldIndex];
                newWeights[i] = oldMeshWeights[oldIndex];
                if (subIndex > 0)
                {
                    newWeights[i].boneIndex0 -= oldMeshBindPoses.Length;
                    newWeights[i].boneIndex1 -= oldMeshBindPoses.Length;
                    newWeights[i].boneIndex2 -= oldMeshBindPoses.Length;
                    newWeights[i].boneIndex3 -= oldMeshBindPoses.Length;
                }
            }
            Mesh newMesh = new Mesh();
            newMesh.vertices = newVertices;
            newMesh.uv = newUV;
            newMesh.triangles = newIndices;
            newMesh.normals = newNormal;
            newMesh.boneWeights = newWeights;
            if (subIndex == 0)
                newMesh.bindposes = oldMeshBindPoses;
            return newMesh;
        }

        private static Vector3[] sNewVerticles = new Vector3[4096];
        private static Vector3[] sNewNormals = new Vector3[4096];
        private static Vector2[] sNewUvs = new Vector2[4096];

        public static Vector3[] CopyVector3(Vector3[] newVectices, int length)
        {
            Vector3[] verticles = new Vector3[length];
            Array.Copy(newVectices, verticles, length);
            return verticles;
        }
        public static Vector2[] CopyVector2(Vector2[] newVectices, int length)
        {
            Vector2[] verticles = new Vector2[length];
            Array.Copy(newVectices, verticles, length);
            return verticles;
        }
        public static Mesh CreateMesh_(Mesh oldMesh, int subIndex)
        {
            Mesh newMesh = new Mesh();
            int[] triangles = oldMesh.GetTriangles(subIndex);
            Dictionary<int, int> oldToNewIndices = new Dictionary<int, int>(oldMesh.vertices.Length);
            int newIndex = 0;
            Vector3[] oldMeshVerticles = oldMesh.vertices;
            Vector3[] oldMeshNormals = oldMesh.normals;
            Vector2[] oldMeshUV = oldMesh.uv;
            for (int i = 0; i < triangles.Length; i++)
            {
                int index = triangles[i];
                if (oldToNewIndices.ContainsKey(index))
                {
                    continue;
                }
                sNewVerticles[newIndex] = oldMeshVerticles[index];
                sNewNormals[newIndex] = oldMeshNormals[index];
                sNewUvs[newIndex] = oldMeshUV[index];
                oldToNewIndices.Add(index, newIndex);
                ++newIndex;
            }
            int[] newTriangles = new int[triangles.Length];
            for (int i = 0; i < newTriangles.Length; i++)
            {
                newTriangles[i] = oldToNewIndices[triangles[i]];
            }
            newMesh.vertices = CopyVector3(sNewVerticles, newIndex);
            newMesh.uv = CopyVector2(sNewUvs, newIndex);
            newMesh.triangles = newTriangles;
            newMesh.normals = CopyVector3(sNewNormals, newIndex);
            return newMesh;
        }
        public static void MergeMeshes_(string skeletonName, GameObject root, List<GameObject> objParts)
        {
            List<SkinnedMeshRenderer> smrs = new List<SkinnedMeshRenderer>(20);
            for (int i = 0; i < objParts.Count; i++)
            {
                smrs.AddRange(objParts[i].GetComponentsInChildren<SkinnedMeshRenderer>());
            }
            List<CombineInstance> combineInstances1 = null;
            for (int i = 0; i < smrs.Count; i++)
            {
                SkinnedMeshRenderer smr = smrs[i];
                if (smr.sharedMesh.subMeshCount > 1)
                {
                    combineInstances1 = new List<CombineInstance>(5);
                    break;
                }
            }
            List<CombineInstance> combineInstances2 = new List<CombineInstance>(5);
            List<Material> materials = new List<Material>(20);
            List<Transform> bones = new List<Transform>(1000);
            SkinnedMeshRenderer r = root.GetComponent<SkinnedMeshRenderer>();
            if (null == r)
            {
                root.AddComponent(typeof(SkinnedMeshRenderer));
                r = root.GetComponent<SkinnedMeshRenderer>();
            }
            Transform[] transforms = root.GetComponentsInChildren<Transform>();
            Dictionary<string, int> dict = SkeletonBonesCachingDict.CalcBonesDict(skeletonName, root);
            CombineInstance ci1;
            CombineInstance ci2;
            for (int i = 0; i < smrs.Count; i++)
            {
                SkinnedMeshRenderer smr = smrs[i];
                materials.AddRange(smr.sharedMaterials);
                if (null != combineInstances1)
                {
                    if (smr.sharedMesh.subMeshCount > 1)
                    {
                        for (int j = 0; j < smr.sharedMesh.subMeshCount; j++)
                        {
                            ci1 = new CombineInstance();
                            ci1.mesh = MeshHelper.CreateMesh(smr.sharedMesh, j); ;
                            ci1.subMeshIndex = 0;
                            combineInstances1.Add(ci1);
                        }
                    }
                    else
                    {
                        ci1 = new CombineInstance();
                        ci1.mesh = smr.sharedMesh;
                        ci1.subMeshIndex = 0;
                        combineInstances1.Add(ci1);
                    }
                }
                ci2 = new CombineInstance();
                ci2.mesh = smr.sharedMesh;
                ci2.subMeshIndex = 0;
                combineInstances2.Add(ci2);
                for (int y = 0; y < smr.bones.Length; y++)
                {
                    string bone = smr.bones[y].name;
                    int boneIndex = -1;
                    if (!dict.TryGetValue(bone, out boneIndex))
                    {
                        continue;
                    }
                    Transform transform = transforms[boneIndex];
                    bones.Add(transform);
                }
            }
            if (null != combineInstances1)
            {
                r.sharedMesh = new Mesh();
                r.sharedMesh.CombineMeshes(combineInstances1.ToArray(), false, false);
            }
            Mesh mesh2 = new Mesh();
            mesh2.CombineMeshes(combineInstances2.ToArray(), false, false);
            if (null == combineInstances1)
            {
                r.sharedMesh = mesh2;
            }
            else
            {
                r.sharedMesh.bindposes = mesh2.bindposes;
                r.sharedMesh.boneWeights = mesh2.boneWeights;
                r.sharedMesh.tangents = mesh2.tangents;
                r.sharedMesh.uv2 = mesh2.uv2;
            }
            r.bones = bones.ToArray();
            r.sharedMaterials = materials.ToArray();
            ResetFashionMaterials(objParts, r);
            //XML_ABManagerTool.MergeGameObject(root, objParts);
            for (int i = 0; i < objParts.Count; i++)
            {
                GameObject obj = objParts[i];
                GameObject.Destroy(obj);
            }
        }
        public static void CheckObjPart(GameObject go)
        {
            SkinnedMeshRenderer[] skinMeshrender = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skinMeshrender.Length; i++)
            {
                if (skinMeshrender[i].sharedMaterials == null)
                {
                    Debug.LogFormat("<color=red>{0} sharedMaterials is null.</color>", go.name);
                    continue;
                }
                for (int j = 0; j < skinMeshrender[i].sharedMaterials.Length; j++)
                {
                    if (skinMeshrender[i].sharedMaterials[j] == null)
                    {
                        Debug.LogFormat("<color=red>{0} {1} sharedMaterials has null material.</color>", go.name, skinMeshrender[i].name);
                    }
                }
            }
        }
        public static void MergeMeshes(string skeletonName, GameObject root, List<GameObject> objParts)
        {
            List<SkinnedMeshRenderer> smrs = new List<SkinnedMeshRenderer>(objParts.Count + 1);
            for (int i = 0; i < objParts.Count; i++)
            {
                smrs.AddRange(objParts[i].GetComponentsInChildren<SkinnedMeshRenderer>(true));
#if UNITY_EDITOR
                CheckObjPart(objParts[i]);
#endif
            }
            SkinnedMeshRenderer r = root.GetComponent<SkinnedMeshRenderer>();
            if (!r)
                r = root.AddComponent<SkinnedMeshRenderer>();
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            Dictionary<string, int> dict = SkeletonBonesCachingDict.CalcBonesDict(skeletonName, root);
            List<CombineInstance> combineInstances = new List<CombineInstance>(objParts.Count + 1);
            List<Material> materials = new List<Material>(objParts.Count + 1);
            List<Transform> bones = new List<Transform>(transforms.Length);
            List<Mesh> submeshesList = new List<Mesh>();
            for (int i = 0; i < smrs.Count; i++)
            {
                SkinnedMeshRenderer smr = smrs[i];
                //EquipSubMesh esm = smr.gameObject.GetComponent<EquipSubMesh>();
                materials.AddRange(smr.sharedMaterials);
                if (smr.sharedMaterials == null)
                {
                    Debug.Log("smr shaderMaterials is null:" + smr.name);
                }
                foreach (var sm in smr.sharedMaterials)
                {
                    if (sm == null)
                    {
                        Debug.Log("smr shaderMaterials have null mat:" + smr.name);
                    }
                }
                //if (esm)
                //{
                //    if (smr.sharedMaterials.Length != esm.subMesh.Length)
                //    {
                //        Debug.LogError("材质球和网格数量不一致," + skeletonName + " " + smr.name);
                //    }
                //    for (int j = 0; j < esm.subMesh.Length; j++)
                //    {
                //        combineInstances.Add(new CombineInstance() { mesh = esm.subMesh[j], subMeshIndex = 0 });
                //    }
                //}
                //else
                {
                    if (smr.sharedMesh == null)
                    {
                        Debug.Log("smr sharedMesh is null" + smr.name);
                    }
                    if (smr.sharedMaterials.Length != smr.sharedMesh.subMeshCount)
                    {
                        Debug.LogError("材质球和网格数量不一致," + skeletonName + " " + smr.name);
                    }
                    if (smr.sharedMesh.subMeshCount > 1)
                    {
                        for (int j = 0; j < smr.sharedMesh.subMeshCount; j++)
                        {
                            Mesh submeshe = MeshHelper.CreateMesh(smr.sharedMesh, j);
                            submeshesList.Add(submeshe);
                            combineInstances.Add(new CombineInstance() { mesh = submeshe, subMeshIndex = 0 });
                        }
                    }
                    else
                    {
                        combineInstances.Add(new CombineInstance() { mesh = smr.sharedMesh, subMeshIndex = 0 });
                    }
                }
                for (int y = 0; y < smr.bones.Length; y++)
                {
                    string bone = smr.bones[y].name;
                    int boneIndex = -1;
                    if (!dict.TryGetValue(bone, out boneIndex))
                        continue;
                    if (transforms.Length > boneIndex)
                    {
                        Transform transform = transforms[boneIndex];
                        bones.Add(transform);
                    }
                    else
                    {
                        Debug.LogError("Bones count error!");
                    }
                }
            }
            foreach (var m in materials)
            {
                if (m == null || m.shader == null)
                    continue;
                if (m.shader.name == "Raptor/ZTestTransparentBlend"
                    || m.shader.name == "Raptor/ZTestTransparent")
                {
                    m.mainTexture.mipMapBias = -0.5f;
                }
            }
            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combineInstances.ToArray(), false, false);
            r.sharedMesh = mesh;
            r.bones = bones.ToArray();
            r.materials = materials.ToArray();
            ResetFashionMaterials(objParts, r);
            //XML_ABManagerTool.MergeGameObject(root, objParts);
            for (int i = 0; i < objParts.Count; i++)
            {
                GameObject obj = objParts[i];
                GameObject.Destroy(obj);
            }
            if (submeshesList.Count > 0)
            {
                foreach (var submeshe in submeshesList)
                {
                    GameObject.Destroy(submeshe);
                }
                submeshesList.Clear();
            }
        }
        public static void MergeMeshes(string skeletonName, GameObject root)
        {
            SkinnedMeshRenderer[] smrs = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            SkinnedMeshRenderer r = root.GetComponent<SkinnedMeshRenderer>();
            if (!r)
                r = root.AddComponent<SkinnedMeshRenderer>();
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            Dictionary<string, int> dict = SkeletonBonesCachingDict.CalcBonesDict(skeletonName, root);
            List<CombineInstance> combineInstances = new List<CombineInstance>(smrs.Length + 1);
            List<Material> materials = new List<Material>(smrs.Length + 1);
            List<Transform> bones = new List<Transform>(transforms.Length);
            List<Mesh> submeshesList = new List<Mesh>();
            for (int i = 0; i < smrs.Length; i++)
            {
                SkinnedMeshRenderer smr = smrs[i];
                smr.transform.SetParent(null);
                for (int j = 0; j < smr.sharedMaterials.Length; j++)
                {
                    var m1 = smr.sharedMaterials[j];
                    bool bFind = false;
                    foreach (var m2 in materials)
                    {
                        if (m1.name == m2.name)
                        {
                            bFind = true;
                            break;
                        }
                    }
                    if (!bFind)
                    {
                        materials.Add(m1);
                    }
                }
                if (smr.sharedMaterials == null)
                {
                    Debug.Log("smr shaderMaterials is null:" + smr.name);
                }
                if (smr.sharedMesh == null)
                {
                    Debug.Log("smr sharedMesh is null" + smr.name);
                }
                if (smr.sharedMaterials.Length != smr.sharedMesh.subMeshCount)
                {
                    Debug.LogError("材质球和网格数量不一致," + skeletonName + " " + smr.name);
                }
                if (smr.sharedMesh.subMeshCount > 1)
                {
                    for (int j = 0; j < smr.sharedMesh.subMeshCount; j++)
                    {
                        Mesh submeshe = MeshHelper.CreateMesh(smr.sharedMesh, j);
                        submeshesList.Add(submeshe);
                        combineInstances.Add(new CombineInstance() { mesh = submeshe, subMeshIndex = 0 });
                    }
                }
                else
                {
                    combineInstances.Add(new CombineInstance() { mesh = smr.sharedMesh, subMeshIndex = 0 });
                }
                for (int y = 0; y < smr.bones.Length; y++)
                {
                    string bone = smr.bones[y].name;
                    int boneIndex = -1;
                    if (!dict.TryGetValue(bone, out boneIndex))
                        continue;
                    Transform transform = transforms[boneIndex];
                    bones.Add(transform);
                }
            }
            foreach (var m in materials)
            {
                if (m.shader == null)
                    continue;
                if (m.shader.name == "Raptor/ZTestTransparentBlend"
                    || m.shader.name == "Raptor/ZTestTransparent")
                {
                    m.mainTexture.mipMapBias = -0.5f;
                }
            }
            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combineInstances.ToArray(), true, false);
            r.sharedMesh = mesh;
            r.bones = bones.ToArray();
            r.sharedMaterials = materials.ToArray();
            for (int i = 0; i < smrs.Length; i++)
            {
                GameObject obj = smrs[i].gameObject;
                GameObject.Destroy(obj);
            }
            if (submeshesList.Count > 0)
            {
                foreach (var submeshe in submeshesList)
                {
                    GameObject.Destroy(submeshe);
                }
                submeshesList.Clear();
            }
        }
        public static void ResetFashionMaterials(List<GameObject> objParts, SkinnedMeshRenderer r)
        {
            if (objParts.Count >= 5)
            {
                if (objParts[0].name.Contains("_1000") || objParts[0].name.Contains("_1001"))
                {
                    for (int i = 0; i < r.sharedMaterials.Length; i++)
                    {
                        r.sharedMaterials[i].shader = Shader.Find("Custom/Mobile/Diffuse");
                    }
                }
            }
        }
    }

    public class SkeletonBonesCachingDict
    {
        private static Dictionary<string, Dictionary<string, int>> sSkeletonBonesDict = new Dictionary<string, Dictionary<string, int>>();
        public static Dictionary<string, int> CalcBonesDict(string skeletonName, GameObject skeletonObj)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            if (sSkeletonBonesDict.TryGetValue(skeletonName, out dict))
            {
                return dict;
            }
            dict = new Dictionary<string, int>();
            Transform[] transforms = skeletonObj.GetComponentsInChildren<Transform>(true);
            for (int z = 0; z < transforms.Length; z++)
            {
                Transform transform = transforms[z];
                dict[transform.name] = z;
            }
            sSkeletonBonesDict[skeletonName] = dict;
            return dict;
        }
    }
}

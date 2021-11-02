using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Core.Framework.Utility;
using System.Diagnostics;

public class CharactorTestWindow : EditorWindow
{
    //public const string AssetPath = "Assets/PublicAssets/EditorAssets/CharactorTestAsset.asset";
    public const string TestObjectName = "CharactorTest";

    private CharacterTestAsset mAsset;
    private SerializedObject mSerializedObject;
    private SerializedProperty mPartsProperty;
    private SerializedProperty mSkeletonProperty;
    private SerializedProperty mControllerProperty;

    private GameObject _character;
    private Animator _animator;
    private Stopwatch _jumpTimer;

    [MenuItem("美術工具/角色測試")]
    public static void ShowCharactorTestWindow()
    {
        var window = GetWindow(typeof(CharactorTestWindow));
    }

    private void OnEnable()
    {
        _jumpTimer = new Stopwatch();

        var guids = AssetDatabase.FindAssets("t:CharacterTestAsset");
        for (int i = 0; i < guids.Length; i++)
        {
            var guid = guids[i];
            var path = AssetDatabase.GUIDToAssetPath(guid);
            mAsset = AssetDatabase.LoadAssetAtPath<CharacterTestAsset>(path);
            if (mAsset != null)
                break;
        }

        //mAsset = AssetDatabase.LoadAssetAtPath<CharacterTestAsset>(AssetPath);
        if(mAsset == null)
        {
            //mAsset = ScriptableObject.CreateInstance<CharacterTestAsset>();
            //AssetDatabase.CreateAsset(mAsset, AssetPath);
            return;
        }
        mSerializedObject = new SerializedObject(mAsset);
        mPartsProperty = mSerializedObject.FindProperty("parts");
        mSkeletonProperty = mSerializedObject.FindProperty("skeleton");
        mControllerProperty = mSerializedObject.FindProperty("controller");

        if (Application.isPlaying)
        {
            _character = GameObject.Find(TestObjectName);
            if (_character != null)
            {
                _animator = _character.GetComponent<Animator>() ?? _character.AddComponent<Animator>();
            }
        }
    }

    private void OnGUI()
    {
        if (mAsset == null)
            return;

        EditorGUILayout.PropertyField(mSkeletonProperty);
        EditorGUILayout.PropertyField(mControllerProperty);
        EditorGUILayout.PropertyField(mPartsProperty);
        mSerializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("組合"))
        {
            if(!Application.isPlaying)
            {
                return;
            }
            if (_character != null)
                Destroy(_character);
            GameObject skeleton = null;
            if (mAsset.skeleton != null)
            {
                skeleton = Instantiate(mAsset.skeleton);
            }
            List<GameObject> parts = new List<GameObject>();
            if(mAsset.parts != null && mAsset.parts.Count > 0)
            {
                foreach(var part in mAsset.parts)
                {
                    if(part != null)
                        parts.Add(Instantiate(part));
                }
            }
            if (skeleton != null && parts.Count > 0)
            {
                MergeRoleObject(mAsset.skeleton.name, skeleton, parts, new Vector3(0.0f, 1.0f, 0.0f), 0.01f, 2.0f);
                _character = skeleton;
                _character.name = TestObjectName;
                _animator = _character.GetComponent<Animator>() ?? _character.AddComponent<Animator>();
                _animator.runtimeAnimatorController = mAsset.controller;
            }
        }

        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("Idle"))
        {
            _animator.SetInteger("speed", 0);
        }
        if (GUILayout.Button("Walk"))
        {
            _animator.SetInteger("speed", 100);
            _animator.SetBool("isRush", false);
            _animator.SetBool("isWalk", true);
        }
        if (GUILayout.Button("Run"))
        {
            _animator.SetInteger("speed", 100);
            _animator.SetBool("isRush", false);
            _animator.SetBool("isWalk", false);
        }
        if (GUILayout.Button("Rush"))
        {
            _animator.SetInteger("speed", 100);
            _animator.SetBool("isRush", true);
            _animator.SetBool("isWalk", false);
        }
        if (GUILayout.Button("Jump"))
        {
            _jumpTimer.Start();
            _animator.SetBool("isJumping", true);
            _animator.SetTrigger("jump");
        }
        if (GUILayout.Button("Damage"))
        {
            _animator.SetTrigger("damage");
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("Attack1"))
        {
            _animator.SetTrigger("attack1");
        }
        if (GUILayout.Button("Attack2"))
        {
            _animator.SetTrigger("attack2");
        }
        if (GUILayout.Button("Attack3"))
        {
            _animator.SetTrigger("attack3");
        }
        if (GUILayout.Button("Attack4"))
        {
            _animator.SetTrigger("attack4");
        }
        if (GUILayout.Button("Attack5"))
        {
            _animator.SetTrigger("attack5");
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("DodgeMain"))
        {
            _animator.SetTrigger("dodgeMain");
        }
        if (GUILayout.Button("DodgeShadow"))
        {
            _animator.SetTrigger("dodgeShadow");
        }
        GUILayout.EndHorizontal();

        if (_jumpTimer.IsRunning && _jumpTimer.Elapsed.TotalSeconds >= 3)
        {
            _animator.SetBool("isJumping", false);
            _jumpTimer.Stop();
            _jumpTimer.Reset();
        }
    }

    public void MergeRoleObject(string skeletonName, GameObject skeleton, List<GameObject> partsList, Vector3 ccCenter, float ccRadius, float ccHeight)
    {
        skeleton.AddComponent<SkinnedMeshRenderer>();
        MeshHelper.MergeMeshes(skeletonName, skeleton, partsList);
    }
}
    
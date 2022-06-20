using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class AudioItem_Editor : ScriptableWizard
{
    public static AudioItem_Editor Instance;

    public static AudioCategory curCat;

    void OnEnable()
    {
        Instance = this;
    }

    void OnDisable()
    {
        Instance = null;
    }

    private static string SearchStr = "";

    private Action<string> callBack;

    private List<string> contents;

    private string curStr;

    private Vector2 scroll = Vector2.zero;

    private bool reScroll;

    private float lastClickTime;

    void OnGUI()
    {
        EditorGUIUtility.labelWidth = 80f;
        string oldStr = SearchStr;
        SearchStr = SearchField(SearchStr);
        if (oldStr != SearchStr)
        {
            reScroll = true;
        }
        if (contents != null)
        {
            bool close = false;
            var searchs =
                contents.FindAll(item => item.IndexOf(SearchStr, StringComparison.OrdinalIgnoreCase) >= 0);
            if (reScroll)
            {
                reScroll = false;
                scroll = Vector2.up * ((EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2) * searchs.IndexOf(curStr));
                //Debug.Log(scroll);
            }
            scroll = GUILayout.BeginScrollView(scroll);
            for (int i = 0; i < searchs.Count; i++)
            {
                Color oldColor = GUI.backgroundColor;
                if (string.Equals(curStr, searchs[i], StringComparison.Ordinal))
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button(searchs[i]))
                {
                    if (!string.Equals(curStr, searchs[i], StringComparison.Ordinal))
                    {
                        curStr = searchs[i];
                        if (callBack != null)
                        {
                            callBack(searchs[i]);
                        }
                    }
                    else
                    {
                        if (Time.realtimeSinceStartup - lastClickTime < 0.5f)
                        {
                            close = true;
                        }
                    }
                    lastClickTime = Time.realtimeSinceStartup;
                }
                GUI.backgroundColor = oldColor;
            }
            GUILayout.EndScrollView();
            if (close)
            {
                Close();
            }
        }
    }

    public static void Invoke(AudioCategory cat, string[] values, int curIndex, Action<string> callBack)
    {
        if (Instance == null)
        {
            DisplayWizard<AudioItem_Editor>("Select Audio Item");
        }

        if (Instance != null)
        {
            curCat = cat;
            Instance.contents = new List<string>(values);
            Instance.contents.Sort();
            Instance.callBack = callBack;
            Instance.curStr = curIndex > 0 && curIndex < values.Length ? values[curIndex] : "";
        }
    }

    public static void ChangeScroll()
    {
        if (Instance != null)
            Instance.reScroll = true;
    }

    private static string SearchField(string value, params GUILayoutOption[] options)
    {
        MethodInfo info = typeof(EditorGUILayout).GetMethod("ToolbarSearchField",
            BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(string), typeof(GUILayoutOption[]) },
            null);
        if (info != null)
        {
            value = (string)info.Invoke(null, new object[] { value, options });
        }
        return value;
    }
}
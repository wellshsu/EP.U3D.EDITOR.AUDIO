//---------------------------------------------------------------------//
//                    GNU GENERAL PUBLIC LICENSE                       //
//                       Version 2, June 1991                          //
//                                                                     //
// Copyright (C) Wells Hsu, wellshsu@outlook.com, All rights reserved. //
// Everyone is permitted to copy and distribute verbatim copies        //
// of this license document, but changing it is not allowed.           //
//                  SEE LICENSE.md FOR MORE DETAILS.                   //
//---------------------------------------------------------------------//
using EP.U3D.EDITOR.BASE;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EP.U3D.EDITOR.AUDIO
{
    public class BuildAudio
    {
        public static Type WorkerType = typeof(BuildAudio);

        [MenuItem(Constants.MENU_PATCH_BUILD_AUDIO)]
        public static void Invoke()
        {
            var worker = Activator.CreateInstance(WorkerType) as BuildAudio;
            worker.Execute();
        }

        public virtual void Execute()
        {
            AudioController controller = AssetDatabase.LoadAssetAtPath<AudioController>(Constants.AUDIO_CONTROLLER);
            string toast;
            if (controller)
            {
                Dictionary<string, AudioItem> oldItems = new Dictionary<string, AudioItem>();
                for (int i = 0; i < controller.AudioCategories.Length; i++)
                {
                    AudioCategory temp = controller.AudioCategories[i];
                    if (temp != null)
                    {
                        for (int j = 0; j < temp.AudioItems.Length; j++)
                        {
                            AudioItem item = temp.AudioItems[j];
                            if (item != null)
                            {
                                if (oldItems.ContainsKey(item.Name) == false)
                                {
                                    oldItems.Add(item.Name, item);
                                }
                            }
                        }
                    }
                }

                Dictionary<string, List<AudioItem>> dict = new Dictionary<string, List<AudioItem>>();
                List<string> audios = new List<string>();
                Helper.CollectAssets(Constants.AUDIO_WORKSPACE, audios);
                for (int i = 0; i < audios.Count; i++)
                {
                    string file = audios[i];
                    AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(file);
                    if (clip)
                    {
                        AudioItem item;
                        oldItems.TryGetValue(clip.name, out item);
                        if (item == null)
                        {
                            item = new AudioItem();
                            item.Name = clip.name;
                            item.subItems = new[] { new AudioSubItem() { Clip = clip } };
                        }
                        string cat = "OTHER";
                        string[] strs = clip.name.Split("_");
                        if (strs.Length > 1) cat = strs[0];
                        List<AudioItem> list;
                        if (dict.TryGetValue(cat, out list) == false)
                        {
                            list = new List<AudioItem>();
                            dict.Add(cat, list);
                        }
                        list.Add(item);
                    }
                }

                List<AudioCategory> cats = new List<AudioCategory>();
                foreach (var kvp in dict)
                {
                    cats.Add(new AudioCategory(controller) { Name = kvp.Key, AudioItems = kvp.Value.ToArray() });
                }
                controller.AudioCategories = cats.ToArray();

                PrefabUtility.SavePrefabAsset(controller.gameObject);
                toast = "Build " + audios.Count + " Audio(s) Success.";
            }
            else
            {
                toast = "Build Audio Fail Caused by Nil AudioController.";
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Helper.ShowToast(toast);
            Helper.Log(toast);
        }
    }
}
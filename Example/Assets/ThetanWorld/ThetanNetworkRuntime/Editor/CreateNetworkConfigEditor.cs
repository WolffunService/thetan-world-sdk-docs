using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace  Wolffun.RestAPI.Editor
{
    public class CreateNetworkConfigEditor : UnityEditor.Editor
    {
        [MenuItem("Tools/Wolffun/CreateNetworkConfig")]
        public static void CreateNetworkConfig()
        {
            var networkConfig = CreateInstance<ThetanNetworkConfig>();

            if(!AssetDatabase.IsValidFolder("Assets/ThetanWorld"))
            {
                AssetDatabase.CreateFolder("Assets", "ThetanWorld");
            }
            
            if(!AssetDatabase.IsValidFolder("Assets/ThetanWorld/Resources"))
            {
                AssetDatabase.CreateFolder("Assets/ThetanWorld", "Resources");
            }
                
            
            AssetDatabase.CreateAsset(networkConfig, "Assets/ThetanWorld/Resources/ThetanSDKNetworkConfig.asset");
            AssetDatabase.SaveAssets();
        }
    }

}

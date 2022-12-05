using UnityEngine;

namespace UI
{
    public class UIDebugMission : IXUI
    {
        public static void Show()
        {
            var view = new UIDebugMission();
            XUI_Manager.Show(view, XUI_Layer.Tips);
        }


        private GameObject _gameObject;
        private XUI_Layer _layerName;
        private uint _uniqueId;

        public string GetPrefabPath()
        {
            return "UI/Prefabs/UIDebugMission.prefab";
        }

        public void BeforeInit(GameObject gameObject, XUI_Layer layerName)
        {
            _gameObject = gameObject;
            _layerName = layerName;
            _uniqueId = XUI_Manager.GenRunTimeId();
        }

        public XUI_Layer GetLayer()
        {
            return _layerName;
        }

        GameObject IXUI.GetGameObject()
        {
            return GetGameObject();
        }

        public GameObject GetGameObject()
        {
            return _gameObject;
        }

        public uint GetUniqueId()
        {
            return _uniqueId;
        }

        public void OnInit()
        {
            var btnExit = ObjectHelper.Find(GetGameObject(), "btnExit").GetComponent<XUI_Button>();;
            btnExit.onClick.AddListener(() =>
            {
                SceneLoader.LoadMainCity(() => { });
            });
            
            var btnTest = ObjectHelper.Find(GetGameObject(), "btnTest").GetComponent<XUI_Button>();;
            btnTest.onClick.AddListener(() =>
            {
                /*foreach (var world in World.All)
                {
                    var flags = BakingUtility.BakingFlags.AddEntityGUID |
                                BakingUtility.BakingFlags.AssignName | BakingUtility.BakingFlags.GameViewLiveConversion;
                    var settings = new BakingSettings(flags, default)
                    {
                        //SceneGUID = sceneWithBuildConfiguration.SceneGUID,
                        //DotsSettings = settingsAsset,
                       // BuildConfiguration = buildConfig,
                       // IsBuiltInBuildsEnabled = sceneWithBuildConfiguration.IsBuiltInBuildsEnabled
                    };

                    BakingUtility.BakeScene(world, SceneManager.GetActiveScene(), settings, false, null);
                }*/
            });
        }

        
        public void OnShow()
        {
           
        }

        public void Update()
        {
        }


        public void OnDestroy()
        {
        }
    }
}
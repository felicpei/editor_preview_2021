using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class UIDebugEntrance : IXUI
    {
        public static void Show()
        {
            var view = new UIDebugEntrance();
            XUI_Manager.Show(view, XUI_Layer.Tips);
        }


        private GameObject _gameObject;
        private XUI_Layer _layerName;
        private uint _uniqueId;

        public string GetPrefabPath()
        {
            return "UI/Prefabs/UIDebugEntrance.prefab";
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
            {
                var btnRoot = ObjectHelper.Find(GetGameObject(), "Mask/dialogBg/grid");
                
                var btnZombie = ObjectHelper.Find(btnRoot, "btn_zombie").GetComponent<XUI_Button>();;
                btnZombie.onClick.AddListener(() =>
                {
                    SceneLoader.Load((int)ESceneId.Test);
                });
            }
           
            {
                var btnRoot = ObjectHelper.Find(GetGameObject(), "Mask/dialogBg/grid2");
            
                var btn1 = ObjectHelper.Find(btnRoot, "btn1").GetComponent<XUI_Button>();
                btn1.onClick.AddListener(() =>
                {
                    XGameSetting.XResolution = XGameSetting.EnumResolution.Low;
                });
            
                var btn2 = ObjectHelper.Find(btnRoot, "btn2").GetComponent<XUI_Button>();
                btn2.onClick.AddListener(() =>
                {
                    XGameSetting.XResolution = XGameSetting.EnumResolution.Mid;
                });
                
                var btn3 = ObjectHelper.Find(btnRoot, "btn3").GetComponent<XUI_Button>();
                btn3.onClick.AddListener(() =>
                {
                    XGameSetting.XResolution = XGameSetting.EnumResolution.High;
                });
            }
            
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
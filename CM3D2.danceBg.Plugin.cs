using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;
using System.Reflection;
using System.Collections;

namespace CM3D2.danceBg
{
    [PluginFilter("CM3D2x64"),
    PluginFilter("CM3D2x86"),
    PluginFilter("CM3D2VRx64"),
    PluginName("danceBg"),
    PluginVersion("0.0.0.1a")]
    public class danceBg : PluginBase
    {

        private XmlManager xmlManager;
        private Boolean maidSetting = false;
        private Maid maid;
        private Transform cameraTransform;
        private Light[] lightList= null;
        private Vector3[] lightVectorList = null;
        

        private DanceMain danceMain = null;
        private FieldInfo fieldMaid = (typeof(Maid)).GetField("m_Param", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        
        private void SetPreset(Maid maid, string fileName)
        {
            var preset = GameMain.Instance.CharacterMgr.PresetLoad(Path.Combine(Path.GetFullPath(".\\") + "Preset", fileName));
            GameMain.Instance.CharacterMgr.PresetSet(maid, preset);
        }

        public void Awake()
        {
            UnityEngine.Object.DontDestroyOnLoad(this);
            xmlManager = new XmlManager();
        }

        private void OnLevelWasLoaded(int level)
        {
            danceMain = (DanceMain)FindObjectOfType(typeof(DanceMain));
            maidSetting = false;
            cameraTransform = null;
            lightList = null;
            if(danceMain == null) return;
            Debug.Log("DanceBg.Plugin:[Maid name] " + xmlManager.BackGroundPreset);
        }


        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape)){
                xmlManager = new XmlManager();
            }
        }
        
        private void LateUpdate(){
            if(danceMain == null) return;

            if(maidSetting == false){
                if(xmlManager.BackGroundPreset != null){
                    String extent = Path.GetExtension(xmlManager.BackGroundPreset);
                    if(extent.Equals(".preset")){
                        maid = searchStockMaid("plugin","backGround");
                        if (maid == null){
                            maid = GameMain.Instance.CharacterMgr.AddStockMaid();
                            MaidParam m_Param = (MaidParam)fieldMaid.GetValue(maid);
                            m_Param.SetName("plugin","backGround");
                        }
                        SetPreset(maid,xmlManager.BackGroundPreset);
                    }
                    else{
                        string[] nameList = xmlManager.BackGroundPreset.Split(' ');
                        if(nameList.Length != 2){
                            Debug.LogError("DanceBg.Plugin:[Maid name Invalid] " +  nameList.Length + "#" + xmlManager.BackGroundPreset);
                            maidSetting = true;
                            return;
                        }
                        maid = searchStockMaid(nameList[0], nameList[1]);
                        if(maid == null){
                            Debug.LogError("DanceBg.Plugin:[Maid not found] #" + nameList[0] + "#"  + nameList[1]);
                            maidSetting = true;
                            return;
                        }
                    }
                }
                if(maid != null) {
                    //表示済みのめいどさんの位置変更
                    int maidIndex;
                    for(maidIndex = 0; maidIndex < GameMain.Instance.CharacterMgr.GetMaidCount(); maidIndex++){
                         Maid maidx = GameMain.Instance.CharacterMgr.GetMaid(maidIndex);
                         if(maidx == null) break;
                         maidx.SetPos(maidx.gameObject.transform.localPosition + xmlManager.BackGroundPos);
                    }
                    // 照明移動は時々？動く ここで初期位置設定
                    // ダンスによってはMainLight以外にもLightがあるのでLightを移動
                    lightList = (Light[])FindObjectsOfType(typeof(Light));
                    lightVectorList = new Vector3[lightList.Length];
                    for (int i = 0; i < lightList.Length; i++){
                        lightList[i].gameObject.transform.position =
                                            new Vector3(lightList[i].gameObject.transform.position.x,
                                                        lightList[i].gameObject.transform.position.y,
                                                        lightList[i].gameObject.transform.position.z) + xmlManager.BackGroundPos;
                        lightVectorList[i] = new Vector3(lightList[i].gameObject.transform.position.x,
                                                          lightList[i].gameObject.transform.position.y,
                                                          lightList[i].gameObject.transform.position.z);
                    //  Debug.LogError("Dance_khg.Plugin:light.transform.position" + lightList[i].gameObject.transform.position.ToString());
                    }
                    // カメラはほぼ毎フレーム移動 トランスフォームの取得しておく
                    cameraTransform = GameMain.Instance.MainCamera.transform;
                    // 背景用メイド設定
                    GameMain.Instance.CharacterMgr.SetActiveMaid(maid,maidIndex);
                    maid.SetPos(xmlManager.BackGroundPos + xmlManager.BackGroundPos2);
                    maid.Visible = true;
                }
                maidSetting = true;
            }
            if(cameraTransform != null){
                cameraTransform.position = new Vector3(cameraTransform.position.x,
                                                       cameraTransform.position.y,
                                                       cameraTransform.position.z) + xmlManager.BackGroundPos;
            }
            if(lightList != null){
                for (int i = 0; i < lightList.Length; i++){
                    if(lightList[i].gameObject.transform.position != lightVectorList[i]){
                        lightList[i].gameObject.transform.position =
                                            new Vector3(lightList[i].gameObject.transform.position.x,
                                                        lightList[i].gameObject.transform.position.y,
                                                        lightList[i].gameObject.transform.position.z) + xmlManager.BackGroundPos;
                        lightVectorList[i] = new Vector3(lightList[i].gameObject.transform.position.x,
                                                         lightList[i].gameObject.transform.position.y,
                                                         lightList[i].gameObject.transform.position.z);
                    //    Debug.LogError("Dance_khg.Plugin:light.transform.position" + lightList[i].gameObject.transform.position.ToString());
                    }
                }
            }
        }
        
        private Maid searchStockMaid(string lastName, string firstName){
            List<Maid> StockMaidList = GameMain.Instance.CharacterMgr.GetStockMaidList();
            foreach(Maid maidn in StockMaidList){
                MaidParam m_Param = (MaidParam)fieldMaid.GetValue(maidn);
                if(lastName.Equals(m_Param.status.last_name) &&
                   firstName.Equals(m_Param.status.first_name)){
                    return maidn;
                }
            }
            return null;
        }

        //------------------------------------------------------xml--------------------------------------------------------------------
        private class XmlManager
        {
            private string xmlFileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Config\danceBg.xml";
            private XmlDocument xmldoc = new XmlDocument();
            public string    BackGroundPreset = null;
            public Vector3   BackGroundPos = new Vector3(0.0f,10.0f,0.0f);
            public Vector3   BackGroundPos2 = new Vector3(0.0f,0.0f,0.0f);
//            public Vector3   BackGroundPos2 = new Vector3(0.0f,0.22f,-4.5f);
            
            public XmlManager()
            {
                try{
                    InitXml();
                }
                catch(Exception e){
                    Debug.LogError("DanceBg.Plugin:" + e.Source + e.Message + e.StackTrace);
                }
            }

            private void InitXml()
            {
                xmldoc.Load(xmlFileName);
                // PresetList
                XmlNodeList presetList = xmldoc.GetElementsByTagName("BackGround");
                BackGroundPreset =((XmlElement)presetList[0]).GetAttribute("FileName");
            }
        }

    }
}


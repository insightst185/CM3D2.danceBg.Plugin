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
    PluginVersion("0.0.0.2")]
    public class danceBg : PluginBase
    {

        private XmlManager xmlManager;
        private Boolean maidSetting = false;
        private Maid maid;
        private Transform cameraTransform;
        private Light[] lightList= null;
        private Vector3[] lightVectorList = null;
        private Vector3 localscale = new Vector3(0.5f,0.5f,0.5f);

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
//            if(danceMain == null) return;
//            Debug.LogError("DanceBg.Plugin:[farClipPlane] #" + GameMain.Instance.MainCamera.camera.farClipPlane);
//            GameMain.Instance.MainCamera.camera.farClipPlane=100;
        }
        
        private void LateUpdate(){
            if(danceMain == null) return;

            // カメラの表示を適当に広げる よくわからんので毎フレームやっとく
            GameMain.Instance.MainCamera.camera.farClipPlane = xmlManager.farClipPlane;

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
//                    for(maidIndex = 1; maidIndex < GameMain.Instance.CharacterMgr.GetMaidCount(); maidIndex++){
                    for(maidIndex = 0; maidIndex < GameMain.Instance.CharacterMgr.GetMaidCount(); maidIndex++){
                         Maid maidx = GameMain.Instance.CharacterMgr.GetMaid(maidIndex);
                         if(maidx == null) break;
//                         if(maidIndex == 0){
//                         maidx.SetRot(maid.GetRot() + new Vector3(0.0f,180.0f,0.0f));
//                         }
//                         else{
                         maidx.SetPos(maidx.gameObject.transform.localPosition + xmlManager.BackGroundPos);
//                         maidx.gameObject.transform.localScale = localscale;
//                         }
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
                    //  Debug.LogError("Dance_khg.Plugin:light.transform.eulerAngles" + lightList[i].gameObject.transform.eulerAngles.ToString());
                    //  Debug.LogError("Dance_khg.Plugin:light.range" + lightList[i].type + " " + lightList[i].range);
                    //  if(lightList[i].type == LightType.Point) lightList[i].range += 30.0f;
                    //  lightList[i].range = lightList[i].range * 2.0f;
                    }
                    // カメラはほぼ毎フレーム移動 トランスフォームの取得しておく
                    cameraTransform = GameMain.Instance.MainCamera.transform;
                    // 背景用メイド設定
                    GameMain.Instance.CharacterMgr.SetActiveMaid(maid,maidIndex);
                    maid.SetPos(xmlManager.BackGroundPos + xmlManager.BackGroundPos2);
                    maid.SetRot(xmlManager.BackGroundrotate);
//                    maid.gameObject.transform.localScale = localscale;
                    maid.Visible = true;
                    
                    // 固定ライト追加
//                    GameObject goSubLight;
//                    goSubLight = new GameObject("sub light");
//                    goSubLight.AddComponent<Light>();
//                    goSubLight.transform.SetParent(goSubCam.transform);
//                    goSubLight.transform.position =
//                                            new Vector3(0.0f,
//                                                        1.0f,
//                                                        0.0f) + xmlManager.BackGroundPos;
//                    goSubLight.transform.eulerAngles =
//                                            new Vector3(90.0f,  全然わからん
//                                                        0.0f,
//                                                        0.0f);

//                    goSubLight.GetComponent<Light>().type = LightType.Spot;
//                    goSubLight.GetComponent<Light>().type = LightType.Directional;
//                    goSubLight.GetComponent<Light>().range = 10;
//                    goSubLight.GetComponent<Light>().enabled = true;
//                    goSubLight.GetComponent<Light>().intensity = 1.2f;
//                  goSubLight.GetComponent<Light>().spotAngle = ssParam.fValue[PKeySubLight][PPropSubLightRange];
                }
                
//                GameObject[] gObjList = (GameObject[])FindObjectsOfType(typeof(GameObject));
//                foreach (GameObject gObj in gObjList){
//                    Debug.LogError("DanceBg.Plugin:" + gObj.tag);
//                }
                
                maidSetting = true;
            }
            if(cameraTransform != null){
                cameraTransform.position = new Vector3(cameraTransform.position.x,
                                                       cameraTransform.position.y,
                                                       cameraTransform.position.z) + xmlManager.BackGroundPos;
//                cameraTransform.position = new Vector3(cameraTransform.position.x * localscale.x,
//                                                       cameraTransform.position.y * localscale.y,
//                                                       cameraTransform.position.z * localscale.z) + xmlManager.BackGroundPos;
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
                    //  Debug.LogError("Dance_khg.Plugin:light.range" + lightList[i].type + " " + lightList[i].range);
                    //  Debug.LogError("Dance_khg.Plugin:light.transform.eulerAngles" + lightList[i].gameObject.transform.eulerAngles.ToString());
                    }
                }
            }
            
//            if(Input.GetKeyDown(KeyCode.Space)){
//                xmlManager = new XmlManager();
//                Maid maidx = GameMain.Instance.CharacterMgr.GetMaid(0);
//                maidx.SetPos(maidx.gameObject.transform.localPosition + xmlManager.BackGroundPos);
//            }
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
//            public Vector3   BackGroundPos2 = new Vector3(0.0f,0.0f,0.0f);
//            public Vector3   BackGroundPos2 = new Vector3(0.0f,-0.1f,-0.8f);
//            public Vector3   BackGroundPos2 = new Vector3(0.0f,0.0f,5.5f);
//            public Vector3   BackGroundPos2 = new Vector3(0.0f,0.22f,-4.5f);
            public Vector3 BackGroundPos2 = new Vector3(0.0f,0.0f,0.0f);
            public Vector3 BackGroundrotate = new Vector3(0.0f,0.0f,0.0f);
            public float farClipPlane = 100;
            
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
                try{
                    BackGroundPos2 = new Vector3(float.Parse(((XmlElement)presetList[0]).GetAttribute("X"))
                                                ,float.Parse(((XmlElement)presetList[0]).GetAttribute("Y"))
                                                ,float.Parse(((XmlElement)presetList[0]).GetAttribute("Z")));
                }
                catch(Exception e){
                    // 握りつぶす
                }
                try{
                    BackGroundrotate = new Vector3(float.Parse(((XmlElement)presetList[0]).GetAttribute("rotateX"))
                                                  ,float.Parse(((XmlElement)presetList[0]).GetAttribute("rotateY"))
                                                  ,float.Parse(((XmlElement)presetList[0]).GetAttribute("rotateZ")));
                }
                catch(Exception e){
                    // 握りつぶす
                }

                XmlNodeList cameraList = xmldoc.GetElementsByTagName("Camera");
                if(cameraList != null){
                    farClipPlane = float.Parse(((XmlElement)cameraList[0]).GetAttribute("farClipPlane"));
                }

            }
        }

    }
}


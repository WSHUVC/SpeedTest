using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Linq;
using System.IO;
using System;
using UnityEditor;

[Serializable]
public class WSH_Layout
{
    public string code;
    public string model;
    public string procTime;
    public string doorOpenTime;
    public string doorCloseTime;
    public string posX;
    public string posY;
    public string posZ;
    public string rotX;
    public string rotY;
    public string rotZ;
    public WSH_Layout_Port[] ports;
}

[Serializable]
public class WSH_Layout_Port
{
    public string code;
    public string model;
    public string posX;
    public string posY;
    public string posZ;
    public string rotX;
    public string rotY;
    public string rotZ;
}
public class WSH_LayoutLoader : MonoBehaviour
{
    string layoutFile;
    [SerializeField]
    string rootNodeName = "FACTORYLAYOUT/process";
    public List<WSH_Layout> layoutDatas = new List<WSH_Layout>();
    [SerializeField]
    List<WSH_Process> processList = new List<WSH_Process>();

    public void SelectLayoutFile()
    {
        layoutFile = EditorUtility.OpenFilePanel("File Open", "", "xml");
    }

    GameObject LoadPrefab(string name)
    {
        string path ="Prefabs/" + name;
        GameObject result = (GameObject)Resources.Load(path);
        return result;
    }
    bool LoadModel(string check, string name, Transform parent, out GameObject result)
    {
        result = null;
        if (check != "model")
            return false;
        var obj = LoadPrefab(name);
        obj = obj == null ? new GameObject(name) : Instantiate(obj);
        obj.transform.SetParent(parent);
        obj.name = name;
        result = obj;
        return true;
    }

    public void Load()
    {
        SelectLayoutFile();
        layoutDatas.Clear();
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(File.ReadAllText(layoutFile));

        XmlNodeList nodes = xmlDoc.SelectNodes(rootNodeName);
        var reflection = typeof(WSH_Layout);
        var singleNodeNames = reflection.GetFields().Select(m => m.Name).ToArray();
        var portNodeNames = typeof(WSH_Layout_Port).GetFields().Select(m => m.Name).ToArray();

        var root = new GameObject();
        root.name = "FactoryLayout";
        foreach (XmlNode node in nodes)
        {
            var layout = new WSH_Layout();
            GameObject process = null;
            var ports = node.SelectNodes("port");
            var layoutPorts = new WSH_Layout_Port[ports.Count];
            var processPorts = new WSH_ProcessPort[layoutPorts.Length];

            for (int i = 0; i < singleNodeNames.Length; ++i)
            {
                var nodeName = singleNodeNames[i];
                if (nodeName != "ports")
                {
                    var innerText = node.SelectSingleNode(nodeName).InnerText;
                    var data = innerText;
                    var t = layout.GetType();
                    var f = t.GetField(nodeName);
                    f.SetValue(layout, data);

                    if (LoadModel(f.Name, data, root.transform, out var obj))
                    {
                        process = obj;
                        process.name = innerText;
                    }
                }
                else
                {
                    layoutPorts = new WSH_Layout_Port[ports.Count];
                    layout.ports = layoutPorts;
                    for (int j = 0; j < ports.Count; ++j)
                    {
                        var layoutPort = new WSH_Layout_Port();
                        GameObject port = null;
                        for (int w = 0; w < portNodeNames.Length; ++w)
                        {
                            var t = layoutPort.GetType();
                            var f = t.GetField(portNodeNames[w]);
                            var data = ports[j].SelectSingleNode(portNodeNames[w]).InnerText;
                            f.SetValue(layoutPort, data);

                            if (LoadModel(f.Name, data, root.transform, out var obj))
                                port = obj;
                        }
                        var pp = port.AddComponent<WSH_ProcessPort>();
                        pp.SetLayoutData(layoutPort);
                        port.name = layoutPort.code;
                        layoutPorts[j] = layoutPort;
                        processPorts[j] = pp;
                    }
                }
            }
            process.transform.SetParent(root.transform);
            process.name = layout.code;
            var p = process.AddComponent<WSH_Process>();
            p.SetLayoutData(layout, processPorts);
            processList.Add(p);
            layoutDatas.Add(layout);
        }
    }

    public void DrawLine()
    {

    }
}

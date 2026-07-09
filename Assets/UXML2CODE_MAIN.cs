using DisplayKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class UxmlToCodeWindow : EditorWindow
{
    VisualTreeAsset asset;
    int RootCanvasID = 0;
    bool AlwaysAppendIdOnName = false;

    Vector2 _scroll;
    Vector2 _log_scroll;
    string _output = string.Empty;
    string _log = string.Empty;

    [MenuItem("UXML To Code/UXML To Code")]
    public static void OpenWindow()
    {
        var w = GetWindow<UxmlToCodeWindow>("UXML To Code");
        w.minSize = new Vector2(600, 400);
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("UXML -> Code", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        asset = (VisualTreeAsset)EditorGUILayout.ObjectField("VisualTreeAsset", asset, typeof(VisualTreeAsset), false);
        RootCanvasID = EditorGUILayout.IntField("RootCanvasID", RootCanvasID);
        AlwaysAppendIdOnName = EditorGUILayout.Toggle("Always Append Id On Name", AlwaysAppendIdOnName);

        EditorGUILayout.Space();
        if (GUILayout.Button("Process"))
        {
            Process();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Generated Output:", EditorStyles.boldLabel);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        EditorGUILayout.TextArea(_output, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Log:", EditorStyles.boldLabel);

        _log_scroll = EditorGUILayout.BeginScrollView(_log_scroll);
        EditorGUILayout.TextArea(_log, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        if (!string.IsNullOrEmpty(_output))
        {
            if (GUILayout.Button("Copy Code To Clipboard"))
            {
                EditorGUIUtility.systemCopyBuffer = _output;
            }
        }
    }

    void Process()
    {
        if (asset == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a VisualTreeAsset.", "OK");
            return;
        }

        var path = AssetDatabase.GetAssetPath(asset);
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            EditorUtility.DisplayDialog("Error", $"Couldn't find asset file for VisualTreeAsset. Asset path:{path}", "OK");
            return;
        }

        string uxmlText;
        try
        {
            uxmlText = File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to read asset file: {ex.Message}", "OK");
            return;
        }

        var reader = new XmlDocument();
        try
        {
            reader.LoadXml(uxmlText);
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to parse UXML: {ex.Message}", "OK");
            return;
        }

        var sb = new StringBuilder();
        var parser = new UxmlParser(RootCanvasID, AlwaysAppendIdOnName, sb);
        parser.Parse(reader);
        _log = sb.ToString();
        var codeSb = new StringBuilder();
        foreach (var item in parser.Nodes)
        {
            item.ToCode(codeSb);
        }
        
        codeSb.Replace("new Color(1f, 0f, 0f, 1f)", "Color.red");
        codeSb.Replace("new Color(0f, 0f, 0f, 1f)", "Color.black");
        codeSb.Replace("new Color(0f, 0f, 1f, 1f)", "Color.blue");
        codeSb.Replace("new Color(0f, 1f, 0f, 1f)", "Color.green");
        codeSb.Replace("new Color(0f, 0f, 0f, 0f)", "Color.clear");
        codeSb.Replace("new Color(0f, 1f, 1f, 1f)", "Color.cyan");
        codeSb.Replace("new Color(1f, 1f, 1f, 1f)", "Color.white");
        codeSb.Replace("new Color(0.1f, 0.1f, 0.1f, 1f)", "Color.gray1");
        codeSb.Replace("new Color(0.2f, 0.2f, 0.2f, 1f)", "Color.gray2");
        // fuck it,im lazy
        _output = codeSb.ToString();
        if(parser.errorCount > 0)
        {
            EditorUtility.DisplayDialog("Error", $"Has happend: {parser.errorCount} error(s),Please check log", "OK");

        }
    }
    class UxmlParser
    {
        int _rootCanvasID;
        bool _alwaysAppend;
        StringBuilder _logSb;
        int _cid = -1;
        List<string> usedName = new();
        public List<Node> Nodes { get; } = new List<Node>();

        public UxmlParser(int rootCanvasID, bool alwaysAppend, StringBuilder logSb)
        {
            _rootCanvasID = rootCanvasID;
            _alwaysAppend = alwaysAppend;
            _logSb = logSb ?? new StringBuilder();
            _cid = _rootCanvasID - 1;
        }

        public int NextID { get { _cid++; return _cid; } }
        public int errorCount = 0;
        public void Parse(XmlDocument reader)
        {
            _logSb.Clear();
            errorCount = 0;
            PrintNodes(null, reader.ChildNodes, _logSb);
        }

        void PrintNodes(Node parent, XmlNodeList nodes, StringBuilder sb)
        {
            var msg = string.Empty;
            sb.AppendLine($"--- Starting Proccess node, parent.id:{parent?.id} ---");
            if (parent != null)
            {
                if (parent.type != NodeType.Label)
                {
                        sb.AppendLine($"    Correct parent type! parent.name:{parent?.GetName(sb)}");
                }
                else
                {
                    msg = $"    Error!Label(name:{parent.GetName(sb)},id:{parent.id}) is parent! Using id {_rootCanvasID}(root canvas) be parent\n";
                    errorCount++;
                    sb.Append(msg);
                    parent = Nodes.Find(x => x.id == _rootCanvasID);
                    if (parent == null) Debug.LogAssertion($"PrevMessage:{sb.ToString()}\nERROR:Failed to find root(id:{_rootCanvasID})!");
                }
            }
            else
            {
                sb.AppendLine($"parent:null due this is Root");
            }

            foreach (XmlNode node in nodes)
            {
                if (node is XmlElement element)
                {
                    sb.AppendLine($"    Node Name: {node.Name} Type: {node.NodeType} Value: {node.Value} {node.InnerText}");
                    var TryToSplitUIType = node.Name.Split(":");
                    if (TryToSplitUIType.Length > 1)
                    {
                        sb.AppendLine($"        UI Type: {TryToSplitUIType[0]} Element Type: {TryToSplitUIType[1]}");
                        var n = new Node();
                        var id = NextID;
                        n.id = id;
                        sb.AppendLine($"        Node.id:{n.id}");
                        NodeType t = NodeType.VisualElement;
                        try
                        {
                            t = Enum.Parse<NodeType>(TryToSplitUIType[1], true);
                        }catch(ArgumentException e)
                        {
                            errorCount++;
                            msg += "Error! "+e.Message + $" --- \"{TryToSplitUIType[1]}\" failed to convent! Only Supported with Label and VisualElement! Replcae it to VisualElement!";
                            sb.AppendLine("Error! " + e.Message + $" --- \"{TryToSplitUIType[1]}\" has not supported! Only Supported with Label and VisualElement!");
                        }
                        n.type = t;
                        n.parent = parent;
                        var HasSetName = false;
                        foreach (XmlNode childNode in element.Attributes)
                        {
                            sb.AppendLine($"        Attribute: {childNode.Name} Value: {childNode.Value}");
                            switch (childNode.Name)
                            {
                                case "name":
                                    var appendId = _alwaysAppend;
                                    var name = childNode.Value;
                                    var willAppendID = usedName.Count(x=>x==name);
                                    if (usedName.Contains(name))
                                    {
                                        appendId = true;
                                    }
                                    usedName.Add(name);
                                    if (_alwaysAppend) willAppendID = n.id;
                                    n.SetName(appendId ? $"{name}_{willAppendID}" : name);
                                    HasSetName = true;
                                    break;
                                case "text":
                                    n.text = childNode.Value;
                                    break;
                                case "style":
                                    n.styles = DisplayKit.StyleParser.Parse(childNode.Value);
                                    break;
                                default:
                                    break;
                            }
                        }
                        if (!HasSetName)
                        {
                            var appendId = _alwaysAppend;
                            var name = n.type.ToString();
                            var willAppendID = usedName.Count(x => x == name);
                            if (usedName.Contains(name))
                            {
                                appendId = true;
                            }
                            usedName.Add(name);
                            if (_alwaysAppend) willAppendID = n.id;
                            n.SetName(appendId ? $"{name}_{willAppendID}" : name);
                        }
                        if(t == NodeType.UXML)
                        {
                            var appendId = _alwaysAppend;
                            var name = "canvas";
                            var willAppendID = usedName.Count(x => x == name);
                            if (usedName.Contains(name))
                            {
                                appendId = true;
                            }
                            usedName.Add(name);
                            if (_alwaysAppend) willAppendID = n.id;
                            n.SetName(appendId ? $"{name}_{willAppendID}" : name);
                        }
                        n.message = msg;
                        Nodes.Add(n);
                        if (node.HasChildNodes)
                        {
                            sb.AppendLine($"");
                            PrintNodes(n, element.ChildNodes, sb);
                        }
                    }
                }
            }
        }
    }

    enum NodeType
    {
        VisualElement,
        Label,
        UXML // to canvas
    }

    class Node
    {
        public NodeType type;
        public int id;
        public string message;
        public string GetName(StringBuilder log = null)
        {
            if (string.IsNullOrEmpty(_name))
            {
                var newName = $"{type}_{id}";
                log?.AppendLine($"_name = null! type:{type} id:{id} setting _name to {newName}");
                _name = newName;
            }
            return _name;
        }

        public void SetName(string value)
        {
            _name = value;
        }

        string _name;
        public Dictionary<string, BaseStyle> styles;
        // Only be filled when the type is Label
        public string text;
        public Node parent;
        public void ToCode(StringBuilder sb)
        {
            var varName = GetName();
            if (!string.IsNullOrEmpty(message))
            {
                sb.AppendLine("/*");
                sb.AppendLine(message);
                sb.AppendLine("*/");
            }
            switch (type)
            {
                case NodeType.VisualElement:
                    sb.AppendLine($"DisplayElement {varName} = {parent.GetName()}.AddElement();");
                    break;
                case NodeType.Label:
                    sb.AppendLine($"DisplayText {varName} = {parent.GetName()}.AddText(\"{text}\");");
                    break;
                case NodeType.UXML:
                    sb.AppendLine($"DisplayCanvas {varName} = DisplayCanvas.Create();");
                    break;
                default:
                    break;
            }
            if (type != NodeType.UXML && styles != null)
            {
                foreach (var style in styles.Values)
                {
                    style.ToCode(varName, sb);
                }
            }
            sb.AppendLine();
        }
    }
}
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
    bool WritePath = true;
    bool CustomCodeIncludeCanvas = false;
    string before = "";
    string after = "";
    // Replacements input: each line in the format old=>new
    string replacements = "";
    Vector2 _scroll;
    Vector2 _log_scroll;
    string _output = string.Empty;
    string _log = string.Empty;
    private Vector2 _tscroll;

    public class ParseContext
    {
        public int CurrentID;
        public List<string> UsedNames = new List<string>();
        public int ErrorCount;
        public bool AlwaysAppendId;
        public bool WritePath;
        public StringBuilder Log;
        public int startID;
        public string BeforeCode;
        public string AfterCode;
        public bool CustomCodeIncludeCanvas = true;
        public ParseContext(int startID, bool alwaysAppendId, bool writePath, StringBuilder log, string beforeCode, string afterCode, bool customCodeIncludeCanvas)
        {
            this.startID = startID;
            CurrentID = startID - 1;
            AlwaysAppendId = alwaysAppendId;
            WritePath = writePath;
            Log = log ?? new StringBuilder();
            BeforeCode = beforeCode;
            AfterCode = afterCode;
            CustomCodeIncludeCanvas = customCodeIncludeCanvas;
        }

        public int NextID()
        {
            CurrentID++;
            return CurrentID;
        }
    }

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
        EditorGUILayout.LabelField("Warning! dont use USS(or stylesheet)!Only extract the data in uxml and template!", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        asset = (VisualTreeAsset)EditorGUILayout.ObjectField("VisualTreeAsset", asset, typeof(VisualTreeAsset), false);
        RootCanvasID = EditorGUILayout.IntField("RootCanvasID", RootCanvasID);
        AlwaysAppendIdOnName = EditorGUILayout.Toggle("Always Append Id On Name", AlwaysAppendIdOnName);
        WritePath = EditorGUILayout.Toggle("Write Path In Message Area", WritePath);

        CustomCodeIncludeCanvas = EditorGUILayout.Toggle("Allow to append code if target is canvas", CustomCodeIncludeCanvas);

        EditorGUILayout.LabelField("Before a element create,Insert:", EditorStyles.boldLabel);
        before = EditorGUILayout.TextArea(before);
        EditorGUILayout.LabelField("After a element create,Insert:", EditorStyles.boldLabel);
        after = EditorGUILayout.TextArea(after);
        EditorGUILayout.LabelField("( \"${name}\" will be replaced by variable name)", EditorStyles.miniLabel);

        EditorGUILayout.LabelField("Replacements (old=>new per line):", EditorStyles.boldLabel);
        replacements = EditorGUILayout.TextArea(replacements);
        EditorGUILayout.LabelField("Example: old=>new", EditorStyles.miniLabel);


        EditorGUILayout.Space();
        if (GUILayout.Button("Process"))
        {
            Process();
        }

        EditorGUILayout.Space();
        _tscroll = EditorGUILayout.BeginScrollView(_tscroll);
        EditorGUILayout.LabelField("Generated Output:", EditorStyles.boldLabel);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        EditorGUILayout.TextArea(_output, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Log:", EditorStyles.boldLabel);

        _log_scroll = EditorGUILayout.BeginScrollView(_log_scroll);
        EditorGUILayout.TextArea(_log, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

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
        Node.templateToPath.Clear();
        var codesb = new StringBuilder();
        var logsb = new StringBuilder();
        var path = AssetDatabase.GetAssetPath(asset);
        var context = new ParseContext(RootCanvasID, AlwaysAppendIdOnName, WritePath, logsb, before, after, CustomCodeIncludeCanvas);
        try
        {
            var error = StartProcess(path, codesb, ref context, template: false, parent: null);
            if (error > 0)
            {
                EditorUtility.DisplayDialog("Error", $"Has happend: {error} error(s),Please check log", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            logsb.AppendLine($"An Error has be catched! Exception ex:{ex}");
        }
        // apply replacements defined in UI (each line: old=>new)
        if (!string.IsNullOrEmpty(replacements))
        {
            foreach (var line in replacements.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var idx = line.IndexOf("=>");
                if (idx <= 0) continue;
                var key = line.Substring(0, idx).Trim();
                var val = line.Substring(idx + 2).Trim();
                if (key.Length == 0) continue;
                try { codesb.Replace(key, val); }
                catch (Exception ex) { logsb.AppendLine($"Replacement failed for '{key}'=>'{val}': {ex.Message}"); }
            }
        }

        _log = logsb.ToString();
        _output = codesb.ToString();
    }

    public static int StartProcess(string path, StringBuilder codesb, ref ParseContext context, bool template, Node parent, string varname = "", int baseIndent = 0)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            context.Log?.AppendLine($"Error: Couldn't find asset file for VisualTreeAsset. Asset path:{path}");
            context.ErrorCount++;
            return context.ErrorCount;
        }

        string uxmlText;
        try
        {
            uxmlText = File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            context.Log?.AppendLine($"Error: Failed to read asset file: {ex.Message}");
            context.ErrorCount++;
            return context.ErrorCount;
        }

        var reader = new XmlDocument();
        try
        {
            reader.LoadXml(uxmlText);
        }
        catch (Exception ex)
        {
            context.Log?.AppendLine($"Error: Failed to parse UXML: {ex.Message}");
            context.ErrorCount++;
            return context.ErrorCount;
        }

        var parser = new UxmlParser(context, template, parent);
        parser.Parse(reader, varname);
        Node firstRealNode = parser.Nodes.FirstOrDefault(n => n.type != NodeType.Message);
        foreach (var item in parser.Nodes)
        {
            var i = item.ToCode(codesb, ref context, baseIndent);
            context.ErrorCount += i;
        }
        codesb.Replace("new Color(1f, 0f, 0f, 1f)", "Color.red");
        codesb.Replace("new Color(0f, 0f, 0f, 1f)", "Color.black");
        codesb.Replace("new Color(0f, 0f, 1f, 1f)", "Color.blue");
        codesb.Replace("new Color(0f, 1f, 0f, 1f)", "Color.green");
        codesb.Replace("new Color(0f, 0f, 0f, 0f)", "Color.clear");
        codesb.Replace("new Color(0f, 1f, 1f, 1f)", "Color.cyan");
        codesb.Replace("new Color(1f, 1f, 1f, 1f)", "Color.white");
        return context.ErrorCount;
    }

    class UxmlParser
    {
        ParseContext _context;
        bool _isTemplate;
        Node _parentNode;
        public List<Node> Nodes { get; } = new List<Node>();

        public UxmlParser(ParseContext context, bool isTemplate, Node parent)
        {
            _context = context;
            _isTemplate = isTemplate;
            _parentNode = parent;
        }
        void Log(string msg)
        {
            _context.Log?.AppendLine(msg);
        }

        public void Parse(XmlDocument reader, string rootVarname)
        {
            PrintNodes(_parentNode, reader.ChildNodes, rootVarname);
        }

        void PrintNodes(Node parent, XmlNodeList nodes, string rootVarname)
        {
            if (_isTemplate && parent == null)
            {
                Log("illegal args! template=true parent=null! override template to false!");
                _context.ErrorCount++;
                _isTemplate = false;
            }

            Log($"--- Starting Proccess node, parent.id:{parent?.id} ---");
            if (parent != null)
            {
                if (parent.type != NodeType.Label)
                {
                    Log($"    Correct parent type! parent.name:{parent?.GetName(_context.Log)}");
                }
                else
                {
                    string errMsg = $"    Error!Label(name:{parent.GetName(_context.Log)},id:{parent.id} {(parent.parent == null ? $"{parent.indexInParent}th child of root" : $"{parent.indexInParent}th child of {parent.parent?.GetName(_context.Log)}")}) is parent! Refuse to continue!\n";
                    _context.ErrorCount++;
                    Log(errMsg);
                    parent = Nodes.Find(x => x.id == _context.startID);
                    var mn = new Node()
                    {
                        type = NodeType.Message,
                        message = errMsg,
                    };
                    Nodes.Add(mn);
                    if (parent == null)
                        Debug.LogAssertion($"PrevMessage:{_context.Log?.ToString()}\nERROR:Failed to find root canvas!");
                    return;
                }
            }
            else
            {
                Log("parent:null due this is Root");
            }

            int childIndex = 1;
            foreach (XmlNode node in nodes)
            {
                if (node is XmlElement element)
                {
                    Log($"    Node Name: {node.Name} Type: {node.NodeType} Value: {node.Value} {node.InnerText}");
                    var parts = node.Name.Split(":");
                    if (parts.Length > 1)
                    {
                        Log($"        UI Type: {parts[0]} Element Type: {parts[1]}");
                        NodeType t;
                        try
                        {
                            t = Enum.Parse<NodeType>(parts[1], true);
                        }
                        catch (ArgumentException e)
                        {
                            _context.ErrorCount++;
                            Log($"Error! {e.Message} --- \"{parts[1]}\" failed to convert! Replaced with VisualElement!");
                            var mn = new Node()
                            {
                                type = NodeType.Message,
                                message = $"Error! {e.Message} --- \"{parts[1]}\" failed to convert! Replaced with VisualElement!",
                            };
                            Nodes.Add(mn);
                            t = NodeType.VisualElement;
                        }
                        if (t == NodeType.Template)
                        {
                            Log($"    trying to resolve template...");
                            string src = null, name = null;
                            foreach (XmlAttribute attr in element.Attributes)
                            {
                                Log($"        Attribute: {attr.Name} Value: {attr.Value}");
                                if (attr.Name == "src") src = attr.Value;
                                if (attr.Name == "name") name = attr.Value;
                            }
                            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(src))
                            {
                                var templatePath = ConvertProjectUrlToAbsolutePath(src);
                                Node.templateToPath[name] = templatePath;
                                Log($"    --- Done! {name}->{templatePath}---");
                            }
                            else
                            {
                                _context.ErrorCount++;
                                Log($"Error! Failed to resolve Template due name or src not set, name:{name} src:{src}");
                                var mn = new Node()
                                {
                                    type = NodeType.Message,
                                    message = $"Error! Failed to resolve Template due name or src not set, name:{name} src:{src}",
                                };
                                Nodes.Add(mn);
                            }
                            continue;
                        }
                        bool hasName = false;
                        var n = new Node();
                        int id = _context.NextID();
                        n.id = id;
                        bool istemplateRoot = false;
                        if (_isTemplate && t == NodeType.UXML)
                        {
                            Log($"        In template! Override UXML to VisualElement!");
                            t = NodeType.VisualElement;
                            istemplateRoot = true;
                        }
                        n.type = t;
                        n.parent = parent;
                        n.indexInParent = childIndex++;
                        n.EndOfTemplate = null;

                        foreach (XmlAttribute attr in element.Attributes)
                        {
                            Log($"        Attribute: {attr.Name} Value: {attr.Value}");
                            switch (attr.Name)
                            {
                                case "name":
                                    string rawName = attr.Value;
                                    string finalName = GenerateUniqueName(rawName, id, out _);
                                    if (_isTemplate && istemplateRoot) finalName = rootVarname;
                                    n.SetName(finalName);
                                    hasName = true;
                                    break;
                                case "text":
                                    n.text = attr.Value;
                                    break;
                                case "style":
                                    n.styles = StyleParser.Parse(attr.Value);
                                    break;
                                case "template":
                                    n.template = attr.Value;
                                    break;
                                default:
                                    break;
                            }
                        }

                        if (!hasName)
                        {
                            string autoName = GenerateUniqueName(t.ToString(), id, out _);
                            if (istemplateRoot == true) autoName = rootVarname;
                            n.SetName(autoName);
                        }
                        if (t == NodeType.UXML)
                        {
                            string canvasName = GenerateUniqueName("canvas", id, out _);
                            n.SetName(canvasName);
                        }
                        if (_context.WritePath)
                        {
                            List<Node> pathNodes = new List<Node>();
                            Node cur = n;
                            while (cur != null)
                            {
                                pathNodes.Add(cur);
                                cur = cur.parent;
                            }
                            pathNodes.Reverse();
                            string pathStr = "";
                            foreach (var pn in pathNodes)
                            {
                                pathStr += $"{pn.GetName(_context.Log)}({pn.type} - id:{pn.id}, {(pn.parent == null ? "Root" : $"{pn.indexInParent}th child of {pn.parent.GetName(_context.Log)}")}) -> ";
                            }
                            if (pathStr.Length > 3)
                                pathStr = pathStr.Remove(pathStr.Length - 3);
                            n.message = pathStr;
                        }

                        Nodes.Add(n);

                        if (element.HasChildNodes)
                        {
                            Log("");
                            PrintNodes(n, element.ChildNodes, "");
                            if (n.type == NodeType.Instance) Nodes.Last(x => x.type != NodeType.Message).EndOfTemplate = n;

                        }
                        else
                        {
                            if (n.type == NodeType.Instance) n.EndOfTemplate = n;

                        }
                    }
                }
            }
        }
        string GenerateUniqueName(string baseName, int id, out bool appended)
        {
            appended = false;
            if (string.IsNullOrEmpty(baseName))
                baseName = "element";

            string candidate = baseName;
            if (_context.AlwaysAppendId)
            {
                candidate = $"{baseName}_{id}";
                appended = true;
                int suffix = id;
                while (_context.UsedNames.Contains(candidate))
                {
                    suffix++;
                    candidate = $"{baseName}_{suffix}";
                }
            }
            else
            {
                candidate = baseName;
                if (_context.UsedNames.Contains(candidate))
                {
                    appended = true;
                    candidate = $"{baseName}_{id}";
                    int suffix = id;
                    while (_context.UsedNames.Contains(candidate))
                    {
                        suffix++;
                        candidate = $"{baseName}_{suffix}";
                    }
                }
            }

            _context.UsedNames.Add(candidate);
            return candidate;
        }
    }

    public enum NodeType
    {
        VisualElement,
        Label,
        Message,
        UXML, // to canvas
        Template,
        Instance
    }

    public static string ConvertProjectUrlToAbsolutePath(string url)
    {
        if (string.IsNullOrEmpty(url) || !url.StartsWith("project://database/"))
        {
            Debug.LogError("invaild url!");
            return null;
        }

        string repath = url.Substring("project://database/".Length);
        int markindex = repath.IndexOf('?');
        if (markindex != -1)
        {
            repath = repath.Substring(0, markindex);
        }
        string assetpath = Uri.UnescapeDataString(repath);
        string root = Path.GetDirectoryName(Application.dataPath);
        if (root != null)
        {
            string abpath = Path.Combine(root, assetpath);
            return Path.GetFullPath(abpath);
        }

        return null;
    }

    public class Node
    {
        public static Dictionary<string, string> templateToPath = new Dictionary<string, string>();
        public NodeType type;
        public int id;
        public string message;
        public string template;
        public Dictionary<string, string> styles;
        public string text;
        public Node parent;
        public int indexInParent = 0;
        public string _name;
        public Node EndOfTemplate;
        public int GetDepth()
        {
            int depth = -1;
            Node current = this;
            while (current.parent != null)
            {
                depth++;
                current = current.parent;
            }
            return depth;
        }
        private string Indent(int level)
        {
            if (level <= 0) return string.Empty;
            return new string('\t', level);
        }
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

        public void SetName(string value) => _name = value;
        public int ToCode(StringBuilder sb, ref ParseContext context, int indentLevel = -1)
        {
            //if (indentLevel < 0)
            indentLevel = GetDepth();

            int errors = 0;
            var varName = GetName(context?.Log);
            string prefix = Indent(indentLevel);

            if (!string.IsNullOrEmpty(message))
            {
                sb.AppendLine(prefix + "/*");
                sb.AppendLine(prefix + message);
                sb.AppendLine(prefix + "*/");
            }

            if (type == NodeType.Message)
            {
                sb.AppendLine();
                return 0;
            }

            var be = context.BeforeCode.Replace("${name}", varName);
            var af = context.AfterCode.Replace("${name}", varName);
            if (type == NodeType.UXML && context.CustomCodeIncludeCanvas)
            {
                be = af = "";
            }
            if (!string.IsNullOrEmpty(be))
            {
                sb.AppendLine(prefix + "//User before code");
                sb.AppendLine(prefix + be);
                sb.AppendLine(prefix + "//User before code done");
            }

            switch (type)
            {
                case NodeType.VisualElement:
                    sb.AppendLine(prefix + $"// start define of {varName}");
                    sb.AppendLine(prefix + $"DisplayElement {varName} = {parent.GetName()}.AddElement();");
                    break;
                case NodeType.Label:
                    sb.AppendLine(prefix + $"// start define of {varName}");
                    sb.AppendLine(prefix + $"DisplayText {varName} = {parent.GetName()}.AddText(\"{text}\");");
                    break;
                case NodeType.UXML:
                    sb.AppendLine(prefix + $"// start define of {varName}");
                    sb.AppendLine(prefix + $"DisplayCanvas {varName} = DisplayCanvas.Create();");
                    break;
                case NodeType.Instance:

                    sb.AppendLine(prefix + $"// template start of {varName}");
                    sb.AppendLine(prefix + "{");
                    sb.AppendLine();
                    context?.Log?.AppendLine("Template Proceed start, try to get path...");
                    if (templateToPath.TryGetValue(template, out var path))
                    {
                        var ret = StartProcess(path, sb,ref context, true, this.parent, varName, baseIndent: indentLevel + 1);
                    }
                    else
                    {
                        context?.Log?.AppendLine($"Error: Failed to get path for template '{template}', ignoring this Instance!");
                        errors++;
                    }
                    prefix = Indent(indentLevel + 1);
                    sb.AppendLine();
                    sb.AppendLine(prefix + $"// Apply styles for {varName}");
                    if (styles != null)
                    {
                        if (styles != null)
                        {
                            StyleCodeGen.WriteAssignments(styles, varName, sb, prefix);
                        }
                    }
                    prefix = Indent(indentLevel);
                    sb.AppendLine();
                    if (EndOfTemplate != null)
                    {
                        sb.AppendLine();
                        sb.AppendLine(prefix + $"// end of template {EndOfTemplate.GetName()}(id:{EndOfTemplate.id})");
                        sb.AppendLine(prefix + "}");
                    }
                    return errors;
                default:
                    break;
            }
            if (type != NodeType.UXML && type != NodeType.Instance && type != NodeType.Template && styles != null)
            {
                if (styles != null)
                {
                    context.Log.AppendLine($"{GetName()}: {styles.Count} styles");
                    StyleCodeGen.WriteAssignments(styles, varName, sb, prefix);
                }
            }
            if (!string.IsNullOrEmpty(af))
            {
                sb.AppendLine(prefix + "//User after code");
                sb.AppendLine(prefix + af);
                sb.AppendLine(prefix + "//User after code end");
            }
            sb.AppendLine();
            if (EndOfTemplate != null)
            {

                sb.AppendLine();
                sb.AppendLine(prefix + $"// end of template {EndOfTemplate.GetName()}(id:{EndOfTemplate.id})");

                sb.AppendLine(prefix + "}");
            }
            return errors;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.XTerminal.WebGLSample
{
    /// <summary>
    /// 登録済みコマンドのリファレンスをUI上に表示する
    /// VerticalLayoutGroup配下にコマンド情報のテキスト要素を動的に生成する
    /// </summary>
    public class CommandReferenceView : MonoBehaviour
    {
        [SerializeField] private Sample.XTerminal terminal;
        [SerializeField] private TMP_Text labelTemplate;
        [SerializeField] private RectTransform content;

        [Header("Style")]
        [SerializeField] private float headerFontSize = 28f;
        [SerializeField] private float bodyFontSize = 20f;
        [SerializeField] private Color headerColor = new(0f, 0.83f, 0.67f);
        [SerializeField] private Color commandNameColor = new(0.4f, 0.75f, 1f);
        [SerializeField] private Color optionColor = new(1f, 0.8f, 0.4f);
        [SerializeField] private Color descriptionColor = new(0.78f, 0.78f, 0.78f);

        private readonly List<GameObject> generatedObjects = new();

        private void Start()
        {
            ResolveReferences();
            GenerateReference();
        }

        private void ResolveReferences()
        {
            if (terminal == null)
                terminal = FindAnyObjectByType<Sample.XTerminal>();

            // VerticalLayoutGroup付きのContentを自動検索
            if (content == null)
            {
                var layout = GetComponentInChildren<VerticalLayoutGroup>();
                if (layout != null)
                    content = layout.GetComponent<RectTransform>();
            }

            // ContentSizeFitter配下のTMP_Textをテンプレートとして使用
            if (labelTemplate == null && content != null)
                labelTemplate = content.GetComponentInChildren<TMP_Text>();
        }

        private void GenerateReference()
        {
            if (terminal == null || terminal.Terminal == null)
            {
                Debug.LogError("[CommandReferenceView] Terminal not found.");
                return;
            }

            if (content == null)
            {
                Debug.LogError("[CommandReferenceView] Content RectTransform not found.");
                return;
            }

            ClearGenerated();

            if (labelTemplate != null)
                labelTemplate.gameObject.SetActive(false);

            var registry = terminal.Terminal.Registry;
            var commands = registry.Commands.Values
                .OrderBy(c => c.CommandName)
                .ToList();

            var categories = CategorizeCommands(commands);

            foreach (var category in categories)
            {
                CreateSectionHeader(category.Key);
                foreach (var command in category.Value)
                    CreateCommandEntry(command);
            }
        }

        private List<KeyValuePair<string, List<CommandMetadata>>> CategorizeCommands(List<CommandMetadata> commands)
        {
            var categoryOrder = new[] { "File / Text", "System", "Variables", "Unity", "Other" };
            var map = new Dictionary<string, List<CommandMetadata>>();

            foreach (var cmd in commands)
            {
                var category = GetCategory(cmd);
                if (!map.ContainsKey(category))
                    map[category] = new List<CommandMetadata>();
                map[category].Add(cmd);
            }

            var result = new List<KeyValuePair<string, List<CommandMetadata>>>();
            foreach (var key in categoryOrder)
            {
                if (map.TryGetValue(key, out var list))
                    result.Add(new KeyValuePair<string, List<CommandMetadata>>(key, list));
            }
            return result;
        }

        private string GetCategory(CommandMetadata cmd)
        {
            var ns = cmd.CommandType.Namespace ?? string.Empty;

            if (ns.Contains("BuiltInCommands"))
            {
                var name = cmd.CommandName.ToLower();
                if (name is "set" or "unset" or "env")
                    return "Variables";
                if (name is "help" or "history" or "clear" or "log")
                    return "System";
                return "File / Text";
            }

            if (ns.Contains("UnityCommands"))
                return "Unity";

            return "Other";
        }

        private void CreateSectionHeader(string title)
        {
            var label = CreateLabel();
            label.fontSize = headerFontSize;
            label.fontStyle = FontStyles.Bold;
            label.text = $"<color=#{ColorUtility.ToHtmlStringRGB(headerColor)}>=== {title} ===</color>";
            label.alignment = TextAlignmentOptions.Left;
            label.margin = new Vector4(8, 12, 8, 4);
        }

        private void CreateCommandEntry(CommandMetadata command)
        {
            var sb = new StringBuilder();
            var nameHex = ColorUtility.ToHtmlStringRGB(commandNameColor);
            var optHex = ColorUtility.ToHtmlStringRGB(optionColor);
            var descHex = ColorUtility.ToHtmlStringRGB(descriptionColor);

            sb.Append($"<color=#{nameHex}>{command.CommandName}</color>");
            sb.Append($"  <color=#{descHex}>{command.Description}</color>");

            if (command.Options.Count > 0)
            {
                sb.AppendLine();
                foreach (var opt in command.Options)
                {
                    var shortPart = string.IsNullOrEmpty(opt.ShortName) ? "     " : $" -{opt.ShortName}, ";
                    var typeSuffix = opt.IsBool ? "" : $" <{GetFriendlyTypeName(opt.OptionType)}>";
                    var required = opt.IsRequired ? " *" : "";

                    sb.Append($"  <color=#{optHex}>{shortPart}--{opt.LongName}{typeSuffix}{required}</color>");
                    if (!string.IsNullOrEmpty(opt.Description))
                        sb.Append($"  <color=#{descHex}>{opt.Description}</color>");

                    if (opt != command.Options[command.Options.Count - 1])
                        sb.AppendLine();
                }
            }

            var label = CreateLabel();
            label.fontSize = bodyFontSize;
            label.text = sb.ToString();
            label.alignment = TextAlignmentOptions.TopLeft;
            label.margin = new Vector4(16, 2, 8, 2);
        }

        private TMP_Text CreateLabel()
        {
            var go = new GameObject("ReferenceEntry", typeof(RectTransform));
            go.transform.SetParent(content, false);
            generatedObjects.Add(go);

            var text = go.AddComponent<TextMeshProUGUI>();
            if (labelTemplate != null)
            {
                text.font = labelTemplate.font;
                text.fontSharedMaterial = labelTemplate.fontSharedMaterial;
            }
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;
            text.richText = true;

            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            var le = go.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;

            return text;
        }

        private void ClearGenerated()
        {
            foreach (var go in generatedObjects)
            {
                if (go != null)
                    Destroy(go);
            }
            generatedObjects.Clear();
        }

        private static string GetFriendlyTypeName(System.Type type)
        {
            if (type == typeof(string)) return "string";
            if (type == typeof(int)) return "int";
            if (type == typeof(float)) return "float";
            if (type == typeof(bool)) return "bool";
            if (type.IsEnum) return "enum";
            return type.Name;
        }
    }
}

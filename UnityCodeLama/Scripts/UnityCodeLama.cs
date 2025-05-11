using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace HardCodeDev.UnityCodeLama
{
    public class UnityCodeLama : EditorWindow
    {
        private Vector2 _scrollPosition;

        private Object _selectedScript;

        private string _prompt, _modelName, _response;
        private bool _clearThinking, _canDrawScriptPath;
        private int _selectedPromptIndex;

        private string[] _promptOptions = new[] { "Explain", "Find bugs", "Improve" };

        private List<string> _pathList = new();

        private void OnEnable()
        {
            _clearThinking = false;
            _prompt = string.Empty;
            _modelName = "qwen2.5:3b";
            _response = string.Empty;
            _pathList.Clear();
            _selectedPromptIndex = 0;
        }

        [MenuItem("HardCodeDev/UnityCodeLama")]
        public static void ShowWindow() 
        {
            GetWindow<UnityCodeLama>("UnityCodeLama");
            Debug.Log("<color=green>Some mysterious Unity GUI errors may appear — don't worry, they won't break the editor or this tool.</color>");
        }

        private async void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, true);

            #region For prompt
            EditorGUILayout.LabelField("Script paths", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Add script path")) _canDrawScriptPath = true;

            if (_canDrawScriptPath)
            {
                _selectedScript = EditorGUILayout.ObjectField(
                new GUIContent("Script File", "Select a C# script file from your project"), _selectedScript, typeof(MonoScript), false);

                if (GUILayout.Button("Add script to prompt"))
                {
                    if (_selectedScript != null)
                    {
                        string path = AssetDatabase.GetAssetPath(_selectedScript);
                        path = path.Substring("Assets/".Length);
                        _pathList.Add(path);
                    }
                    else Debug.LogError("Script path is empty!");
                }
            }

            if (_pathList.Count > 0)
            {
                if (GUILayout.Button("Clear script list"))
                {
                    _pathList.Clear();
                    _canDrawScriptPath = false;
                }
                EditorGUILayout.Space(5);
                DrawList(_pathList);
            }
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);

            _modelName = EditorGUILayout.TextField("Model name", _modelName);

            _clearThinking = EditorGUILayout.Toggle(
                new GUIContent("Clear <think>", "If you use thinking models it will disable their thoguhts in the response"),
                _clearThinking);

            EditorGUILayout.Space(15);
            _selectedPromptIndex = EditorGUILayout.Popup("Prompt Profile", _selectedPromptIndex, _promptOptions);

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Prompt", EditorStyles.boldLabel);
            _prompt = EditorGUILayout.TextArea(_prompt, GUILayout.Height(300));

            if (GUILayout.Button("Send to Ollama")) await SendOllamaPrompt();

            #endregion

            #region Ollama's response
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Response", EditorStyles.boldLabel);

            GUI.enabled = false;
            var style = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
            _response = EditorGUILayout.TextArea(_response, style, GUILayout.Height(500));
            GUI.enabled = true;

            if (GUILayout.Button("Copy response")) EditorGUIUtility.systemCopyBuffer = _response;
            if (GUILayout.Button("Clear response")) _response = string.Empty;

            #endregion

            EditorGUILayout.EndScrollView();
        }

        private async Task SendOllamaPrompt()
        {
            try
            {
                var scriptInfo = string.Empty;
                foreach (var path in _pathList)
                {
                    if (string.IsNullOrEmpty(path)) continue;
                    else scriptInfo += ScriptReader.ReadScript(path);
                }
                var promptInstruction = _promptOptions[_selectedPromptIndex] switch
                {
                    "Explain" => "Explain what this code does.",
                    "Find bugs" => "Find any bugs or potential issues in this code.",
                    "Improve" => "Suggest improvements to this code.",
                    _ => ""
                };

                var fullPrompt = $"{promptInstruction}\n{_prompt}. Here's the script(s): {scriptInfo}";
                _response = await OllamaBase.SendMessage(_modelName, fullPrompt, _clearThinking);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"An error occurred while sending the prompt to Ollama: {ex}");
            }
        }

        private void DrawList(List<string> scripts)
        {
            if (scripts != null)
            {
                GUI.enabled = false;
                for (int i = 0; i < scripts.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    scripts[i] = EditorGUILayout.TextField("Script path for Ollama: ", scripts[i]);
                    EditorGUILayout.EndHorizontal();
                }
                GUI.enabled = true;
            }
        }
    }
}
#endif
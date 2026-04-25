using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Widwickyy.SaveSystem
{
    public class SaveFileInspectorWindow : EditorWindow
    {
        private const string WindowTitle = "Save File Inspector";

        private string[] _saveFilePaths = Array.Empty<string>();
        private string[] _saveFileNames = Array.Empty<string>();
        private int _selectedFileIndex = -1;

        private bool _decryptPayload;
        private string _encryptionKey = string.Empty;
        private bool _showRawPayload;

        private string _rawPayload = string.Empty;
        private string _inspectedPayload = string.Empty;
        private string _statusMessage = string.Empty;
        private DateTime _lastWriteTime;
        private long _fileSizeBytes;

        private Vector2 _scroll;

        [MenuItem("Tools/Save System/Save File Inspector")]
        private static void OpenWindow()
        {
            var window = GetWindow<SaveFileInspectorWindow>(WindowTitle);
            window.minSize = new Vector2(700f, 450f);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshFiles();
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space();
            DrawSelection();
            EditorGUILayout.Space();
            DrawOptions();
            EditorGUILayout.Space();
            DrawMetadata();
            EditorGUILayout.Space();
            DrawPayload();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Refresh Files", GUILayout.Width(110f)))
                RefreshFiles();

            using (new EditorGUI.DisabledScope(_selectedFileIndex < 0))
            {
                if (GUILayout.Button("Reload Selected", GUILayout.Width(110f)))
                    LoadSelectedFile();
            }

            if (GUILayout.Button("Open Save Folder", GUILayout.Width(120f)))
                EditorUtility.RevealInFinder(Application.persistentDataPath);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSelection()
        {
            if (_saveFileNames.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    $"No .json save files found in '{Application.persistentDataPath}'.",
                    MessageType.Info);
                return;
            }

            var selectedIndex = EditorGUILayout.Popup("Save File", _selectedFileIndex, _saveFileNames);
            if (selectedIndex != _selectedFileIndex)
            {
                _selectedFileIndex = selectedIndex;
                LoadSelectedFile();
            }

            EditorGUILayout.SelectableLabel(_saveFilePaths[_selectedFileIndex], EditorStyles.textField, GUILayout.Height(18f));
        }

        private void DrawOptions()
        {
            var decryptPayload = EditorGUILayout.ToggleLeft("Decrypt payload (AES)", _decryptPayload);
            if (decryptPayload != _decryptPayload)
            {
                _decryptPayload = decryptPayload;
                RebuildInspectedPayload();
            }

            using (new EditorGUI.DisabledScope(!_decryptPayload))
            {
                EditorGUI.BeginChangeCheck();
                _encryptionKey = EditorGUILayout.PasswordField("Encryption Key", _encryptionKey);
                if (EditorGUI.EndChangeCheck())
                    RebuildInspectedPayload();
            }

            _showRawPayload = EditorGUILayout.ToggleLeft("Show raw payload", _showRawPayload);
        }

        private void DrawMetadata()
        {
            if (_selectedFileIndex < 0)
                return;

            EditorGUILayout.LabelField("Last Modified", _lastWriteTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
            EditorGUILayout.LabelField("File Size", $"{_fileSizeBytes} bytes");

            if (!string.IsNullOrWhiteSpace(_statusMessage))
                EditorGUILayout.HelpBox(_statusMessage, MessageType.Warning);
        }

        private void DrawPayload()
        {
            if (_selectedFileIndex < 0)
                return;

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            if (_showRawPayload)
            {
                EditorGUILayout.LabelField("Raw Payload", EditorStyles.boldLabel);
                EditorGUILayout.TextArea(_rawPayload, GUILayout.ExpandHeight(true));
                EditorGUILayout.Space();
            }

            EditorGUILayout.LabelField("Inspected Payload", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(_inspectedPayload, GUILayout.ExpandHeight(true));

            EditorGUILayout.EndScrollView();
        }

        private void RefreshFiles()
        {
            var persistentPath = Application.persistentDataPath;

            if (!Directory.Exists(persistentPath))
                Directory.CreateDirectory(persistentPath);

            var previousPath = _selectedFileIndex >= 0 && _selectedFileIndex < _saveFilePaths.Length
                ? _saveFilePaths[_selectedFileIndex]
                : null;

            _saveFilePaths = Directory
                .GetFiles(persistentPath, "*.json", SearchOption.TopDirectoryOnly)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .ToArray();

            _saveFileNames = _saveFilePaths
                .Select(path => Path.GetFileNameWithoutExtension(path))
                .ToArray();

            if (_saveFilePaths.Length == 0)
            {
                _selectedFileIndex = -1;
                _rawPayload = string.Empty;
                _inspectedPayload = string.Empty;
                _statusMessage = string.Empty;
                return;
            }

            _selectedFileIndex = ResolveSelectionIndex(previousPath);
            LoadSelectedFile();
        }

        private int ResolveSelectionIndex(string previousPath)
        {
            if (!string.IsNullOrWhiteSpace(previousPath))
            {
                var index = Array.IndexOf(_saveFilePaths, previousPath);
                if (index >= 0)
                    return index;
            }

            return 0;
        }

        private void LoadSelectedFile()
        {
            if (_selectedFileIndex < 0 || _selectedFileIndex >= _saveFilePaths.Length)
                return;

            var path = _saveFilePaths[_selectedFileIndex];
            var fileInfo = new FileInfo(path);

            _rawPayload = File.ReadAllText(path);
            _fileSizeBytes = fileInfo.Length;
            _lastWriteTime = fileInfo.LastWriteTimeUtc;

            RebuildInspectedPayload();
        }

        private void RebuildInspectedPayload()
        {
            _statusMessage = string.Empty;
            var payload = _rawPayload;

            if (_decryptPayload)
            {
                if (string.IsNullOrWhiteSpace(_encryptionKey))
                {
                    _inspectedPayload = string.Empty;
                    _statusMessage = "Enter an encryption key to decrypt this payload.";
                    return;
                }

                try
                {
                    payload = new AesStringCipher(_encryptionKey).Decrypt(payload);
                }
                catch (Exception ex)
                {
                    _inspectedPayload = string.Empty;
                    _statusMessage = $"Decryption failed: {ex.Message}";
                    return;
                }
            }

            _inspectedPayload = TryFormatJson(payload);
        }

        private static string TryFormatJson(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            try
            {
                var token = JToken.Parse(text);
                return token.ToString(Formatting.Indented);
            }
            catch
            {
                return text;
            }
        }
    }
}

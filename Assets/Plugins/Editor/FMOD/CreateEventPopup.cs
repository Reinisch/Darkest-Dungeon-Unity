using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace FMODUnity
{
    class CreateEventPopup : EditorWindow
    {        
        class FolderEntry
        {
            public FolderEntry parent;
            public string name;
            public string guid;
            public List<FolderEntry> entries = new List<FolderEntry>();
            public Rect rect;
        }

        SerializedProperty outputProperty;
        internal void SelectEvent(SerializedProperty property)
        {
            outputProperty = property;
        }

        class BankEntry
        {
            public string name;
            public string guid;
        }

        FolderEntry rootFolder;
        FolderEntry currentFolder;
        List<BankEntry> banks;

        public CreateEventPopup()
        {
        }
        
        private void BuildTree()
        {
            var rootGuid = EditorUtils.GetScriptOutput("studio.project.workspace.masterEventFolder.id");
            rootFolder = new FolderEntry();
            rootFolder.guid = rootGuid;
            BuildTreeItem(rootFolder);
            wantsMouseMove = true;
            banks = new List<BankEntry>();

            EditorUtils.GetScriptOutput("children = \"\";");
            EditorUtils.GetScriptOutput("func = function(val) {{ children += \",\" + val.id + val.name; }};");
            EditorUtils.GetScriptOutput("studio.project.workspace.masterBankFolder.items.forEach(func, this); ");
            string bankList = EditorUtils.GetScriptOutput("children;");
            string[] bankListSplit = bankList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var bank in bankListSplit)
            {
                var entry = new BankEntry();
                entry.guid = bank.Substring(0, 38);
                entry.name = bank.Substring(38);
                banks.Add(entry);
            }

            banks.Sort((a, b) => a.name.CompareTo(b.name));
        }
        
        private void BuildTreeItem(FolderEntry entry)
        {
            // lookup the entry
            EditorUtils.GetScriptOutput(string.Format("cur = studio.project.lookup(\"{0}\");", entry.guid));

            // get child count
            string itemCountString = EditorUtils.GetScriptOutput("cur.items.length;");
            int itemCount;
            Int32.TryParse(itemCountString, out itemCount);
            
            // iterate children looking for folder
            for (int item = 0; item < itemCount; item++)
            {
                EditorUtils.GetScriptOutput(String.Format("child = cur.items[{0}]", item));
                
                // check if it's a folder
                string isFolder = EditorUtils.GetScriptOutput("child.isOfExactType(\"EventFolder\")");
                if (isFolder == "false")
                {
                    continue;
                }

                // Get guid and name
                string info = EditorUtils.GetScriptOutput("child.id + child.name");

                var childEntry = new FolderEntry();
                childEntry.guid = info.Substring(0, 38);
                childEntry.name = info.Substring(38);
                childEntry.parent = entry;
                entry.entries.Add(childEntry);
            }

            // Recurse for child entries
            foreach(var childEntry in entry.entries)
            {
                BuildTreeItem(childEntry);
            }
        }

        int lastHover = 0;
        string eventFolder = "/";
        string eventName = "";
        string currentFilter = "";
        int selectedBank = 0;
        bool resetCursor = true;
        Vector2 scrollPos = new Vector2();
        Rect scrollRect = new Rect();
        bool isConnected = false;
        
        public void OnGUI()
        {
            var borderIcon = EditorGUIUtility.Load("FMOD/Border.png") as Texture2D;
            var border = new GUIStyle(GUI.skin.box);
            border.normal.background = borderIcon;
            GUI.Box(new Rect(1, 1, position.width - 1, position.height - 1), GUIContent.none, border);

            if (Event.current.type == EventType.Layout)
            {
                isConnected = EditorUtils.IsConnectedToStudio();
            }

            if (!isConnected)
            {
                this.ShowNotification(new GUIContent("FMOD Studio not running"));
                return;
            }

            this.RemoveNotification();

            if (rootFolder == null)
            {
                BuildTree();
                currentFolder = rootFolder;
            }

            var arrowIcon = EditorGUIUtility.Load("FMOD/ArrowIcon.png") as Texture;
            var hoverIcon = EditorGUIUtility.Load("FMOD/SelectedAlt.png") as Texture2D;
            var titleIcon = EditorGUIUtility.Load("IN BigTitle") as Texture2D;
            
    
            var nextEntry = currentFolder;

            var filteredEntries = currentFolder.entries.FindAll((x) => x.name.StartsWith(currentFilter, StringComparison.CurrentCultureIgnoreCase));
            

            // Process key strokes for the folder list
            {
                if (Event.current.keyCode == KeyCode.UpArrow)
                {
                    if (Event.current.type == EventType.KeyDown)
                    {
                        lastHover = Math.Max(lastHover - 1, 0);
                        if (filteredEntries[lastHover].rect.y < scrollPos.y)
                        {
                            scrollPos.y = filteredEntries[lastHover].rect.y;
                        }
                    }
                    Event.current.Use();
                }
                if (Event.current.keyCode == KeyCode.DownArrow)
                {
                    if (Event.current.type == EventType.KeyDown)
                    { 
                        lastHover = Math.Min(lastHover + 1, filteredEntries.Count - 1);
                        if (filteredEntries[lastHover].rect.y + filteredEntries[lastHover].rect.height > scrollPos.y + scrollRect.height)
                        {
                            scrollPos.y = filteredEntries[lastHover].rect.y - scrollRect.height + filteredEntries[lastHover].rect.height * 2;
                        }
                    }
                    Event.current.Use();
                }
                if (Event.current.keyCode == KeyCode.RightArrow)
                {
                    if (Event.current.type == EventType.KeyDown)
                        nextEntry = filteredEntries[lastHover];
                    Event.current.Use();
                }
                if (Event.current.keyCode == KeyCode.LeftArrow)
                {
                    if (Event.current.type == EventType.KeyDown)
                        if (currentFolder.parent != null)
                            nextEntry = currentFolder.parent;
                    Event.current.Use();
                }
            }

            
            bool disabled = eventName.Length == 0;
            EditorGUI.BeginDisabledGroup(disabled);
            if (GUILayout.Button("Create Event"))
            {
                CreateEventInStudio();
                this.Close();
            }
            EditorGUI.EndDisabledGroup();

            {
                GUI.SetNextControlName("name");
                
                EditorGUILayout.LabelField("Name");
                eventName = EditorGUILayout.TextField(eventName);
            }

            {
                EditorGUILayout.LabelField("Bank");
                selectedBank = EditorGUILayout.Popup(selectedBank, banks.Select(x => x.name).ToArray());
            }

            bool updateEventPath = false;
            {
                GUI.SetNextControlName("folder");
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Path");
                eventFolder = GUILayout.TextField(eventFolder);
                if (EditorGUI.EndChangeCheck())
                {
                    updateEventPath = true;
                }
            }
            
            if (resetCursor)
            {
                resetCursor = false;

                var textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                if (textEditor != null)
                {
                    textEditor.MoveCursorToPosition(new Vector2(9999, 9999));
                }
            }

            // Draw the current folder as a title bar, click to go back one level
            {
                Rect currentRect = EditorGUILayout.GetControlRect();
                
                var bg = new GUIStyle(GUI.skin.box);
                bg.normal.background = titleIcon;
                Rect bgRect = new Rect(currentRect);
                bgRect.x = 2;
                bgRect.width = position.width-4;
                GUI.Box(bgRect, GUIContent.none, bg);


                Rect textureRect = currentRect;
                textureRect.width = arrowIcon.width;
                if (currentFolder.name != null)
                {
                    GUI.DrawTextureWithTexCoords(textureRect, arrowIcon, new Rect(1, 1, -1, -1));
                }


                Rect labelRect = currentRect;
                labelRect.x += arrowIcon.width + 50;
                labelRect.width -= arrowIcon.width + 50;
                GUI.Label(labelRect, currentFolder.name != null ? currentFolder.name : "Folders", EditorStyles.boldLabel);

                if (Event.current.type == EventType.MouseDown && currentRect.Contains(Event.current.mousePosition) &&
                    currentFolder.parent != null)
                {
                    nextEntry = currentFolder.parent;
                    Event.current.Use();
                }
            }
            
            var normal = new GUIStyle(GUI.skin.label);
            normal.padding.left = 14;
            var hover = new GUIStyle(normal);
            hover.normal.background = hoverIcon;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
            
            for (int i = 0; i < filteredEntries.Count; i++)
            {
                var entry = filteredEntries[i];
                var content = new GUIContent(entry.name);
                var rect = EditorGUILayout.GetControlRect();
                if ((rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseMove) || i == lastHover)
                {
                    lastHover = i;
                    
                    GUI.Label(rect, content, hover);
                    if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                    {
                        nextEntry = entry;
                    }                    
                }
                else
                {
                    GUI.Label(rect, content, normal);
                }

                Rect textureRect = rect;
                textureRect.x = textureRect.width - arrowIcon.width;
                textureRect.width = arrowIcon.width;
                GUI.DrawTexture(textureRect, arrowIcon);

                if (Event.current.type == EventType.Repaint)
                {
                    entry.rect = rect;
                }
            }
            EditorGUILayout.EndScrollView();

            if (Event.current.type == EventType.Repaint)
            {
                scrollRect = GUILayoutUtility.GetLastRect();
            }

            if (currentFolder != nextEntry)
            {
                lastHover = 0;
                currentFolder = nextEntry;
                UpdateTextFromList();
                Repaint();
            }

            if (updateEventPath)
            {
                UpdateListFromText();
            }            

            if (Event.current.type == EventType.MouseMove)
            {
                Repaint();
            }
            
        }        

        private void CreateEventInStudio()
        {
            string eventGuid = EditorUtils.CreateStudioEvent(eventFolder, eventName);

            if (!string.IsNullOrEmpty(eventGuid))
            {
                EditorUtils.GetScriptOutput(String.Format("studio.project.lookup(\"{0}\").relationships.banks.add(studio.project.lookup(\"{1}\"));", eventGuid, banks[selectedBank].guid));
                EditorUtils.GetScriptOutput("studio.project.build();");

                string fullPath = "event:" + eventFolder + eventName;
                outputProperty.stringValue = fullPath;
                EditorUtils.UpdateParamsOnEmitter(outputProperty.serializedObject, fullPath);
                outputProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        private void UpdateListFromText()
        {
            int endFolders = eventFolder.LastIndexOf("/");
            currentFilter = eventFolder.Substring(endFolders + 1);

            var folders = eventFolder.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            FolderEntry entry = rootFolder;
            int i;
            for (i = 0; i < folders.Length; i++)
            {
                var newEntry = entry.entries.Find((x) => x.name.Equals(folders[i], StringComparison.CurrentCultureIgnoreCase));
                if (newEntry == null)
                {
                    break;
                }
                entry = newEntry;
            }
            currentFolder = entry;

            // Treat an exact filter match as being in that folder and clear the filter
            if (entry.name != null && entry.name.Equals(currentFilter, StringComparison.CurrentCultureIgnoreCase))
            {
                currentFilter = "";
            }
        }

        private void UpdateTextFromList()
        {
            string path = "";
            var entry = currentFolder;
            while (entry.parent != null)
            {
                path = entry.name + "/" + path;
                entry = entry.parent;
            }

            eventFolder = "/" + path;
            resetCursor = true;
            currentFilter = "";
        }
    }
}
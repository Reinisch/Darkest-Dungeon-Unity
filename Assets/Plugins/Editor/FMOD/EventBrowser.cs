using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace FMODUnity
{
    class EventBrowser : EditorWindow, ISerializationCallbackReceiver
    {
        [MenuItem("FMOD/Event Browser", priority = 1)]
        public static void ShowEventBrowser()
        {
            EventBrowser eventBrowser = EditorWindow.GetWindow<EventBrowser>("FMOD Events");
            eventBrowser.minSize = new Vector2(280, 600);
            eventBrowser.Show();
        }

        void OnDestroy()
        {
            EditorUtils.PreviewStop();
        }

        List<bool> expandedState;
        int selectedIndex = -1;
        public void OnBeforeSerialize()
        {
            expandedState = new List<bool>();
            Action<TreeItem> isExpanded = null;
            isExpanded = (x) => { selectedIndex = (x == selectedItem) ? expandedState.Count : selectedIndex; expandedState.Add(x.Expanded); x.Children.ForEach(isExpanded); };
            treeItems.ForEach(isExpanded);
        }

        public void OnAfterDeserialize()
        {
        }

        class TreeItem
        {
            public string Name;
            public bool Expanded = false;
            public EditorEventRef EventRef = null;
            public EditorBankRef BankRef = null;
            public List<TreeItem> Children = new List<TreeItem>();
            public TreeItem Next = null;
            public TreeItem Prev = null;
            public Rect Rect;
            public bool Exists;
        }

        [NonSerialized]
        List<TreeItem> treeItems;

        Texture searchIcon;
        Texture eventIcon;
        Texture folderOpenIcon;
        Texture folderClosedIcon;
        Texture bankIcon;
        Texture snapshotIcon;
        Texture2D borderIcon;
        GUIStyle eventStyle;

        [NonSerialized]
        TreeItem selectedItem = null;

        Dictionary<string, float> previewParamValues = new Dictionary<string, float>();
        float previewDistance = 0;
        float previewOrientation = 0;

        string searchString = "";

        private SerializedProperty outputProperty;

        bool fromInspector = false;
        bool showEvents = true;
        bool showBanks = true;

        Vector2 treeScroll;
        Vector2 paramScroll;

        [NonSerialized]
        TreeItem lastDrawnItem;
        [NonSerialized]
        int itemCount;
        [NonSerialized]
        bool forceRepaint;
        [NonSerialized]
        float lastRepaintTime;

        void Update()
        {
            if (forceRepaint && lastRepaintTime < Time.time + (1/30.0f))
            {
                Repaint();
                lastRepaintTime = Time.time;
            }
        }

        void SetPreviewEvent(EditorEventRef eventRef)
        {
            forceRepaint = false;
            EditorUtils.PreviewStop();
            previewParamValues.Clear();

            previewDistance = 0;
            previewOrientation = 0;

            if (eventRef != null)
            {
                foreach (var paramRef in eventRef.Parameters)
                {
                    previewParamValues.Add(paramRef.Name, paramRef.Default);
                }
            }
            eventPosition = new Vector2(0, 0);
        }

        void SetSelectedItem(TreeItem item)
        {
            //if (item != selectedItem)
            {
                selectedItem = item;
                
                if (item != null)
                {
                    SetPreviewEvent(item.EventRef);
                }
                else
                {
                    SetPreviewEvent(null);
                }
            }
        }

        void ShowEventFolder(TreeItem item, Predicate<TreeItem> filter)
        {
            eventStyle.padding.left += 17;
            
            {
                // Highlight first found item
                if (item.EventRef != null || item.BankRef != null)
                {
                    if (!String.IsNullOrEmpty(searchString) &&
                        itemCount == 0 &&
                        selectedItem == null                       
                        )
                    {
                        SetSelectedItem(item);
                    }

                    itemCount++;
                }

                item.Next = null;
                item.Prev = lastDrawnItem;
                if (lastDrawnItem != null)
                {
                    lastDrawnItem.Next = item;
                }
                lastDrawnItem = item;
            }

            if (item.EventRef != null)
            {
                // Rendering and GUI event handling to show an event
                GUIContent content = new GUIContent(item.Name, item.EventRef.Path.StartsWith("snapshot") ? snapshotIcon : eventIcon);

                eventStyle.normal.background = selectedItem == item ? EditorGUIUtility.Load("FMOD/Selected.png") as Texture2D : null;
                GUILayout.Label(content, eventStyle, GUILayout.ExpandWidth(true));
                
                Event e = Event.current;

                Rect rect = GUILayoutUtility.GetLastRect();
                if (e.type == EventType.MouseDown &&
                    e.button == 0 &&
                    rect.Contains(e.mousePosition))
                {
                    e.Use();

                    if (fromInspector && e.clickCount >= 2)
                    {
                        outputProperty.stringValue = "";
                        outputProperty.stringValue = item.EventRef.Path;
                        EditorUtils.UpdateParamsOnEmitter(outputProperty.serializedObject, item.EventRef.Path);
                        outputProperty.serializedObject.ApplyModifiedProperties();
                        
                        Close();
                    }

                    SetSelectedItem(item);
                }
                if (e.type == EventType.mouseDrag && rect.Contains(e.mousePosition) && !fromInspector)
                {
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new UnityEngine.Object[] { ScriptableObject.Instantiate(item.EventRef) };
                    DragAndDrop.StartDrag("New FMOD Studio Emitter");
                    e.Use();
                }
                if (Event.current.type == EventType.Repaint)
                {
                    item.Rect = rect;
                }
            }
            else if (item.BankRef != null)
            {
                // Rendering and event handling for a bank
                GUIContent content = new GUIContent(item.Name, bankIcon);

                eventStyle.normal.background = selectedItem == item ? EditorGUIUtility.Load("FMOD/Selected.png") as Texture2D : null;
                GUILayout.Label(content, eventStyle, GUILayout.ExpandWidth(true));

                Event e = Event.current;

                Rect rect = GUILayoutUtility.GetLastRect();
                if (e.type == EventType.MouseDown &&
                    e.button == 0 &&
                    rect.Contains(e.mousePosition))
                {
                    e.Use();

                    if (fromInspector && e.clickCount >= 2)
                    {
                        outputProperty.stringValue = item.BankRef.Name;
                        outputProperty.serializedObject.ApplyModifiedProperties();
                        Close();
                    }

                    SetSelectedItem(item);
                }
                if (e.type == EventType.mouseDrag && rect.Contains(e.mousePosition) && !fromInspector)
                {
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new UnityEngine.Object[] { ScriptableObject.Instantiate(item.BankRef) };
                    DragAndDrop.StartDrag("New FMOD Studio Bank Loader");
                    e.Use();
                }
                if (Event.current.type == EventType.Repaint)
                {
                    item.Rect = rect;
                }
            }
            else
            {
                eventStyle.normal.background = selectedItem == item ? EditorGUIUtility.Load("FMOD/Selected.png") as Texture2D : null;

                bool expanded = item.Expanded || !string.IsNullOrEmpty(searchString);
                GUIContent content = new GUIContent(item.Name, expanded ? folderOpenIcon : folderClosedIcon);
                GUILayout.Label(content, eventStyle, GUILayout.ExpandWidth(true));

                Rect rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.MouseDown && 
                    Event.current.button == 0 && 
                    rect.Contains(Event.current.mousePosition))
                {
                    Event.current.Use();
                    item.Expanded = !item.Expanded;
                    SetSelectedItem(item);
                }
                
                if (item.Expanded || !string.IsNullOrEmpty(searchString))
                {
                    if (item.Name.ToLower().Contains(searchString.ToLower()))
                    {
                        foreach(var childFolder in item.Children)
                        {
                            ShowEventFolder(childFolder, (x) => true);
                        }
                    }
                    else
                    {                        
                        foreach (var childFolder in item.Children.FindAll(filter))
                        {
                            ShowEventFolder(childFolder, filter);
                        }
                    }
                }

                if (Event.current.type == EventType.Repaint)
                {
                    item.Rect = rect;
                }
            }
            eventStyle.padding.left -= 17;
        }

        Vector2 eventPosition;

        void OnGUI()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                // Brute force hack to stop us calling DLL functions while Unity is starting up
                // playing in editor mode and will cause us to leak system objects
                this.ShowNotification(new GUIContent("Playing In Editor Starting"));
                return;
            }

            if (!EventManager.IsValid)
            {
                this.ShowNotification(new GUIContent("No FMOD Studio banks loaded. Please check your settings."));
                return;
            }

            if (Event.current.type == EventType.Layout)
            {
                RebuildDisplayFromCache();
            }

            //if (eventStyle == null)
            {
                eventStyle = new GUIStyle(GUI.skin.button);
                eventStyle.normal.background = null;
                eventStyle.focused.background = null;
                eventStyle.active.background = null;
                eventStyle.onFocused.background = null;
                eventStyle.onNormal.background = null;
                eventStyle.onHover.background = null;
                eventStyle.onActive.background = null;
                eventStyle.stretchWidth = false;
                eventStyle.padding.left = 0;
                eventStyle.stretchHeight = false;
                eventStyle.fixedHeight = eventStyle.lineHeight + eventStyle.margin.top + eventStyle.margin.bottom;
                eventStyle.alignment = TextAnchor.MiddleLeft;

                eventIcon = EditorGUIUtility.Load("FMOD/EventIcon.png") as Texture;
                folderOpenIcon = EditorGUIUtility.Load("FMOD/FolderIconOpen.png") as Texture;
                folderClosedIcon = EditorGUIUtility.Load("FMOD/FolderIconClosed.png") as Texture;
                searchIcon = EditorGUIUtility.Load("FMOD/SearchIcon.png") as Texture;
                bankIcon = EditorGUIUtility.Load("FMOD/BankIcon.png") as Texture;
                snapshotIcon = EditorGUIUtility.Load("FMOD/SnapshotIcon.png") as Texture;
                borderIcon = EditorGUIUtility.Load("FMOD/Border.png") as Texture2D;
            }

            if (fromInspector)
            {
                var border = new GUIStyle(GUI.skin.box);
                border.normal.background = borderIcon;
                GUI.Box(new Rect(1, 1, position.width - 1, position.height - 1), GUIContent.none, border);
            }

            // Split the window int search box, tree view, preview pane (only if full browser)
            Rect searchRect, listRect, previewRect;
            SplitWindow(out searchRect, out listRect, out previewRect);

            // Scroll the selected item in the tree view - put above the search box otherwise it will take
            // our key presses
            if (selectedItem != null && Event.current.type == EventType.keyDown)
            {
                if (Event.current.keyCode == KeyCode.UpArrow)
                {
                    if (selectedItem.Prev != null)
                    {
                        SetSelectedItem(selectedItem.Prev);

                        // make sure it's visible
                        if (selectedItem.Rect.y < treeScroll.y)
                        {
                            treeScroll.y = selectedItem.Rect.y;
                        }
                    }
                    Event.current.Use();
                }
                if (Event.current.keyCode == KeyCode.DownArrow)
                {
                    if (selectedItem.Next != null)
                    {
                        SetSelectedItem(selectedItem.Next);
                        // make sure it's visible
                        if (selectedItem.Rect.y + selectedItem.Rect.height > treeScroll.y + listRect.height)
                        {
                            treeScroll.y += (selectedItem.Rect.y + selectedItem.Rect.height) - listRect.height;
                        }
                    }
                    Event.current.Use();
                }
                if (Event.current.keyCode == KeyCode.RightArrow)
                {
                    selectedItem.Expanded = true;
                    Event.current.Use();
                }
                if (Event.current.keyCode == KeyCode.LeftArrow)
                {
                    selectedItem.Expanded = false;
                    Event.current.Use();
                }
            }

            // Show the search box at the top
            GUILayout.BeginArea(searchRect);
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(searchIcon), GUILayout.ExpandWidth(false));
            GUI.SetNextControlName("SearchBox");
            searchString = EditorGUILayout.TextField(searchString);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            if (fromInspector)
            {
                EditorGUI.FocusTextInControl("SearchBox");

                if (selectedItem != null && Event.current.isKey && Event.current.keyCode == KeyCode.Return
                    && !(selectedItem.EventRef == null && selectedItem.BankRef == null))
                {
                    Event.current.Use();

                    if (selectedItem.EventRef != null)
                    {
                        outputProperty.stringValue = "";
                        outputProperty.stringValue = selectedItem.EventRef.Path;
                        EditorUtils.UpdateParamsOnEmitter(outputProperty.serializedObject, selectedItem.EventRef.Path);
                    }
                    else
                    {
                        outputProperty.stringValue = selectedItem.BankRef.Name;
                    }
                    outputProperty.serializedObject.ApplyModifiedProperties();
                    Close();
                }
                if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape)
                {
                    Close();
                }
            }

            // Show the tree view

            Predicate<TreeItem> searchFilter = null;
            searchFilter = (x) => (x.Name.ToLower().Contains(searchString.ToLower()) || x.Children.Exists(searchFilter));

            // Check if our selected item still matches the search string
            if (selectedItem != null && !String.IsNullOrEmpty(searchString) && selectedItem.Children.Count == 0)
            {
                Predicate<TreeItem> containsSelected = null;
                containsSelected = (x) => (x == selectedItem || x.Children.Exists(containsSelected));
                Predicate<TreeItem> matchForSelected = null;
                matchForSelected = (x) => (x.Name.ToLower().Contains(searchString.ToLower()) && (x == selectedItem || x.Children.Exists(containsSelected))) || x.Children.Exists(matchForSelected);
                if (!treeItems.Exists(matchForSelected))
                {
                  SetSelectedItem(null);
                }
            }

            GUILayout.BeginArea(listRect);
            treeScroll = GUILayout.BeginScrollView(treeScroll, GUILayout.ExpandHeight(true));

            lastDrawnItem = null;
            itemCount = 0;

            if (showEvents)
            {
                treeItems[0].Expanded = fromInspector ? true : treeItems[0].Expanded;
                ShowEventFolder(treeItems[0], searchFilter);
                ShowEventFolder(treeItems[1], searchFilter);
            }
            if (showBanks)
            {
                treeItems[2].Expanded = fromInspector ? true : treeItems[2].Expanded;
                ShowEventFolder(treeItems[2], searchFilter);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            // If the standalone event browser show a preview of the selected item
            if (!fromInspector)
            {

                GUI.Box(previewRect, GUIContent.none);

                if (selectedItem != null && selectedItem.EventRef != null && selectedItem.EventRef.Path.StartsWith("event:"))
                {
                    PreviewEvent(previewRect, selectedItem.EventRef);
                }

                if (selectedItem != null && selectedItem.EventRef != null && selectedItem.EventRef.Path.StartsWith("snapshot:"))
                {
                    PreviewSnapshot(previewRect, selectedItem.EventRef);
                }

                if (selectedItem != null && selectedItem.BankRef != null)
                {
                    PreviewBank(previewRect, selectedItem.BankRef);
                }
            }
        }

        private void SplitWindow(out Rect searchRect, out Rect listRect, out Rect previewRect)
        {
            searchRect = new Rect(4, 4, position.width - 8, 16);
            float previewBoxHeight = fromInspector ? 0 : 300;
            listRect = new Rect(0, searchRect.height + 6, position.width, position.height - previewBoxHeight - searchRect.height - 15);
            previewRect = new Rect(0, position.height - previewBoxHeight, position.width, previewBoxHeight);
        }

        Rect previewCustomRect;
        Rect previewPathRect;

        private void PreviewEvent(Rect previewRect, EditorEventRef selectedEvent)
        {
            GUILayout.BeginArea(previewRect);

            bool isNarrow = previewRect.width < 400;

            var style = new GUIStyle(EditorStyles.label);
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorGUIUtility.labelWidth = 75;

            var copyIcon = EditorGUIUtility.Load("FMOD/CopyIcon.png") as Texture;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Full Path", selectedEvent.Path, style, GUILayout.ExpandWidth(true));
            if (GUILayout.Button(copyIcon, GUILayout.ExpandWidth(false)))
            {
                EditorGUIUtility.systemCopyBuffer = selectedEvent.Path;
            }
            EditorGUILayout.EndHorizontal();

            

            StringBuilder builder = new StringBuilder();
            selectedEvent.Banks.ForEach((x) => { builder.Append(Path.GetFileNameWithoutExtension(x.Path)); builder.Append(", "); });
            EditorGUILayout.LabelField("Banks", builder.ToString(0, Math.Max(0, builder.Length - 2)), style);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Panning", selectedEvent.Is3D ? "3D" : "2D", style);
            EditorGUILayout.LabelField("Oneshot", selectedEvent.IsOneShot.ToString(), style);
            if (!isNarrow) EditorGUILayout.LabelField("Streaming", selectedEvent.IsStream.ToString(), style);
            EditorGUILayout.EndHorizontal();
            if (isNarrow) EditorGUILayout.LabelField("Streaming", selectedEvent.IsStream.ToString(), style);

            EditorGUIUtility.labelWidth = 0;
            EditorStyles.label.fontStyle = FontStyle.Normal;            

            if (Event.current.type == EventType.Repaint)
            {
                var lastRect = GUILayoutUtility.GetLastRect();
                if (!isNarrow)
                {
                    previewCustomRect = new Rect(lastRect.width / 2 - 200, lastRect.yMax + 10, 400, 150);
                }
                else
                {
                    previewCustomRect = new Rect(lastRect.width / 2 - 130, lastRect.yMax + 10, 260, 150);
                }
            }
            
            GUI.Box(new Rect(0, previewCustomRect.yMin, previewRect.width, 1), GUIContent.none);
            GUI.Box(new Rect(0, previewCustomRect.yMax, previewRect.width, 1), GUIContent.none);

            GUILayout.BeginArea(previewCustomRect);
            
            Texture playOff = EditorGUIUtility.Load("FMOD/TransportPlayButtonOff.png") as Texture;
            Texture playOn = EditorGUIUtility.Load("FMOD/TransportPlayButtonOn.png") as Texture;
            Texture stopOff = EditorGUIUtility.Load("FMOD/TransportStopButtonOff.png") as Texture;
            Texture stopOn = EditorGUIUtility.Load("FMOD/TransportStopButtonOn.png") as Texture;
            Texture openIcon = EditorGUIUtility.Load("FMOD/transportOpen.png") as Texture;

            var transportButtonStyle = new GUIStyle();
            transportButtonStyle.padding.left = 4;
            transportButtonStyle.padding.top = 10;

            var previewState = EditorUtils.PreviewState;
            bool playing = previewState == PreviewState.Playing;
            bool paused = previewState == PreviewState.Paused;
            bool stopped = previewState == PreviewState.Stopped;
            EditorGUILayout.BeginVertical();
            if (!isNarrow) GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(stopped || paused ? stopOn : stopOff, transportButtonStyle, GUILayout.ExpandWidth(false)))
            {
                forceRepaint = false;
                if (paused)
                {
                    EditorUtils.PreviewStop();
                }
                if (playing)
                {
                    EditorUtils.PreviewPause();
                }
            }
            if (GUILayout.Button(playing ? playOn : playOff, transportButtonStyle, GUILayout.ExpandWidth(false)))
            {
                if (playing || stopped)
                {
                    EditorUtils.PreviewEvent(selectedEvent);
                }
                else
                {
                    EditorUtils.PreviewPause();
                }
                forceRepaint = true;
            }
            if (GUILayout.Button(new GUIContent(openIcon, "Show Event in FMOD Studio"), transportButtonStyle, GUILayout.ExpandWidth(false)))
            {
                string cmd = string.Format("studio.window.navigateTo(studio.project.lookup(\"{0}\"))", selectedEvent.Guid.ToString("b"));
                EditorUtils.SendScriptCommand(cmd);
            }


            EditorGUILayout.EndHorizontal();
            if (!isNarrow) GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
            
            {
                Texture circle = EditorGUIUtility.Load("FMOD/preview.png") as Texture;
                Texture circle2 = EditorGUIUtility.Load("FMOD/previewemitter.png") as Texture;

                var originalColour = GUI.color;
                if (!selectedEvent.Is3D)
                {
                    GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);
                }
                
                Rect rect = new Rect(isNarrow ? 120 : 150, 10, 128, 128);
                GUI.DrawTexture(rect, circle);

                Vector2 centre = rect.center;
                Rect rect2 = new Rect(rect.center.x + eventPosition.x - 6, rect.center.y + eventPosition.y - 6, 12, 12);
                GUI.DrawTexture(rect2, circle2);

                GUI.color = originalColour;


                if (selectedEvent.Is3D && (Event.current.type == EventType.mouseDown || Event.current.type == EventType.mouseDrag) && rect.Contains(Event.current.mousePosition))
                {
                    var newPosition = Event.current.mousePosition;
                    Vector2 delta = (newPosition - centre);
                    float distance = delta.magnitude;
                    if (distance < 60)
                    {
                        eventPosition = newPosition - rect.center;
                        previewDistance = distance / 60.0f * selectedEvent.MaxDistance;
                        delta.Normalize();
                        float angle = Mathf.Atan2(delta.y, delta.x);
                        previewOrientation = angle + Mathf.PI * 0.5f;
                    }
                    Event.current.Use();
                }

                EditorUtils.PreviewUpdatePosition(previewDistance, previewOrientation);
            }


            float hoffset = isNarrow ? 15 : 300;
            float voffset = isNarrow ? 50 : 10;
            Texture meterOn = EditorGUIUtility.Load("FMOD/LevelMeter.png") as Texture;
            Texture meterOff = EditorGUIUtility.Load("FMOD/LevelMeterOff.png") as Texture;
            float[] metering = EditorUtils.GetMetering();
            int meterHeight = isNarrow ? 86 : 128;
            int meterWidth = (int)((128 / (float)meterOff.height) * meterOff.width);
            foreach (float rms in metering)
            {
                GUI.DrawTexture(new Rect(hoffset, voffset, meterWidth, meterHeight), meterOff);
                
                float db = 20.0f * Mathf.Log10(rms * Mathf.Sqrt(2.0f));
                db = Mathf.Clamp(db, -80.0f, 10.0f);
                float visible = 0;
                int[] segmentPixels = new int[] { 0, 18, 38, 60, 89, 130, 187, 244, 300 };
                float[] segmentDB = new float[] { -80.0f, -60.0f, -50.0f, -40.0f, -30.0f, -20.0f, -10.0f, 0, 10.0f };
                int segment = 1;
                while (segmentDB[segment] < db)
                {
                    segment++;
                }
                visible = segmentPixels[segment - 1] + ((db - segmentDB[segment - 1]) / (segmentDB[segment] - segmentDB[segment - 1])) * (segmentPixels[segment] - segmentPixels[segment - 1]);

                visible *= meterHeight / (float)meterOff.height;

                Rect levelPosRect = new Rect(hoffset, meterHeight - visible + voffset, meterWidth, visible);
                Rect levelUVRect = new Rect(0, 0, 1.0f, visible / meterHeight);
                GUI.DrawTextureWithTexCoords(levelPosRect, meterOn, levelUVRect);
                hoffset += meterWidth + 5.0f;
            }

            GUILayout.EndArea();
            
            Rect paramRect = new Rect(0, previewCustomRect.yMax + 10, previewRect.width, previewRect.height - (previewCustomRect.yMax + 10));
            GUILayout.BeginArea(paramRect);

            paramScroll = GUILayout.BeginScrollView(paramScroll, false, false);
            foreach (var paramRef in selectedEvent.Parameters)
            {
                if (!previewParamValues.ContainsKey(paramRef.Name))
                {
                    previewParamValues[paramRef.Name] = paramRef.Default;
                }
                previewParamValues[paramRef.Name] = EditorGUILayout.Slider(paramRef.Name, previewParamValues[paramRef.Name], paramRef.Min, paramRef.Max);
                EditorUtils.PreviewUpdateParameter(paramRef.Name, previewParamValues[paramRef.Name]);
            }
            GUILayout.EndScrollView();

            GUILayout.EndArea();
            GUILayout.EndArea();
        }

        private void PreviewBank(Rect previewRect, EditorBankRef selectedBank)
        {
            GUILayout.BeginArea(previewRect);
            EditorGUIUtility.labelWidth = 75;
            string[] SizeSuffix = { "B", "KB", "MB", "GB" };
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("Platform Bank Sizes");
            EditorStyles.label.fontStyle = FontStyle.Normal;
            EditorGUI.indentLevel++;
            foreach (var sizeInfo in selectedBank.FileSizes)
            {
                int order = 0;
                long len = sizeInfo.Value;
                while (len >= 1024 && order + 1 < SizeSuffix.Length)
                {
                    order++;
                    len /= 1024;
                }
                EditorGUILayout.LabelField(sizeInfo.Name, String.Format("{0} {1}", len, SizeSuffix[order]));
            }
            EditorGUI.indentLevel--;
            EditorGUIUtility.labelWidth = 0;
            GUILayout.EndArea();
        }

        private void PreviewSnapshot(Rect previewRect, EditorEventRef selectedEvent)
        {
            GUILayout.BeginArea(previewRect);
            var style = new GUIStyle(EditorStyles.label);
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorGUIUtility.labelWidth = 75;

            EditorGUILayout.LabelField("Full Path", selectedEvent.Path, style);

            EditorGUIUtility.labelWidth = 0;
            EditorStyles.label.fontStyle = FontStyle.Normal;
            GUILayout.EndArea();
        }

        private void RebuildDisplayFromCache()
        {

            Action<TreeItem> nullEvents = null;
            nullEvents = (x) => { x.Exists = false; x.Children.ForEach(nullEvents); };
            Predicate<TreeItem> isStale = (x) => !x.Exists;
            Action<TreeItem> removeStaleChildren = null;
            removeStaleChildren = (x) => { x.Children.RemoveAll(isStale); x.Children.ForEach(removeStaleChildren); };

            if (showEvents)
            {
                var editorEvents = EventManager.Events;

                treeItems[0].Children.ForEach(nullEvents);
                treeItems[1].Children.ForEach(nullEvents);

                foreach (var editorEvent in editorEvents)
                {
                    string[] split = editorEvent.Path.Split('/');
                    var level = split[0] == "snapshot:" ? treeItems[1].Children : treeItems[0].Children;
                    for (int i = 1; i < split.Length; i++)
                    {
                        TreeItem item = level.Find((x) => x.Name == split[i]);
                        if (item != null)
                        {
                            if (i == (split.Length - 1))
                            {
                                item.EventRef = editorEvent;
                            }
                            else
                            {
                                level = item.Children;
                            }
                        }
                        else
                        {
                            item = new TreeItem();
                            if (i == (split.Length - 1))
                            {
                                item.EventRef = editorEvent;
                            }
                            item.Name = split[i];
                            level.Add(item);
                            level.Sort((a, b) => a.EventRef != b.EventRef ? (a.EventRef != null ? 1 : -1) : a.Name.CompareTo(b.Name));
                            level = item.Children;
                        }
                        item.Exists = true;
                    }
                }

                removeStaleChildren(treeItems[0]);
                removeStaleChildren(treeItems[1]);
            }

            if (showBanks)
            {
                var editorBanks = EventManager.Banks;
                var children = treeItems[2].Children;

                children.ForEach(nullEvents);

                foreach (var editorBank in editorBanks)
                {
                    var name = Path.GetFileNameWithoutExtension(editorBank.Path);
                    TreeItem item = children.Find((x) => x.Name == name);
                    if (item == null)
                    {
                        item = new TreeItem();
                        item.Name = name;
                        item.BankRef = editorBank;
                        children.Add(item);
                    }
                    item.Exists = true;
                }
                children.Sort((a, b) => a.Name.CompareTo(b.Name));

                removeStaleChildren(treeItems[2]);
            }

            if (expandedState != null && EventManager.IsLoaded)
            {
                int i = 0;
                Action<TreeItem> setExpanded = null;
                setExpanded = (x) => { selectedItem = (i == selectedIndex) ? x : selectedItem; x.Expanded = expandedState[i++]; x.Children.ForEach(setExpanded); };
                try
                {
                    treeItems.ForEach(setExpanded);
                }
                catch
                {
                }

                expandedState = null;

                if (selectedItem != null)
                {
                    SetPreviewEvent(selectedItem.EventRef);
                }
                else
                {
                    SetPreviewEvent(null);
                }
            }
        }

        internal void SelectEvent(SerializedProperty property)
        {
            fromInspector = true;
            showBanks = false;
            outputProperty = property;
            JumpToEvent(outputProperty.stringValue);
        }

        internal void SelectBank(SerializedProperty property)
        {
            fromInspector = true;
            showEvents = false;
            outputProperty = property;
            JumpToBank(outputProperty.stringValue);
        }

        public void JumpToEvent(string eventPath)
        {
            if (!String.IsNullOrEmpty(eventPath))
            {
                searchString = "";
                RebuildDisplayFromCache();
                TreeItem currentItem = null;
                if (eventPath.StartsWith("event:/"))
                {
                    currentItem = treeItems[0];
                    eventPath = eventPath.Replace("event:/", "");
                }
                else if (eventPath.StartsWith("snapshot:/"))
                {
                    currentItem = treeItems[1];
                    eventPath = eventPath.Replace("snapshot:/", "");
                }
                else
                {
                    return;
                }

                currentItem.Expanded = true;
                var pathElements = eventPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var pathElement in pathElements)
                {
                    var nextItem = currentItem.Children.Find(x => x.Name.Equals(pathElement, StringComparison.CurrentCultureIgnoreCase));
                    if (nextItem == null)
                    {
                        return;
                    }
                    nextItem.Expanded = true;
                    if (nextItem.EventRef)
                    {
                        SetSelectedItem(nextItem);

                        Rect searchRect, listRect, previewRect;
                        SplitWindow(out searchRect, out listRect, out previewRect);
                        if (selectedItem.Rect.y < treeScroll.y)
                        {
                            treeScroll.y = selectedItem.Rect.y;
                        }
                        else if (selectedItem.Rect.y + selectedItem.Rect.height > treeScroll.y + listRect.height)
                        {
                            treeScroll.y += (selectedItem.Rect.y + selectedItem.Rect.height) - listRect.height;
                        }
                        return;
                    }
                    currentItem = nextItem;
                }
            }
        }

        void JumpToBank(string bankName)
        {
            if (!String.IsNullOrEmpty(bankName))
            {
                RebuildDisplayFromCache();
                TreeItem currentItem = treeItems[2];

                currentItem.Expanded = true;
                var nextItem = currentItem.Children.Find(x => x.Name.Equals(bankName, StringComparison.CurrentCultureIgnoreCase));
                if (nextItem == null)
                {
                    return;
                }
                if (nextItem.BankRef)
                {
                    SetSelectedItem(nextItem);
                    return;
                }
            }
        }

        public EventBrowser()
        {
            treeItems = new List<TreeItem>();
            treeItems.Add(new TreeItem());
            treeItems.Add(new TreeItem());
            treeItems.Add(new TreeItem());
            treeItems[0].Name = "Events";
            treeItems[1].Name = "Snapshots";
            treeItems[2].Name = "Banks";
        }

        public static void RepaintEventBrowser()
        {
            if (instance)
            {
                instance.Repaint();
            }
        }

        static EventBrowser instance;

        public void OnEnable()
        {
            SceneView.onSceneGUIDelegate += SceneUpdate;
            EditorApplication.hierarchyWindowItemOnGUI += HierachachyUpdate;
            instance = this;
        }
        
        // This is an event handler on the hierachy view to handle dragging our objects from the browser
        void HierachachyUpdate(int instance, Rect rect)
        {
            Event e = Event.current;
            if (e.type == EventType.dragPerform && rect.Contains(e.mousePosition))
            {
                if (DragAndDrop.objectReferences.Length > 0 &&
                    DragAndDrop.objectReferences[0] != null &&
                        (DragAndDrop.objectReferences[0].GetType() == typeof(EditorEventRef) ||
                         DragAndDrop.objectReferences[0].GetType() == typeof(EditorBankRef)))
                {
                    GameObject target = (GameObject)EditorUtility.InstanceIDToObject(instance);
                    if (DragAndDrop.objectReferences[0].GetType() == typeof(EditorEventRef))
                    {
                        var emitter = Undo.AddComponent<StudioEventEmitter>(target);
                        emitter.Event = ((EditorEventRef)DragAndDrop.objectReferences[0]).Path;
                        var so = new SerializedObject(emitter);
                        so.ApplyModifiedProperties();
                    }
                    else
                    {
                        var loader = Undo.AddComponent<StudioBankLoader>(target);
                        loader.Banks = new List<string>();
                        loader.Banks.Add(((EditorBankRef)DragAndDrop.objectReferences[0]).Name);
                    }
                    Selection.activeObject = target;
                    e.Use();
                }
            }
        }

        // This is an event handler on the scene view to handle dragging our objects from the browser
        // and creating new gameobjects
        void SceneUpdate(SceneView sceneView)
        {
            Event e = Event.current;
            if (e.type == EventType.dragPerform)
            {
                if (DragAndDrop.objectReferences.Length > 0 &&
                    DragAndDrop.objectReferences[0] != null &&
                        (DragAndDrop.objectReferences[0].GetType() == typeof(EditorEventRef) ||
                         DragAndDrop.objectReferences[0].GetType() == typeof(EditorBankRef)))
                {
                    GameObject newObject = null;
                    if (DragAndDrop.objectReferences[0].GetType() == typeof(EditorEventRef))
                    {
                        string path = ((EditorEventRef)DragAndDrop.objectReferences[0]).Path;
                        string name = path.Substring(path.LastIndexOf("/") + 1);
                        newObject = new GameObject(name + " Emitter");
                        var emitter = newObject.AddComponent<StudioEventEmitter>();
                        emitter.Event = path;
                        var so = new SerializedObject(emitter);
                        so.ApplyModifiedProperties();
                        Undo.RegisterCreatedObjectUndo(newObject, "Create FMOD Studio Emitter");
                    }
                    else
                    {
                        newObject = new GameObject("FMOD Studio Loader");
                        var loader = newObject.AddComponent<StudioBankLoader>();
                        loader.Banks = new List<string>();
                        loader.Banks.Add(((EditorBankRef)DragAndDrop.objectReferences[0]).Name);
                        Undo.RegisterCreatedObjectUndo(newObject, "Create FMOD Studio Loader");
                    }
                    Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                    var hit = HandleUtility.RaySnap(ray);
                    if (hit != null)
                    {
                        newObject.transform.position = ((RaycastHit)hit).point;
                    }
                    else
                    {
                        newObject.transform.position = ray.origin + ray.direction * 10.0f;
                    }
                    Selection.activeObject = newObject;
                    e.Use();
                }
            }
            if (e.type == EventType.DragUpdated)
            {
                if (DragAndDrop.objectReferences.Length > 0 &&
                    DragAndDrop.objectReferences[0] != null &&
                        (DragAndDrop.objectReferences[0].GetType() == typeof(EditorEventRef) ||
                         DragAndDrop.objectReferences[0].GetType() == typeof(EditorBankRef)))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    DragAndDrop.AcceptDrag();
                    e.Use(); 
                }
            }
        }
    }
}

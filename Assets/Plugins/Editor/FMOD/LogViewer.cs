using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace FMODUnity
{
    class LogViewer : EditorWindow
    {
        [MenuItem("FMOD/Log Viewer", priority = 2)]
        static void ShowLogViewer()
        {
            LogViewer logViewer = EditorWindow.GetWindow<LogViewer>("FMOD log");
            logViewer.Show();
        }

        StreamReader logReader;
        List<string> logContents;
        long lastStreamLength = 0;

        LogViewer()
        {
            var fileStream = new FileStream(RuntimeUtils.LogFileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            logReader = new StreamReader(fileStream);

            logContents = new List<string>();

            while (!logReader.EndOfStream)
            {
                logContents.Add(logReader.ReadLine());
            }
            lastStreamLength = logReader.BaseStream.Length;
        }

        Vector2 scroll;

        void OnGUI()
        {
            int lines = logContents.Count;
            float lineHeight = GUI.skin.textArea.lineHeight;
            float totalHeight = lineHeight * (float)lines;
            int viewHeight = (int)(position.height / lineHeight) + 1;
            int scrollLine = (int)(scroll.y / lineHeight);
            StringBuilder builder = new StringBuilder();
            for (int i = scrollLine; i < Math.Min(scrollLine + viewHeight, lines); i++)
            {
                builder.AppendLine(logContents[i]);
            }

            Rect windowRect = new Rect(0, 0, position.width, position.height);
            Rect virtualRect = new Rect(0, 0, position.width, totalHeight); ;
            Rect textRect = new Rect(scroll.x, scroll.y, position.width, position.height);
            scroll = GUI.BeginScrollView(windowRect, scroll, virtualRect);
            GUI.TextArea(textRect, builder.ToString());
            GUI.EndScrollView();
        }

        void Update()
        {
            try
            {
                if (logReader != null)
                {
                    bool needRepaint = false;
                    if (logReader.BaseStream.Length < lastStreamLength)
                    {
                        logContents.Clear();
                        logReader.BaseStream.Seek(0, SeekOrigin.Begin);
                        logReader.DiscardBufferedData();
                        logReader.BaseStream.Flush();
                        needRepaint = true;
                    }
                    while (!logReader.EndOfStream)
                    {
                        var line = logReader.ReadLine();
                        if (line != null)
                        {
                            logContents.Add(line);
                        }
                        needRepaint = true;
                        lastStreamLength = logReader.BaseStream.Length;
                    }
                    if (needRepaint)
                    {
                        this.Repaint();
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }
    }
}

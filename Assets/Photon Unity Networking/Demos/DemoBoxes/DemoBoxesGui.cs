using System.Collections;
using UnityEngine;

public class DemoBoxesGui : MonoBehaviour
{
    public bool HideUI = false;

    /// <summary>A GUI element to show tips in.</summary>
    public GUIText GuiTextForTips;

    private int tipsIndex;

    private readonly string[] tips = new[]
                                     {
                                         "Click planes to instantiate boxes.",
                                         "Click a box to send an RPC. This will flash the box.",
                                         "Double click a box to destroy it. If it's yours.",
                                         "Boxes send ~10 updates per second when moving.",
                                         "Movement is not smoothed at all. It shows the updates 1:1.",
                                         "The script ColorPerPlayer assigns a color per player.",
                                         "When players leave, their boxes get destroyed. That's called clean up.",
                                         "Scene Objects are not cleaned up. The Master Client can Instantiate them.",
                                         "Scene Objects are not colored. They are controlled by the Master Client.",
                                         "The elevated planes instantiate Scene Objects. Those don't get cleaned up.",
                                         "Are you still reading?"
                                     };

    private const float TimePerTip = 3.0f;
    private float timeSinceLastTip;
    private const float FadeSpeedForTip = 0.05f;

    private void Update()
    {
        if (this.GuiTextForTips == null)
        {
            return;
        }

        this.timeSinceLastTip += Time.deltaTime;
        if (this.timeSinceLastTip > TimePerTip)
        {
            this.timeSinceLastTip = 0;
            StartCoroutine("SwapTip"); // this does the fading. the coroutine ends when fading is done.
        }
    }


    public IEnumerator SwapTip()
    {
        float alpha = 1.0f;
        while (alpha > 0)
        {
            alpha -= FadeSpeedForTip;
            this.timeSinceLastTip = 0;
            this.GuiTextForTips.color = new Color(this.GuiTextForTips.color.r, this.GuiTextForTips.color.r, this.GuiTextForTips.color.r, alpha);
            yield return null;
        }
        this.tipsIndex = (this.tipsIndex + 1)%this.tips.Length;
        this.GuiTextForTips.text = this.tips[this.tipsIndex];
        while (alpha < 1.0f)
        {
            alpha += FadeSpeedForTip;
            this.timeSinceLastTip = 0;
            this.GuiTextForTips.color = new Color(this.GuiTextForTips.color.r, this.GuiTextForTips.color.r, this.GuiTextForTips.color.r, alpha);
            yield return null;
        }
    }


    private void OnGUI()
    {
        if (this.HideUI)
        {
            return;
        }

        GUILayout.BeginArea(new Rect(0, 0, 300, Screen.height));
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        if (!PhotonNetwork.connected)
        {
            if (GUILayout.Button("Connect", GUILayout.Width(100)))
            {
                PhotonNetwork.ConnectUsingSettings(null);
            }
        }
        else
        {
            if (GUILayout.Button("Disconnect", GUILayout.Width(100)))
            {
                PhotonNetwork.Disconnect();
            }
        }
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}
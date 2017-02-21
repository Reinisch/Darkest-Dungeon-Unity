using Photon;
using UnityEngine;
using System.Collections;

public class DemoMecanimGUI : PunBehaviour
{
    #region Properties

    public GUISkin Skin;
    
    #endregion


    #region Members
    
    private PhotonAnimatorView m_AnimatorView;  // local animatorView. set when we create our character in CreatePlayerObject()
    private Animator m_RemoteAnimator;          // to display the synchronized values on the right side in the GUI. A third player will simply be ignored (until the second player leaves)

    private float m_SlideIn = 0f;
    private float m_FoundPlayerSlideIn = 0f;
    private bool m_IsOpen = false;

    #endregion


    #region Unity

    public void Awake()
    {

    }

    public void Update()
    {
        FindRemoteAnimator();

        m_SlideIn = Mathf.Lerp( m_SlideIn, m_IsOpen ? 1f : 0f, Time.deltaTime * 9f );
        m_FoundPlayerSlideIn = Mathf.Lerp( m_FoundPlayerSlideIn, m_AnimatorView == null ? 0f : 1f, Time.deltaTime * 5f );
    }

    /// <summary>Finds the Animator component of a remote client on a GameObject tagged as Player and sets m_RemoteAnimator.</summary>
    public void FindRemoteAnimator()
    {
        if( m_RemoteAnimator != null )
        {
            return;
        }

        // the prefab has to be tagged as Player
        GameObject[] gos = GameObject.FindGameObjectsWithTag( "Player" );
        for( int i = 0; i < gos.Length; ++i )
        {
            PhotonView view = gos[ i ].GetComponent<PhotonView>();
            if( view != null && view.isMine == false )
            {
                m_RemoteAnimator = gos[ i ].GetComponent<Animator>();
            }
        }
    }

    public void OnGUI()
    {
        GUI.skin = Skin;

        string[] synchronizeTypeContent = new string[] { "Disabled", "Discrete", "Continuous" };

        GUILayout.BeginArea( new Rect( Screen.width - 200 * m_FoundPlayerSlideIn - 400 * m_SlideIn, 0, 600, Screen.height ), GUI.skin.box );
        {
            GUILayout.Label( "Mecanim Demo", GUI.skin.customStyles[ 0 ] );

            GUI.color = Color.white;
            string label = "Settings";

            if( m_IsOpen == true )
            {
                label = "Close";
            }

            if( GUILayout.Button( label, GUILayout.Width( 110 ) ) )
            {
                m_IsOpen = !m_IsOpen;
            }

            string parameters = "";

            if( m_AnimatorView != null )
            {
                parameters += "Send Values:\n";

                for( int i = 0; i < m_AnimatorView.GetSynchronizedParameters().Count; ++i )
                {
                    PhotonAnimatorView.SynchronizedParameter parameter = m_AnimatorView.GetSynchronizedParameters()[ i ];
                    
                    try
                    {
                        switch( parameter.Type )
                        {
                        case PhotonAnimatorView.ParameterType.Bool:
                            parameters += parameter.Name + " (" + ( m_AnimatorView.GetComponent<Animator>().GetBool( parameter.Name ) ? "True" : "False" ) + ")\n";
                            break;
                        case PhotonAnimatorView.ParameterType.Int:
                            parameters += parameter.Name + " (" + m_AnimatorView.GetComponent<Animator>().GetInteger( parameter.Name ) + ")\n";
                            break;
                        case PhotonAnimatorView.ParameterType.Float:
                            parameters += parameter.Name + " (" + m_AnimatorView.GetComponent<Animator>().GetFloat( parameter.Name ).ToString( "0.00" ) + ")\n";
                            break;
                        }
                    }
                    catch
                    {
                        Debug.Log( "derrrr for " + parameter.Name );
                    }
                }
            }

            if( m_RemoteAnimator != null )
            {
                parameters += "\nReceived Values:\n";

                for( int i = 0; i < m_AnimatorView.GetSynchronizedParameters().Count; ++i )
                {
                    PhotonAnimatorView.SynchronizedParameter parameter = m_AnimatorView.GetSynchronizedParameters()[ i ];

                    try
                    {
                        switch( parameter.Type )
                        {
                        case PhotonAnimatorView.ParameterType.Bool:
                            parameters += parameter.Name + " (" + ( m_RemoteAnimator.GetBool( parameter.Name ) ? "True" : "False" ) + ")\n";
                            break;
                        case PhotonAnimatorView.ParameterType.Int:
                            parameters += parameter.Name + " (" + m_RemoteAnimator.GetInteger( parameter.Name ) + ")\n";
                            break;
                        case PhotonAnimatorView.ParameterType.Float:
                            parameters += parameter.Name + " (" + m_RemoteAnimator.GetFloat( parameter.Name ).ToString( "0.00" ) + ")\n";
                            break;
                        }
                    }
                    catch
                    {
                        Debug.Log( "derrrr for " + parameter.Name );
                    }
                }
            }

            GUIStyle style = new GUIStyle( GUI.skin.label );
            style.alignment = TextAnchor.UpperLeft;

            GUI.color = new Color( 1f, 1f, 1f, 1 - m_SlideIn );
            GUI.Label( new Rect( 10, 100, 600, Screen.height ), parameters, style );

            if( m_AnimatorView != null )
            {
                GUI.color = new Color( 1f, 1f, 1f, m_SlideIn );

                GUILayout.Space( 20 );
                GUILayout.Label( "Synchronize Parameters" );

                for( int i = 0; i < m_AnimatorView.GetSynchronizedParameters().Count; ++i )
                {
                    GUILayout.BeginHorizontal();
                    {
                        PhotonAnimatorView.SynchronizedParameter parameter = m_AnimatorView.GetSynchronizedParameters()[ i ];

                        GUILayout.Label( parameter.Name, GUILayout.Width( 100 ), GUILayout.Height( 36 ) );

                        int selectedValue = (int)parameter.SynchronizeType;
                        int newValue = GUILayout.Toolbar( selectedValue, synchronizeTypeContent );

                        if( newValue != selectedValue )
                        {
                            m_AnimatorView.SetParameterSynchronized( parameter.Name, parameter.Type, (PhotonAnimatorView.SynchronizeType)newValue );
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }
        GUILayout.EndArea();
    }

    #endregion


    #region Photon

    public override void OnJoinedRoom()
    {
        CreatePlayerObject();
    }

    private void CreatePlayerObject()
    {
        Vector3 position = new Vector3( -2, 0, 0 );
        position.x += Random.Range( -3f, 3f );
        position.z += Random.Range( -4f, 4f );

        GameObject newPlayerObject = PhotonNetwork.Instantiate( "Robot Kyle Mecanim", position, Quaternion.identity, 0 );
        m_AnimatorView = newPlayerObject.GetComponent<PhotonAnimatorView>();
    }

    #endregion
}

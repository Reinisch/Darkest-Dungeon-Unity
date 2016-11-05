using UnityEngine;

public class AudioRpc : Photon.MonoBehaviour
{

    public AudioClip marco;
    public AudioClip polo;

    AudioSource m_Source;

    void Awake()
    {
        m_Source = GetComponent<AudioSource>();
    }

    [PunRPC]
    void Marco()
    {
        if( !this.enabled )
        {
            return;
        }

        Debug.Log( "Marco" );

        m_Source.clip = marco;
        m_Source.Play();
    }

    [PunRPC]
    void Polo()
    {
        if( !this.enabled )
        {
            return;
        }

        Debug.Log( "Polo" );

        m_Source.clip = polo;
        m_Source.Play();
    }

    void OnApplicationFocus( bool focus )
    {
        this.enabled = focus;
    }
}

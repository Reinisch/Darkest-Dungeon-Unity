using UnityEngine;
using System.Collections;

public class PlayerDiamond : MonoBehaviour
{
    #region Properties
    public Transform HeadTransform;
    public float HeightOffset = 0.5f;
    #endregion

    #region Members
    PhotonView m_PhotonView;
    PhotonView PhotonView
    {
        get
        {
            if( m_PhotonView == null )
            {
                m_PhotonView = transform.parent.GetComponent<PhotonView>();
            }

            return m_PhotonView;
        }
    }

    Renderer m_DiamondRenderer;
    Renderer DiamondRenderer
    {
        get
        {
            if( m_DiamondRenderer == null )
            {
                m_DiamondRenderer = GetComponentInChildren<Renderer>();
            }

            return m_DiamondRenderer;
        }
    }

    float m_Rotation;
    float m_Height;
    #endregion

    #region Update
    void Start()
    {
        m_Height = HeightOffset;

        if( HeadTransform != null )
        {
            m_Height += HeadTransform.position.y;
        }
    }

    void Update() 
    {
        UpdateDiamondPosition();
        UpdateDiamondRotation();
        UpdateDiamondVisibility();
    }

    void UpdateDiamondPosition()
    {
        Vector3 targetPosition = Vector3.zero;

        if( HeadTransform != null )
        {
            targetPosition = HeadTransform.position;
        }

        targetPosition.y = m_Height;

        if( float.IsNaN( targetPosition.x ) == false && float.IsNaN( targetPosition.z ) == false )
        {
            transform.position = Vector3.Lerp( transform.position, targetPosition, Time.deltaTime * 10f );
        }
    }

    void UpdateDiamondRotation()
    {
        m_Rotation += Time.deltaTime * 180f;
        m_Rotation = m_Rotation % 360;

        transform.rotation = Quaternion.Euler( 0, m_Rotation, 0 );
    }

    void UpdateDiamondVisibility()
    {
        DiamondRenderer.enabled = true;

        if( PhotonView == null || PhotonView.isMine == false )
        {
            DiamondRenderer.enabled = false;
        }
    }
    #endregion
}

using UnityEngine;

/// <summary>
/// Base class for holograms that provides ability to be dragged by user. Also receives AirTap.
/// </summary>
public abstract class InteractiveHologramBehaviorBase : MonoBehaviour, ITapTarget, IManipulationTarget
{
    private Vector3? m_manipulationStartPosition;
    private Rigidbody m_myRigidbody;
    private bool m_hasRigidbody;
    private bool m_isRigidbodyKinematic;
    private bool m_isRigidbodyDetectCollisions;

    protected virtual void Start()
    {
        m_myRigidbody = GetComponent<Rigidbody>();
        m_hasRigidbody = null != m_myRigidbody;
    }

    public void OnManipulationStarted()
    {
        if (m_manipulationStartPosition.HasValue)
        {
            // Manipulation was started but did not finish; assuming fault state,
            // restoring pre-manipulation position.
            transform.position = m_manipulationStartPosition.Value;
        }
        else
        {
            m_manipulationStartPosition = transform.position;
        }
        MyPrepareRigidbodyForMove();
    }

    public void OnManipulationUpdated(GestureManager.ManipulationEventData data)
    {
        if (m_manipulationStartPosition.HasValue)
        {
            MyManipulationMove(data.CumulativeDelta);
        }
    }

    public void OnManipulationCompleted(GestureManager.ManipulationEventData data)
    {
        if (m_manipulationStartPosition.HasValue)
        {
            MyManipulationMove(data.CumulativeDelta);

            m_manipulationStartPosition = null;

            MyRestoreRigidbody();
        }
    }

    public void OnManipulationCancelled()
    {
        m_manipulationStartPosition = null;
        MyRestoreRigidbody();
    }

    public void OnTapped()
    {
        OnAirTap();
    }

    protected virtual void OnAirTap()
    {
    }

    // When moving the object with manipulation, some Rigidbody functionality like gravity and collisions 
    // are not required and should be disabled.
    private void MyPrepareRigidbodyForMove()
    {
        if (m_hasRigidbody)
        {
            m_isRigidbodyKinematic = m_myRigidbody.isKinematic;
            m_isRigidbodyDetectCollisions = m_myRigidbody.detectCollisions;
            m_myRigidbody.isKinematic = true;
            m_myRigidbody.detectCollisions = false;
        }
    }

    private void MyRestoreRigidbody()
    {
        if (m_hasRigidbody)
        {
            m_myRigidbody.isKinematic = m_isRigidbodyKinematic;
            m_myRigidbody.detectCollisions = m_isRigidbodyDetectCollisions;
        }
    }

    private void MyManipulationMove(Vector3 delta)
    {
        if (m_hasRigidbody)
            m_myRigidbody.MovePosition(m_manipulationStartPosition.Value + delta);
        else
            transform.position = m_manipulationStartPosition.Value + delta;
    }
}


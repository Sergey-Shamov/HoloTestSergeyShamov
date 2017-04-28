using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.VR.WSA.Input;

/// <summary>
/// Interface for receiving AirTap event. Use to implement click on object.
/// </summary>
public interface ITapTarget : IEventSystemHandler
{
    void OnTapped();
}

/// <summary>
/// Interface for receiving manioulation events. Use to implement dragging an object with fingers.
/// </summary>
public interface IManipulationTarget : IEventSystemHandler
{
    void OnManipulationStarted();
    void OnManipulationUpdated(GestureManager.ManipulationEventData data);
    void OnManipulationCompleted(GestureManager.ManipulationEventData data);
    void OnManipulationCancelled();
}

/// <summary>
/// Manages gesture recognition and sends events to the event system. Implemented gestures are AirTap and Manipulation.
/// </summary>
public class GestureManager : MonoBehaviour
{
    #region Nested class ManipulationEventData

    public class ManipulationEventData : BaseEventData
    {
        /// <summary>
        /// Position change since manipulation start.
        /// </summary>
        public Vector3 CumulativeDelta { get; private set; }

        public ManipulationEventData(EventSystem eventSystem, Vector3 delta) : base(eventSystem)
        {
            CumulativeDelta = delta;
        }
    }

    #endregion Nested class ManipulationEventData

    private GestureRecognizer m_recognizer;
    private GameObject m_manipulationTarget;

    void Start () {
        m_recognizer = new GestureRecognizer();
        m_recognizer.TappedEvent += Recognizer_TappedEvent;
        m_recognizer.ManipulationStartedEvent += Recognizer_ManipulationStartedEvent;
        m_recognizer.ManipulationUpdatedEvent += Recognizer_ManipulationUpdatedEvent;
        m_recognizer.ManipulationCompletedEvent += Recognizer_ManipulationCompletedEvent;
        m_recognizer.ManipulationCanceledEvent += Recognizer_ManipulationCanceledEvent;
        m_recognizer.SetRecognizableGestures(GestureSettings.Tap | GestureSettings.ManipulationTranslate);
        m_recognizer.StartCapturingGestures();
	}

    #region Recognizer callbacks

    // Callbacks send event data to event system.
    // AirTap is sent to the object at gaze point.
    // For manipulation, manipulation target is managed because target object may be not at gaze point.

    private void Recognizer_ManipulationStartedEvent(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay)
    {
        GameObject target = GazeManager.Instance.HitObject;
        if (null != target)
        {
            if (null != m_manipulationTarget)
            {
                ExecuteEvents.ExecuteHierarchy(m_manipulationTarget, null, OnManipulationCancelled);        // Second start arrived: notify about incomplete manipulation
                m_manipulationTarget = null;
            }
            m_manipulationTarget = target;
            ExecuteEvents.ExecuteHierarchy(m_manipulationTarget, null, OnManipulationStarted);
        }
    }

    private void Recognizer_ManipulationUpdatedEvent(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay)
    {
        if (null != m_manipulationTarget)
        {
            ManipulationEventData data = new ManipulationEventData(EventSystem.current, cumulativeDelta);
            ExecuteEvents.ExecuteHierarchy(m_manipulationTarget, data, OnManipulationUpdated);
        }
    }

    private void Recognizer_ManipulationCompletedEvent(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay)
    {
        if (null != m_manipulationTarget)
        {
            ManipulationEventData data = new ManipulationEventData(EventSystem.current, cumulativeDelta);
            ExecuteEvents.ExecuteHierarchy(m_manipulationTarget, data, OnManipulationCompleted);
            m_manipulationTarget = null;
        }
    }

    private void Recognizer_ManipulationCanceledEvent(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay)
    {
        if (null != m_manipulationTarget)
        {
            ExecuteEvents.ExecuteHierarchy(m_manipulationTarget, null, OnManipulationCancelled);
            m_manipulationTarget = null;
        }
    }

    private void Recognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay)
    {
        GameObject target = GazeManager.Instance.HitObject;
        if (null != target)
        {
            BaseEventData data = new BaseEventData(EventSystem.current);
            ExecuteEvents.ExecuteHierarchy(target, data, OnTapped);
            data = GazeManager.Instance.UnityUIPointerEvent;
            ExecuteEvents.ExecuteHierarchy(target, data, ExecuteEvents.pointerClickHandler);
        }
    }

    #endregion Recognizer callbacks

    private void OnDestroy()
    {
        m_recognizer.StopCapturingGestures();
        m_recognizer.TappedEvent -= Recognizer_TappedEvent;
        m_recognizer.ManipulationStartedEvent -= Recognizer_ManipulationStartedEvent;
        m_recognizer.ManipulationUpdatedEvent -= Recognizer_ManipulationUpdatedEvent;
        m_recognizer.ManipulationCompletedEvent -= Recognizer_ManipulationCompletedEvent;
        m_recognizer.ManipulationCanceledEvent -= Recognizer_ManipulationCanceledEvent;
        m_recognizer.Dispose();
    }

    #region Event delegates

    private static readonly ExecuteEvents.EventFunction<ITapTarget> OnTapped =
        delegate (ITapTarget handler, BaseEventData d)
        {
            handler.OnTapped();
        };

    private static readonly ExecuteEvents.EventFunction<IManipulationTarget> OnManipulationStarted =
        delegate (IManipulationTarget handler, BaseEventData d)
        {
            handler.OnManipulationStarted();
        };

    private static readonly ExecuteEvents.EventFunction<IManipulationTarget> OnManipulationUpdated =
        delegate (IManipulationTarget handler, BaseEventData d)
        {
            ManipulationEventData data = ExecuteEvents.ValidateEventData<ManipulationEventData>(d);
            handler.OnManipulationUpdated(data);
        };

    private static readonly ExecuteEvents.EventFunction<IManipulationTarget> OnManipulationCompleted =
        delegate (IManipulationTarget handler, BaseEventData d)
        {
            ManipulationEventData data = ExecuteEvents.ValidateEventData<ManipulationEventData>(d);
            handler.OnManipulationCompleted(data);
        };

    private static readonly ExecuteEvents.EventFunction<IManipulationTarget> OnManipulationCancelled =
        delegate (IManipulationTarget handler, BaseEventData d)
        {
            handler.OnManipulationCancelled();
        };

    #endregion Event delegates
}

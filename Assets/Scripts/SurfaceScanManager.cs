using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;

public class SurfaceScanManager : Singleton<SurfaceScanManager>
{
    public delegate void ScanningDoneHandler();
    public event ScanningDoneHandler OnScanCompleted;
    
    public int scanTime = 10;
    
    private bool m_isDone = false;
    private float m_timeout;
    
    private void Start()
    {
        SurfaceMeshesToPlanes.Instance.MakePlanesComplete += MyOnPlaneScanComplete;
        SpatialMappingManager.Instance.StartObserver();
        m_timeout = SpatialMappingManager.Instance.StartTime + scanTime;
    }
    
    private void Update()
    {
        if (!m_isDone)
        {
            if (Time.time >= m_timeout)
            {
                // 1. Shutdown the scanner if it's still running
                if (SpatialMappingManager.Instance.IsObserverRunning())
                {
                    SpatialMappingManager.Instance.StopObserver();
                }

                // 2. Create planes
                MyMakePlanes();

                // 3. Report scan completion
                if (null != OnScanCompleted)
                    OnScanCompleted();

                m_isDone = true;
            }
        }
    }
    
    private void MyOnPlaneScanComplete(object source, System.EventArgs args)
    {
        List<GameObject> horizontal = new List<GameObject>();
        horizontal = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Floor | PlaneTypes.Table | PlaneTypes.Ceiling);
        
        // It's reasonable to at least have 1 horizontal plane
        if (horizontal.Count >= 0)
        {
            // We don't need space meshes for our goals - clear them
            SpatialMappingManager.Instance.CleanupObserver();

        }
        else
        {
            // No horizontal planes - try again
            SpatialMappingManager.Instance.StartObserver();
            m_isDone = false;

        }
    }
    
    private void MyMakePlanes()
    {
        SurfaceMeshesToPlanes meshToPlanes = SurfaceMeshesToPlanes.Instance;
        if (null != meshToPlanes && meshToPlanes.enabled)
        {
            meshToPlanes.MakePlanes();
        }
    }
    
    protected override void OnDestroy()
    {
        if (SurfaceMeshesToPlanes.Instance != null)
        {
            SurfaceMeshesToPlanes.Instance.MakePlanesComplete -= MyOnPlaneScanComplete;
        }
        base.OnDestroy();
    }
}
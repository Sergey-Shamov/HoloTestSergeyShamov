using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides the following behavior: hologram can be dragged with fingers; hologram jumps on AirTap.
/// </summary>
public class JumpHologramBehavior : InteractiveHologramBehaviorBase
{
    private Rigidbody m_rigidbody;

    protected override void Start()
    {
        base.Start();
        m_rigidbody = GetComponent<Rigidbody>();
    }

    protected override void OnAirTap()
    {
        m_rigidbody.AddForce(Vector3.up * 6, ForceMode.Impulse);
    }
}
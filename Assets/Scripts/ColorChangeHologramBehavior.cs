using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides the following behavior: hologram can be dragged with fingers; hologram changes color on AirTap.
/// </summary>
public class ColorChangeHologramBehavior : InteractiveHologramBehaviorBase
{
    private Renderer m_renderer;

    protected override void Start()
    {
        base.Start();
        m_renderer = GetComponent<Renderer>();
    }

    protected override void OnAirTap()
    {
        if (null != m_renderer)
            m_renderer.material.color = UnityEngine.Random.ColorHSV();
    }
}

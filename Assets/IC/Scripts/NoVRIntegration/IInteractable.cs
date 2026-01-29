using System;
using UnityEngine;


public interface IInteractable
{
    public void Select();
    public void Deselect();
    public void Activate();
    public void Deactivate();

    public GameObject GetObject();

    public EPIType Type { get; }
}

public enum EPIType
{
    Colete = 0,
    Garra = 1,
    Extintor = 2,
    Oculos = 3
}
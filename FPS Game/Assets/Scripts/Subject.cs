using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Subject : MonoBehaviourPunCallbacks
{
    private List<IObserver> _observers = new List<IObserver>();

    public void AddObserver(IObserver observer)
    {
        _observers.Add(observer);
    }

    public void RemoveObserver(IObserver observer)
    {
        _observers.Remove(observer);
    }

    protected void NotifyObservers(ObserverData data)
    {
        _observers.ForEach(observer => observer.OnNotify(data));
    }
}

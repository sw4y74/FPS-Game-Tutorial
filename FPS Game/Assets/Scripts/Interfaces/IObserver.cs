using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ObserverData
{
    int something;
    string somethingElse;

    public ObserverData(int something, string somethingElse)
    {
        this.something = something;
        this.somethingElse = somethingElse;
    }
}

public interface IObserver
{
    public void OnNotify(ObserverData data);
}
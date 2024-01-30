using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISubject
{
    public void AddObserver(IObserver o);
    public void RemoveObserver(IObserver o);
    public void Notify();
} 

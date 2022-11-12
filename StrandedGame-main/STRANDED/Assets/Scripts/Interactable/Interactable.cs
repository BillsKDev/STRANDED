﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{

    private static HashSet<Interactable> _interactablesInRange = new HashSet<Interactable>();
    public static IReadOnlyCollection<Interactable> interactablesInRange => _interactablesInRange;

    public static event Action<bool> InteractablesInRangeChanged;
    public static event Action<Interactable, string> AnyInteractionComplete;

    [SerializeField] float _timeToInteract = 3f;
    [SerializeField, TextArea] string _completedInteractionText;
    [SerializeField] UnityEvent OnInteractionCompleted;

    InteractableData _data;
    public bool WasFullyInteracted => InteractionProgress >= 1;
    public float InteractionProgress => (_data?.TimeInteracted ?? 0f) / _timeToInteract;

    public void Bind(InteractableData inspectableData)
    {
        _data = inspectableData;
        if (WasFullyInteracted)
            RestoreInteractionState();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && WasFullyInteracted == false)
        {
            _interactablesInRange.Add(this);
            InteractablesInRangeChanged?.Invoke(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_interactablesInRange.Remove(this))
                InteractablesInRangeChanged?.Invoke(_interactablesInRange.Any());
        }
    }

    public void Interact()
    {
        if (WasFullyInteracted)
            return;

        _data.TimeInteracted += Time.deltaTime;
        if (WasFullyInteracted) 
            CompleteInteracton();
    }

    void CompleteInteracton()
    {
        _interactablesInRange.Remove(this);
        InteractablesInRangeChanged?.Invoke(_interactablesInRange.Any());
        OnInteractionCompleted?.Invoke();
        AnyInteractionComplete?.Invoke(this, _completedInteractionText);
    }

    public void RestoreInteractionState() => OnInteractionCompleted?.Invoke();
}
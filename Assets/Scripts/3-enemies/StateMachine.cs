using System;
using System.Collections.Generic;
using UnityEngine;

using State = UnityEngine.MonoBehaviour;
using Transition = System.Tuple<UnityEngine.MonoBehaviour, System.Func<bool>, UnityEngine.MonoBehaviour>;

/// <summary>
/// Generic finite state machine.
/// Each state is a MonoBehaviour on the same GameObject.
/// Only the active state is enabled, all others are disabled.
/// </summary>
public class StateMachine : MonoBehaviour
{
    private readonly List<State> states = new List<State>();
    private readonly List<Transition> transitions = new List<Transition>();

    private State activeState = null;

    public StateMachine AddState(State state)
    {
        if (state == null)
        {
            Debug.LogError("Trying to add a null state to StateMachine on " + gameObject.name);
            return this;
        }

        if (!states.Contains(state))
        {
            states.Add(state);
            state.enabled = false; // will be enabled only when active
        }

        return this;
    }

    public StateMachine AddTransition(State from, Func<bool> condition, State to)
    {
        if (from == null || to == null || condition == null)
        {
            Debug.LogError("Trying to add invalid transition in StateMachine on " + gameObject.name);
            return this;
        }

        transitions.Add(new Transition(from, condition, to));
        return this;
    }

    private void Start()
    {
        if (states.Count == 0)
        {
            Debug.LogWarning("StateMachine on " + gameObject.name + " has no states.");
            return;
        }

        GoToState(states[0]); // first added state is the initial one
    }

    private void GoToState(State newState)
    {
        if (activeState == newState)
            return;

        // Disable all states
        foreach (State state in states)
        {
            if (state != null)
                state.enabled = false;
        }

        activeState = newState;

        if (activeState != null)
            activeState.enabled = true;
    }

    private void Update()
    {
        if (activeState == null)
            return;

        foreach (Transition t in transitions)
        {
            // t.Item1 = from, t.Item2 = condition, t.Item3 = to
            if (t.Item1 == activeState && t.Item2())
            {
                GoToState(t.Item3);
                break;
            }
        }
    }
}

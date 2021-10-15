using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A simple lightweight Timer class with some advanced features such as Unity Actions.
/// </summary>
public class Timer : MonoBehaviour
{
    /// <summary>
    /// The name of our timer class, wince we could use many timers
    /// </summary>
    public string timerName;

    /// <summary>
    /// The current time of the timer
    /// </summary>
    public float time;

    /// <summary>
    /// Shorthand check for if our timer has elapsed
    /// </summary>
    private bool timeIsUp => time >= maxTime;

    /// <summary>
    /// The maximum allowable time before the time lapses and calls the onComplete action
    /// </summary>
    public float maxTime;

    /// <summary>
    /// The amount we want to increment our time variable
    /// </summary>
    private float increment;

    /// <summary>
    /// Has our timer already started?
    /// </summary>
    public bool isStarted;

    /// <summary>
    /// Has our timer been paused?
    /// </summary>
    public bool isPaused;

    /// <summary>
    /// Is our timer dynamic? Still under dev. Signifies the use of Time.deltaTime instead of an increment, when the increment given is float.MaxValue
    /// </summary>
    private bool isDynamic;

    /// <summary>
    /// The action to do every tick
    /// </summary>
    public UnityEngine.Events.UnityAction onTick;

    /// <summary>
    /// The action to do every laps of the timer
    /// </summary>
    public UnityEngine.Events.UnityAction onComplete;


    private IEnumerator Tick()
    {
        while(isStarted && !isPaused)
        {
            onTick.Invoke();

            if (timeIsUp)
            {
                onComplete.Invoke();
                time = 0;
            }
            else
            {
                if (isDynamic == false)
                {
                    time += increment;
                }
                else
                {
                    time += Time.deltaTime;
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// Starts the timer with: a start time, an amount to increment per tick, an end time and actions for a) when the Timer ticks and b) when the timer lapses
    /// <br></br><br></br>Note: Pass float.MaxValue in the increment field to use Time.deltaTime instead of a fixed increment
    /// </summary>
    /// <param name="_startTime"></param>
    /// <param name="_increment"></param>
    /// <param name="_endTime"></param>
    /// <param name="_onTickAction"></param>
    /// <param name="_onComplete"></param>
    public void Begin(float _startTime, float _increment, float _endTime, UnityAction _onTickAction, UnityAction _onComplete)
    {
        time = _startTime;

        if (_increment == float.MaxValue)
            isDynamic = true;
        else
        {
            isDynamic = false;
            increment = _increment;
        }

        maxTime = _endTime;

        onTick = _onTickAction;
        onComplete = _onComplete;

        isStarted = true;
        isPaused = false;

        StartCoroutine(Tick());
    }

    /// <summary>
    /// Stops our timer and changes everything back to defaults
    /// </summary>
    public void Reset()
    {
        StopAllCoroutines();
        time = 0;
        increment = 0;
        isStarted = false;
        isPaused = false;
        onTick = null;
        onComplete = null;
    }

    /// <summary>
    /// Stops our timer, but only:  isStarted and isPaused set to false and stops all coroutines
    /// </summary>
    public void Restart()
    {
        StopAllCoroutines();
        isStarted = false;
        isPaused = false;
        time = 0;
    }

    /// <summary>
    /// Sets the name of this timer
    /// </summary>
    /// <param name="_name"></param>
    public void SetName(string _name)
    {
        timerName = _name;
    }
}
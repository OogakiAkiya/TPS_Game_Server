using System;
using System.Collections.Generic;

public class StateMachine<T>
{
    private class State
    {
        private readonly Action enter;
        private readonly Action update;
        private readonly Action end;

        public State(Action _enter=null,Action _update = null, Action _end = null)
        {
            enter = _enter??delegate {};
            update = _update??delegate {};
            end = _end??delegate {};
        }

        public void Enter()
        {
            enter();
        }
        public void Update()
        {
            update();
        }
        public void End()
        {
            end();
        }

    }

    private Dictionary<T, State> stateDirectinary=new Dictionary<T, State>();
    private State currentState;
    public T currentKey { get; set; }
    public void AddState(T _key, Action _enter = null, Action _update = null, Action _end = null)
    {
        stateDirectinary.Add(_key ,new State( _enter, _update, _end));
    }

    public void ChangeState(T _key)
    {
        currentState?.End();

        //新しいstate代入
        currentKey = _key;
        currentState = stateDirectinary?[_key];
        currentState?.Enter();
    }

    public void Update()
    {
        currentState?.Update();
    }

    public void Clear()
    {
        stateDirectinary.Clear();
        currentState?.End();
        currentState = null;
    }
}

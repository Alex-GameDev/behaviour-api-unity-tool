using System;
using System.Collections.Generic;

namespace BehaviourAPI.StateMachines.StackFSMs
{
    using Core;
    using Core.Actions;
    using Core.Perceptions;

    public class StackFSM : FSM
    {
        Stack<State> _stateStack = new Stack<State>();

        public PopTransition CreatePopTransition(string name, State from, Perception perception = null, Action action = null, StatusFlags statusFlags = StatusFlags.Actived)
        {
            PopTransition transition = CreateInternalTransition<PopTransition>(name, from, perception, action, statusFlags);
            transition.SetStackFSM(this);
            return transition;
        }

        public PushTransition CreatePushTransition(string name, State from, State to, Perception perception = null, Action action = null, StatusFlags statusFlags = StatusFlags.Actived)
        {
            PushTransition transition = CreateInternalTransition<PushTransition>(name, from, perception, action, statusFlags);
            Connect(transition, to);
            transition.SetTargetState(to);
            transition.SetStackFSM(this);
            return transition;
        }

        public PopTransition CreatePopTransition(State from, Perception perception = null, Action action = null, StatusFlags statusFlags = StatusFlags.Actived)
        {
            PopTransition transition = CreateInternalTransition<PopTransition>(from, perception, action, statusFlags);
            transition.SetStackFSM(this);
            return transition;
        }

        public PushTransition CreatePushTransition(State from, State to, Perception perception = null, Action action = null, StatusFlags statusFlags = StatusFlags.Actived)
        {
            PushTransition transition = CreateInternalTransition<PushTransition>(from, perception, action, statusFlags);
            Connect(transition, to);
            transition.SetTargetState(to);
            transition.SetStackFSM(this);
            return transition;
        }

        public void Push(State targetState)
        {
            _stateStack.Push(_currentState);
            SetCurrentState(targetState);
        }

        public void Pop()
        {
            var targetState = _stateStack.Pop();
            SetCurrentState(targetState);
        }

        public override object Clone()
        {
            var fsm = (StackFSM)base.Clone();
            fsm._stateStack = new Stack<State>();
            return fsm;
        }

        public override void Stop()
        {
            base.Stop();
            _stateStack.Clear();
        }
    }
}

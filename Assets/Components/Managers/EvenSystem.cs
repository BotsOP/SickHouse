using System.Collections.Generic;

namespace Managers
{
    public enum EventType
    {
        //INPUT
        SELECT_TILE,
        CHANGE_TILE,
        FORCE_CHANGE_TILE,
        
        //Managing
        AMOUNT_APPLES,
        GAIN_APPLES,
        COLLECTED_APPLE,
        CHANGE_BRUSH,
        DESTROY_GAMEOBJECT,
        
        //Spawning
        SPAWN_RACOON,
        SPAWN_BEAVOR,
    }


    public static class EventSystem
    {
        private static Dictionary<EventType, System.Action> eventRegister = new();

        public static void Subscribe(EventType _evt, System.Action _func)
        {
            if (!eventRegister.ContainsKey(_evt))
            {
                eventRegister.Add(_evt, null);
            }

            eventRegister[_evt] += _func;
        }

        public static void Unsubscribe(EventType _evt, System.Action _func)
        {
            if (eventRegister.ContainsKey(_evt))
            {
                eventRegister[_evt] -= _func;
            }
        }

        public static void RaiseEvent(EventType _evt)
        {
            eventRegister[_evt]?.Invoke();
        }
    }

    public static class EventSystem<T>
    {
        private static Dictionary<EventType, System.Action<T>> eventRegister = new Dictionary<EventType, System.Action<T>>();

        public static void Subscribe(EventType _evt, System.Action<T> _func)
        {
            if (!eventRegister.ContainsKey(_evt))
            {
                eventRegister.Add(_evt, null);
            }

            eventRegister[_evt] += _func;
        }

        public static void Unsubscribe(EventType _evt, System.Action<T> _func)
        {
            if (eventRegister.ContainsKey(_evt))
            {
                eventRegister[_evt] -= _func;
            }
        }

        public static void RaiseEvent(EventType _evt, T _arg)
        {
            eventRegister[_evt]?.Invoke(_arg);
        }
    }

    public static class EventSystem<T, A>
    {
        private static Dictionary<EventType, System.Action<T, A>> eventRegister = new Dictionary<EventType, System.Action<T, A>>();

        public static void Subscribe(EventType _evt, System.Action<T, A> _func)
        {
            if (!eventRegister.ContainsKey(_evt))
            {
                eventRegister.Add(_evt, null);
            }

            eventRegister[_evt] += _func;
        }

        public static void Unsubscribe(EventType _evt, System.Action<T, A> _func)
        {
            if (eventRegister.ContainsKey(_evt))
            {
                eventRegister[_evt] -= _func;
            }
        }

        public static void RaiseEvent(EventType _evt, T _arg1, A _arg2)
        {
            eventRegister[_evt]?.Invoke(_arg1, _arg2);
        }
    }

    public static class EventSystem<T, A, B>
    {
        private static Dictionary<EventType, System.Action<T, A, B>> eventRegister = new Dictionary<EventType, System.Action<T, A, B>>();

        public static void Subscribe(EventType _evt, System.Action<T, A, B> _func)
        {
            if (!eventRegister.ContainsKey(_evt))
            {
                eventRegister.Add(_evt, null);
            }

            eventRegister[_evt] += _func;
        }

        public static void Unsubscribe(EventType _evt, System.Action<T, A, B> _func)
        {
            if (eventRegister.ContainsKey(_evt))
            {
                eventRegister[_evt] -= _func;
            }
        }

        public static void RaiseEvent(EventType _evt, T _arg1, A _arg2, B _arg3)
        {
            eventRegister[_evt]?.Invoke(_arg1, _arg2, _arg3);
        }
    }

    public static class EventSystem<T, A, B, C>
    {
        private static Dictionary<EventType, System.Action<T, A, B, C>> eventRegister = new Dictionary<EventType, System.Action<T, A, B, C>>();

        public static void Subscribe(EventType _evt, System.Action<T, A, B, C> _func)
        {
            if (!eventRegister.ContainsKey(_evt))
            {
                eventRegister.Add(_evt, null);
            }

            eventRegister[_evt] += _func;
        }

        public static void Unsubscribe(EventType _evt, System.Action<T, A, B, C> _func)
        {
            if (eventRegister.ContainsKey(_evt))
            {
                eventRegister[_evt] -= _func;
            }
        }

        public static void RaiseEvent(EventType _evt, T _arg1, A _arg2, B _arg3, C _arg4)
        {
            eventRegister[_evt]?.Invoke(_arg1, _arg2, _arg3, _arg4);
        }
    }
}
using System;
using System.Collections.Generic;

namespace Pedroca2005BR.Utilities
{
    public static class EventManager
    {

        // This dictionary will store the events it needs to call
        private static Dictionary<string, Action<object>> eventDictionary = new Dictionary<string, Action<object>>();

        // Subscribe is called by every class that wants to hear about an event. It needs to call subscribe for every event it wants to hear. Do this in the classe's OnEnable function.
        public static void Subscribe(string eventName, Action<object> functionToCall)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] += functionToCall;
            }
            else
            {
                eventDictionary[eventName] = functionToCall;
            }
        }

        // Unsubscribed is called to release memory and avoid phantom calls. Every event a class is subscribed to needs to be unsubscribed in the OnDisable function.
        public static void Unsubscribe(string eventName, Action<object> functionToCall)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] -= functionToCall;

                // Removes the event if there are no subscribers
                if (eventDictionary[eventName] == null)
                {
                    eventDictionary.Remove(eventName);
                }
            }
        }

        // TriggerEvent calls the event to every subscribed class.
        // Since there's only one parameter, use special objects, lists, or other types of data structures to send multiple variables. Attribute/variable separation should be delegated to the subscribed class.
        public static void TriggerEvent(string eventName, object parameter)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName]?.Invoke(parameter);
            }
        }
    }
}

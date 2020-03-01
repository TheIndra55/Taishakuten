using System;

namespace Kurisu.Configuration
{
    public class ConVar
    {
        public object Value
        {
            get { return _value; }
            set
            {
                // TODO check if typing matches
                _value = value;

                // call callback
                Callback?.Invoke();
            }
        }
        public Action Callback { get; set; }

        private object _value;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace yuelinLib
{
    public class Observable
    {
        private readonly Dictionary<string, List<Action<object>>> _subscribers = new Dictionary<string, List<Action<object>>>();

        public void SetProperty<T>(string propertyName, ref T field, T newValue)
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
            {
                field = newValue;
                Notify(propertyName, newValue);
            }
        }

        public void Subscribe<T>(string propertyName, Action<object, T> callback)
        {
            if (!_subscribers.ContainsKey(propertyName))
            {
                _subscribers[propertyName] = new List<Action<object>>();
            }
            _subscribers[propertyName].Add((value) => callback(this, (T)value));
        }

        private void Notify(string propertyName, object value)
        {
            if (_subscribers.TryGetValue(propertyName, out var callbacks))
            {
                foreach (var callback in callbacks)
                {
                    callback(value);
                }
            }
        }
    }

    public class ViewModel : Observable
    {
        private string _text = "default";

        public string Text
        {
            get => _text;
            set => SetProperty(nameof(Text), ref _text, value);
        }
    }

    public class View
    {
        public void Bind(ViewModel vm)
        {
            vm.Subscribe<string>(nameof(ViewModel.Text), (sender, value) =>
            {
                Console.WriteLine($"Text changed to {value}");
            });
        }
    }
}

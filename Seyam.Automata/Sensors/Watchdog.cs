using System;
using System.Collections.Generic;

using Seyam.Automata.Core;

namespace Seyam.Automata.Sensors 
{
    public class Watchdog<TContext> where TContext : class, new()
    {
        private readonly List<Sensor<TContext>> _sensors;
        private readonly Machine<TContext> _machine;

        public Watchdog(Machine<TContext> machine)
        {
            _machine = machine ?? throw new ArgumentNullException(nameof(machine));
            _sensors = new List<Sensor<TContext>>();
        }

        public void AddSensor(Sensor<TContext> sensor)
        {
            if (sensor != null && !_sensors.Contains(sensor))
                _sensors.Add(sensor);
        }

        public void RemoveSensor(Sensor<TContext> sensor)
        {
            _sensors.Remove(sensor);
        }

        public void ClearSensors()
        {
            _sensors.Clear();
        }

        public void Update()
        {
            // Iterate backwards in case a sensor removes itself during the loop (common game dev trick)
            for (int i = _sensors.Count - 1; i >= 0; i--)
                if (_sensors[i].IsActive)
                    _sensors[i].Update(_machine.Context, _machine);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GrandTheftAuto.Diagnostics {
    public class PerformanceTest {

        public uint Loops { get; set; }
        public List<Action> Actions { get; private set; }

        public PerformanceTest(params Action[] acts) {
            Loops = 1;
            Actions = acts.ToList();
        }

        public PerformanceTest(uint loops, params Action[] acts) {
            Loops = loops;
            Actions = acts.ToList();
        }

        public void Run() {
            using(Timing.Get("Performance Test")) {
                if(Loops < 1)
                    throw new ArgumentException("Need at least 1 loop");
                if(Actions == null || Actions.Count < 1)
                    throw new ArgumentException("Need at least 1 action");

                var stopwatch = new Stopwatch();
                var log = new StringBuilder();

                foreach(var action in Actions)
                    using(Timing.Get(action.Method.Name)) {
                        stopwatch.Reset();
                        stopwatch.Start();

                        for(var loop = 0; loop < Loops; loop++)
                            action();

                        stopwatch.Stop();
                        log.AppendFormat("{0} calls on {1} took {2}\n", Loops, action.Method.Name, stopwatch.Elapsed.GetLongTimeFormatted());
                    }

                Log.Message(log);
            }
        }

    }
}
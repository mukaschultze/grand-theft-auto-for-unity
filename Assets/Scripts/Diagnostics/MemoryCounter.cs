using System;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace GrandTheftAuto.Diagnostics {
    public class MemoryCounter : IDisposable {

        private const char BAR_CHAR = '█';
        private const int BAR_CHAR_COUNT = 75;
        private StringBuilder log = new StringBuilder();

        public MemoryCounter() {
            log.AppendLine("Memory Information\n");
            log.AppendLine("Before");
            AddMemoryInfo();
        }

        private void AddMemoryInfo() {
            GC.Collect();

            var memorySize = SystemInfo.systemMemorySize * 1024d * 1024d;
            var totalMemory = Profiler.GetTotalReservedMemoryLong() + Profiler.GetMonoHeapSizeLong();
            var usedMemory = Profiler.GetTotalAllocatedMemoryLong() + Profiler.GetMonoUsedSizeLong();

            var totalMemoryPercent = totalMemory / memorySize;
            var usedMemoryPercent = usedMemory / memorySize;

            log.Append("<color=#9575cd>");

            for(var i = 0; i < BAR_CHAR_COUNT; i++) {
                log.Append(BAR_CHAR);

                if(i == (int)(usedMemoryPercent * BAR_CHAR_COUNT))
                    log.Append("</color><color=#ffcdd2>");

                if(i == (int)(totalMemoryPercent * BAR_CHAR_COUNT))
                    log.Append("</color>");
            }

            log.AppendFormat("\nMemory Usage: {0:00.0%} ({1})\n", usedMemoryPercent, Utilities.GetBytesFormated(usedMemory));
            log.AppendFormat("Memory Allocated: {0:00.0%} ({1})\n", totalMemoryPercent, Utilities.GetBytesFormated(totalMemory));
        }

        public void Dispose() {
            log.AppendLine("\nAfter");
            AddMemoryInfo();
            Log.Message(log);
        }

    }
}

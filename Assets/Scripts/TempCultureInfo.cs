using System;
using System.Globalization;
using GrandTheftAuto.Diagnostics;

namespace GrandTheftAuto {
    public class TempCultureInfo : IDisposable {

        public CultureInfo PreviousCulture { get; private set; }
        public CultureInfo CurrentCulture { get; private set; }

        private bool log;

        public TempCultureInfo(CultureInfo culture) : this(culture, true) { }

        private TempCultureInfo(CultureInfo culture, bool log) {
            PreviousCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CurrentCulture = culture;

            if(this.log = log)
                Log.Message("Changed culture info to \"{0}\"", culture);
        }

        public void Dispose() {
            CultureInfo.CurrentCulture = PreviousCulture;

            if(log)
                Log.Message("Reset current culture to \"{0}\"", PreviousCulture);
        }
    }
}
using System;
using System.Globalization;
using GrandTheftAuto.Diagnostics;

namespace GrandTheftAuto.Shared {
    public class TempCultureInfo : IDisposable {

        public CultureInfo CurrentCulture { get; private set; }
        public CultureInfo PreviousCulture { get; private set; }

        public TempCultureInfo(CultureInfo culture) {
            PreviousCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CurrentCulture = culture;
            Log.Message("Changed culture info to \"{0}\"", culture);
        }

        public void Dispose() {
            CultureInfo.CurrentCulture = PreviousCulture;
            Log.Message("Reset current culture to \"{0}\"", PreviousCulture);
        }

    }
}
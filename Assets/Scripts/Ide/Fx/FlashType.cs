namespace GrandTheftAuto.Ide.Fx {
    public enum FlashType {
        /// <summary>
        /// Constantly lit all time.
        /// </summary>
        LitAllTime = 0,
        /// <summary>
        /// Constantly lit at night.
        /// </summary>
        LitAtNight = 1,
        /// <summary>
        /// Occasional flicker all time.
        /// </summary>
        FlickerAllTime = 2,
        /// <summary>
        /// Occasional flicker at night.
        /// </summary>
        FlickerAtNight = 3,
        /// <summary>
        /// ~1 second flashes.
        /// </summary>
        FastFlash = 4,
        /// <summary>
        /// ~1 second flashes at night.
        /// </summary>
        FastFlashAtNight = 5,
        /// <summary>
        /// ~2 seconds flashes.
        /// </summary>
        Flash = 6,
        /// <summary>
        /// ~2 seconds flashes at nigh.
        /// </summary>
        FlashAtNight = 7,
        /// <summary>
        /// ~3 seconds flashes.
        /// </summary>
        SlowFlash = 8,
        /// <summary>
        /// ~3 seconds flashes at night.
        /// </summary>
        SlowFlashAtNight = 9,
        /// <summary>
        /// Random flicker.
        /// </summary>
        RandomFlicker = 10,
        /// <summary>
        /// Random flicker at night.
        /// </summary>
        RandomFlickerAtNight = 11,
        /// <summary>
        /// Hardcoded traffic lights properties.
        /// </summary>
        TrafficLights = 12
    }
}
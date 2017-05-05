using System;

namespace GrandTheftAuto.Ide {
    [Flags]
    public enum DefinitionFlags {
        None = 0,
        /// <summary>
        /// Objects appear darker.
        /// Normal Cull in GTA III.
        /// </summary>
        WetEffect = 1 << 0,
        /// <summary>
        /// Object appears at night.
        /// Do not fade the object when it is being loaded into or out of view. (??)
        /// </summary>
        NightObject = 1 << 1,
        /// <summary>
        /// Model is transparent. Render this object after all opaque objects, 
        /// allowing transparencies of other objects to be visible through this object.
        /// </summary>
        DrawLast = 1 << 2,
        /// <summary>
        /// Render with additive blending. <see cref="NightObject"/> must be enabled too.
        /// </summary>
        AlphaTransparency2 = 1 << 3,
        /// <summary>
        /// Opposite to <see cref="NightObject"/>.
        /// </summary>
        DayFlag = 1 << 4,
        /// <summary>
        /// Indicates an object to be used inside an interior.
        /// Requires object.dat registration.
        /// </summary>
        InteriorObject = 1 << 5,
        /// <summary>
        /// Disable shadow mesh.
        /// (GTA III) Model is a shadow. Disable writing to z-buffer when rendering it
        /// </summary>
        Shadows = 1 << 6,
        /// <summary>
        /// Object surface will not be culled.
        /// </summary>
        CullOff = 1 << 7,
        /// <summary>
        /// Disables draw distance. Only used for LOD objects with an LOD value greater than 299.
        /// </summary>
        DrawDistanceOff = 1 << 8,
        /// <summary>
        /// Object is breakable, like glass.
        /// Requires object.dat registration.
        /// </summary>
        Breakable = 1 << 9,
        /// <summary>
        /// Similar to <see cref="Breakable"/>, object first cracks on a strong collision, then it breaks.
        /// Requires object.dat registration.
        /// </summary>
        BreakableWithCracks = 1 << 10,
        /// <summary>
        /// Indicates an object as an garage door
        /// Requires object.dat registration.
        /// </summary>
        GarageDoors = 1 << 11,
        /// <summary>
        /// Indicates a multi mesh object, object switches from mesh 2 to mesh 1 after collision.
        /// Requires object.dat registration.
        /// /// </summary>
        TwoClumpObject = 1 << 12,
        /// <summary>
        /// Indicates a vegetation object, object moves in wind.
        /// Requires object.dat registration.
        /// </summary>
        SmallFlora = 1 << 13,
        /// <summary>
        /// Standard flora, like palms and big trees.
        /// Requires object.dat registration.
        /// </summary>
        StandardFlora = 1 << 14,
        /// <summary>
        /// Uses object brightness from the current weather definition.
        /// </summary>
        TimeCyclePoleShadow = 1 << 15,
        /// <summary>
        /// Object explodes after getting hit.
        /// Requires object.dat registration.
        /// </summary>
        Explosive = 1 << 16,
        /// <summary>
        /// Object will switch from mesh 2 to mesh 1 after getting sprayed by the player.
        /// </summary>
        Graffity = 1 << 20,
        /// <summary>
        /// Disables backface culling, as an result the texture will be drawed on both sides of the model.
        /// (Always enabled for GTA III and Vice City)
        /// </summary>
        FaceCullingOff = 1 << 21
    }
}
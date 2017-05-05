namespace GrandTheftAuto.Ide {
    public enum IdeSection {
        Unknow,
        /// <summary>
        /// End of the section.
        /// </summary>
        END,
        /// <summary>
        /// Defines objects for the map. 
        /// </summary>
        OBJS,
        /// <summary>
        /// Functions similarly to <see cref="OBJS"/> but has two additional parameters defining the ingame time range the object can get rendered. 
        /// </summary>
        TOBJ,
        /// <summary>
        /// Unknown.
        /// </summary>
        HIER,
        /// <summary>
        /// Used to define vehicles.
        /// </summary>
        CARS,
        /// <summary>
        /// Used to define pedestrians.
        /// </summary>
        PEDS,
        /// <summary>
        /// Used to create waypoints for random NPC spawns.
        /// </summary>
        PATH,
        /// <summary>
        /// Used to add particle effects and simple ped behaviors to defined objects.
        /// </summary>
        _2DFX,
        /// <summary>
        /// Used to define weapons.
        /// See <see cref="Fx"/>
        /// </summary>
        WEAP,
        /// <summary>
        /// Functions similarly to <see cref="OBJS"/>, but it has one additional parameter indicating an IFP file to assign an animation to the object.
        /// </summary>
        ANIM,
        /// <summary>
        /// Used to virtually extend <see cref="Txd.TxdFile"/>.
        /// </summary>
        TXDP
    }
}
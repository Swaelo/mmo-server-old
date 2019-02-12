using System.Collections.Generic;

namespace Swaelo_Server
{
    public interface ISpaceUpdateable : ISpaceObject
    {
        /// <summary>
        /// Gets and sets whether or not the updateable should be updated sequentially even in a multithreaded space.
        /// If this is true, the updateable can make use of the space's ParallelLooper for internal multithreading.
        /// </summary>
        bool IsUpdatedSequentially { get; set; }

        /// <summary>
        /// Gets and sets whether or not the updateable should be updated by the space.
        /// </summary>
        bool IsUpdating { get; set; }

        ///<summary>
        /// List of managers owning the updateable.
        ///</summary>
        List<UpdateableManager> Managers { get; }



    }
}

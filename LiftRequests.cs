using System;
using System.Collections.Generic;
using System.IO;

namespace lift
{

    /// <summary>
    /// Creating the lift request object that contains the attributes of each passenger/request.
    ///</summary>
  
    public class LiftRequests
    {

        //  Lift request properties extracted from CSV
        public int PersonID { get; }
        public int CallingFloor { get; }
        public int DestinationFloor { get; }
        public TimeSpan RequestTime { get; }

        // Lift request properties useful for final analysis (such as time lift, time waiting before entering.. 
        public TimeSpan EntryTime { get; set; }
        public TimeSpan WaitTime { get; set; }



        public LiftRequests(int personID, int callingFloor, int destinationFloor, TimeSpan requestTime)
        {
            PersonID = personID;
            CallingFloor = callingFloor;
            DestinationFloor = destinationFloor;
            RequestTime = requestTime;
        }
    }

}
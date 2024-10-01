using System;
using System.Collections.Generic;
using System.IO;


namespace lift
{

    /// <summary>
    /// Lift class implements the lift logic. 
    /// I.e. Direction algorithm that continues servicing one direction picking up only the
    /// passengers travelling in that direction until there are no more requests in that direction, then switching. 
    /// Manages passenger pickup, drop off, and overload check.
    /// </summary>
    

    public class Lift
    {

        // Lift Basic properties
        public List<(int PersonID, TimeSpan TimeSpent)> TimeSpentInLift { get; set; } = [];
        public List<(int PersonID, TimeSpan WaitTime)> TimeSpentWaiting { get; set; } = [];
        public TimeSpan CurrentTime { get; set; } = TimeSpan.FromSeconds(0);
        public List<LiftRequests> PassengersInLift { get; set; } = [];
        public string LiftDirection { get; set; } = "UP";
        public int CurrentLoad { get; set; } = 0;

        public List<LiftRequests> PendingRequests { get; set; }

        public int directionCounter = 0;

        public TimeSpan TotalUpTime { get; private set; } = TimeSpan.FromSeconds(0);
        public TimeSpan TotalDownTime { get; private set; } = TimeSpan.FromSeconds(0);

        private const int MaxCapacity = 8;

        private const int TravelTimePerFloor = 10;


        // Making sure LiftFloor can only be set to a value between 1 and 10
        private int liftFloor = 1;
        public int LiftFloor
        {
            get => liftFloor;
            set
            {
                if (value >= 1 && value <= 10)
                {
                    liftFloor = value;
                }
            }
        }

        // csvWriter object to use CSVWriter class
        private CSVWriter csvWriter;




        public Lift(List<LiftRequests> requests)
        {
            PendingRequests = requests;

            // Instantiate CSVwriter object
            csvWriter = new CSVWriter();


        }

        // Function that handles lift movement 
        public void MoveLift(string outputFilePath)

        {

            while (PendingRequests.Count > 0 || PassengersInLift.Count > 0)
            {

                // Only consider pending requests that are active at the current point of lift time.
                var activeRequests = PendingRequests.Where(r => r.RequestTime <= CurrentTime).ToList();



                // If no active requests and no passengers in the lift, increment the time by 1s and check again
                if (activeRequests.Count == 0 && PassengersInLift.Count == 0)
                {

                    Console.WriteLine($"[Time: {CurrentTime.TotalSeconds}s] No active requests. Waiting for the next request.");
                    CurrentTime += TimeSpan.FromSeconds(1);
                    continue;
                }


                if (LiftDirection == "UP")
                {
                    ProcessUp(activeRequests);
                }
                else
                {
                    ProcessDown(activeRequests);
                }


            }


            // Log (max,min,average) time spent in lift
            csvWriter.LogTimeInLift(TimeSpentInLift);


            // Log (max,min,average) time spent waiting for lift
            csvWriter.LogTimeWaitLift(TimeSpentWaiting);


            //Log median travel time
            csvWriter.MedianTravelTime(TimeSpentInLift);

            //Log median wait time
            csvWriter.MedianWaitTime(TimeSpentWaiting);

            //Log total up travel and down travel time
            csvWriter.TimeDirection(TotalUpTime, TotalDownTime);

            // Log total number of direction changes
            csvWriter.DirectionChange(directionCounter);



            // Save the file using the dynamically generated path
            csvWriter.SaveToFile(outputFilePath);
        }


        // Function to implement 'up moving' algorithm
        private void ProcessUp(List<LiftRequests> activeRequests)

        {
            // Iterate from the current floor up to top floor
            for (int floor = LiftFloor; floor <= 10; floor++)
            {
                LiftFloor = floor;

                // Update active requests at current time
                activeRequests = PendingRequests.Where(r => r.RequestTime <= CurrentTime).ToList();

                Console.WriteLine($"[Time: {CurrentTime.TotalSeconds}s] Lift reaches floor {LiftFloor}");


                // Drop of passengers
                DropOffPassengers();

                // Pick up passengers 
                PickUpPassengers(activeRequests);
                


                // If no more requests above the current floor, switch direction
                if (!activeRequests.Any(r => r.CallingFloor > LiftFloor) &&
                !PassengersInLift.Any(p => p.DestinationFloor > LiftFloor))
                {
                    // if last request, write to CSV before breaking
                    if (PendingRequests.Count == 0)
                    {
                        csvWriter.WriteStateToCsv(CurrentTime, LiftFloor, PassengersInLift, activeRequests, LiftDirection);
                        break;
                    }

                    // Increment counter for direction change
                    directionCounter++;

                    LiftDirection = "DOWN";
                    Console.WriteLine("Switching direction to DOWN");
                    break;
                }

                //Write state at the end of lift servicing that particular floor
                csvWriter.WriteStateToCsv(CurrentTime, LiftFloor, PassengersInLift, activeRequests, LiftDirection);



                // Simulate time passing as the lift moves between floors
                CurrentTime += TimeSpan.FromSeconds(TravelTimePerFloor);


                // Update total time travelling up
                TotalUpTime += TimeSpan.FromSeconds(TravelTimePerFloor);

            }
        }

        // Function to implement 'down moving' algorithm

        private void ProcessDown(List<LiftRequests> activeRequests)
        {

            // Iterate from the current floor down to the ground floor 1
            for (int floor = LiftFloor; floor >= 1; floor--)
            {
                LiftFloor = floor;


                

                // Update active requests for current time
                activeRequests = PendingRequests.Where(r => r.RequestTime <= CurrentTime).ToList();

                Console.WriteLine($"[Time: {CurrentTime.TotalSeconds}s] Lift reaches floor {LiftFloor}");


                //Drop off passengers
                DropOffPassengers();

                //Pick up passengers
                PickUpPassengers(activeRequests);




                // If no more requests below the current floor, switch direction
                if (!activeRequests.Any(r => r.CallingFloor < LiftFloor) &&
                !PassengersInLift.Any(p => p.DestinationFloor < LiftFloor))

                {   
                    // If last request, write to CSV before breaking
                    if (PendingRequests.Count == 0)
                    {
                        csvWriter.WriteStateToCsv(CurrentTime, LiftFloor, PassengersInLift, activeRequests, LiftDirection);
                        break;
                    }

                    // Increment direction switching counter
                    directionCounter++;

                    LiftDirection = "UP";
                    Console.WriteLine("Switching direction to UP");
                    break;
                }

                //Write final floor state to CSV after lift's activities finished on this floor.
                csvWriter.WriteStateToCsv(CurrentTime, LiftFloor, PassengersInLift, activeRequests, LiftDirection);


                // Simulate time passing as the lift moves between floors
                CurrentTime += TimeSpan.FromSeconds(TravelTimePerFloor);

                // Update total time travelling down
                TotalDownTime += TimeSpan.FromSeconds(TravelTimePerFloor);


            }
        }

        // Function to drop off passengers whose destination matches the current floor
        private void DropOffPassengers()
        {

            var passengersToDropOff = PassengersInLift.Where(p => p.DestinationFloor == LiftFloor).ToList();

            foreach (var passenger in passengersToDropOff)
            {
                // Time spent in lift for each passenger
                TimeSpan timeSpent = CurrentTime - passenger.EntryTime;

                TimeSpentInLift.Add((passenger.PersonID, timeSpent));

                Console.WriteLine($"[Time: {CurrentTime.TotalSeconds}s] Dropping off passenger {passenger.PersonID} at floor {LiftFloor}");

                // Remove passenger from passengers in lift
                PassengersInLift.Remove(passenger);
            }
        }

        // Function to pick up passengers waiting at the current floor
        private void PickUpPassengers(List<LiftRequests> activeRequests)


        {   // Only pick up passengers who have already requested lift and travelling in same direction  
            var passengersToPickUp = activeRequests.Where(p => p.CallingFloor == LiftFloor &&
                                                                 ((LiftDirection == "UP" && p.DestinationFloor > LiftFloor) ||
                                                                  (LiftDirection == "DOWN" && p.DestinationFloor < LiftFloor))).ToList();
            foreach (var passenger in passengersToPickUp)
            {
                // Check if lift overloaded.
                if (PassengersInLift.Count >= MaxCapacity)
                {
                    Console.WriteLine($"Cannot pick up passenger {passenger.PersonID}: lift is at maximum capacity of {MaxCapacity}.");
                    break;
                }

                // Add the passenger to the lift
                PassengersInLift.Add(passenger);  

                // Time spent waiting for each passenger before entering lift.
                passenger.WaitTime = CurrentTime - passenger.RequestTime;
                TimeSpentWaiting.Add((passenger.PersonID, passenger.WaitTime));
                passenger.EntryTime = CurrentTime;

                // Remove the request from the pending list
                PendingRequests.Remove(passenger); 
                Console.WriteLine($"[Time: {CurrentTime.TotalSeconds}s] Picking up passenger {passenger.PersonID} at floor {LiftFloor}");
            }
        }
    }
}
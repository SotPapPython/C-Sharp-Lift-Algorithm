using System;
using System.Collections.Generic;
using System.IO;

namespace lift
{


    /// <summary>
    /// Class to write final lift state: 
    /// time of reaching fllor, current lift floor, number of passengers in lift, ordered calling queue)
    /// Also contains analytic functions such as total lift direction changes, 
    /// total, max, min, average, median travel time / wait time per passenger
    /// to evaluate efficiency. 
    /// </summary>

    public class CSVWriter
    {


        // List to store each row to be outputted to CSV 
        private List<string> csvOutput = [];


        // Function to add headings of CSVOutput.
        public CSVWriter()
        {
            csvOutput.Add("Current Time,Lift Floor,Passengers In Lift,Floor Queue");
        }

        // Function to write to the CSV file
        public void WriteStateToCsv(TimeSpan currentTime, int liftFloor, List<LiftRequests> passengersInLift, List<LiftRequests> activeRequests, string liftDirection)
        {
            // Get PersonIDs of passengers in the lift
            var passengersInLiftIds = string.Join(",", passengersInLift.Select(p => p.PersonID.ToString()));

            // Separate call requests into those above and below the current floor

            var callFloorsAbove = activeRequests
                .Where(r => r.CallingFloor > liftFloor)
                .Select(r => r.CallingFloor.ToString())
                .ToList();

            var callFloorsBelow = activeRequests
                .Where(r => r.CallingFloor < liftFloor)
                .Select(r => r.CallingFloor.ToString())
                .ToList();

            // Separate destination requests into those above and below the current floor
            var destinationFloorsAbove = passengersInLift
                .Where(p => p.DestinationFloor > liftFloor)
                .Select(p => p.DestinationFloor.ToString())
                .ToList();

            var destinationFloorsBelow = passengersInLift
                .Where(p => p.DestinationFloor < liftFloor)
                .Select(p => p.DestinationFloor.ToString())
                .ToList();

            // Combine calls and destinations in up and down lists
            var allCallsAbove = callFloorsAbove.Concat(destinationFloorsAbove).ToList();
            var allCallsBelow = callFloorsBelow.Concat(destinationFloorsBelow).ToList();

            // Sort the lists based on the lift direction
            List<string> sortedActiveRequests;

            if (liftDirection == "UP")
            {
                // Sort calls above the current floor in ascending order
                var sortedCallsAbove = allCallsAbove.OrderBy(f => int.Parse(f)).ToList();

                // Sort calls below the current floor in descending order (to be serviced after going up)
                var sortedCallsBelow = allCallsBelow.OrderByDescending(f => int.Parse(f)).ToList();

                // Combine calls above and below the current floor by putting lists together
                sortedActiveRequests = sortedCallsAbove.Concat(sortedCallsBelow).ToList();
            }

            // Else lift direction is down as it is always either up or down
            else 
            {
                // Sort calls below the current floor in descending order
                var sortedCallsBelow = allCallsBelow.OrderByDescending(f => int.Parse(f)).ToList();
                // Sort calls above the current floor in ascending order (to be serviced after going down)
                var sortedCallsAbove = allCallsAbove.OrderBy(f => int.Parse(f)).ToList();

                // Combine calls above and below the current floor by putting lists together
                sortedActiveRequests = sortedCallsBelow.Concat(sortedCallsAbove).ToList();
            }
           
            // Avoid adding repeated calls 
            sortedActiveRequests = sortedActiveRequests.Distinct().ToList();



            // Convert the sorted list to a string for CSV output
            var sortedActiveRequestsString = string.Join(",", sortedActiveRequests);

            // Add the state to the CSV output
            csvOutput.Add($"{currentTime.TotalSeconds},{liftFloor},\"{passengersInLiftIds}\",\"{sortedActiveRequestsString}\"");

        }


        // Function to calculate and add the total time spent in the lift for each passenger
        public void LogTimeInLift(List<(int PersonID, TimeSpan TimeSpent)> timeInLift)
        {
            foreach (var entry in timeInLift)
            {
                csvOutput.Add($"Passenger {entry.PersonID} spent {entry.TimeSpent.TotalSeconds} seconds in the lift.");

            }

            csvOutput.Add($"\"Maximum travel time: {timeInLift.Select(tuple => tuple.Item2.TotalSeconds).Max()} seconds, Minimum travel time: {timeInLift.Select(tuple => tuple.Item2.TotalSeconds).Min()} seconds, Average travel time: {timeInLift.Select(tuple => tuple.Item2.TotalSeconds).Average()} seconds\"");

        }

        // Function to calculate and add the total time spent waiting for the lift for each passenger

        public void LogTimeWaitLift(List<(int PersonID, TimeSpan waitTime)> timewait)
        {
            foreach (var entry in timewait)
            {
                csvOutput.Add($"Passenger {entry.PersonID} waited {entry.waitTime.TotalSeconds} seconds for the lift.");

            }

            csvOutput.Add($"\"Maximum wait time: {timewait.Select(tuple => tuple.Item2.TotalSeconds).Max()} seconds, Minimum wait time: {timewait.Select(tuple => tuple.Item2.TotalSeconds).Min()} seconds, Average wait time: {timewait.Select(tuple => tuple.Item2.TotalSeconds).Average()} seconds\"");


        }

        // Function to calculate and add how much time (and %) was spent in ech direction to CSV

        public void TimeDirection(TimeSpan TotalUpTime, TimeSpan TotalDownTime)

        {
            double a = (TotalUpTime / (TotalUpTime + TotalDownTime)) * 100;


            csvOutput.Add($"Lift traveled upwards for {TotalUpTime.TotalSeconds} seconds ({a.ToString("F2")}%) & downwards for {TotalDownTime.TotalSeconds} ({(100 - a).ToString("F2")}%)");


        }


        // Function to determine the number of direction changes
        public void DirectionChange(int x)
        {

            csvOutput.Add($"There were a total of {x} direction changes");

        }

        //Function to determine the median wait time in the lift per passenger
        public void MedianWaitTime(List<(int PersonID, TimeSpan waitTime)> time)
        {
            List<double> t = time.Select(tuple => tuple.Item2.TotalSeconds).ToList();

            List<double> ordered = t.OrderBy(o => o).ToList();

            double medianWaitTime;
            int count = ordered.Count;


            if (ordered.Count % 2 == 0)
            {
                medianWaitTime = (ordered[count / 2 - 1] + ordered[count / 2]) / 2.0;

                csvOutput.Add($"Median wait time of {medianWaitTime} seconds");
            }

            else
            {
                medianWaitTime = ordered[count / 2];
                csvOutput.Add($"Median wait time of {medianWaitTime} seconds");

            }

        }

        //Function to determine the median travel time in the lift per passenger

        public void MedianTravelTime(List<(int PersonID, TimeSpan travelTime)> time)
        {
            List<double> t = time.Select(tuple => tuple.Item2.TotalSeconds).ToList();

            List<double> ordered = t.OrderBy(o => o).ToList();

            double medianTravelTime;
            int count = ordered.Count;


            if (ordered.Count % 2 == 0)
            {
                medianTravelTime = (ordered[count / 2 - 1] + ordered[count / 2]) / 2.0;

                csvOutput.Add($"Median travel time of {medianTravelTime} seconds");
            }

            else
            {
                medianTravelTime = ordered[count / 2];
                csvOutput.Add($"Median travel time of {medianTravelTime} seconds");

            }

        }



        // Function to write and save lines in csvOutput list to output file
        public void SaveToFile(string filePath)
        {
            File.WriteAllLines(filePath, csvOutput);
        }
    }
}
     

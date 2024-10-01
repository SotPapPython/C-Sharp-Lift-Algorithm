using System;
using System.Collections.Generic;
using System.IO;

namespace lift
{

    /// <summary>
    /// Class to read the CSV data in and create the lift request object with properties given by columns in CSV file
    /// </summary>
    
    public static class CSVReader
    {

        public static List<LiftRequests> ReadCsv(string filePath)
        {

            // List to hold request objects
            List<LiftRequests> requests = [];

            using (var reader = new StreamReader(filePath))
            {
                reader.ReadLine();

                while (!reader.EndOfStream)
                {

                    var line = reader.ReadLine();
                    var values = line.Split(',');


                    // Parse to convert string from CSV into integer
                    int personID = int.Parse(values[0]);
                    int callingFloor = int.Parse(values[1]);
                    int destinationFloor = int.Parse(values[2]);
                    int seconds = int.Parse(values[3]);

                    // Converts the CSV column of time into a Timespan object in seconds.
                    TimeSpan RequestTime = TimeSpan.FromSeconds(seconds);

                    // Create instance of liftRequest class and add it to requests list
                    var request = new LiftRequests(personID, callingFloor, destinationFloor, RequestTime);
                    requests.Add(request);
                }
            }

            return requests;
        }

    }
}
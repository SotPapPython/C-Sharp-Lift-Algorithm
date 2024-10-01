/*/
Title: Lift console application
Produced by: Sotiris Papadakis 
/*/

using System;
using System.Collections.Generic;
using System.IO;


namespace lift
{


    /// <summary>
    /// The main entry point of the application. 
    /// Sets up output file for CSV.
    /// Instantiates the lift object using the requests csv.
    /// Runs the MoveLift function to operate the lift.
    /// </summary>
    


    public class Program
    {
        static void Main()
        {
            // Get the directory of the executable
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Navigate to the project root directory
            string projectDirectory = Path.GetFullPath(Path.Combine(exeDirectory, @"..\..\..\"));

            // Define the outputs folder in the project directory
            string outputsFolder = Path.Combine(projectDirectory, "outputs");

            // Ensure the outputs directory exists
            if (!Directory.Exists(outputsFolder))
            {
                Directory.CreateDirectory(outputsFolder);

            }

            // Create the full file path for the input CSV
            string inputFileName = "Example Lift Data.csv";
            string filePath = Path.Combine(projectDirectory, inputFileName);

            // Ensure the input file exists
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Input file not found: {filePath}");
                return;
            }

            // Create the full file path for the output CSV
            string fileName = "CSVoutput.csv";
            string outputFilePath = Path.Combine(outputsFolder, fileName);






            // Read the lift requests from the CSV file instantiating the list of requests as LiftRequests objects
            List<LiftRequests> liftRequests = CSVReader.ReadCsv(filePath);

            // Instantiate the lift with the list of requests
            var Lift1 = new Lift(liftRequests);

            // Start the lift operation and save the output CSV
            Lift1.MoveLift(outputFilePath);


        }

    }
}
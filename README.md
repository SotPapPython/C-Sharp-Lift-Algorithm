# C# Lift Algorithm

*Abstract*:

A C# application implementing an advanced elevator scheduling algorithm that optimally services all passenger requests. The program calculates the total time taken and provides in-depth analytics on operational efficiency, including detailed metrics for performance evaluation.  

## Introduction

The earliest lifts were operated manually by human attendants who would directly coordinate
their movements according to the specific requests of each user. As with many such 
mechanisms, modern lifts are now automated by sophisticated computerised algorithms, the 
efficiency of which is largely a function of their ability to implement the paths that minimise 
selected optimisation parameters. For instance, one may optimise efficiency by minimising
average wait times of users before entering the elevator; by minimising average travel times of 
users while inside the elevator; by minimising over crowdedness; or by minimising energy 
usage. Naturally, each building may have particular criteria that make one of the above 
scheduling strategies more suitable than another. Therefore, in order to design an efficient lift 
system, one must first consider the explicit and implicit assumptions of the given contracted 
building.

The 'Elevator Algorithm' is one in which the lift travels in a single direction, servicing requests 
only in that direction until requests in that direction are exhausted. That is, passengers are picked up
only if they are travelling in the same direction as the lift. This would minimise unnecessary direction changes, minimising the overall distance travelled, 
and therefore minimise the average travel time. This algorithm would do particularly well when 
general traffic is directional. Otherwise, sub-optimal wait times will be experienced by those 
requesting travel in the opposite direction to that of the current. This shows that optimising for 
minimum average travel time can, in some cases, lead to de-optimising the longest waiting time 
– a form of request starvation. This request starvation would not be as prominent, however, as 
compared to a ‘nearest neighbour optimisation’ schedule – requests are prioritised based on 
the nearest floor to be serviced – where one can imagine a situation where the lift oscillates 
between a few floors and struggles to reach ones further away. In the direction grouping 
algorithm, it is guaranteed that each request will be serviced in a reasonable time. Given, also, 
it is probable that a lift situated in a working office will generally have a directional flow of traffic, upwards in the morning and downwards at 
the end of the working day, without highly variable traffic patterns, and particularly as day-to-day patterns will be similar, the direction grouping algorithm is most suited.
In order to maintain the efficiency of such a lift that keeps in the same direction making
necessary stops on the way, logic must be implemented to switch the direction if there are no 
future requests in that same direction, as opposed to only switching direction when at the top 
of the building. 

As a final remark on algorithmic efficiency, the particular optimisation parameter that is 
preferred is, in the absence of other information, ultimately coming from untested assumptions
– the testing of which would require the cost of deployment of multiple algorithms. One could
simulate the implementation of various algorithms constrained by some cost metric - noting as 
we did above, that optimising only for average metrics may lead to some other metrics taking an 
extremely long time to be serviced, and thus make the algorithm unusable. A more 
sophisticated method might leverage the tools of reinforcement learning to dynamically 
augment the lift’s decision making with efficiency judgements that may be adapted 
autonomously through trial and adaptation. This would ensure that lift functionality is 
especially suited to that particular building’s requirements and constraints.

## Programme Structure

I have written this document as a top-down overview of how I structured the
lift algorithm with regards to input and output CSV files, C# files, and C# classes and methods. 

## Input CSV file

The input CSV file must consist of rows of four columns of data representing each request:
- Person id
- Calling floor
- Destination floor
- Calling time 

The 'Example Lift Data.csv' file is the one I use to present the lift algorithm.


## File Structure


├── LiftRequests.cs:         Contains the LiftRequest class


├── CSVReader.cs:       Contains the CSVReader class


├── LiftLogic.cs:                Contains the Lift class


├── CSVWriter.cs:      Contains the CSVWriter class


├── Program.cs : Contians Main() Entry point


## Classes


### 1. LiftRequests 

#### Functionality:

Creates a Lift request object with properties given by features of each passenger (columns given in the input CSV).
This class contains no methods.

### 2. CSVReader
 
#### Functionality:

 Reads the input CSV file and stores the request information as instances of the above LiftRequests class.
 
 #### Methods

 1. The ReadCsv method parses each request object into the pending requests list. 


### 3. Lift Logic 

#### Functionality:

The fundamental lift object is created. It applies the lift algorithm logic that services requests by preserves direction until requests are exhausted in that direction before switching.

#### Methods

1. The MoveLift method waits for active requests by incrementing one second in between checks, then depending on direction, process the up/down movements with ProcessUp/ProcessDown methods.

2. The ProcessUp method implements the direction algorithm by incrementing from (floor) to (floor + 1) and applying the PickUpPassengers method and the DropOffPassengers method. It also increments the total time by 10s and the time travelling upwards by 10s (assuming each floor request is on averqge 10 seconds)  

3. The ProcessDown method is the dual to the ProcessUp method and acts analogously. 

4. The DropOffPassengers method removes passengers from inside the lift, updating that lift property, if passengers are at their destination. It also calculates the time spent in the lift for each passenger.

5. The PickUpPassengers method picks up passengers who are both calling at the current floor and are travelling in the same direction. It adds these passengers to the lift counter and checks for lift overload. It calculates the time spent by the passenger waiting for the lift and removes the passenger from the pending requests. 

### 4. CSV Writer

#### Functionality: 

This class is responsible for writing the output state at each floor (current time, number of passengers in lift, lift floor, and ordered call queue).

#### Methods

1. CSVWriter is the basic function that adds the headings to the output CSV file.

2. WriteStateToCsv is the fundamental writing function that uses the objects defined in the lift logic to write the final lift state on each floor that is reached.

3. LogTimeInLift calculates the total time spent in the lift per passenger.

4. LogTimeWaitLift calculates the total time spent waiting for the lift per passenger.

5. TimeDirection calculates the total time the lift spent in each direction. This will give some indication if there is a directional flow which may further be used to optimise the lift algorithm.

6. DirectionChange calulates the number of direction changes in the total period of lift operation.

7. MedianWaitTime calculates the median wait time. This is again a useful lift analytic.

8. MedianTravelTime calculates the median travel time. 

9. Finally the SaveToFile function writes all the above into the CSV and saves.

### 5. Program 

#### Functionality:

This is the main entry point that runs the entire lift operation by bringing together the elements from the other classes.

#### Methods:

1. The Main function first ascertains the project directory and creates an output folder to send the output CSV file. It then reads the lift requests from the CSV file, instantiating them as LiftRequest objects. These are then used to create a Lift object called MedicineChest which then runs the Move Lift method, initiating the lift operation. 

## Output

The output CSV contains the full state of the lift after every floor change, that is: current time, lift floor, passenger ID, ordered floor queue. The total time of completion is thus the time at the final row of data. (For instance, using the example passenger request data, it shows the lift algorithm takes 665 seconds to service all requests).

It also outputs some analytic measurements of efficiency, including:

- The time each passenger spent inside the lift.
- The maximum, minimum, average, and median time each passenger spent in the lift 
- The time each passenger waited before entering the lift.
- The maximum, minimum, average, and median time each passenger waited before entering the lift.
- The total amount of time (and percentage of total time) the lift traveled upwards and downwards for. 
- The total number of direction changes.       

# ProducerConsumerTest
This is a Producer-consumer implementation, using the c#.net 6.0 core library, with a visualization front-end.

## Purposes
This application demonstrates the capacity to produce and consume data fetching the last produced data when available.

## Logs
The log is responsible for tracking data allocation and generation.
Every log message is dispatched on Windows.Trace, File (in .bin/Logs folder) and even on HMI using events.

## Data
There is a generic Frame class container that can carry an object payload.
Every Frame, once created, raises an event that the application can intercept to populate the Payload Property.
Additional data for the frame is:
* Frame ID
* TimeStamp 
* Frame Status: Reflect the state of the frame in the producer consumer:
  - unknown, Constructor default state.
  - created, Producer has the frame in charge.
  - dropped, Producer drops the information.
  - processing, Consumer has started the processing.
  - processed, Consumer has successfully processed the frame.
  - skipped, Consumer skips the data from the stack. It is an unusual condition that occurs when the consumer and producer are suddenly restarted.
  - rejected, Consumer rejected the frame because has no resources to process it.
  - aborted, Consumer rejected the frame 'cause abort operation is pending.


## Producer 
The producer continuously generates frames data and pushes them in an interlocked stack (instead of a classic queue).
It also raises an OnFrameReady event every time the frame is created, to allow the event caller to populate a generic Payload, making the class abstactable.
It updates a queue of Frame data to allow View inspection.
Finally, if a consumer is instantiated it signals the presence of new Frame data, otherwise, it drops all the frames in the stack, except the last one.

## Consumer
The consumer, once instantiated, checks the presence of a data frame in the stack.
If exists it starts processing the frame asynchronously using a task, then waits for any new data.
Every frame processed is inserted in a queue list of tasks. Higher the degree of parallelism, the more tasks can process data at the same time, making the class fully scalable.
When the maximum parallelism degree is reached the consumers start to reject frames until a task completes the processing.

## HMI
Main Form:
*TOP - Setup panel
*RIGHT - View processed frame
*CENTER - Virew configuration and data generator properties
*BOTTOM - Log View  
![Alt text](./Images/001_Main.jpg?raw=true)  
Configuration presets: set a default environment and click on Update Setup.  
![Alt text](./Images/002_Setup_config.jpg?raw=true)  
Then validate the configuration.  
![Alt text](./Images/003_Validate.jpg?raw=true)  
Start/Stop producer-consumer.  
![Alt text](./Images/004_Run.jpg?raw=true)  
![Alt text](./Images/005_Scalability.jpg?raw=true)  
Adjust producer-consumer scalability increasing the parallelism degree.
![Alt text](./Images/005_Scalability2.jpg?raw=true)  
Stop the producer-consumer to verify counters.  
![Alt text](./Images/007_results.jpg?raw=true)  


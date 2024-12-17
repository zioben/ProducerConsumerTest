# ProducerConsumerTest
This is a Producer-consumer implementation, using the c#.net 6.0 core library, with a visualization front-end.

## Purposes
This application demonstrates the capacity to produce and consume data fetching the last produced data when available.

## Data
There is a generic Frame class container that can carry an object payload.
Every Frame, once created, raises an event that the application can intercept to populate the Payload Property.
Additional data for the frame is:
-Frame ID
-TimeStamp 
-Frame Status: reflect the state of the frame in the producer consumer



## Producer Consumer
There are two threads that manages signals.
The producer continuosly generates data and push in in a interlocked stack (instead of a classic queue).
When the consumer is not instantiated the producer automatically drops the old acquisition in the frame and updates with the new data generated.
When consumer is instantiated the producer notify an AutoResetEvent


## HMI
![Alt text](./Images/001_Main.jpg?raw=true)


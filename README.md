# ProducerConsumerTest
This is a Producer-consumer implementation, using the c#.net 6.0 core library, with a visualization front-end.

## Purposes
This application demonstrates the capacity to produce and consume data fetching the last produced data when available.

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
  - skipped, Consumer skips the data from the stack. It is an unusual condition that occurs when consumer and producer are suddenly restarted.
  - rejected, Consumer rejected the frame because has no resources to process it.
  - aborted, Consumer rejected the frame 'cause abort operation is pending.


## Producer 
The producer continuously generates frames data and pushes them in an interlocked stack (instead of a classic queue).
It also raises an OnFrameReady event every time the frame is created, to allow the interceptor caller to populate a generic Payload, making the class abstactable.
It updates a queue of Frame data to allow View inspection.
Finally, if a consumer is instantiated it signals the presence of new Frame data, otherwise, it drops all the frames in the stack, except the last one.

## Consumer

There are two threads that manages signals.
The producer continuosly generates data and push in in a interlocked stack (instead of a classic queue).
When the consumer is not instantiated the producer automatically drops the old acquisition in the frame and updates with the new data generated.
When consumer is instantiated the producer notify an AutoResetEvent


## HMI
![Alt text](./Images/001_Main.jpg?raw=true)


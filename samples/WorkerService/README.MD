# Sample WorkerService

This example demonstrates how the distributed lock is work.
By default, this application creates 5 workers that do some dummy work wrapped into a distributed lock. 
So you can see there is only one worker instance do stuff in one moment even you start a few instances of the application
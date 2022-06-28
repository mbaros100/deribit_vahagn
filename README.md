Architecture and Design Workflow

On top of DeriBit api wrappers and classes implemented a program which establishes connection to the exchange and retrieves, stores data with extensive multi layer log inormation.

Here are the completed steps in the program:


1. Create a test deribit class (on top of websocket) for auxiliary data retrieval.
2. Send a test response to see if the connection is working.
3. Send a time responce to sync time with the exchange and get the diff.*
4. Get available future instruments for ETH and BTC
5. Create list of websockets, 1 per instument.**
6. Create Dictionary objects which contains bid/ask data as well as shared memory***
7. Initialize websockets and create callback function to subscribe to orderbook updates.
8. Get real-time quotes and print them in the console.
9. Send subscribe requests for all the instruments and update the underlying shared memory.****

** Needs a researchL maybe 1 socket per insrument is not effective and they can be batched
*** Bid/Ask are being saved in a sorted dictionary and then being serialized into raw memory which is being written into the shared MemoryMappedFile object. Check to make for not exceeding 20 elements while if there are less than 20, those are in advance initialized with 0.
**** Subscibtion to raw interval didn't work for me, so i subscribed to 100ms one
* Since i have subscribed to 100ms interval, i will perform separate timing tests for exchange and local timestamps and separate one for measuring the time needed to write data into shared memory. Both can be found in the log.


Future work:
1. Create unit tests for all the available and applicable functions
2. Create documentation for the internal data structures and classes
3. Better handling of writing into shared memory files
4. Create external processes that will try to extract real-time data from shared memory files
5. Apply multithreading wherever applicable in the main program loop
6. Proper testing of memory leaks and release of shared memory map objects

References and re-used Safe WebSocket apis: https://github.com/tekr/DeribitDotNet/

6/15/2018

"transparity TMDD-EC-20170927" is the sample project given by McCain. A good reference for how to do various queries

"IntersectionApi" is the project for the server

The server is a REST server with the following paths:

api/intersection
This will return the list of all intersecitons in Eugene. A JSON string is returned
representing an array of intersection name, intersection ID pairs

api/interseciton/{id}
Gets the status of the intersection with the specified id

api/intersection/register/{id}
Registers the intersection for logging and continuous updates

api/intersection/unregister/{id}
Unregisters the intersetion for logging and continuous updates

Each intersection status has the following values:
ID (string): The ID of the intersection
Name (string): Currently not used
GroupGreens (int): Current group greens value
ActivePhases (List<int>): Current phases active
AllPhases (List<PhaseInfo>): Information about all phases. See below for details about PhaseInfo

Each PhaseInfo has the following data:
PhaseID (int): The ID of the phase
MinGreen (int): Minimum green value
MaxGreen (int): Maximum green value
LastActiveTime (float)*: The length of time in seconds this phase was active for the last time it was active
CurrentActiveTime (float)*: The length of time in seconds this phase has been active for (0 if not currently active)
BecameActiveTimestamp (DateTime)*: Timestamp of when this phase became active (only set if currently active)
CurrentlyActive (bool): True if this phase is currently active, false if it isn't

* - These fields will only be set if the intersection is registered for continuous updates.
    Otherwise they wil be set to default values for each type.
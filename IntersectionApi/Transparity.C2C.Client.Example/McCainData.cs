using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Transparity.Services.C2C.Interfaces.TMDDInterface.Client;
using Transparity.Services.C2C.McCainTMDD;
using Authentication = Transparity.Services.C2C.Interfaces.TMDDInterface.Client.Authentication;
using CenterActiveVerificationRequest = Transparity.Services.C2C.Interfaces.TMDDInterface.Client.CenterActiveVerificationRequest;
using DeviceInformationRequest = Transparity.Services.C2C.Interfaces.TMDDInterface.Client.DeviceInformationRequest;
using IntersectionSignalTimingInventoryRequest = Transparity.Services.C2C.Interfaces.TMDDInterface.Client.IntersectionSignalTimingInventoryRequest;
using OrganizationInformation = Transparity.Services.C2C.Interfaces.TMDDInterface.Client.OrganizationInformation;

namespace Transparity.C2C.Client.Example
{
    public class McCainData
    {
        private const string Username = "93a7c97b-dd75-4373-9dd6-81683de20d86";
        private const string Password = "yrAteFcHBE1A05YWKjT7";
        // Path to the log file
        private const string LOGFILE_PATH = "C:\\Users\\Ben\\Desktop\\IntersectionLog\\";
        private const string ERROR_LOG_PATH = "C:\\Users\\Ben\\Desktop\\IntersectionErrorLog\\";

        private static readonly string MyExternalCenterUrl = Properties.Settings.Default.ExternalCenterUrl;
        private static readonly Guid SubscriptionId = Guid.NewGuid();
        private static string orgId = String.Empty;
        private static List<IntersectionInventoryItem> inventory = new List<IntersectionInventoryItem>();
        private static bool running = true;

        // This dictionary holds the intersections we want to continuously query
        private static Dictionary<string, IntersectionStatus> statusDictionary = new Dictionary<string, IntersectionStatus>();
        private static Thread updateThread;
        private static Object statusLock = new Object();        
        
        /// <summary>
        /// Constructor. Perform setup and start the update thread.
        /// </summary>
        public McCainData()
        {
            // Disable SSL validation, which prevents a successfull request to a service
            // with a self-signed certificate.
            DisableSelfSignedCertValidation();

            // Get the organization-id from center active verification response.
            orgId = SubmitCenterActiveVerificationRequest();
            // Register 18th and Alder for continuouse updates. 
            RegisterIntersectionForUpdates("b57d7710-5361-4d81-83a2-a73000a88971");
            // Start the update thread
            updateThread = new Thread(UpdatePhaseStatuses);
            updateThread.Start();                
        }

        // Make sure we stop the update thread
        ~McCainData()
        {
            running = false;
            try
            {
                updateThread.Join();
            }
            catch(Exception e)
            { }
        }

        /// <summary>
        /// This function will run in a seperate thread constantly updaing the status of each intersection registered for status updates
        /// </summary>
        private void UpdatePhaseStatuses()
        {
            while(running)
            {
                lock(statusLock)
                {
                    var keys = new List<string>(statusDictionary.Keys);
                    List<IntersectionStatus> statuses = GetIntersectionStatusNoLock(keys.ToArray());

                    foreach (IntersectionStatus status in statuses)
                    {
                        if (statusDictionary.ContainsKey(status.ID))
                        {
                            IntersectionStatus oldStatus = statusDictionary[status.ID];
                            bool logChange = false;

                            // If this isn't the first update for this intersection
                            if (oldStatus != null)
                            {
                                for (int i = 0; i < status.AllPhases.Count; i++)
                                {
                                    PhaseInfo oldPhase = oldStatus.AllPhases.Find(x => x.PhaseID == status.AllPhases[i].PhaseID);

                                    // Phase has become inactive
                                    if (oldPhase.CurrentlyActive && !status.AllPhases[i].CurrentlyActive)
                                    {
                                        status.AllPhases[i].CurrentActiveTime = 0f;
                                        status.AllPhases[i].LastActiveTime = (float)(DateTime.Now - oldPhase.BecameActiveTimestap).TotalSeconds;
                                        logChange = true;
                                    }
                                    // Phase has just become active
                                    else if (!oldPhase.CurrentlyActive && status.AllPhases[i].CurrentlyActive)
                                    {
                                        status.AllPhases[i].LastActiveTime = oldPhase.LastActiveTime;
                                        status.AllPhases[i].BecameActiveTimestap = DateTime.Now;
                                        logChange = true;
                                    }
                                    // No change in phase state and active
                                    else if (status.AllPhases[i].CurrentlyActive)
                                    {
                                        status.AllPhases[i].LastActiveTime = oldPhase.LastActiveTime;
                                        status.AllPhases[i].BecameActiveTimestap = oldPhase.BecameActiveTimestap;
                                        status.AllPhases[i].CurrentActiveTime = (float)(DateTime.Now - status.AllPhases[i].BecameActiveTimestap).TotalSeconds;
                                    }
                                    // No change in phase state and not active
                                    else
                                    {
                                        status.AllPhases[i] = oldPhase;
                                    }
                                }
                            }
                            else
                            {
                                foreach (int activePhase in status.ActivePhases)
                                {
                                    status.AllPhases.Find(x => x.PhaseID == activePhase).BecameActiveTimestap = DateTime.Now;
                                }
                            }

                            statusDictionary[status.ID] = status;

                            if (logChange)
                            {
                                string text = DateTime.Now.ToString("MM/dd/yyyy\tHH:mm:ss") + '\t';

                                foreach (int i in status.ActivePhases)
                                {
                                    text += i.ToString() + '\t';
                                }
                                text += '\n';

                                try
                                {
                                    if(!Directory.Exists(LOGFILE_PATH))
                                    {
                                        Directory.CreateDirectory(LOGFILE_PATH);
                                    }
                                    File.AppendAllText(LOGFILE_PATH + status.ID + ".lg", text);
                                }
                                catch (Exception e)
                                {
                                    LogError(e.Message);
                                }
                            }
                        }
                    }

                }
                // Wait one second before checking status
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Distable self signed cert validation. This was a part of the McCain project given to use. I assume it's necessary
        /// </summary>
        private void DisableSelfSignedCertValidation()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
        }

        /// <summary>
        /// Registers an intersetion for continuous updates
        /// </summary>
        /// <param name="id">ID of the intersection to register</param>
        public void RegisterIntersectionForUpdates(string id)
        {
            lock (statusLock)
            {
                if (!statusDictionary.ContainsKey(id))
                {                    
                    statusDictionary.Add(id, null);
                }
            }
        }

        /// <summary>
        /// Unregisters an intersection from receiving continuous updates
        /// </summary>
        /// <param name="id">ID of the intersection to unregister</param>
        public void UnregisterIntersectionForUpdates(string id)
        {
            lock (statusLock)
            {
                if (statusDictionary.ContainsKey(id))
                {
                    statusDictionary.Remove(id);
                }
            }
        }

        /// <summary>
        /// Gets the signal inventory for all intersection. Signal inventory = name and id of each intersection
        /// </summary>
        /// <returns>List of intersection inventory items</returns>
        public List<IntersectionInventoryItem> GetSignalInventory()
        {
            if(inventory.Count == 0)
            {
                // Create the client
                var client = new TmddEnhancedServiceClient();

                // Create the inventory request.  
                var request = new DeviceInformationRequest
                {
                    deviceinformationtype = Constants.DeviceInformationTypes.Inventory,
                    devicetype = Constants.DeviceTypes.SignalController,
                    authentication = new Authentication
                    {
                        userid = Username,
                        password = Password
                    },

                    // This is the caller's "organization id".  In a production environment,
                    // this should be assigned by the Traffic Management Center administrator.
                    organizationrequesting = new OrganizationInformation
                    {
                        organizationid = 1.ToString()
                    },

                    // This is the organization id of the organization you are requesting
                    // inventory for.  This is found by inspecting the
                    // centerActiveVerification response or the organizationInformation response.  
                    // If you omit this, you will receive inventory for all organizations 
                    // at this endpoint. This endpoint is specific to this test server and 
                    // contains only a sample city's data.
                    // Here we are simply passing it what was passed to this method.
                    organizationinformation = new OrganizationInformation()
                    {
                        organizationid = orgId
                    },
                };

                try
                {
                    var response = client.dlIntersectionSignalInventoryRequest(request);

                    for(int i = 0; i < response.intersectionsignalinventoryitem.Length; i++)
                    {
                        IntersectionInventoryItem inv = new IntersectionInventoryItem();
                        inv.Name = response.intersectionsignalinventoryitem[i].deviceinventoryheader.devicename;
                        inv.ID = response.intersectionsignalinventoryitem[i].deviceinventoryheader.deviceid;
                        inventory.Add(inv);
                    }
                }
                catch (Exception ex)
                {
                    inventory = null;
                    LogError(ex.Message);
                }
            }

            return inventory;
        }

        /// <summary>
        /// Gets the status of on an intersection without acquiring a lock. 
        /// This should only be called if the lock has already been acquired
        /// </summary>
        /// <param name="id">The ID the intersection to query</param>
        /// <returns>The intersection status</returns>
        private List<IntersectionStatus> GetIntersectionStatusNoLock(string[] id)
        {

            var client = new TmddEnhancedServiceClient();

            List<IntersectionStatus> returnStatus = new List<IntersectionStatus>();
            IntersectionSignalStatus[] status = PerformStatusQuery(id, client);

            foreach (IntersectionSignalStatus s in status)
            {
                IntersectionStatus curStatus = new IntersectionStatus();
                curStatus.ID = s.devicestatusheader.deviceid;
                IntersectionSignalTimingInventory inventory = PerformTimingInventoryQuery(s.devicestatusheader.deviceid, client);
                curStatus.Name = "";
                curStatus.GroupGreens = s.phasestatus.phasestatusgroup[0].phasestatusgroupgreens;
                curStatus.AllPhases = new List<PhaseInfo>();
                curStatus.ActivePhases = GetActivePhases(s.phasestatus.phasestatusgroup[0].phasestatusgroupgreens);

                foreach (var phase in inventory.phases.phases)
                {
                    PhaseInfo item = new PhaseInfo();
                    item.PhaseID = phase.phaseidentifier;
                    item.MinGreen = phase.MinGreen;
                    item.MaxGreen = phase.MaxLimit;
                    item.LastActiveTime = 0;
                    item.CurrentlyActive = curStatus.ActivePhases.Contains(item.PhaseID);
                    curStatus.AllPhases.Add(item);
                }
                returnStatus.Add(curStatus);
            }

            return returnStatus;
        }

        /// <summary>
        /// Gets the status of an intersection. The status is returned from the statusDictionary if it is getting continuous updates
        /// </summary>
        /// <param name="id">ID of the intersection to get the status of</param>
        /// <param name="forceQuery">If true the status will not be returned from the statusDictionary even if it exists in it</param>
        /// <returns>The intersection status</returns>
        public IntersectionStatus GetIntersectionStatus(string id, bool forceQuery = false)
        {
            lock (statusLock)
            {
                // If we are already tracking the status of the intersection return that value
                if (statusDictionary.ContainsKey(id) && !forceQuery && statusDictionary[id] != null)
                {
                    IntersectionStatus status = new IntersectionStatus();
                    status.ActivePhases = statusDictionary[id].ActivePhases;
                    status.AllPhases = statusDictionary[id].AllPhases;
                    status.GroupGreens = statusDictionary[id].GroupGreens;
                    status.ID = statusDictionary[id].ID;
                    status.Name = statusDictionary[id].Name;
                    return status;
                }

                // If we are not tracking still return the intersection status
                else
                {
                    var client = new TmddEnhancedServiceClient();

                    IntersectionStatus returnStatus = new IntersectionStatus();
                    IntersectionSignalStatus status = PerformStatusQuery(new string[] { id }, client)[0];
                    IntersectionSignalTimingInventory inventory = PerformTimingInventoryQuery(id, client);
                    returnStatus.ID = status.devicestatusheader.deviceid;
                    returnStatus.Name = "";
                    returnStatus.GroupGreens = status.phasestatus.phasestatusgroup[0].phasestatusgroupgreens;
                    returnStatus.AllPhases = new List<PhaseInfo>();
                    returnStatus.ActivePhases = GetActivePhases(status.phasestatus.phasestatusgroup[0].phasestatusgroupgreens);

                    foreach (var phase in inventory.phases.phases)
                    {
                        PhaseInfo item = new PhaseInfo();
                        item.PhaseID = phase.phaseidentifier;
                        item.MinGreen = phase.MinGreen;
                        item.MaxGreen = phase.MaxLimit;
                        item.LastActiveTime = 0;
                        item.CurrentlyActive = returnStatus.ActivePhases.Contains(item.PhaseID);
                        returnStatus.AllPhases.Add(item);
                    }

                    return returnStatus;
                }
            }
        }

        /// <summary>
        /// Transforms a groupGreens value into a list of active intersection IDs
        /// Group greens specifies the active phases by the bits that are set to 1, where the lease significant bit is phase 1
        /// For example if groupGreen = 37
        /// 36 in binary = 00100101
        /// So phases 1, 3, and 6 are active
        /// </summary>
        /// <param name="groupGreens">The group greens value</param>
        /// <returns>List of active phases IDs</returns>
        private List<int> GetActivePhases(byte groupGreens)
        {
            byte mask = 1;
            List<int> activePhases = new List<int>();

            for(int i = 0; i < 8; i++)
            {
                if((groupGreens & mask) == 1)
                {
                    activePhases.Add(i + 1);
                }
                groupGreens = (byte)(groupGreens >> 1);
            }

            return activePhases;
        }

        /// <summary>
        /// Queries the TMDD service for the current status of the specified intersections
        /// </summary>
        /// <param name="id">The intersections ID to query</param>
        /// <param name="client">The TMDD client</param>
        /// <returns>The intersection status</returns>
        private IntersectionSignalStatus[] PerformStatusQuery(string[] id, TmddEnhancedServiceClient client)
        {
            IntersectionSignalStatus[] status = null;

            // Create the inventory request.  
            var request = new DeviceInformationRequest
            {
                deviceinformationtype = Constants.DeviceInformationTypes.Status,
                devicetype = Constants.DeviceTypes.SignalController,
                authentication = new Authentication
                {
                    userid = Username,
                    password = Password
                },

                // This is the caller's "organization id".  In a production environment,
                // this should be assigned by the Traffic Management Center administrator.
                organizationrequesting = new OrganizationInformation
                {
                    organizationid = 1.ToString()
                },

                // This is the organization id of the organization you are requesting
                // inventory for.  This is found by inspecting the
                // centerActiveVerification response or the organizationInformation response.  
                // If you omit this, you will receive inventory for all organizations 
                // at this endpoint. This endpoint is specific to this test server and 
                // contains only a sample city's data.
                // Here we are simply passing it what was passed to this method.
                organizationinformation = new OrganizationInformation()
                {
                    organizationid = orgId
                },
                // Filter the request to only get information about the desired intersection id
                devicefilter = new DeviceInformationRequestFilter()
                {
                    deviceidlist = new DeviceInformationRequestFilterDeviceidlist()
                    {
                        deviceid =  id
                    }
                },
            };

            try
            {
                var response = client.dlIntersectionSignalStatusRequest(request);

                // What this message returns is an array of IntersectionSignalStatus items.
                // Iterate through the collection to inspect the objects.
                if (response.intersectionsignalstatusitem != null)
                {
                    status = response.intersectionsignalstatusitem;
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                status = null;
            }
            return status;
        }

        /// <summary>
        /// Queries the TMDD client for the timing inventory of an intersection
        /// </summary>
        /// <param name="id">ID of the intersection</param>
        /// <param name="client">TMDD client</param>
        /// <returns>The instersection signal timing inventory</returns>
        private IntersectionSignalTimingInventory PerformTimingInventoryQuery(string id, TmddEnhancedServiceClient client)
        {
            IntersectionSignalTimingInventory returnValue = null;

            var request = new IntersectionSignalTimingInventoryRequest
            {
                deviceinformationrequestheader = new DeviceInformationRequest
                {
                    deviceinformationtype = Constants.DeviceInformationTypes.Inventory,
                    devicetype = Constants.DeviceTypes.SignalController,
                    authentication = new Authentication
                    {
                        userid = Username,
                        password = Password
                    },

                    // This is the caller's "organization id".  In a production environment,
                    // this should be assigned by the Traffic Management Center administrator.
                    organizationrequesting = new OrganizationInformation
                    {
                        organizationid = 1.ToString()
                    },

                    // This is the organization id of the organization you are requesting
                    // inventory for.  This is found by inspecting the
                    // centerActiveVerification response or the organizationInformation response.  
                    // If you omit this, you will receive inventory for all organizations 
                    // at this endpoint. This endpoint is specific to this test server and 
                    // contains only a sample city's data.
                    // Here we are simply passing it what was passed to this method.
                    organizationinformation = new OrganizationInformation()
                    {
                        organizationid = orgId
                    },
                    devicefilter = new DeviceInformationRequestFilter()
                    {
                        deviceidlist = new DeviceInformationRequestFilterDeviceidlist()
                        {
                            deviceid = new[] { id }
                        }
                    },
                }
            };

            try
            {
                var response = client.dlIntersectionSignalTimingInventoryRequest(request);
                if (response.intersectionsignaltiminginventoryitem.Length > 0)
                {
                    returnValue = response.intersectionsignaltiminginventoryitem[0];
                }
                
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
            return returnValue;
        }

        /// <summary>
        /// Request information about the Traffic Management Center
        /// </summary>
        /// <returns>The organization id</returns>
        private string SubmitCenterActiveVerificationRequest()
        {
            var client = new TmddEnhancedServiceClient();

            var request = new CenterActiveVerificationRequest
            {
                authentication = new Authentication
                {
                    userid = Username,
                    password = Password
                },
                organizationrequesting = new OrganizationInformation
                {
                    organizationid = 1.ToString()
                }
            };

            try
            {
                var response = client.dlCenterActiveVerificationRequest(request);
                return response.organizationinformation.organizationid;
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                return null;
            }
        }

        private void LogError(string error)
        {
            if(!Directory.Exists(ERROR_LOG_PATH))
            {
                Directory.CreateDirectory(ERROR_LOG_PATH);
            }
            File.AppendAllText(ERROR_LOG_PATH + "error_log.txt", error);
        }
    }
}

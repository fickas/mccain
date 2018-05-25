using System;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using Transparity.Services.C2C.Interfaces.TMDDInterface.Client;
using Transparity.Services.C2C.McCainTMDD;
using Authentication = Transparity.Services.C2C.Interfaces.TMDDInterface.Client.Authentication;
using C2cMessageSubscription = Transparity.Services.C2C.Interfaces.TMDDInterface.Client.C2cMessageSubscription;
using CenterActiveVerificationRequest = Transparity.Services.C2C.Interfaces.TMDDInterface.Client.CenterActiveVerificationRequest;
using DeviceInformationRequest = Transparity.Services.C2C.Interfaces.TMDDInterface.Client.DeviceInformationRequest;
using IntersectionSignalTimingInventoryRequest = Transparity.Services.C2C.Interfaces.TMDDInterface.Client.IntersectionSignalTimingInventoryRequest;
using IntersectionSignalTimingPatternInventoryRequest = Transparity.Services.C2C.Interfaces.TMDDInterface.Client.IntersectionSignalTimingPatternInventoryRequest;
using ITmddOCEnhancedService = Transparity.Services.C2C.Interfaces.TMDDInterface.ITmddOCEnhancedService;
using OrganizationInformation = Transparity.Services.C2C.Interfaces.TMDDInterface.Client.OrganizationInformation;
using SubscriptionAction = Transparity.Services.C2C.Interfaces.TMDDInterface.Client.SubscriptionAction;
using SubscriptionType = Transparity.Services.C2C.Interfaces.TMDDInterface.Client.SubscriptionType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transparity.C2C.Client.Example
{
    public class McCainData
    {
        private const string Username = "93a7c97b-dd75-4373-9dd6-81683de20d86";
        private const string Password = "yrAteFcHBE1A05YWKjT7";

        private static readonly string MyExternalCenterUrl = Properties.Settings.Default.ExternalCenterUrl;
        private static readonly Guid SubscriptionId = Guid.NewGuid();
        private string orgId = String.Empty;
        private List<IntersectionInventoryItem> inventory = new List<IntersectionInventoryItem>();
        

        public McCainData()
        {
            // Disable SSL validation, which prevents a successfull request to a service
            // with a self-signed certificate.
            DisableSelfSignedCertValidation();

            // Get the organization-id from center active verification response.
            orgId = SubmitCenterActiveVerificationRequest();
        }

        private void DisableSelfSignedCertValidation()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
        }

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
                    // TODO: Log error
                }
            }

            return inventory;
        }

        public IntersectionStatus GetIntersectionStatus(string id)
        {
            
            var client = new TmddEnhancedServiceClient();

            IntersectionStatus returnStatus = new IntersectionStatus();
            IntersectionSignalStatus status = PerformStatusQuery(id, client);
            IntersectionSignalTimingInventory inventory = PerformTimingInventoryQuery(id, client);
            returnStatus.ID = status.devicestatusheader.deviceid;
            returnStatus.Name = "";
            returnStatus.GroupGreens = status.phasestatus.phasestatusgroup[0].phasestatusgroupgreens;
            returnStatus.ActivePhases = new List<PhaseInfo>();
            returnStatus.AllPhases = new List<PhaseInfo>();
            List<int> activePhases = GetActivePhases(status.phasestatus.phasestatusgroup[0].phasestatusgroupgreens);

            foreach(var phase in inventory.phases.phases)
            {
                PhaseInfo item = new PhaseInfo();
                item.PhaseID = phase.phaseidentifier;
                item.MinGreen = phase.MinGreen;
                item.MaxGreen = phase.MaxLimit;
                returnStatus.AllPhases.Add(item);

                if(activePhases.Contains(item.PhaseID))
                {
                    returnStatus.ActivePhases.Add(item);
                }
            }

            return returnStatus;
        }

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

        private IntersectionSignalStatus PerformStatusQuery(string id, TmddEnhancedServiceClient client)
        {
            IntersectionSignalStatus status = null;

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
                        deviceid = new[] { id }
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
                    status = response.intersectionsignalstatusitem[0];
                }
            }
            catch (Exception ex)
            {
                //TODO: log error
                status = null;
            }
            return status;
        }

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
                            deviceid = new[] { "b3c0fe11-bbe0-4dd2-9a6d-a77700e13754" }
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
                // TODO: Log error
            }
            return returnValue;
        }

        /// <summary>
        /// Request information about the Traffic Management Center
        /// </summary>
        /// <returns></returns>
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
                // TODO: Log error
                return null;
            }
        }
    }
}

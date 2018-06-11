//
//
// THIS IS NOT USED. THIS WAS GIVEN AS AN EXAMPLE FROM MCCAIN
//
//
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

namespace Transparity.C2C.Client.Example
{
    public class Program
    {
        private const string Username = "93a7c97b-dd75-4373-9dd6-81683de20d86";
        private const string Password = "yrAteFcHBE1A05YWKjT7";

        private static readonly string MyExternalCenterUrl = Properties.Settings.Default.ExternalCenterUrl;
        private static readonly Guid SubscriptionId = Guid.NewGuid();
        private static Guid _firstIntersectionSignalId;

        static void Main(string[] args)
        {
            Console.WriteLine("{0} started.", typeof(Program).FullName);

            // Disable SSL validation, which prevents a successfull request to a service
            // with a self-signed certificate.
            DisableSelfSignedCertValidation();

            // Get the organization-id from center active verification response.
            var orgId = SubmitCenterActiveVerificationRequest();

            // Get the signal inventory for this organization.
            if (AskToPerform("IntersectionSignalInventoryRequest")) SubmitIntersectionSignalInventoryRequest(orgId);

            // Get the signal inventory for this organization.
            if (AskToPerform("IntersectionSignalStatusRequest")) SubmitIntersectionSignalStatusRequest(orgId);

            // Get the signal inventory for this organization.
            if (AskToPerform("InventorySignalTimingPatternRequest")) SubmitInventorySignalTimingPatternRequest(orgId);

            // Get the signal inventory for this organization.
            if (AskToPerform("InventorySignalTimingInventoryRequest")) SubmitInventorySignalTimingInventoryRequest(orgId);

            Console.WriteLine("\n{0} OC calls finished.  Press Y to continue to subscription calls.\nPress any other key to exit.\n\n\tNote: In order to receive data from the publications,\n\tyou need to be running the External Center app.\n", typeof(Program).FullName);
            var key = Console.ReadKey();
            if (!key.Key.Equals(ConsoleKey.Y))
            {
                Environment.Exit(0);
            }

            // Cancel all prior subscriptions.
            if (AskToPerform("IntersectionStatusSubscriptionRequest (CancelAllPriorSubscriptions)")) SubmitIntersectionStatusSubscriptionRequest(orgId, SubscriptionActionEnum.CancelAllPriorSubscriptions);

            // Create a subscription for status updates from the first signal.
            if (AskToPerform("IntersectionStatusSubscriptionRequest (NewSubscription)")) SubmitIntersectionStatusSubscriptionRequest(orgId, SubscriptionActionEnum.NewSubscription);

            // At this point, your External Center should start receiving updates based on
            // the subscriptions you created.

            Console.WriteLine($"\nSubscription {SubscriptionId} has been created.");

            // Create a subscription for status updates from the first signal.
            if (AskToPerform("IntersectionStatusSubscriptionRequest (CancelSubscription)")) SubmitIntersectionStatusSubscriptionRequest(orgId, SubscriptionActionEnum.CancelSubscription);

            Console.WriteLine("\n{0} finished.  Press Ctrl-C to exit.", typeof(Program).FullName);
            using (var e = new ManualResetEvent(false))
            {
                e.WaitOne();
            }
        }

        private static bool AskToPerform(string methodName)
        {
            Console.WriteLine("\nCall {0}? [Y]/n", methodName);

            var performCall = false;
            var keyinfo = Console.ReadKey();
            while (keyinfo.Key != ConsoleKey.Y && keyinfo.Key != ConsoleKey.N)
            {
                Console.WriteLine("\tInvalid key.  Please select Y or N.");
                keyinfo = Console.ReadKey();
            }
            if (keyinfo.Key == ConsoleKey.Y) performCall = true;

            return performCall;
        }

        /// <summary>
        /// Request information about the Traffic Management Center
        /// </summary>
        /// <returns></returns>
        private static string SubmitCenterActiveVerificationRequest()
        {
            Console.WriteLine("\nSubmitting CenterActiveVerification request...");

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
                Console.WriteLine("Center Id: {0}\nCenter Name: {1}",
                    response.organizationinformation.organizationid,
                    response.organizationinformation.organizationname);

                return response.organizationinformation.organizationid;
            }
            catch (FaultException fe)
            {
                Console.WriteLine("Fault exception encountered: {0}", fe.Message);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception encountered: {0}", ex.Message);
                return null;
            }

        }

        /// <summary>
        /// Request list of traffic controllers from Traffic Management Center
        /// </summary>
        /// <param name="orgId"></param>
        private static void SubmitIntersectionSignalInventoryRequest(string orgId)
        {
            Console.WriteLine("\nSubmitting IntersectionSignalInventoryRequest...");

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

                // What this message returns is an array of IntersectionSignalInventory items.
                // Iterate through the collection to inspect the objects.
                for (int i = 0; i < response.intersectionsignalinventoryitem.Length; i++)
                {
                    var item = response.intersectionsignalinventoryitem[i];

                    // Save this ID so we can create a subscription using it later.
                    if (i == 0 && Guid.TryParse(item.deviceinventoryheader.deviceid, out var intersectionId))
                    {
                        _firstIntersectionSignalId = intersectionId;
                    }

                    Console.WriteLine(
                        "\nOrganization ID: {0}\nOrganization Name: {1}\nDevice Id: {2}\nDevice Name: {3}\nDevice Location: {4}, {5}",
                        item.deviceinventoryheader.organizationinformation.organizationid,
                        item.deviceinventoryheader.organizationinformation.organizationname,
                        item.deviceinventoryheader.deviceid,
                        item.deviceinventoryheader.devicename,
                        item.deviceinventoryheader.devicelocation.latitude,
                        item.deviceinventoryheader.devicelocation.longitude);
                }
            }
            catch (FaultException fe)
            {
                Console.WriteLine("Fault exception encountered: {0}", fe.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception encountered: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Request list of traffic controller status from Traffic Management Center
        /// </summary>
        /// <param name="orgId"></param>
        private static void SubmitIntersectionSignalStatusRequest(string orgId)
        {
            Console.WriteLine("\nSubmitting IntersectionSignalStatusRequest...");

            // Create the client
            var client = new TmddEnhancedServiceClient();

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
            };

            try
            {
                var response = client.dlIntersectionSignalStatusRequest(request);

                // What this message returns is an array of IntersectionSignalStatus items.
                // Iterate through the collection to inspect the objects.
                if (response.intersectionsignalstatusitem == null)
                {
                    Console.WriteLine("No intersection signal status found.");
                    return;
                }

                for (int i = 0; i < response.intersectionsignalstatusitem.Length; i++)
                {
                    var item = response.intersectionsignalstatusitem[i];

                    // Save this ID so we can create a subscription using it later.
                    if (i == 0 && Guid.TryParse(item.devicestatusheader.deviceid, out var intersectionId))
                    {
                        _firstIntersectionSignalId = intersectionId;
                    }

                    Console.WriteLine(
                        "\nOrganization ID: {0}\nOrganization Name: {1}\nDevice Id: {2}\nStatus: {3}\nCurrent Pattern: {4}\nPhase Greens: {5}",
                        item.devicestatusheader.organizationinformation.organizationid,
                        item.devicestatusheader.organizationinformation.organizationname,
                        item.devicestatusheader.deviceid,
                        item.devicestatusheader.devicestatus,
                        item.timingpatternidcurrent,
                        (item.phasestatus?.phasestatusgroup != null && item.phasestatus.phasestatusgroup.Any())
                            ? item.phasestatus.phasestatusgroup[0].phasestatusgroupgreens.ToString()
                            : "Unknown"
                    );
                }
            }
            catch (FaultException fe)
            {
                Console.WriteLine("Fault exception encountered: {0}", fe.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception encountered: {0}", ex.Message);
            }
        }

        private static void SubmitInventorySignalTimingPatternRequest(string orgId)
        {
            Console.WriteLine("\nSubmitting InventorySignalTimingPatternRequest...");

            // Create the client
            var client = new TmddEnhancedServiceClient();

            var request = new IntersectionSignalTimingPatternInventoryRequest
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
                }
            };

            try
            {
                var response = client.dlIntersectionSignalTimingPatternInventoryRequest(request);

                // What this message returns is an array of IntersectionSignalTimingPatternInventory items.
                // Iterate through the collection to inspect the objects.
                foreach (var item in response.intersectionsignaltimingpatterninventoryitem)
                {
                    // Print out some retrieved data
                    if (item.stagestplist?.stages != null && item.stagestplist.stages.Any())
                    {
                        Console.WriteLine(
                            "\nOrganization ID: {0}\nOrganization Name: {1}\nDevice Id: {2}\nPattern: {3}\nCycle Length: {4},\nOffset: {5}\nStage 1 Duration: {6}",
                            item.organizationinformation.organizationid,
                            item.organizationinformation.organizationname,
                            item.deviceid,
                            item.timingpatternid,
                            item.cyclelength,
                            item.offsettime,
                            item.stagestplist.stages[0].Duration);
                    }
                    else
                    {
                        // Gather sequence info, if any
                        var r1B1 = string.Empty;
                        if (item.sequenceinformation?.sequenceinformation != null &&
                            item.sequenceinformation.sequenceinformation.Any() &&
                            item.sequenceinformation.sequenceinformation[0].sequencedata.BarrierData != null &&
                            item.sequenceinformation.sequenceinformation[0].sequencedata.BarrierData.Any())
                        {
                            r1B1 = string.Join(",", item.sequenceinformation.sequenceinformation[0].sequencedata.BarrierData[0].phaseidentifier);
                        }

                        if (r1B1 != string.Empty)
                            Console.WriteLine(
                                $"\nOrganization ID: {item.organizationinformation.organizationid}\nOrganization Name: {item.organizationinformation.organizationname}\nDevice Id: {item.deviceid}\nPattern: {item.timingpatternid}\nCycle Length: {item.cyclelength},\nOffset: {item.offsettime}\nRing 1 Barrier 1 Sequence: {r1B1}");
                        else
                            Console.WriteLine(
                                $"\nOrganization ID: {item.organizationinformation.organizationid}\nOrganization Name: {item.organizationinformation.organizationname}\nDevice Id: {item.deviceid}\nPattern: {item.timingpatternid}\nCycle Length: {item.cyclelength},\nOffset: {item.offsettime}");
                    }
                }
            }
            catch (FaultException fe)
            {
                Console.WriteLine("Fault exception encountered: {0}", fe.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception encountered: {0}", ex.Message);
            }
        }

        private static void SubmitInventorySignalTimingInventoryRequest(string orgId)
        {
            Console.WriteLine("\nSubmitting InventorySignalTimingInventoryRequest...");

            // Create the client
            var client = new TmddEnhancedServiceClient();

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
                }
            };

            try
            {
                var response = client.dlIntersectionSignalTimingInventoryRequest(request);

                // What this message returns is an array of IntersectionSignalTimingPatternInventory items.
                // Iterate through the collection to inspect the objects.
                foreach (var item in response.intersectionsignaltiminginventoryitem)
                {
                    // Print out some retrieved data
                    if (item.intervals?.intervals != null && item.intervals.intervals.Any())
                    {
                        Console.WriteLine(
                            "\nOrganization ID: {0}\nOrganization Name: {1}\nDevice Id: {2}\nInterval #: {3}\nInterval Time: {4}",
                            item.organizationinformation.organizationid,
                            item.organizationinformation.organizationname,
                            item.deviceid,
                            item.intervals.intervals[0].intervalidentifier,
                            item.intervals.intervals[0].IntervalTime);
                    }
                    else if (item.phases != null && item.phases.phases.Any())
                    {
                        Console.WriteLine(
                            @"\nOrganization ID: {0}\nOrganization Name: {1}\nDevice Id: {2}\Phase 1 Min Green: {3}",
                            item.organizationinformation.organizationid,
                            item.organizationinformation.organizationname,
                            item.deviceid,
                            item.phases.phases[0].MinGreen);
                    }
                }
            }
            catch (FaultException fe)
            {
                Console.WriteLine("Fault exception encountered: {0}", fe.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception encountered: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Create a subscription for intersection signal status.  In this request, we will ask
        /// for new status to be sent to our External Center (at the specified returnAddress)
        /// on status change.  
        /// 
        /// In our example, we will only request status for the first traffic controller 
        /// received in the prior service call; however, this subscription could easily have
        /// requested for multiple controllers or all controllers (blank devicefilter).
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="action"></param>
        private static void SubmitIntersectionStatusSubscriptionRequest(string orgId, SubscriptionActionEnum action)
        {
            try
            {
                var c2CMessageSubscription = new C2cMessageSubscription()

                {
                    informationalText = "Example informationalText",
                    subscriptionID = $"{SubscriptionId}",
                    returnAddress = MyExternalCenterUrl,
                    subscriptionAction = new SubscriptionAction
                    {
                        subscriptionActionitem = new[]
                        {
                           action.ToString(),
                        }
                    },
                    subscriptionType = new SubscriptionType()
                    {
                        subscriptionTypeitem = SubscriptionTypeEnum.Periodic.ToString()
                    },
                    subscriptionFrequency = 5
                };
                var deviceInformationRequest = new DeviceInformationRequest()
                {
                    organizationinformation = new OrganizationInformation()
                    {
                        organizationid = $"{orgId}"
                    },
                    authentication = new Authentication()
                    {
                        userid = Username,
                        password = Password
                    },
                    //devicefilter = new DeviceInformationRequestFilter()
                    //{
                    //    deviceidlist = new DeviceInformationRequestFilterDeviceidlist()
                    //    {
                    //        deviceid = new[] { _firstIntersectionSignalId.ToString() }
                    //    }
                    //},
                    devicetype = Constants.DeviceTypes.SignalController,
                    deviceinformationtype = Constants.DeviceInformationTypes.Status
                };

                // Create the client
                var client = new TmddEnhancedServiceClient();

                var response = client.dlDeviceInformationSubscription(c2CMessageSubscription, deviceInformationRequest);
                Console.WriteLine($"Response: {response.informationalText}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void DisableSelfSignedCertValidation()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
        }
    }
}

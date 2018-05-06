using System;
using System.Collections.Generic;
using System.Linq;
using Transparity.Services.C2C.Interfaces.TMDDInterface;
using Transparity.Services.C2C.Utils;

namespace Transparity.Services.C2C.McCainTMDD.ExCenter
{
    public class McCainSvc : ItmddECSoapHttpServicePortType
    {
        private const string Banner = "**********************************************************************";
        public McCainSvc() { }

        #region ImplementedMethods
        public dlCenterActiveVerificationSubscriptionResponse dlDetectorInventoryUpdate(dlDetectorInventoryUpdateRequest request)
        {
            var strlist = new List<string>
            {
                $"     request.detectorInventoryMsg.detectorinventoryitem[0].detectorinventorylist.detector.Length: {request.detectorInventoryMsg?.detectorinventoryitem?[0].detectorinventorylist?.detector?.Length}"
            };
            return ProcessRequest("dlDetectorInventoryUpdate", request.c2cMessagePublication, strlist);
        }

        public dlCenterActiveVerificationSubscriptionResponse dlDetectorDataUpdate(dlDetectorDataUpdateRequest request)
        {
            var strlist = new List<string>
            {
                $"     request.detectorDataMsg.detectordataitem[0].detectordatalist.detectordatadetail.Length: {request.detectorDataMsg?.detectordataitem?[0].detectordatalist?.detectordatadetail?.Length}"
            };
            return ProcessRequest("dlDetectorDataUpdate", request.c2cMessagePublication, strlist);
        }

        public dlCenterActiveVerificationSubscriptionResponse dlIntersectionSignalInventoryUpdate(dlIntersectionSignalInventoryUpdateRequest request)
        {
            var strlist = new List<string>
            {
                $"     request.intersectionSignalInventoryMsg.intersectionsignalinventoryitem.Length: {request.intersectionSignalInventoryMsg?.intersectionsignalinventoryitem?.Length}"
            };
            return ProcessRequest("dlIntersectionSignalInventoryUpdate", request.c2cMessagePublication, strlist);
        }

        public dlCenterActiveVerificationSubscriptionResponse dlIntersectionSignalStatusUpdate(dlIntersectionSignalStatusUpdateRequest request)
        {
            var strlist = new List<string>();
            if (request?.intersectionSignalStatusMsg?.intersectionsignalstatusitem != null)
                strlist.AddRange(request.intersectionSignalStatusMsg.intersectionsignalstatusitem.Select(status => status.devicestatusheader.deviceid));
            strlist.Add($"     request.intersectionSignalStatusMsg.intersectionsignalstatusitem.Length: {request?.intersectionSignalStatusMsg?.intersectionsignalstatusitem?.Length}");

            if (request?.intersectionSignalStatusMsg?.intersectionsignalstatusitem?.Length > 0)
            {
                var item = request?.intersectionSignalStatusMsg?.intersectionsignalstatusitem?[0];
                strlist.Add($"     Item 1: {item.controllertimestamp}, {item.timingpatternidcurrent}, {item.signalcontrolsource}");
            }

            return ProcessRequest("dlIntersectionSignalStatusUpdate", request?.c2cMessagePublication, strlist);
        }
        #endregion

        #region Private Helper Functions

        private dlCenterActiveVerificationSubscriptionResponse ProcessRequest(string updatemethod, C2cMessagePublication messagepub, IEnumerable<string> messages)
        {
            try
            {
                Console.WriteLine($@"{DateTime.Now}  {Banner}");
                Console.WriteLine($"   {updatemethod} invoked ...");
                Console.WriteLine($"   {updatemethod}Request contents:");
                foreach (var message in messages)
                    Console.WriteLine(message);

                Console.WriteLine();
                return GenericCAVSResponse($"{updatemethod} response", messagepub);
            }
            catch (Exception ex)
            {
                ServiceErrorHandler(ex);
            }
            return null;
        }

        private void ServiceErrorHandler(Exception ex)
        {
            // Pass through if ex is a FaultException
            if (ex.GetType().FullName.Contains("ErrorReport"))
                throw ex;

            // put logging here 
            var msg = $"internal server error: {ex.Message}";
            C2CUtils.ThrowFaultException(msg, msg, msg);
        }

        private dlCenterActiveVerificationSubscriptionResponse GenericCAVSResponse(string message, C2cMessagePublication messpub)
        {
            Console.WriteLine(message);
            var messpubmessage =
                $@"InformationalText: {messpub.informationalText}
                SubscriptionID: {messpub.subscriptionID}
                SubscriptionName: {messpub.subscriptionName}
                SubscriptionFrequency: {messpub.subscriptionFrequency}";
            C2CUtils.PrintRequestHeaderToConsole(messpubmessage);
            Console.WriteLine();
            Console.WriteLine();
            return new dlCenterActiveVerificationSubscriptionResponse()
            {
                c2cMessageReceipt = new C2cMessageReceipt()
                {
                    informationalText = message
                }
            };
        }
        #endregion

        public dlCenterActiveVerificationSubscriptionResponse dlCCTVInventoryUpdate(dlCCTVInventoryUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlCCTVStatusUpdate(dlCCTVStatusUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlCenterActiveVerificationUpdate(dlCenterActiveVerificationUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlDetectorStatusUpdate(dlDetectorStatusUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlDMSInventoryUpdate(dlDMSInventoryUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlDMSMessageInventoryUpdate(dlDMSMessageInventoryUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlDMSStatusUpdate(dlDMSStatusUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlESSInventoryUpdate(dlESSInventoryUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlESSStatusUpdate(dlESSStatusUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlESSObservationReportUpdate(dlESSObservationReportUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlFullEventUpdateUpdate(dlFullEventUpdateUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlEventIndexUpdate(dlEventIndexUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlActionLogUpdate(dlActionLogUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlGateInventoryUpdate(dlGateInventoryUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlGateStatusUpdate(dlGateStatusUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlGateControlScheduleUpdate(dlGateControlScheduleUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlHARInventoryUpdate(dlHARInventoryUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlHARMessageInventoryUpdate(dlHARMessageInventoryUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlHARStatusUpdate(dlHARStatusUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlHARControlScheduleUpdate(dlHARControlScheduleUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlIntersectionSignalTimingPatternInventoryUpdate(dlIntersectionSignalTimingPatternInventoryUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlIntersectionSignalControlScheduleUpdate(dlIntersectionSignalControlScheduleUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlLCSInventoryUpdate(dlLCSInventoryUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlLCSStatusUpdate(dlLCSStatusUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlLCSControlScheduleUpdate(dlLCSControlScheduleUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlOrganizationInformationUpdate(
            dlOrganizationInformationUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlRampMeterInventoryUpdate(dlRampMeterInventoryUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlRampMeterStatusUpdate(dlRampMeterStatusUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlRampMeterControlScheduleUpdate(
            dlRampMeterControlScheduleUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlRampMeterPlanInventoryUpdate(
            dlRampMeterPlanInventoryUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlSectionStatusUpdate(dlSectionStatusUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlSectionControlScheduleUpdate(
            dlSectionControlScheduleUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlSectionSignalTimingPatternInventoryUpdate(
            dlSectionSignalTimingPatternInventoryUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlLinkInventoryUpdate(dlLinkInventoryUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlLinkStatusUpdate(dlLinkStatusUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlNodeInventoryUpdate(dlNodeInventoryUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlNodeStatusUpdate(dlNodeStatusUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlRouteInventoryUpdate(dlRouteInventoryUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlRouteStatusUpdate(dlRouteStatusUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlVideoSwitchInventoryUpdate(dlVideoSwitchInventoryUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public dlCenterActiveVerificationSubscriptionResponse dlVideoSwitchStatusUpdate(dlVideoSwitchStatusUpdateRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
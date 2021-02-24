using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanPriorityQueue
{
    public static class FC
    {
        public static class EMFields
        {
            public const string investor = "VEND.X263";
            public const string fm_LoanDocType = "MORNET.X40";
            public const string loanType = "1172";
            public const string cdSentDate = "3977";
            public const string ms_LastFinished = "Log.MS.LastCompleted";
            public const string loanFolder = "LOANFOLDER";
            public const string ms_CompletionDate = "MS.STATUSDATE"; //TODO: Make sure this is the correct field to be used for 'Milestone Completion Date'
            public const string occupancyType = "3335"; //TODO: check field ID
        }

        public static class CustFields
        {
            public const string serviceTrackerStatus = "CX.SERVICE.TRACKER.STATUS";
            public const string hotlistPriorityNumber = "CX.HL.PRIORITY.NUMB";
            public const string ms_ConditionalApprovalDate = "CX.MILEST.CONDAPP.FIRSTDATE"; //Milestone Conditional Approval Date
            public const string newLOCondDate = "CX.LOCONDITIONS.NEW.DATE";
            public const string loConditionsCompletionDate = "CX.LOCONDITIONS.DATE";
            public const string pscEscalationToLO = "CX.ST.ESCALATION.LO";
            public const string cdSignedDate = "CX.CD.INITIAL.SIGN.DATE"; //TODO: Make sure this is correct field
            public const string loSchedulingRequest = "CX.SCH.LOREQUEST"; //TODO: Check field ID. Checkbox Field.
            public const string scheduledClosingDate = "CX.SCHEDULED.CLOSING.DATE"; //TODO: Check field id
            public const string titleReceivedDate = "CX.ST.TITLE.REC.DATE"; //TODO: check field id
            public const string nextPaymentDue = "CX.PAYOFF.NEXTPMTDUE"; //TODO: check field id -- MONTHDAY
            public const string borrUnresponsive = "CX.BORR.UNRESPONSIVE";

            public static class LOA
            {
                public const string loa_LastStatusTime = "CX.LOA.CT.STATUS.TIME";
                public const string loa_LastContactDate = "CX.LOA.CT.STATUS.DATE";
                public const string loa_LastConfirmedContactDate = "CX.LOA.CT.LCONF.STATUS.DATE";
                public const string loa_LastCallStatus = "CX.LOA.CT.STATUS";
            }
        }

        public static class MSNames
        {
            public const string docSigning = "Doc Signing";
            public const string started = "Started";
            public const string funding = "Funding";
            public const string completion = "Completion";
        }
    }
}

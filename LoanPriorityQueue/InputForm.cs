using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EllieMae.Encompass.Automation;
using EllieMae.Encompass.BusinessObjects.Loans;
using EllieMae.Encompass.Forms;

namespace LoanPriorityQueue
{
    public class InputForm : Form
    {
        internal Button btnTest = null;

        private List<string> emDateValues;
        private DateTime _azLastConfirmedContactDate;
        private DateTime _azLastContactDate;
        private DateTime _expectedClosingDate;
        private DateTime _msBookSentDownDate;
        private Loan _loan = EncompassApplication.CurrentLoan;

        public InputForm()
        {
            Load += new EventHandler(FormLoadEvent);
        }

        public override void CreateControls()
        {
            btnTest = FindControl("btnTest") as Button;
            btnTest.Click += new EventHandler(btnTest_Click);
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            PriorityOneHundred();
            //PriorityTwoHundred();
        }

        private void FormLoadEvent(object sender, EventArgs e)
        {
            ParseDates();
        }

        private void ParseDates()
        {
            if (string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.LOA.loa_LastConfirmedContactDate].UnformattedValue) == false)
            {
                var lastConfirmedContactDate = DateTime.Parse(_loan.Fields[FC.CustFields.LOA.loa_LastConfirmedContactDate].UnformattedValue);
                _azLastConfirmedContactDate = ConvertToArizonaTime(lastConfirmedContactDate);
            }

            if (string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.LOA.loa_LastContactDate].UnformattedValue) == false)
            {
                var lastContactDate = DateTime.Parse(_loan.Fields[FC.CustFields.LOA.loa_LastContactDate].UnformattedValue);
                _azLastContactDate = ConvertToArizonaTime(lastContactDate);
            }

//             if (string.IsNullOrWhiteSpace(_loan.Fields["Expected Closing Date"].UnformattedValue) == false)
//             {
//                 var expectedClosingDate = DateTime.Parse(_loan.Fields["Expected Closing Date"].UnformattedValue);
//                 _expectedClosingDate = ConvertToArizonaTime(expectedClosingDate);
//             }
// 
//             if (string.IsNullOrWhiteSpace(_loan.Fields["Milestone Book Sent Down Date"].UnformattedValue) == false)
//             {
//                 var msBookSentDownDate = DateTime.Parse(_loan.Fields["Milestone Book Sent Down Date"].UnformattedValue);
//                 _msBookSentDownDate = ConvertToArizonaTime(msBookSentDownDate);
//             }
        }

        private void PriorityUnderOneHundred()
        {

        }

        private void PriorityOneHundred()
        {
            /*		--When  IFNULL(`LOA Last Contact Date`,1)<>1
					--	AND IFNULL(`LOA Last Confirmed Contact Date`,1)<>1
					--Then 110
            */
            if (string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.LOA.loa_LastContactDate].UnformattedValue) == false
                && string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.LOA.loa_LastConfirmedContactDate].UnformattedValue) == false)
            {
                SetPriorityNumber(110);
                return;
            }

            /*		--WHEN IFNULL(`LOA Last Confirmed Contact Date`,1)=1
					--THEN 114
            */
            if (string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.LOA.loa_LastConfirmedContactDate].UnformattedValue))
            {
                SetPriorityNumber(114);
                return;
            }

            /*		when (INSTR(`LOA Last Call Status`,'Reviews')>0
						and `LOA Last Contact Date` <> `LOA Last Confirmed Contact Date`
						and IFNULL(`Milestone Completion Date`,1)<>1)
						
					OR (INSTR(`LOA Last Call Status`,'Reviews')=0
						AND IFNULL(`Milestone Completion Date`,1)<>1)
						and (DATEDIFF(DATE(ADDTIME(CURRENT_TIMESTAMP(),-25200)),`LOA Last Contact Date`)=0)
					Then 113
            */
            if (_loan.Fields[FC.CustFields.LOA.loa_LastCallStatus].UnformattedValue.Contains("Reviews"))
            {
                if (string.IsNullOrWhiteSpace(_loan.Fields[FC.EMFields.ms_CompletionDate].UnformattedValue) == false)
                {
                    if (_azLastContactDate.Date != _azLastConfirmedContactDate.Date)
                    {
                        SetPriorityNumber(113);
                        return;
                    }
                }
            }

            /*		when IFNULL(`Milestone Conditional Approval Date`,1)=1
						AND IFNULL(`New LO Conditions Date`,1)<>1
						AND IFNULL(`LO Conditions - Completion Date`,1)=1
						AND IFNULL(`New LO Conditions Date`,`Last Finished Milestone Date`)>=IFNULL(`LOA Last Contact Date`, `Milestone Book Sent Down Date`)
						AND (DATEDIFF(DATE(ADDTIME(CURRENT_TIMESTAMP(),-25200)),`LOA Last Contact Date`)=0)
					then 112
            */
            if (string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.ms_ConditionalApprovalDate].UnformattedValue)
                && string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.newLOCondDate].UnformattedValue) == false
                && string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.loConditionsCompletionDate].UnformattedValue
                /*&& IFNULL(`New LO Conditions Date`,`Last Finished Milestone Date`)>=IFNULL(`LOA Last Contact Date`, `Milestone Book Sent Down Date`)*/))
            {
                SetPriorityNumber(112);
                return;
            }

            /*		--When IFNULL(`Milestone Conditional Approval Date`,1)<>1
					--	AND INSTR(`Last Finished Milestone`,'Complet')=0
					--	AND IFNULL(`LO Conditions - Completion Date`,1)=1
					--	AND (DATEDIFF(DATE(ADDTIME(CURRENT_TIMESTAMP(),-25200)),`LOA Last Contact Date`) = 0) 
					--Then 112
            */
            if (string.IsNullOrWhiteSpace(_loan.Fields["MilestoneConditionalApprovalDate"].UnformattedValue) == false)
            {
                if (_loan.Fields[FC.EMFields.ms_LastFinished].UnformattedValue.Contains("Complet"))
                {
                    if (string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.loConditionsCompletionDate].UnformattedValue))
                    {
                        SetPriorityNumber(112);
                        return;
                    }
                }
            }

            /*		When IFNULL(`Milestone Loan Assignment Date`,1)<>1
						AND INSTR(`Last Finished Milestone`,'Complet')=0
						AND IFNULL(`PSC Escalation to LO`,1)<>1
						AND (DATEDIFF(DATE(ADDTIME(CURRENT_TIMESTAMP(),-25200)),`LOA Last Contact Date`) = 0) 
					Then 112
            */
            if (string.IsNullOrWhiteSpace(_loan.Fields["MilestoneLoanAssignmentDate"].UnformattedValue) == false)
            {
                if (_loan.Fields[FC.EMFields.ms_LastFinished].UnformattedValue.Contains("Complet"))
                {
                    if (string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.pscEscalationToLO].UnformattedValue) == false)
                    {
                        SetPriorityNumber(112);
                        return;
                    }
                }
            }

            /*when IFNULL(`CD Sent Date`,1)<>1
             * And IFNULL(`CD Signed`,1)=1
             * AND (DATEDIFF(DATE(ADDTIME(CURRENT_TIMESTAMP(),-25200)),`LOA Last Contact Date`) = 0) 
             * then 111
             */
            if (string.IsNullOrWhiteSpace(_loan.Fields[FC.EMFields.cdSentDate].UnformattedValue) == false)
            {
                if (string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.cdSignedDate].UnformattedValue))
                {
                    SetPriorityNumber(111);
                    return;
                }
            }

            /*		When IFNULL(`Milestone Final Approved Date`,1)<>1
						AND INSTR(`Last Finished Milestone`,'Complet')=0
						AND (IFNULL(`LO Conditions - Completion Date`,1)=1
							 OR IFNULL(`PSC Escalation to LO`,1)<>1)     
						AND (DATEDIFF(DATE(ADDTIME(CURRENT_TIMESTAMP(),-25200)),`LOA Last Contact Date`) = 0)                  
					Then 106
            */
            if (string.IsNullOrWhiteSpace(_loan.Fields["MilestoneFinalApprovedDate"].UnformattedValue) == false)
            {
                if (_loan.Fields[FC.EMFields.ms_LastFinished].UnformattedValue.Contains("Complet"))
                {
                    if (string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.loConditionsCompletionDate].UnformattedValue)
                        || string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.pscEscalationToLO].UnformattedValue) == false)
                    {
                        SetPriorityNumber(106);
                        return;
                    }
                }
            }

            /*		When IFNULL(`Milestone Docs Signing Date`,1)<>1
						AND INSTR(`Last Finished Milestone`,'Complet')=0
						AND (IFNULL(`LO Conditions - Completion Date`,1)=1
									OR IFNULL(`PSC Escalation to LO`,1)<>1)       
						AND (DATEDIFF(DATE(ADDTIME(CURRENT_TIMESTAMP(),-25200)),`LOA Last Contact Date`) = 0)                
					Then 103
            */
            if (string.IsNullOrWhiteSpace(_loan.Fields["MilestoneDocsSigningDate"].UnformattedValue) == false
                && _loan.Fields[FC.EMFields.ms_LastFinished].UnformattedValue.Contains("Complet"))
            {
                if (string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.loConditionsCompletionDate].UnformattedValue)
                        || string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.pscEscalationToLO].UnformattedValue) == false)
                {
                    SetPriorityNumber(103);
                    return;
                }
            }

            /*	    When IFNULL(`Milestone Docs Signing Date`,1)<>1
             *	        AND INSTR(`Last Finished Milestone`,'Complet')=0
						AND IFNULL(`Milestone Funding Date`,1)=1					
						AND IFNULL(`LO Conditions - Completion Date`,1)=1       
						AND (DATEDIFF(DATE(ADDTIME(CURRENT_TIMESTAMP(),-25200)),`LOA Last Contact Date`) = 0)                
					Then 101
            */
            if (string.IsNullOrWhiteSpace(_loan.Fields["MilestoneDocsSigningDate"].UnformattedValue) == false
                && _loan.Fields[FC.EMFields.ms_LastFinished].UnformattedValue.Contains("Complet"))
            {
                if (string.IsNullOrWhiteSpace(_loan.Fields["MilestoneFundingDate"].UnformattedValue))
                {
                    if (string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.loConditionsCompletionDate].UnformattedValue))
                    {
                        SetPriorityNumber(101);
                        return;
                    }
                }
            }

            /*		When
						IFNULL(`Milestone Final Approved Date`,1)<>1
						and IFNULL(`LO Scheduling Request`,1) <> 1
						and IFNULL(`LO Conditions - Completion Date`,1)<>1
						and IFNULL(`Scheduled Closing Date`,1)=1
						and IFNULL(`CD Sent Date`,1)<>1
						and IFNULL(`Title Received Date`,1)<>1
						and (`Service Tracker Status` = 'Rolled Loan Services Complete'
						or `Service Tracker Status` = 'Pending Corrections Complete'
						or `Service Tracker Status` = 'Services Complete')
						and IFNULL(`CTC UW Approval Date`,1)<>1 
						and (DATEDIFF(DATE(ADDTIME(CURRENT_TIMESTAMP(),-25200)),`LOA Last Contact Date`) = 0)
					Then 102
            */
            if (string.IsNullOrWhiteSpace(_loan.Fields["MilestoneFinalApprovedDate"].UnformattedValue) == false)
            {
                if (string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.loSchedulingRequest].UnformattedValue) == false)
                {
                    if (string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.loConditionsCompletionDate].UnformattedValue) == false)
                    {
                        if (string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.scheduledClosingDate].UnformattedValue))
                        {
                            if (string.IsNullOrWhiteSpace(_loan.Fields[FC.EMFields.cdSentDate].UnformattedValue) == false)
                            {
                                if (string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.titleReceivedDate].UnformattedValue) == false)
                                {
                                    string value = _loan.Fields[FC.CustFields.serviceTrackerStatus].UnformattedValue;
                                    if (value.EqualsAnyOf("Rolled Loan Services Complete", "Pending Corrections Complete", "Services Complete"))
                                    {
                                        if (string.IsNullOrWhiteSpace(_loan.Fields["CTC_UW_Approval_Date"].UnformattedValue) == false)
                                        {
                                            SetPriorityNumber(102);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            /*		when
						`Fannie Mae Loan Doc Type Code`='StreamlineRefinance'
						and `Loan Type`='FHA'
						and IFNULL(`MIlestone Resubmitted for Final Approval Date`,1)=1
						and (DATE(CONCAT(MONTH(`Next Payment Due`),'-01-',DAY(`Next Payment Due`)))<'1/01/2021' or IFNULL(`Next Payment Due`,1)=1)
						and (`Expected Closing Date` < '1/1/2021')
					then 104

            		when INSTR(`Investor`,'Bridge')>0 -- IFNULL(`Scheduled Closing Date`,1)=1 <-- only thing thats different
						and `Fannie Mae Loan Doc Type Code`='StreamlineRefinance'
						and `Loan Type`='FHA'
						and IFNULL(`MIlestone Resubmitted for Final Approval Date`,1)=1
						and (DATE(CONCAT(MONTH(`Next Payment Due`),'-01-',DAY(`Next Payment Due`)))<'1/01/2021' or IFNULL(`Next Payment Due`,1)=1)
						and (`Expected Closing Date` < '1/1/2021')
					then 104
            */
            if (_loan.Fields[FC.EMFields.fm_LoanDocType].UnformattedValue.Equals("StreamlineRefinance")
                && _loan.Fields[FC.EMFields.loanType].UnformattedValue.Equals("FHA"))
            {
                if (string.IsNullOrWhiteSpace(_loan.Fields["Milestone Resubmitted For Final Approval"].UnformattedValue) == false)
                {
                    DateTime dt = new DateTime(2021, 1, 1);
                    if (_expectedClosingDate.Date < dt)
                    {
                        //TODO:
                    }
                }
            }

            /*		when (`Last Finished Milestone`<>'Doc Signing' and `Last Finished Milestone`<>'Started' and `Last Finished Milestone`<>'Funding' and `Last Finished Milestone`<>'Completion')
						and `Milestone Book Sent Down Date`<'1/28/2021' 
						and (`LOA Last Confirmed Contact Date`<'1/28/2021' or IFNULL(`LOA Last Confirmed Contact Date`,1)=1)
						and `Fannie Mae Loan Doc Type Code`='StreamlineRefinance'
						and `Loan Type`='FHA'
						and INSTR(`Occupancy Type`,'Prim')>0
						and (DATE(CONCAT(MONTH(`Next Payment Due`),'-01-',DAY(`Next Payment Due`)))<'2/01/2021' or IFNULL(`Next Payment Due`,1)=1)
					then 105
            */
            if (_loan.Fields[FC.EMFields.ms_LastFinished].UnformattedValue != $"{FC.MSNames.docSigning}"
                && _loan.Fields[FC.EMFields.ms_LastFinished].UnformattedValue != $"{FC.MSNames.started}"
                && _loan.Fields[FC.EMFields.ms_LastFinished].UnformattedValue != $"{FC.MSNames.funding}"
                && _loan.Fields[FC.EMFields.ms_LastFinished].UnformattedValue != $"{FC.MSNames.completion}")
            {
                DateTime dt = new DateTime(2021, 1, 28);
                if (_msBookSentDownDate < dt)
                {
                    if (_azLastConfirmedContactDate < dt || string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.LOA.loa_LastConfirmedContactDate].UnformattedValue))
                    {
                        if (_loan.Fields[FC.EMFields.fm_LoanDocType].UnformattedValue.Equals("StreamlineRefinance")
                            && _loan.Fields[FC.EMFields.loanType].UnformattedValue.Equals("FHA"))
                        {
                            if (_loan.Fields[FC.EMFields.occupancyType].UnformattedValue.Contains("Prim"))
                            {
                                //IF (DATE(CONCAT(MONTH(`Next Payment Due`),'-01-',DAY(`Next Payment Due`)))<'2/01/2021' or IFNULL(`Next Payment Due`,1)=1)
                                SetPriorityNumber(105);
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void PriorityTwoHundred()
        {
            /*		When IFNULL(`Scheduled Closing Date`,1)<>1
					Then 202
            */
            if (string.IsNullOrWhiteSpace(_loan.Fields[FC.CustFields.scheduledClosingDate].UnformattedValue) == false)
            {
                SetPriorityNumber(202);
                return;
            }

            /*		When (INSTR(UPPER(`LOA Last Call Status`),'ON HOLD')>0
						And IFNULL(`Milestone Docs Signing Date`,1)=1
						And DATEDIFF(DATE(ADDTIME(CURRENT_TIMESTAMP(),-25200)),`LOA Last Contact Date`)<=2 )
					Then 200
            */
            if (_loan.Fields[FC.CustFields.LOA.loa_LastCallStatus].UnformattedValue.Contains("On Hold"))
            {
                if (string.IsNullOrWhiteSpace(_loan.Fields["Milestone Docs Signing Date"].UnformattedValue))
                {
                    SetPriorityNumber(200);
                    return;
                }
            }

            /*		When INSTR(`LOA Last Call Status`,'ESCALATED')>0 
					    and INSTR(`Loan Folder Name`,'My Pipeline')>0 
					Then 201
                    
                    When `Borrower Unresponsive Status`<>'Active' and `Borrower Unresponsive Status`<>'Restructure Complete'
					    and INSTR(`Loan Folder Name`,'My Pipeline')>0 
					Then 201

            		When `Last Finished Milestone`='Suspended'
					    and INSTR(`Loan Folder Name`,'My Pipeline')>0 
					Then 201
            */
            if (_loan.Fields[FC.EMFields.loanFolder].UnformattedValue.Equals("My Pipeline"))
            {
                if (_loan.Fields[FC.CustFields.LOA.loa_LastCallStatus].UnformattedValue.Contains("ESCALATED"))
                {
                    SetPriorityNumber(201);
                    return;
                }

                if (_loan.Fields[FC.CustFields.borrUnresponsive].UnformattedValue != "Active"
                && _loan.Fields[FC.CustFields.borrUnresponsive].UnformattedValue != "Restructure Complete")
                {
                    SetPriorityNumber(201);
                    return;
                }

                if (_loan.Fields[FC.EMFields.ms_LastFinished].UnformattedValue.Equals("Suspended"))
                {
                    SetPriorityNumber(201);
                    return;
                }
            }
        }

        private void SetPriorityNumber(double priorityNumber)
        {
            _loan.Fields[FC.CustFields.hotlistPriorityNumber].Value = $"{priorityNumber}";
        }

        public static DateTime ConvertToArizonaTime(DateTime dateTimeToConvert)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTimeToConvert, "US Mountain Standard Time");
        }

        //TODO: Investigate what this line of code is supposed to be doing aside from correcting the date to MST (DATEDIFF(DATE(ADDTIME(CURRENT_TIMESTAMP(),-25200)),`LOA Last Contact Date`) = 0)
        //Are they checking to see if it's blank? Not blank? etc.
    }
}

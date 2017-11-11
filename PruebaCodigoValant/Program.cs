using System;
using System.Linq;
using System.Collections.Generic;

namespace PruebaCodigoValant
{
    class Program
    {
        static void Main(string[] args)
        {
            String S = "00:01:07,800-000-001\n" + 
                       "00:05:01,800-000-001\n" + 
                       "00:02:10,800-000-001\n" +
                       "00:06:15,800-000-002\n" +
                       "00:03:11,800-000-002\n" +
                       "00:02:20,800-000-002\n" +
                       "00:05:30,800-000-003\n" +
                       "00:03:30,800-000-003\n" +
                       "00:03:30,800-000-003\n" +
                       "00:12:30,800-000-004";
            var log = new CallLog(S);

            foreach (var call in log.Calls)
            {
                Console.WriteLine(String.Format("Duration: {0}, Billing: {1}, " +
                    "Number: {2}", call.Duration, call.GetBilling(),
                    call.PhoneNumber));
            }

            Console.WriteLine(String.Format("Total: {0}", log.GetFinalBilling()));
            Console.ReadLine();
        }
    }
    #region MyCode
    /// <summary>
    /// A single call
    /// </summary>
    public class Call
    {
        private String _duration;
        private String _phoneNumber;

        /// <summary>
        /// Public readonly call duration
        /// </summary>
        public String Duration
        {
            get { return _duration; }
        }

        /// <summary>
        /// Public readonly phone number
        /// </summary>
        public String PhoneNumber
        {
            get { return _phoneNumber; }
        }

        /// <summary>
        /// Returns the total number of seconds 
        /// of the duration formatted HH:MM:SS
        /// Any other format returns 0
        /// </summary>
        /// <returns></returns>
        public int GetTotalSeconds()
        {
            if (_duration.Length == 8)
            {
                int hours = int.Parse(_duration.Substring(0, 2));
                int minutes = int.Parse(_duration.Substring(3, 2));
                int seconds = int.Parse(_duration.Substring(6, 2));

                return (hours * 3600) + (minutes * 60) + seconds;
            }
            else return 0;
        }

        /// <summary>
        /// Returns the total billing of a call based on two billing steps
        /// If the call is less than or equal to 5 minutes, the price is seconds * 3
        /// If the call is more than 5 minutes, the prices is minutes * 150
        /// That means started minutes, so 5:01 are six minutes
        /// </summary>
        /// <returns></returns>
        public int GetBilling()
        {
            int callSeconds = GetTotalSeconds();

            if (callSeconds < 300) // 1st billing applies
            {
                return callSeconds * 3;
            }
            else
            {
                int minutes = (int)Math.Round((double)(callSeconds / 60), 0);
                var extra = callSeconds % 60;
                if (extra > 0) { minutes++; }
                return minutes * 150;
            }
        }

        /// <summary>
        /// Mandatory ctor injects parameters
        /// </summary>
        /// <param name="duration">Call duration in HH:MM:SS format</param>
        /// <param name="phoneNumber">Phone number in 999-999-999 format</param>
        public Call(string duration, string phoneNumber)
        {
            _duration = duration;
            _phoneNumber = phoneNumber;
        }
    }

    /// <summary>
    /// CallLog is a class representing all the calls received
    /// It has a collection of calls, it must be constructed injecting the 
    /// call log in string format, and it returns the final bill account
    /// </summary>
    public class CallLog 
    {
        private List<Call> _calls;
        private Dictionary<string, int> _groupedAmount;
        private Dictionary<string, int> _groupedTime;

        /// <summary>
        /// Public readonly list of calls
        /// </summary>
        public List<Call> Calls
        {
            get { return _calls; }
        }

        /// <summary>
        /// Calculates the final billing. 
        /// Groups calls by phone number and then calculates the
        /// final billing adding all the individual calls bills
        /// EXCEPT the most called number, which is discounted from the bill
        /// </summary>
        /// <returns>Total bill price in cents</returns>
        public int GetFinalBilling()
        {
            // Which phone has the longest accumulated call time?
            var longestAccumulatedTime = _groupedTime.Values.Max();
            var longestCalledNumbers = _groupedTime.Where(x => x.Value == longestAccumulatedTime);
            // There can be only one!!
            if (longestCalledNumbers.Count() > 1)
            {
                var longestCalledNumber = longestCalledNumbers.Min(x => GetNumericalValue(x.Key));
                var allRemainingCalls = _groupedAmount.Where(x => GetNumericalValue(x.Key) != longestCalledNumber);
                return allRemainingCalls.Sum(x => x.Value);
            }
            else
            {
                var allRemainingCalls = _groupedAmount.Where(x => x.Key != longestCalledNumbers.First().Key);
                return allRemainingCalls.Sum(x => x.Value);
            }
        }


        private int GetNumericalValue(string phoneNumber)
        {
            var numbers = phoneNumber.Split('-');
            return int.Parse(numbers[0]) + int.Parse(numbers[1]) + int.Parse(numbers[2]);
        }

        /// <summary>
        /// Mandatory ctor with parameter
        /// Injects the call log in string format in the class constructor
        /// so we initialise it with a list of calls already formatted
        /// </summary>
        /// <param name="log">The call log in string format</param>
        public CallLog(String log)
        {
            _calls = new List<Call>();
            _groupedAmount = new Dictionary<string, int>();
            _groupedTime = new Dictionary<string, int>();

            String[] callLog = log.Split((char)10);
            foreach (string entry in callLog)
            {
                var splitPos = entry.IndexOf(",");
                if (splitPos != -1)
                {
                    Call call = new Call(entry.Substring(0, splitPos),
                                        entry.Substring(splitPos + 1));
                    _calls.Add(call);
                    
                    // We use the foreach loop to fill the dictionaries brouped by number
                    if (_groupedAmount.ContainsKey(call.PhoneNumber))
                    {
                        _groupedAmount[call.PhoneNumber] += call.GetBilling();
                    }
                    else
                    {
                        _groupedAmount.Add(call.PhoneNumber, call.GetBilling());
                    }

                    if (_groupedTime.ContainsKey(call.PhoneNumber))
                    {
                        _groupedTime[call.PhoneNumber] += call.GetTotalSeconds();
                    }
                    else
                    {
                        _groupedTime.Add(call.PhoneNumber, call.GetTotalSeconds());
                    }
                }
            }
        }
    }
    #endregion
}

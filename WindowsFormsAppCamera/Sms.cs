using System;
using Azure.Communication.Sms;
using System.Diagnostics;

namespace WindowsFormsAppCamera
{
    // sends SMS alerts if drones not seen - usually indicates agent death or delta
    // option to block between 0100 and 0559
    public class SmsAlert
    {
        private readonly    SmsClient   _smsClient;
        private             DateTime    _cooldownLastMessageSent;

#if DEBUG
        private TimeSpan    _cooldownTime = new TimeSpan(0, 1, 0);     // Send SMS message no more than every 1 min in debug
#else
        private TimeSpan    _cooldownTime = new TimeSpan(0, 15, 0);    // Send SMS message no more than every 15mins
#endif

        public string   MachineName { get; set; }
        public string   ConnectionString { get; set; }
        public string   SmsFrom { get; set; }
        public string   SmsTo { get; set; }
        public bool     BlockLateNightSms { get; set; } = true;

        public SmsAlert(string machineName, string connectionString, string smsFrom, string smsTo)
        {
            MachineName = machineName;
            ConnectionString = connectionString;
            SmsFrom = smsFrom;
            SmsTo = smsTo;

            ResetCooldown();

            _smsClient = new SmsClient(ConnectionString);

            Trace.TraceInformation("SmsAlert -> ctor");
        }

        // send an SMS message to the recipient
        public bool RaiseAlert(string msg)
        {
            Trace.TraceInformation("SmsAlert -> RaiseAlert");

            if (_smsClient == null)
                return false;

            DateTime now = DateTime.Now;

            // don't keep spamming SMS messages
            // this takes the current time, subtracts the last time we sent an SMS message
            // and returns if it's still in the cooldown window
            if (now.Subtract(_cooldownLastMessageSent) < _cooldownTime)
                return false;

            // block SMS messages between 0100 and 0559
            if (BlockLateNightSms)
            {
                Trace.TraceInformation("SmsAlert -> block nights");

                if (now.Hour >= 1 && now.Hour <= 5)
                    return false;
            }

            bool ok = false;
            try
            {
                Trace.TraceInformation("SmsAlert -> sending");

                // send the SMS msg!
                SmsSendResult sendResult = _smsClient.Send(
                    from: SmsFrom,
                    to: SmsTo,
                    message: msg
                );

                // start the cooldown
                if (sendResult.Successful)
                {
                    _cooldownLastMessageSent = now;
                    ok = true;
                }
            } 
            catch(Exception ex)
            {
                Trace.TraceWarning($"EXCEPTION in SmsAlert - {ex.Message}");
                ok = false;
            }

            return ok;
        }

        // reset cooldown
        public void ResetCooldown()
        {
            _cooldownLastMessageSent = new DateTime(2021, 1, 1);
        }
    }
}

using System;
using Azure.Communication.Sms;

namespace WindowsFormsAppCamera
{
    // sends SMS alerts if drones not seen - usually indicates agent death or delta
    // option to block between 0100 and 0559
    public class SmsAlert
    {
        private string      _machineName;
        private string      _connectionString, _smsFrom, _smsTo;
        private bool        _blockLateNightSms = true;
        private SmsClient   _smsClient;
        private DateTime    _cooldownLastMessageSent;

#if DEBUG
        private TimeSpan    _cooldownTime = new TimeSpan(0, 1, 0);     // Send SMS message no more than every 1 min in debug
#else
        private TimeSpan _cooldownTime = new TimeSpan(0, 15, 0);    // Send SMS message no more than every 15mins
#endif

        public string   MachineName { get => _machineName; set => _machineName = value; }
        public string   ConnectionString { get => _connectionString; set => _connectionString = value; }
        public string   SmsFrom { get => _smsFrom; set => _smsFrom = value; }
        public string   SmsTo { get => _smsTo; set => _smsTo = value; }
        public bool     BlockLateNightSms { get => _blockLateNightSms; set => _blockLateNightSms = value; }

        public SmsAlert() { }
        public SmsAlert(string machineName, string connectionString, string smsFrom, string smsTo)
        {
            MachineName = machineName;
            ConnectionString = connectionString;
            SmsFrom = smsFrom;
            SmsTo = smsTo;

            ResetCooldown();

            _smsClient = new SmsClient(ConnectionString);
        }

        // send an SMS message to the recipient
        public bool RaiseAlert(string msg)
        {
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
                if (now.Hour >= 1 && now.Hour <= 5)
                    return false;
            }

            // finally, send the SMS msg!
            SmsSendResult sendResult = _smsClient.Send(
                from: SmsFrom,
                to: SmsTo,
                message: msg
            );

            // start the cooldown
            if (sendResult.Successful == true)
                _cooldownLastMessageSent = now;

            return sendResult.Successful;
        }

        // reset cooldown
        public void ResetCooldown()
        {
            _cooldownLastMessageSent = new DateTime(2021, 1, 1);
        }
    }
}

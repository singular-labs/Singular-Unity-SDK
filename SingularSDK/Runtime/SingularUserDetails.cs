using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Singular
{
    [Preserve]
    public class SingularUserDetails
    {
        private const string EMAIL_KEY = "email";
        private const string PHONE_NUMBER_KEY = "phoneNumber";
        private const string EMAIL_STD_KEY = "emailSTD";
        private const string EMAIL_NO_DOTS_KEY = "emailNoDots";
        private const string PHONE_E164_KEY = "phoneE164";
        private const string PHONE_DIGITS_KEY = "phoneDigits";

        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

        [Preserve]
        public SingularUserDetails()
        {
        }

        /// <summary>Cleartext email - the SDK normalizes and hashes it.</summary>
        public SingularUserDetails SetEmail(string email)
        {
            SetValue(EMAIL_KEY, email);
            return this;
        }

        /// <summary>Cleartext phone number - the SDK normalizes and hashes it.</summary>
        public SingularUserDetails SetPhoneNumber(string phoneNumber)
        {
            SetValue(PHONE_NUMBER_KEY, phoneNumber);
            return this;
        }

        /// <summary>Pre-hashed email (trimmed + lowercased, SHA-256). Stored as-is.</summary>
        public SingularUserDetails SetEmailSTD(string hashedEmail)
        {
            SetValue(EMAIL_STD_KEY, hashedEmail);
            return this;
        }

        /// <summary>Pre-hashed email with the gmail local part dots removed (SHA-256). Stored as-is.</summary>
        public SingularUserDetails SetEmailNoDots(string hashedEmail)
        {
            SetValue(EMAIL_NO_DOTS_KEY, hashedEmail);
            return this;
        }

        /// <summary>Pre-hashed E.164 phone number (SHA-256). Stored as-is.</summary>
        public SingularUserDetails SetPhoneE164(string hashedPhone)
        {
            SetValue(PHONE_E164_KEY, hashedPhone);
            return this;
        }

        /// <summary>Pre-hashed digits-only phone number (SHA-256). Stored as-is.</summary>
        public SingularUserDetails SetPhoneDigits(string hashedPhone)
        {
            SetValue(PHONE_DIGITS_KEY, hashedPhone);
            return this;
        }

        public string GetEmail()
        {
            return GetValue(EMAIL_KEY);
        }

        public string GetPhoneNumber()
        {
            return GetValue(PHONE_NUMBER_KEY);
        }

        public string GetEmailSTD()
        {
            return GetValue(EMAIL_STD_KEY);
        }

        public string GetEmailNoDots()
        {
            return GetValue(EMAIL_NO_DOTS_KEY);
        }

        public string GetPhoneE164()
        {
            return GetValue(PHONE_E164_KEY);
        }

        public string GetPhoneDigits()
        {
            return GetValue(PHONE_DIGITS_KEY);
        }

        public bool IsEmpty()
        {
            return _values.Count == 0;
        }

        private void SetValue(string key, string value)
        {
            if (string.IsNullOrEmpty(value) || value.Trim() == string.Empty)
            {
                _values.Remove(key);
                return;
            }

            _values[key] = value;
        }

        private string GetValue(string key)
        {
            string value;
            return _values.TryGetValue(key, out value) ? value : null;
        }

        internal Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>(_values);
        }

        internal string ToJsonString()
        {
            return JsonConvert.SerializeObject(_values);
        }
    }
}

using Microsoft.FeatureManagement.FeatureFilters;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace dotnetcore.featureflags
{
    public class EmailAddressTargetingContextAccessor : ITargetingContextAccessor
    {
        private MailAddress? _emailAddress;
        private string _domain;
        private Regex hostNameValidation = new Regex(@"^(?!-)[A-Za-z0-9-]{1,63}(?<!-)(\.[A-Za-z0-9-]{1,63})*$", RegexOptions.Compiled);
        public EmailAddressTargetingContextAccessor(string? emailAddress) : this(emailAddress, null)
        {
        }
        public EmailAddressTargetingContextAccessor(string? emailAddress, string? domain)
        {
            if (string.IsNullOrEmpty(emailAddress) && string.IsNullOrEmpty(domain))
                throw new ArgumentException($"{nameof(emailAddress)} or {nameof(domain)} is required");

            if (!string.IsNullOrEmpty(emailAddress) && !MailAddress.TryCreate(emailAddress, out _emailAddress))
            {
                throw new ArgumentException("Invalid", nameof(emailAddress));
            }

            if (!string.IsNullOrEmpty(domain) && !hostNameValidation.Match(domain).Success)
            {
                throw new ArgumentException("Invalid", nameof(domain));
            }
            _domain = domain!;
        }
        public ValueTask<TargetingContext> GetContextAsync()
        {
            return ValueTask.FromResult(new TargetingContext
            {
                Groups = new List<string> { _domain },
                UserId = _emailAddress!.Address
            });
        }
    }
}

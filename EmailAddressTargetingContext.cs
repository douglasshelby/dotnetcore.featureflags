using Microsoft.FeatureManagement.FeatureFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace dotnetcore.featureflags
{
    public class EmailAddressTargetingContext : ITargetingContext
    {
        private string _emailAddress;
        private IEnumerable<string> _groups;
        
        public string UserId { get => _emailAddress; set { _emailAddress = value; } }
        public IEnumerable<string> Groups { get => (IReadOnlyList<string>)_groups; set { _groups = value; } }
    }
}

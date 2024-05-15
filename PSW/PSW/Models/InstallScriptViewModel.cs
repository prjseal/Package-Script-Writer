using PSW.Configuration;
using System.ComponentModel.DataAnnotations;

namespace PSW.Models
{
    public class InstallScriptViewModel
    {
        [Display(Name = "Output to single line command")]
        public bool OnelinerOutput { get; set; }
        [Display(Name = "Remove comments")]
        public bool RemoveComments { get; set; }
        public string? Output { get; set; }
        public bool HasQueryString { get; set; }

        public InstallScriptViewModel(bool hasQueryString, bool oneLinerOutput, bool removeComments, string? output) 
        { 
            HasQueryString = hasQueryString;
            Output = output;
            OnelinerOutput = oneLinerOutput;
            RemoveComments = removeComments;
        }
    }
}

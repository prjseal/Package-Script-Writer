using PSW.Shared.Models;

namespace PackageCliTool.Extensions
{
    /// <summary>
    /// Provides extension methods for converting <see cref="GeneratorApiRequest"/> to <see cref="PackagesViewModel"/>.
    /// </summary>
    public static class GeneratorApiRequestExtensions
    {
        /// <summary>
        /// Converts a <see cref="GeneratorApiRequest"/> instance to a <see cref="PackagesViewModel"/>.
        /// </summary>
        /// <param name="apiRequest">The API request model to convert.</param>
        /// <returns>A <see cref="PackagesViewModel"/> populated with values from the API request.</returns>
        public static PackagesViewModel ToViewModel(this GeneratorApiRequest apiRequest)
        {
            return new PackagesViewModel(apiRequest);
        }
    }
}

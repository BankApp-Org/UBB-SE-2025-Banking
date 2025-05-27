namespace StockApp.ViewModels
{
    using Common.Models;
    using Common.Services;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// ViewModel for updating user profile details including username, image, description, and visibility.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UpdateProfilePageViewModel"/> class with a specified stockService.
    /// </remarks>
    /// <param name="service">Service used to retrieve and update profile information.</param>
    public class UpdateProfilePageViewModel(IStockService stockService, IUserService userService, IAuthenticationService authenticationService)
    {
        private readonly IStockService stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
        private readonly IUserService userService = userService ?? throw new ArgumentNullException(nameof(userService));
        private readonly IAuthenticationService authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));

        /// <summary>
        /// Gets the URL of the user's profile image.
        /// </summary>
        /// <returns>The image URL as a string.</returns>
        public async Task<string> GetImage()
        {
            // Inline: delegate image retrieval to service
            return (await this.userService.GetCurrentUserAsync()).Image;
        }

        /// <summary>
        /// Gets the username of the current user.
        /// </summary>
        /// <returns>The username as a string.</returns>
        public async Task<string> GetUsername()
        {
            // Inline: delegate username retrieval to service
            return (await this.userService.GetCurrentUserAsync()).UserName ?? throw new ArgumentNullException("UserName", "Username cannot be null.");
        }

        /// <summary>
        /// Gets the description text for the current user.
        /// </summary>
        /// <returns>The description as a string.</returns>
        public async Task<string> GetDescription()
        {
            // Inline: delegate description retrieval to service
            return (await this.userService.GetCurrentUserAsync()).Description;
        }

        /// <summary>
        /// Determines whether the user's profile is hidden.
        /// </summary>
        /// <returns><c>true</c> if hidden; otherwise, <c>false</c>.</returns>
        public async Task<bool> IsHidden()
        {
            // Inline: delegate visibility check to service
            return (await this.userService.GetCurrentUserAsync()).IsHidden;
        }

        /// <summary>
        /// Determines whether the current user has administrative privileges.
        /// </summary>
        /// <returns><c>true</c> if admin; otherwise, <c>false</c>.</returns>
        public bool IsAdmin()
        {
            return this.authenticationService.IsUserAdmin();
        }

        /// <summary>
        /// Gets the list of stocks associated with the user.
        /// </summary>
        /// <returns>A list of <see cref="Stock"/> objects.</returns>
        public async Task<List<Stock>> GetUserStocks()
        {
            return await this.stockService.UserStocksAsync(this.authenticationService.GetUserCNP());
        }

        /// <summary>
        /// Updates all user profile fields at once.
        /// </summary>
        /// <param name="newUsername">The new username.</param>
        /// <param name="newImage">The new profile image URL.</param>
        /// <param name="newDescription">The new description text.</param>
        /// <param name="newHidden">New hidden status for the profile.</param>
        public async Task UpdateAllAsync(string newUsername, string newImage, string newDescription, bool newHidden)
        {
            // TODO: Validate inputs (e.g., non-null, length constraints)
            // FIXME: Consider handling exceptions from service to provide user feedback

            // Get current user to maintain other properties
            var currentUser = await this.userService.GetCurrentUserAsync();

            // Create an updated user object with the new values
            var updatedUser = new User
            {
                UserName = newUsername,
                Image = newImage,
                Description = newDescription,
                IsHidden = newHidden,
                // Maintain other properties from current user
                Email = currentUser.Email,
                PhoneNumber = currentUser.PhoneNumber,
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                Birthday = currentUser.Birthday
            };

            await this.userService.UpdateUserAsync(updatedUser);
        }

        /// <summary>
        /// Updates only the administrative mode of the user.
        /// </summary>
        /// <param name="newIsAdmin"><c>true</c> to grant admin; otherwise, <c>false</c>.</param>
        public async Task UpdateAdminModeAsync(bool newIsAdmin)
        {
            // Inline: delegate admin mode toggle to service
            await this.userService.UpdateIsAdminAsync(newIsAdmin);
        }
    }
}

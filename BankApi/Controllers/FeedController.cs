using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Common.Models.Social;
using Common.Services;
using Common.Services.Social;
using LoanShark.Service.SocialService.Implementations;
using LoanShark.EF.Repository.SocialRepository;
using LoanShark.Domain;
using LoanShark.API.Models;
using LoanShark.Service.SocialService.Interfaces;

namespace LoanShark.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedController : ControllerBase
    {
        private readonly IFeedService _feedService;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;

        public FeedController(ILoanSharkDbContext dbContext, INotificationService notificationService, IUserService userService, IFeedService feedService)
        {
            // Probabil o sa fie nevoie de ceva modificare dupa ce termina George cu repo
            // ANTO/ZELU
            // In rest cred ca merge
            this._notificationService = notificationService;
            this._userService = userService;
            this._feedService = feedService;
        }

        [HttpGet("content")]
        public async Task<ActionResult<List<FeedViewModel>>> GetFeedContent()
        {
            List<Post> posts = await this._feedService.GetFeedContent();
            if (posts == null || posts.Count == 0)
            {
                return Ok(null);
            }

            List<FeedViewModel> dto = posts.Select(post => new FeedViewModel
            {
                PostID = post.PostID,
                Title = post.Title,
                Category = post.Category,
                Content = post.Content,
                Timestamp = post.Timestamp,
            }).ToList();

            return Ok(dto);
        }
    }
}
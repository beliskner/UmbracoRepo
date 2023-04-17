using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using UmbracoRepo.Web.Helpers;
using UmbracoRepo.Web.NotificationHandlers;
using UmbracoRepo.Web.Services.BackOfficePreviewService;
using UmbracoRepo.Web.Services.BlockRenderService;

namespace UmbracoRepo.Web.Composers;

public sealed class UmbracoRepoComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ServerVariablesParsingNotification, ServerVariablesParsingNotificationHandler>();

        builder.Services.TryAddScoped<IViewComponentHelperWrapper>(sp =>
        {
            if (sp.GetRequiredService<IViewComponentHelper>() is DefaultViewComponentHelper helper)
                return new ViewComponentHelperWrapper<DefaultViewComponentHelper>(helper);
            throw new InvalidOperationException($"Expected {nameof(DefaultViewComponentHelper)} when resolving {nameof(IViewComponentHelperWrapper)}");
        });
        
        builder.Services.TryAddScoped<IBlockRenderService, BlockRenderService>();
        builder.Services.TryAddScoped<IBackOfficePreviewService, BackOfficePreviewService>();
    }
}
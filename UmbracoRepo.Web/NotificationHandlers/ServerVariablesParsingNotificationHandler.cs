using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;
using UmbracoRepo.Web.Controllers;

namespace UmbracoRepo.Web.NotificationHandlers;

public sealed class ServerVariablesParsingNotificationHandler : INotificationHandler<ServerVariablesParsingNotification>
{
    private readonly LinkGenerator _linkGenerator;
    public ServerVariablesParsingNotificationHandler(LinkGenerator linkGenerator) => _linkGenerator = linkGenerator;

    public void Handle(ServerVariablesParsingNotification notification)
    {
        notification.ServerVariables.Add("UmbracoRepo", new
        {
            PreviewApi = _linkGenerator.GetPathByAction(nameof(BlockPreviewApiController.PreviewMarkup), ControllerExtensions.GetControllerName<BlockPreviewApiController>())
        });
    }
}
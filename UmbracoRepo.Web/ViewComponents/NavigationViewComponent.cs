using Microsoft.AspNetCore.Mvc;

namespace UmbracoRepo.Web.ViewComponents;

public sealed class NavigationViewComponent : ViewComponent
{
    public Task<IViewComponentResult> InvokeAsync()
    {
        return Task.FromResult<IViewComponentResult>(View());
    }
}
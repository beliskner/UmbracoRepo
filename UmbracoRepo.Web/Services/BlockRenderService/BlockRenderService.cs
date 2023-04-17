using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core.Models.Blocks;
using UmbracoRepo.Web.Helpers;

namespace UmbracoRepo.Web.Services.BlockRenderService;

public sealed class BlockRenderService : IBlockRenderService
{
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IViewComponentHelperWrapper _viewComponentHelperWrapper;
    private readonly IRazorViewEngine _razorViewEngine;
    
    private const string BlockPartialPathFormat = "/Views/Partials/blocklist/{0}.cshtml";

    public BlockRenderService(ITempDataProvider tempDataProvider, IViewComponentHelperWrapper viewComponentHelperWrapper, IRazorViewEngine razorViewEngine)
    {
        _tempDataProvider = tempDataProvider;
        _viewComponentHelperWrapper = viewComponentHelperWrapper;
        _razorViewEngine = razorViewEngine;
    }

    public async Task<string> RenderPartial(ControllerContext controllerContext, ViewDataDictionary viewData, string contentAlias)
    {
        var actionContext = new ActionContext(controllerContext.HttpContext, new RouteData(), new ActionDescriptor());
        var path = string.Format(BlockPartialPathFormat, contentAlias);

        await using var sw = new StringWriter();
        var viewResult = _razorViewEngine.GetView(path, path, false);

        if (viewResult?.View == null) return sw.ToString();
        
        var tempDataDict = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);
        var viewContext = new ViewContext(actionContext, viewResult.View, viewData, tempDataDict, sw, new HtmlHelperOptions());
        await viewResult.View.RenderAsync(viewContext);

        return sw.ToString();
    }

    public async Task<string> RenderViewComponent(ControllerContext controllerContext, ViewDataDictionary viewData,
        ViewComponentDescriptor viewComponent, IBlockReference? blockListItem)
    {
        var tempDataDict = new TempDataDictionary(controllerContext.HttpContext, _tempDataProvider);
        await using var sw = new StringWriter();
        var viewContext = new ViewContext(controllerContext, new FakeView(), viewData, tempDataDict, sw, new HtmlHelperOptions());
        _viewComponentHelperWrapper.Contextualize(viewContext);

        var result = await _viewComponentHelperWrapper.InvokeAsync(viewComponent.TypeInfo.AsType(), blockListItem);
        result.WriteTo(sw, HtmlEncoder.Default);
        return sw.ToString();
    }

    private sealed class FakeView : IView
    {
        public string Path => string.Empty;

        public Task RenderAsync(ViewContext context)
        {
            return Task.CompletedTask;
        }
    }
}
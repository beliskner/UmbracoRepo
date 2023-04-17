using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Umbraco.Cms.Core.Models.Blocks;

namespace UmbracoRepo.Web.Services.BlockRenderService;

public interface IBlockRenderService
{
    Task<string> RenderPartial(ControllerContext controllerContext, ViewDataDictionary viewData, string contentAlias);
    Task<string> RenderViewComponent(ControllerContext controllerContext, ViewDataDictionary viewData, ViewComponentDescriptor viewComponent, IBlockReference? blockListItem);
}
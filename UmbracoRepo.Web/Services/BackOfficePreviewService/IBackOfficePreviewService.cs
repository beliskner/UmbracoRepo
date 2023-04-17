using Microsoft.AspNetCore.Mvc;
using UmbracoRepo.Web.Models.Business;

namespace UmbracoRepo.Web.Services.BackOfficePreviewService;

public interface IBackOfficePreviewService
{
    Task<string> GetMarkupForBlock(BackOfficeBlock blockData, ControllerContext controllerContext);
}
using System.Globalization;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Extensions;
using UmbracoRepo.Web.Models.Business;
using UmbracoRepo.Web.Services.BackOfficePreviewService;

namespace UmbracoRepo.Web.Controllers;

/// <summary>
/// Represents the block preview api controller.
/// </summary>
public sealed class BlockPreviewApiController : UmbracoAuthorizedJsonController
{
    private readonly IPublishedRouter _publishedRouter;
    private readonly ILogger<BlockPreviewApiController> _logger;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IBackOfficePreviewService _backOfficePreviewService;
    private readonly ISiteDomainMapper _siteDomainMapper;

    public BlockPreviewApiController(IPublishedRouter publishedRouter, ILogger<BlockPreviewApiController> logger, IUmbracoContextAccessor umbracoContextAccessor,
        IVariationContextAccessor variationContextAccessor, IBackOfficePreviewService backOfficePreviewService, ISiteDomainMapper siteDomainMapper)
    {
        _publishedRouter = publishedRouter;
        _logger = logger;
        _umbracoContextAccessor = umbracoContextAccessor;
        _variationContextAccessor = variationContextAccessor;
        _backOfficePreviewService = backOfficePreviewService;
        _siteDomainMapper = siteDomainMapper;
    }

    /// <summary>
    /// Renders a preview for a block using the associated razor view.
    /// </summary>
    /// <param name="block">The json data of the block.</param>
    /// <param name="pageId">The current page id.</param>
    /// <param name="culture">The culture</param>
    /// <returns>The markup to render in the preview.</returns>
    [HttpPost]
    public async Task<IActionResult> PreviewMarkup([FromBody] BackOfficeBlock block, [FromQuery] int pageId = 0, [FromQuery] string culture = "")
    {
        string markup;

        try
        {
            // If the page is new, then the ID will be zero
            if (pageId <= 0 || GetPublishedContentForPage(pageId) is not { } page)
                return Ok("The page is not saved yet, so we can't create a preview. Save the page first.");

            await SetupPublishedRequest(culture, page);
            markup = await _backOfficePreviewService.GetMarkupForBlock(block, ControllerContext);
        }
        catch (Exception ex)
        {
            markup = "Something went wrong rendering a preview.";
            _logger.LogError(ex, "Error rendering preview for a block {ContentTypeAlias}", (block.Data?.ContentTypeAlias).IfNullOrWhiteSpace("without alias"));
        }

        return Ok(CleanUpMarkup(markup));
    }

    private async Task SetupPublishedRequest(string culture, IPublishedContent page)
    {
        // set the published request for the page we are editing in the back office
        if (_umbracoContextAccessor.TryGetUmbracoContext(out var context) is not true) return;

        // set the published request
        var requestBuilder = await _publishedRouter.CreateRequestAsync(new Uri(Request.GetDisplayUrl()));
        requestBuilder.SetPublishedContent(page);
        context.PublishedRequest = requestBuilder.Build();

        // if in a culture variant setup also set the correct language.
        var currentCulture = culture.IfNullOrWhiteSpace(page.GetCultureFromDomains(_umbracoContextAccessor, _siteDomainMapper));
        if (currentCulture is null || page.Cultures.ContainsKey(currentCulture) is not true) return;

        var cultureInfo = new CultureInfo(page.Cultures[currentCulture].Culture);
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
        _variationContextAccessor.VariationContext = new VariationContext(cultureInfo.Name);
    }

    private IPublishedContent? GetPublishedContentForPage(int pageId)
        => _umbracoContextAccessor.TryGetUmbracoContext(out var context) ? context.Content?.GetById(pageId) ?? context.Content?.GetById(true, pageId) : null;

    private static string CleanUpMarkup(string markup)
    {
        if (string.IsNullOrWhiteSpace(markup)) return markup;

        var content = new HtmlDocument();
        content.LoadHtml(markup);

        // make sure links are not clickable in the back office, because this will prevent editing
        var links = content.DocumentNode.SelectNodes("//a")?.ToArray() ?? Array.Empty<HtmlNode>();
        // disable forms so they can't be submitted via tab
        var formElements = content.DocumentNode.SelectNodes("//input | //textarea | //select | //button")?.ToArray() ?? Array.Empty<HtmlNode>();
        if (links.Any() is not true && formElements.Any() is not true) return markup;

        foreach (var link in links)
            link.SetAttributeValue("href", "javascript:;");

        foreach (var formElement in formElements)
            formElement.SetAttributeValue("disabled", "disabled");

        return content.DocumentNode.OuterHtml;
    }
}
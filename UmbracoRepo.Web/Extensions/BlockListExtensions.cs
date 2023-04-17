using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using UmbracoRepo.Web.Enums;
using UmbracoRepo.Web.Models.PublishedContent;

namespace UmbracoRepo.Web.Extensions;

public static class BlockListExtensions
{
    public static HeroImageLayout GetImageLayout(this BlockListItem<Hero> hero)
    {
        if (hero.Content is not { Image: { } }) return HeroImageLayout.None;
        if (hero.Settings is not HeroSettings heroSettings) return HeroImageLayout.Fullwidth;
        return heroSettings.ImagePosition switch
        {
            "Left" => HeroImageLayout.Left,
            "Right" => HeroImageLayout.Right,
            _ => HeroImageLayout.Fullwidth
        };
    }

    public static Task<IHtmlContent> BlocksAsync(this IHtmlHelper helper)
    {
        var blocks = Enumerable.Empty<BlockListItem>();
        if (helper.ViewData.Model is IHeaderComposition { Header: { Count: > 0 } header })
            blocks = blocks.Concat(header);
        if (helper.ViewData.Model is IContentComposition { Blocks: { Count: > 0 } contentBlocks })
            blocks = blocks.Concat(contentBlocks);

        return helper.PartialAsync("~/Views/Partials/router.cshtml", new BlockListModel(blocks.ToArray()));
    }
}
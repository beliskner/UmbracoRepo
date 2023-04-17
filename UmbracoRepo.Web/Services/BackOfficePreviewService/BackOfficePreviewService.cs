using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Extensions;
using UmbracoRepo.Web.Models.Business;
using UmbracoRepo.Web.Services.BlockRenderService;

namespace UmbracoRepo.Web.Services.BackOfficePreviewService;

public sealed class BackOfficePreviewService : IBackOfficePreviewService
{
    private readonly BlockEditorConverter _blockEditorConverter;
    private readonly ITypeFinder _typeFinder;
    private readonly IPublishedValueFallback _publishedValueFallback;
    private readonly IViewComponentSelector _viewComponentSelector;
    private readonly IBlockRenderService _blockRenderService;

    public BackOfficePreviewService(BlockEditorConverter blockEditorConverter, ITypeFinder typeFinder, IPublishedValueFallback publishedValueFallback,
        IViewComponentSelector viewComponentSelector, IBlockRenderService blockRenderService)
    {
        _blockEditorConverter = blockEditorConverter;
        _typeFinder = typeFinder;
        _publishedValueFallback = publishedValueFallback;
        _viewComponentSelector = viewComponentSelector;
        _blockRenderService = blockRenderService;
    }

    public async Task<string> GetMarkupForBlock(BackOfficeBlock block, ControllerContext controllerContext)
    {
        if (CanConvertToBlockListItem(block, out var blockListItem) is not true || blockListItem is null) return string.Empty;
        var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = blockListItem
            };
        var contentAlias = blockListItem.Content.ContentType.Alias.ToFirstUpper();
        var viewComponent = _viewComponentSelector.SelectComponent(contentAlias);

        return viewComponent is not null ?
            await _blockRenderService.RenderViewComponent(controllerContext, viewData, viewComponent, blockListItem) :
            await _blockRenderService.RenderPartial(controllerContext, viewData, contentAlias);
    }

    private bool CanConvertToBlockListItem(BackOfficeBlock block, out BlockListItem? blockListItem)
    {
        blockListItem = null;
        if (block.Data is not { } blockData) return false;
        
        var data = _blockEditorConverter.ConvertToElement(blockData, PropertyCacheLevel.None, true);
        if (data is null) return false;
        var blockDataType = _typeFinder.FindClassesWithAttribute<PublishedModelAttribute>()
            .FirstOrDefault(x => x.GetCustomAttribute<PublishedModelAttribute>(false)?.ContentTypeAlias == data.ContentType.Alias);
        if (blockDataType is null) return false;
        // create instance of the models builder type based from the element
        var blockDataInstance = Activator.CreateInstance(blockDataType, data, _publishedValueFallback);
        // get a generic block list item type based on the models builder type
        var blockListItemType = typeof(BlockListItem<>).MakeGenericType(blockDataType);

        object? blockSettingsInstance = null;
        if (block.Settings is { } blockSettings)
        {
            var settings = _blockEditorConverter.ConvertToElement(blockSettings, PropertyCacheLevel.None, true);
            var blockSettingsType = settings is null ? null : _typeFinder.FindClassesWithAttribute<PublishedModelAttribute>()
                    .FirstOrDefault(x => string.Equals(x.GetCustomAttribute<PublishedModelAttribute>(false)?.ContentTypeAlias, settings.ContentType.Alias));
            blockSettingsInstance = blockSettingsType is null ? null : Activator.CreateInstance(blockDataType, settings, _publishedValueFallback);
        }

        // create instance of the block list item
        // if you want to use settings this will need to be changed.
        blockListItem = (BlockListItem?)Activator.CreateInstance(blockListItemType, blockData.Udi, blockDataInstance, block.Settings?.Udi, blockSettingsInstance);
        return true;
    }
}
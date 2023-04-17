using Newtonsoft.Json;
using Umbraco.Cms.Core.Models.Blocks;

namespace UmbracoRepo.Web.Models.Business;

public sealed class BackOfficeBlock
{
    [JsonProperty("data")]
    public BlockItemData? Data { get; set; }
    [JsonProperty("settings")]
    public BlockItemData? Settings { get; set; }
}
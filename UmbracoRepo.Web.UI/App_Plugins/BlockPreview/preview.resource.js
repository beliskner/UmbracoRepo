(function () {
    'use strict';

    function previewResource($http, umbRequestHelper) {
        var apiUrl = "/umbraco/backoffice/api/blockpreviewapi/previewmarkup";
        var resource = {
            getPreview: getPreview
        };
        return resource;

        function getPreview(data, settings, pageId, culture) {
            const block = {
                data: data,
                settings: settings
            };
            return umbRequestHelper.resourcePromise($http.post(apiUrl + '?pageId=' + pageId + '&culture=' + culture, block), 'Failed getting preview markup');
        }
    }
    angular.module('umbraco.resources').factory('UmbracoRepo.Resources.PreviewResource', previewResource);
})();
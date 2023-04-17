angular.module('umbraco').controller('UmbracoRepo.Controllers.BlockPreviewController',
    ['$scope', '$sce', '$timeout', '$element', '$compile', 'editorState', 'UmbracoRepo.Resources.PreviewResource',
        function ($scope, $sce, $timeout, $element, $compile, editorState, previewResource) {
            $scope.language = editorState.getCurrent().variants.find(function (v) {
                return v.active;
            }).language?.culture;

            $scope.id = editorState.getCurrent().id;
            $scope.loading = true;
            $scope.markup = $sce.trustAsHtml('Loading preview');
            var shadowRoot = $element[0].querySelector('.preview-wrapper').attachShadow({ mode: 'open' });

            function loadPreview(blockData, blockSettings) {
                $scope.markup = $sce.trustAsHtml('Loading preview');
                $scope.loading = true;
                previewResource.getPreview(blockData, blockSettings, $scope.id, $scope.language).then(function (data) {
                    console.log(data)
                    shadowRoot.innerHTML = `
                        <style>
                        @import "/assets/main.css";
                        </style>
                        <div class="the-valley-block" style="border: dashed gray 2px; padding: 16px; margin: 8px 0;">
                            ${$sce.trustAsHtml(data)}
                        </div>
                    `;
                    $compile(shadowRoot)($scope);
                    $scope.loading = false;
                });
            }

            $scope.translate = function(block) {
                if (block.data.title) {
                    alert("okee dokee boss translated for you here you go: " + block.data.title + " (in frysk)");
                }
                else {
                    alert("this prototype kind of assumed there would be a title property so you're all outta luck champ no translations");
                }
            }

            var timeoutPromiseData;
            $scope.$watch('block.data', function (newValue) {
                $timeout.cancel(timeoutPromiseData);
                timeoutPromiseData = $timeout(function () {
                    loadPreview(newValue, $scope.block.settingsData);
                }, 500);
            }, true);

            var timeoutPromiseSettings;
            $scope.$watch('block.settingsData', function (newValue) {
                $timeout.cancel(timeoutPromiseSettings);
                timeoutPromiseSettings = $timeout(function () {   //Set timeout
                    loadPreview($scope.block.data, newValue);
                }, 500);
            }, true);
        }
]);
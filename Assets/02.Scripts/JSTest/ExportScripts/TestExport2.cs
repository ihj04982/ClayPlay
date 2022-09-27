using UnityEngine;
using GLTFast;
using GLTFast.Export;

public class TestExport2 : MonoBehaviour
{
    private void Start()
    {
        AdvancedExport();
    }
    [SerializeField]
    string path;

    async void AdvancedExport()
    {
        path = "C://Final/ServerTest/Assets/ExportObj/ex3";
        // CollectingLogger lets you programatically go through
        // errors and warnings the export raised
        var logger = new CollectingLogger();

        // ExportSettings allow you to configure the export
        // Check its source for details
        var exportSettings = new ExportSettings
        {
            format = GltfFormat.Binary,
            fileConflictResolution = FileConflictResolution.Overwrite
        };

        // GameObjectExport lets you create glTFs from GameObject hierarchies
        var export = new GameObjectExport(exportSettings, logger:logger);

        // Example of gathering GameObjects to be exported (recursively)
        var rootLevelNodes = GameObject.FindGameObjectsWithTag("ExportMe");

        // Add a scene
        export.AddScene(rootLevelNodes, "My new glTF scene");

        // Async glTF export
        bool success = await export.SaveToFileAndDispose(path);

        if (!success)
        {
            Debug.LogError("Something went wrong exporting a glTF");
            // Log all exporter messages
            logger.LogAll();
        }
    }
}
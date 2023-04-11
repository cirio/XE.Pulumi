using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using System.Collections.Generic;

return await Pulumi.Deployment.RunAsync(() =>
{
    // Crea un contenitore di risorse  (Azure Resource Group)
    var resourceGroup = new ResourceGroup("resourceGroup");

    // Crea lo spazio dove verrà ospitato il sito (Storage Account)
    var storageAccount = new StorageAccount("sa", new StorageAccountArgs
    {
        ResourceGroupName = resourceGroup.Name,
        Sku = new SkuArgs
        {
            Name = SkuName.Standard_LRS
        },
        Kind = Kind.StorageV2
    });

    var staticWebSiteName = $"sa-xestaticwebsite";
    var staticWebsite = new StorageAccountStaticWebsite(staticWebSiteName, new StorageAccountStaticWebsiteArgs
    {
        AccountName = storageAccount.Name,
        ResourceGroupName = resourceGroup.Name,
        IndexDocument = "index.html",
        Error404Document = "404.html"
    });

    var indexHtml = new Blob("index.html", new BlobArgs
    {
        ResourceGroupName = resourceGroup.Name,
        AccountName = storageAccount.Name,
        ContainerName = staticWebsite.ContainerName,
        Source = new FileAsset("./wwwroot/index.html"),
        ContentType = "text/html"
    });
    
    var notfoundHtml = new Blob("404.html", new BlobArgs
    {
        ResourceGroupName = resourceGroup.Name,
        AccountName = storageAccount.Name,
        ContainerName = staticWebsite.ContainerName,
        Source = new FileAsset("./wwwroot/404.html"),
        ContentType = "text/html"
    });

    Output<string>? staticEndpoint = null;
    staticEndpoint = storageAccount.PrimaryEndpoints.Apply(primaryEndpoints => primaryEndpoints.Web);


    // In Output verrà mostrato l'endpoint del sito web
    return new Dictionary<string, object?>
    {
        ["staticWebSiteHost"] = staticEndpoint
    };
});
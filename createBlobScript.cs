using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;



namespace ConsoleApplicationblob
{
    class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Azure Storage Exercise");
            Console.WriteLine();
            ProcessAsync().GetAwaiter().GetResult();
            Console.WriteLine("Press any key and hit enter to exit the sample applica-tion.");
            Console.ReadLine();

        }
        static async Task ProcessAsync()
        {
            CloudStorageAccount storageAccount = null;
            
            CloudBlobClient cloudBlobClient = null;

            CloudBlobContainer cloudBlobContainer = null;

            string sourceFile = "";
            string destinationFile = "";

            string storageConnectionString = Environment.GetEnvironmentVariable("storageconnectionstring");

            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                Console.WriteLine("Successfully Parsed Storage Connection String");
            }
            else
            {
                Console.WriteLine("Failed to Parse Storage Connection String");
            }

            try
            {
                cloudBlobClient = storageAccount.CreateCloudBlobClient();
                cloudBlobContainer = cloudBlobClient.GetContainerReference("testcontainer" + Guid.NewGuid().ToString());
                await cloudBlobContainer.CreateAsync();
                Console.WriteLine("Created container '{0}'", cloudBlobContainer.Name);
                Console.WriteLine();

                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };
                await cloudBlobContainer.SetPermissionsAsync(permissions);

                string localPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string localFileName = "BlobFile_" + Guid.NewGuid().ToString() + ".txt";
                sourceFile = Path.Combine(localPath, localFileName);
                File.WriteAllText(sourceFile, "Hello, World!");

                Console.WriteLine("Temp file = {0}", sourceFile);
                Console.WriteLine("Uploading to Blob storage as blob '{0}'", localFileName);
                Console.WriteLine();

                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(localFileName);
                await cloudBlockBlob.UploadFromFileAsync(sourceFile);

                Console.WriteLine("Listing blobs in container.");
                BlobContinuationToken blobContinuationToken = null;
                do
                {
                    var results = await cloudBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                    blobContinuationToken = results.ContinuationToken;
                    foreach (IListBlobItem item in results.Results)
                    {
                        Console.WriteLine(item.Uri);
                    }
                } while (blobContinuationToken != null);
                Console.WriteLine();

                destinationFile = sourceFile.Replace(".txt", "_DOWNLOADED.txt");
                Console.WriteLine("Downloading blob to {0}", destinationFile);
                Console.WriteLine();
                await cloudBlockBlob.DownloadToFileAsync(destinationFile, FileMode.Create);


            }
            catch (StorageException e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
            finally
            {
                Console.WriteLine("Press any key and hit enter to delete the sample files and example container.");
                Console.ReadLine();
                Console.WriteLine("Deleting the container and any blobs it contains");
                if (cloudBlobContainer != null)
                {
                    await cloudBlobContainer.DeleteIfExistsAsync();
                }
                Console.WriteLine("Deleting all created local files");
                Console.WriteLine();
                File.Delete(sourceFile);
                File.Delete(destinationFile);

            }



        }


    }
}

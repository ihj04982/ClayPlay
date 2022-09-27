using System.Collections;
using System.Collections.Generic;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

public class AWSManager : MonoBehaviour
{
    
    public static AWSManager instance;
    public string S3Region = RegionEndpoint.APNortheast2.SystemName;

    private RegionEndpoint _S3Region
    {
        get { return RegionEndpoint.GetBySystemName(S3Region); }
    }

    private AmazonS3Client _s3Client;

    public AmazonS3Client S3Client
    {
        get
        {
            if (_s3Client == null)
            {
                // Amazon Cognito 인증 공급자를 초기화합니다
                CognitoAWSCredentials credentials = new CognitoAWSCredentials(
                    "ap-northeast-2:c82b3d82-2a53-4971-a860-c017c8ef6a90", // 자격 증명 풀 ID
                    RegionEndpoint.APNortheast2 // 리전
                );

                _s3Client = new AmazonS3Client(credentials, _S3Region);
            }
            return _s3Client;
        }
    }
    
    
    public void UploadToS3(string filePath, string fileName)
    {
        FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

        PostObjectRequest request = new PostObjectRequest()
        {
            Bucket = "medicifinalchoi",
            Key = fileName,
            InputStream = stream,
            CannedACL = S3CannedACL.Private,
            Region = _S3Region
        };

        S3Client.PostObjectAsync(request, (responeObj) =>
        {
            if (responeObj.Exception == null)
            {
                Debug.Log("업로드 성공");
            }
            else
            {
                Debug.Log("업로드 실퍠 " + responeObj.Exception);
            }
        });
    }

    public void LoadFromS3(string filename)
    {
        string target = filename;
        var request = new ListObjectsRequest()
        {
            BucketName = "medicifinalchoi"
        };

        S3Client.ListObjectsAsync(request, (responeObj) =>
        {
            if (responeObj.Exception == null)
            {
                bool caseFound = responeObj.Response.S3Objects.Any(obj => obj.Key == target);

                if (caseFound == true)
                {
                    Debug.Log("Found case!");
                    S3Client.GetObjectAsync("medicifinalchoi", target, (responeObj) =>
                    {
                        if(responeObj.Response.ResponseStream != null)
                        {
                            byte[] data = null;

                            using(StreamReader reader= new StreamReader(responeObj.Response.ResponseStream))
                            {
                                using (MemoryStream memoryStream = new MemoryStream())
                                {
                                    var buffer = new byte[512];
                                    var bytesRead = default(int);

                                    while((bytesRead = reader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        memoryStream.Write(buffer, 0, bytesRead);
                                    }

                                    data = memoryStream.ToArray();
                                  //  File.WriteAllBytesAsync("C://Final/ServerTest/Assets/ExportObj/" +filename, data);
                                    File.WriteAllBytesAsync(Application.persistentDataPath + "/" + filename, data);
                                }
                            }

                            using (MemoryStream memory = new MemoryStream(data))
                            {
                                BinaryFormatter formatter = new BinaryFormatter();
                            }
                        }
                    });
                }
                else
                { 
                    Debug.Log("No case found");
                }
            }
        });
    }
    void Awake()
    {
        instance = this;

        UnityInitializer.AttachToGameObject(this.gameObject);

        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;

        S3Client.ListBucketsAsync(new ListBucketsRequest(), (responseobject) =>
        {
            if (responseobject.Exception == null)
            {
                responseobject.Response.Buckets.ForEach((s3b) =>
                {
                    Debug.Log("Bucket Name :" + s3b.BucketName);
                });
            }
            else
            {
                Debug.Log("AWS Error : " + responseobject.Exception);
            }
        });
    }
}

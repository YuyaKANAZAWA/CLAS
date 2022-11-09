using GritZ3.Classes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace GritZ3.Components.Settings
{
    public partial class DrawerDataFileSettings
    {
        [Parameter]
        public DataFileSettings? Stg { get; set; }
        [Parameter]
        public DrawerInfo? DrawerInfo { get; set; }

        [Parameter]
        public EventCallback<int> OnClickCallback { get; set; }


        private DataFileSettings NowStatus { get; set; } = new();
        private DataFileSettings KeepStatus { get; set; } = new();


        private void SetReturnStatus(DataFileSettings stg, DataFileSettings sts)
        {
            stg.FileName = sts.FileName;
            stg.ServerFullPath = sts.ServerFullPath;
            stg.LastModified = sts.LastModified;
            stg.ContentType = sts.ContentType;
            stg.Size = sts.Size;
        }

    private async Task Pushed_Cancel()
        {
            await Task.Run(() => NowStatus = KeepStatus.Clone());
            Close_This();
        }

        private async Task Pushed_Apply()
        {
            await Task.Run(() => KeepStatus = NowStatus.Clone());
            await Task.Run(() => SetReturnStatus(Stg, NowStatus));

            await OnClickCallback.InvokeAsync(1);
            Close_This();
        }

        private void Close_This()
        {
            DrawerInfo.Visible = false;
        }

        bool isLoading;
        string errorMessage = "";


        private async Task CopyToServer(InputFileChangeEventArgs e)
        {
            isLoading = true;
            errorMessage = string.Empty;

            try
            {
                string currentDir = System.IO.Directory.GetCurrentDirectory();
                //string path = @"C:\Users\ykubo\Desktop\tempfile";
                string path = currentDir + @"\tempfile";
                var filee = e.File;
                var stream = filee.OpenReadStream(maxAllowedSize: 1024000 * 50);
                //FileStream fs = File.Create(@"C:\Users\ykubo\Desktop\tempfile");
                FileStream fs = File.Create(path);
                await stream.CopyToAsync(fs);
                stream.Close();
                fs.Close();

                NowStatus.FileName = filee.Name;
                NowStatus.ContentType = filee.ContentType;
                NowStatus.Size = filee.Size;
                NowStatus.LastModified = filee.LastModified;
                NowStatus.ServerFullPath = path;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            finally
            {
                isLoading = false;
            }
        }
    }

    public class DataFileSettings
    {
        public string ServerFullPath { get; set; } = "";
        public string FileName { get; set; } = "";
        public DateTimeOffset LastModified { get; set; }
        public string ContentType { get; set; } = "";
        public long Size { get; set; }

        public DataFileSettings Clone()
        {
            return (DataFileSettings)MemberwiseClone();
        }
    }
}

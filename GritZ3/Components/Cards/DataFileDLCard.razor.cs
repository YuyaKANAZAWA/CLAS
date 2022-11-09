using Microsoft.AspNetCore.Components;

namespace GritZ3.Components.Cards
{
    public partial class DataFileDLCard
    {

        [Parameter]
        public DataFileDLCardData? Data { get; set; }
        [Parameter]
        public EventCallback<string> OnClickGetFileCallback { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await Task.Run(() => SetDisp());
        }
        private void SetDisp()
        {

        }



        private void OnClickKMLButton()
        {
            OnClickGetFileCallback.InvokeAsync("KML");
        }

        private void OnClickCSVButton()
        {
            OnClickGetFileCallback.InvokeAsync("CSV");
        }






    }

    public class DataFileDLCardData
    {
        public string Title { get; set; } = "";



        public string KmlDescription { get; set; } = String.Empty;
        public string KmlFileName { get; set; } = String.Empty;
        public string KmlSaveAsName { get; set; } = String.Empty;


    }

}

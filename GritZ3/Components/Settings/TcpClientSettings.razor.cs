using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;


namespace GritZ3.Components.Settings
{
    public partial class TcpClientSettings
    {
        [Parameter]
        public DataSourceStatus NowStatus { get; set; }


        List<ServerInfo>? _list;
        int? _selectedValue;
        ServerInfo? _selectedItem;
        string _name = string.Empty;

        protected override void OnInitialized()
        {
            _list = new List<ServerInfo>
                {
                    new ServerInfo {Id = 1, IpAddress = "127.0.0.1", Name = "127.0.0.1 (localhost)"},
                    new ServerInfo {Id = 2, IpAddress = "192.168.3.1", Name = "192.168.3.1 (Septentrio/default ip over usb)"},
                    new ServerInfo {Id = 3, IpAddress = "133.19.154.242", Name = "133.19.154.242"}
                };
            _selectedItem = _list.FirstOrDefault();
            _selectedValue = 3;
        }



        protected override async Task OnParametersSetAsync()
        {
            await Task.Run(() => SetDisp());
        }

        private void SetDisp()
        {
            var ip = NowStatus.TcpIpAddress;
            var item = _list.Where(s=> s.IpAddress == ip).FirstOrDefault();
            if (item != null)
            {
                _selectedItem = item;
                _selectedValue = item.Id;
            }
        }


        private void OnSelectedItemChangedHandler(ServerInfo value)
        {
            _selectedItem = value;
            NowStatus.TcpIpAddress = _selectedItem.IpAddress;
        }


        private void AddItem(MouseEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(_name) && _list != null)
            {
                // _nameがipかどうかを確かめること
                if (System.Net.IPAddress.TryParse(_name, out _))
                {
                    // IPアドレスとして認識
                    int newId = _list.Count + 1;
                    _list.Add(new ServerInfo { Id = newId, IpAddress = _name, Name = _name });
                }
                else
                {
                    Error(_name);
                }
                _name = string.Empty;
            }
        }

        private async Task Error(string ip)
        {
            await _message.Error($"{ip}: Invalid IP address.");

        }

    }
}
